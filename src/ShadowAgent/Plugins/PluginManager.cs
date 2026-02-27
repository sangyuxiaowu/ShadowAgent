using System.Reflection;
using System.Text.Json;
using ShadowAgent.Commands;

namespace ShadowAgent.Plugins;

/// <summary>
/// 插件管理器 - 负责加载、管理和卸载插件
/// </summary>
public class PluginManager
{
    private readonly Dictionary<string, PluginInfo> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly ExtendedCommandRegistry _commandRegistry;
    private readonly string _pluginsDirectory;
    
    /// <summary>
    /// 插件信息
    /// </summary>
    private class PluginInfo
    {
        public Assembly Assembly { get; set; } = null!;
        public IPlugin Plugin { get; set; } = null!;
        public List<ICommand> Commands { get; set; } = new();
        public DateTime LoadedTime { get; set; }
    }

    public PluginManager(ExtendedCommandRegistry commandRegistry, string? pluginsDirectory = null)
    {
        _commandRegistry = commandRegistry;
        
        // 尝试从配置文件读取插件目录
        if (string.IsNullOrEmpty(pluginsDirectory))
        {
            pluginsDirectory = GetPluginsDirectoryFromConfig();
        }
        
        _pluginsDirectory = pluginsDirectory ?? Path.Combine(AppContext.BaseDirectory, "plugins");
        
        // 确保插件目录存在
        if (!Directory.Exists(_pluginsDirectory))
        {
            Directory.CreateDirectory(_pluginsDirectory);
            Console.WriteLine($"[插件] 创建插件目录：{_pluginsDirectory}");
        }
    }

    private string? GetPluginsDirectoryFromConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var config = JsonDocument.Parse(json).RootElement;
                
                if (config.TryGetProperty("PluginsDirectory", out var pluginsDir))
                {
                    var dir = pluginsDir.GetString();
                    if (!string.IsNullOrEmpty(dir))
                    {
                        // 如果是相对路径，转换为绝对路径
                        if (!Path.IsPathRooted(dir))
                        {
                            dir = Path.Combine(AppContext.BaseDirectory, dir);
                        }
                        return dir;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[插件] 读取插件目录配置失败：{ex.Message}");
            }
        }
        
        return null;
    }

    /// <summary>
    /// 加载所有插件
    /// </summary>
    public async Task LoadAllPluginsAsync()
    {
        Console.WriteLine($"[插件] 扫描插件目录：{_pluginsDirectory}");
        
        var dllFiles = Directory.GetFiles(_pluginsDirectory, "*.dll", SearchOption.AllDirectories);
        Console.WriteLine($"[插件] 找到 {dllFiles.Length} 个 DLL 文件");
        
        foreach (var dllPath in dllFiles)
        {
            try
            {
                await LoadPluginAsync(dllPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[插件] 加载插件失败 {Path.GetFileName(dllPath)}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 加载指定插件
    /// </summary>
    public async Task<bool> LoadPluginAsync(string dllPath)
    {
        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"[插件] 文件不存在：{dllPath}");
            return false;
        }

        var fileName = Path.GetFileName(dllPath);
        Console.WriteLine($"[插件] 正在加载插件：{fileName}");

        try
        {
            // 加载程序集
            var assembly = Assembly.LoadFrom(dllPath);
            
            // 查找所有实现 IPlugin 接口的类型
            var pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .ToList();

            if (pluginTypes.Count == 0)
            {
                Console.WriteLine($"[插件] {fileName} 中没有找到插件实现");
                return false;
            }

            bool anyLoaded = false;
            
            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    // 创建插件实例
                    var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;
                    
                    // 初始化插件
                    await plugin.InitializeAsync();
                    
                    // 获取插件提供的命令
                    var commands = plugin.GetCommands().ToList();
                    
                    // 注册插件信息
                    var pluginInfo = new PluginInfo
                    {
                        Assembly = assembly,
                        Plugin = plugin,
                        Commands = commands,
                        LoadedTime = DateTime.UtcNow
                    };
                    
                    _plugins[plugin.Name] = pluginInfo;
                    
                    // 注册命令到命令注册表
                    _commandRegistry.RegisterCommands(commands);
                    
                    Console.WriteLine($"[插件] ✓ 成功加载插件：{plugin.Name} v{plugin.Version}");
                    Console.WriteLine($"[插件]   描述：{plugin.Description}");
                    Console.WriteLine($"[插件]   提供 {commands.Count} 个命令");
                    
                    anyLoaded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[插件] 创建插件实例失败 {pluginType.Name}: {ex.Message}");
                }
            }
            
            return anyLoaded;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[插件] 加载插件失败 {fileName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 卸载指定插件
    /// </summary>
    public async Task<bool> UnloadPluginAsync(string pluginName)
    {
        if (!_plugins.TryGetValue(pluginName, out var pluginInfo))
        {
            Console.WriteLine($"[插件] 插件未找到：{pluginName}");
            return false;
        }

        Console.WriteLine($"[插件] 正在卸载插件：{pluginName}");
        
        try
        {
            // 执行插件清理
            await pluginInfo.Plugin.CleanupAsync();
            
            // 从命令注册表中移除命令
            var commandNames = pluginInfo.Commands.Select(c => c.Name).ToList();
            _commandRegistry.UnregisterCommands(commandNames);
            
            // 从插件列表中移除
            _plugins.Remove(pluginName);
            
            Console.WriteLine($"[插件] ✓ 成功卸载插件：{pluginName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[插件] 卸载插件失败 {pluginName}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取已加载的插件列表
    /// </summary>
    public IEnumerable<PluginInfo> GetLoadedPlugins() => _plugins.Values;

    /// <summary>
    /// 获取插件信息
    /// </summary>
    public PluginInfo? GetPlugin(string pluginName)
    {
        return _plugins.TryGetValue(pluginName, out var pluginInfo) ? pluginInfo : null;
    }

    /// <summary>
    /// 重新加载所有插件
    /// </summary>
    public async Task ReloadAllPluginsAsync()
    {
        Console.WriteLine($"[插件] 重新加载所有插件");
        
        // 先卸载所有插件
        var pluginNames = _plugins.Keys.ToList();
        foreach (var pluginName in pluginNames)
        {
            await UnloadPluginAsync(pluginName);
        }
        
        // 重新加载
        await LoadAllPluginsAsync();
    }
}