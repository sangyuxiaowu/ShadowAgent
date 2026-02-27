using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ShadowAgent.Commands;
using ShadowAgent.Plugins;

namespace ShadowAgent;

public class Program
{
    private static Socket? _listener;
    private static bool _running = true;
    private static ExtendedCommandRegistry? _registry;
    private static PluginManager? _pluginManager;
    private static string _socketPath = "/tmp/shadow-agent.sock";
    private static string _magicToken = "SHADOW";

    public static async Task Main(string[] args)
    {
        Console.WriteLine("👻 墨影代理服务启动中...");
        
        // 加载配置
        LoadConfig();
        
        // 清理旧的 socket 文件
        if (File.Exists(_socketPath))
        {
            File.Delete(_socketPath);
            Console.WriteLine($"已清理旧的 socket 文件");
        }

        // 创建扩展的命令注册表（初始只有ping命令）
        var commands = new List<ICommand>
        {
            new PingCommand(),
        };
        
        _registry = new ExtendedCommandRegistry(commands);
        
        // 创建插件管理器
        _pluginManager = new PluginManager(_registry);
        
        // 加载所有插件
        await _pluginManager.LoadAllPluginsAsync();
        
        // 创建status命令（需要插件管理器）
        var statusCommand = new StatusCommand(_pluginManager);
        _registry.RegisterCommand(statusCommand);
        
        // 注册插件管理命令
        var pluginCommands = new List<ICommand>
        {
            new LoadPluginCommand(_pluginManager),
            new UnloadPluginCommand(_pluginManager),
            new ListPluginsCommand(_pluginManager),
            new ReloadPluginsCommand(_pluginManager),
        };
        
        _registry.RegisterCommands(pluginCommands);
        
        // 创建帮助命令（需要所有命令列表）
        var helpCommand = new HelpCommand(_registry.GetAllCommands());
        _registry.RegisterCommand(helpCommand);

        // 创建 Unix Domain Socket 监听器
        _listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        var endPoint = new UnixDomainSocketEndPoint(_socketPath);
        _listener.Bind(endPoint);
        _listener.Listen(10);
        
        // 设置 socket 权限
        try
        {
            File.SetUnixFileMode(_socketPath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherWrite);
            Console.WriteLine($"Socket 权限已设置");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[警告] 设置 socket 权限失败：{ex.Message}");
        }
        
        Console.WriteLine($"监听 Socket: {_socketPath}");
        Console.WriteLine($"Magic Token: {_magicToken}");
        Console.WriteLine($"可用命令：{string.Join(", ", _registry.GetAllCommands().Select(c => c.Name))}");
        Console.WriteLine($"按 Ctrl+C 停止服务\n");

        // 处理取消信号
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            _running = false;
            Console.WriteLine("\n正在停止服务...");
        };

        try
        {
            while (_running)
            {
                try
                {
                    var client = await _listener.AcceptAsync();
                    _ = HandleClientAsync(client);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Console.WriteLine($"[错误] 接受连接失败：{ex.Message}");
                    }
                }
            }
        }
        finally
        {
            _listener?.Close();
            _listener?.Dispose();
            if (File.Exists(_socketPath))
            {
                File.Delete(_socketPath);
            }
            Console.WriteLine("服务已停止");
        }
    }

    private static void LoadConfig()
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var config = JsonDocument.Parse(json).RootElement;
                
                if (config.TryGetProperty("SocketPath", out var socketPath))
                    _socketPath = socketPath.GetString() ?? _socketPath;
                    
                if (config.TryGetProperty("MagicToken", out var magicToken))
                    _magicToken = magicToken.GetString() ?? _magicToken;
                    
                Console.WriteLine($"[配置] 已加载 config.json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[警告] 加载配置失败：{ex.Message}，使用默认配置");
            }
        }
        else
        {
            Console.WriteLine($"[配置] 未找到 config.json，使用默认配置");
        }
    }

    private static async Task HandleClientAsync(Socket client)
    {
        using var stream = new NetworkStream(client);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        try
        {
            var request = await reader.ReadLineAsync();
            
            if (string.IsNullOrEmpty(request))
            {
                await writer.WriteLineAsync("ERROR: 空请求");
                return;
            }

            Console.WriteLine($"[收到] {request}");

            // 解析请求：格式为 "TOKEN command [args]"
            var parts = request.Split(' ', 3);
            
            if (parts.Length < 2 || parts[0] != _magicToken)
            {
                await writer.WriteLineAsync("ERROR: 认证失败，格式应为 'SHADOW <command> [args]'");
                return;
            }

            var commandName = parts[1];
            var commandArgs = parts.Length > 2 ? parts[2].Split(' ') : Array.Empty<string>();

            // 查找并执行命令
            var command = _registry?.GetCommand(commandName);
            if (command == null)
            {
                await writer.WriteLineAsync($"ERROR: 未知命令 '{commandName}'，使用 'help' 查看可用命令");
                return;
            }

            Console.WriteLine($"[执行] {commandName}({string.Join(", ", commandArgs)})");
            
            var result = await command.ExecuteAsync(commandArgs);
            
            if (result.Success)
            {
                await writer.WriteLineAsync($"OK: {result.Message}");
            }
            else
            {
                await writer.WriteLineAsync($"ERROR: {result.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[错误] 处理客户端失败：{ex.Message}");
            try
            {
                await writer.WriteLineAsync($"ERROR: {ex.Message}");
            }
            catch { }
        }
        finally
        {
            client.Close();
        }
    }
}