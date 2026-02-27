using ShadowAgent.Commands;
using ShadowAgent.Plugins;

namespace ShadowAgent.Commands;

/// <summary>
/// 加载插件命令
/// </summary>
public class LoadPluginCommand : ICommand
{
    private readonly PluginManager _pluginManager;

    public LoadPluginCommand(PluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public string Name => "load";
    public string Description => "加载插件 DLL 文件";

    public async Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        if (args.Length == 0)
        {
            return CommandResult.Fail("请指定要加载的 DLL 文件路径");
        }

        var dllPath = args[0];
        
        // 如果是相对路径，转换为绝对路径
        if (!Path.IsPathRooted(dllPath))
        {
            dllPath = Path.Combine(AppContext.BaseDirectory, dllPath);
        }

        Console.WriteLine($"[命令] 加载插件：{dllPath}");
        
        var success = await _pluginManager.LoadPluginAsync(dllPath);
        
        if (success)
        {
            return CommandResult.Ok($"插件加载成功：{Path.GetFileName(dllPath)}");
        }
        else
        {
            return CommandResult.Fail($"插件加载失败：{Path.GetFileName(dllPath)}");
        }
    }
}

/// <summary>
/// 卸载插件命令
/// </summary>
public class UnloadPluginCommand : ICommand
{
    private readonly PluginManager _pluginManager;

    public UnloadPluginCommand(PluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public string Name => "unload";
    public string Description => "卸载指定插件";

    public async Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        if (args.Length == 0)
        {
            return CommandResult.Fail("请指定要卸载的插件名称");
        }

        var pluginName = args[0];
        Console.WriteLine($"[命令] 卸载插件：{pluginName}");
        
        var success = await _pluginManager.UnloadPluginAsync(pluginName);
        
        if (success)
        {
            return CommandResult.Ok($"插件卸载成功：{pluginName}");
        }
        else
        {
            return CommandResult.Fail($"插件卸载失败：{pluginName}");
        }
    }
}

/// <summary>
/// 列出插件命令
/// </summary>
public class ListPluginsCommand : ICommand
{
    private readonly PluginManager _pluginManager;

    public ListPluginsCommand(PluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public string Name => "plugins";
    public string Description => "列出已加载的插件";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        Console.WriteLine($"[命令] 列出插件");
        
        var plugins = _pluginManager.GetLoadedPlugins().ToList();
        
        if (plugins.Count == 0)
        {
            return Task.FromResult(CommandResult.Ok("没有已加载的插件"));
        }

        var pluginList = new List<string>();
        pluginList.Add($"已加载 {plugins.Count} 个插件：");
        
        foreach (var pluginInfo in plugins)
        {
            var plugin = pluginInfo.Plugin;
            var commands = pluginInfo.Commands;
            var loadedTime = pluginInfo.LoadedTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
            
            pluginList.Add($"");
            pluginList.Add($"  [{plugin.Name}]");
            pluginList.Add($"  版本: {plugin.Version}");
            pluginList.Add($"  描述: {plugin.Description}");
            pluginList.Add($"  加载时间: {loadedTime}");
            pluginList.Add($"  命令: {string.Join(", ", commands.Select(c => c.Name))}");
        }

        return Task.FromResult(CommandResult.Ok(string.Join("\n", pluginList)));
    }
}

/// <summary>
/// 重新加载插件命令
/// </summary>
public class ReloadPluginsCommand : ICommand
{
    private readonly PluginManager _pluginManager;

    public ReloadPluginsCommand(PluginManager pluginManager)
    {
        _pluginManager = pluginManager;
    }

    public string Name => "reload-plugins";
    public string Description => "重新加载所有插件";

    public async Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        Console.WriteLine($"[命令] 重新加载所有插件");
        
        await _pluginManager.ReloadAllPluginsAsync();
        
        return CommandResult.Ok("所有插件已重新加载");
    }
}