using ShadowAgent.Plugins;

namespace ShadowAgent.Commands;

/// <summary>
/// 状态查询命令
/// </summary>
public class StatusCommand : ICommand
{
    private readonly PluginManager? _pluginManager;

    public StatusCommand(PluginManager? pluginManager = null)
    {
        _pluginManager = pluginManager;
    }

    public string Name => "status";
    public string Description => "查询服务状态和插件信息";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var memory = GC.GetTotalMemory(false) / 1024 / 1024;
        var startTime = DateTime.Now.AddMilliseconds(-Environment.TickCount64);
        
        var status = new List<string>
        {
            "👻 墨影代理服务状态",
            $"├─ 启动时间：{startTime:yyyy-MM-dd HH:mm:ss}",
            $"├─ 运行时间：{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
            $"├─ 内存占用：{memory} MB",
            $"├─ 系统平台：{Environment.OSVersion.Platform} ({Environment.OSVersion.Version})",
            $"├─ 处理器数：{Environment.ProcessorCount}",
            $"├─ .NET 版本：{Environment.Version}",
            $"├─ 工作目录：{Environment.CurrentDirectory}",
            $"└─ 用户：{Environment.UserName}@{Environment.MachineName}"
        };

        // 如果有插件管理器，显示插件信息
        if (_pluginManager != null)
        {
            var plugins = _pluginManager.GetLoadedPlugins().ToList();
            status.Add($"");
            status.Add($"📦 插件状态 ({plugins.Count} 个插件)");
            
            if (plugins.Count == 0)
            {
                status.Add($"   └─ 无已加载插件");
            }
            else
            {
                for (int i = 0; i < plugins.Count; i++)
                {
                    var plugin = plugins[i].Plugin;
                    var commands = plugins[i].Commands;
                    var loadedTime = plugins[i].LoadedTime.ToLocalTime().ToString("MM-dd HH:mm");
                    var prefix = i == plugins.Count - 1 ? "   └─ " : "   ├─ ";
                    
                    status.Add($"{prefix}[{plugin.Name}] v{plugin.Version}");
                    status.Add($"      ├─ 描述：{plugin.Description}");
                    status.Add($"      ├─ 加载时间：{loadedTime}");
                    status.Add($"      └─ 命令：{string.Join(", ", commands.Select(c => c.Name))}");
                }
            }
        }

        return Task.FromResult(CommandResult.Ok(string.Join("\n", status)));
    }
}