using ShadowAgent.Commands;
using ShadowAgent.Plugins;

namespace ShadowAgent.BasePlugins;

/// <summary>
/// 基础系统插件 - 提供关机、重启等基础系统命令
/// </summary>
public class BaseSystemPlugin : IPlugin
{
    public string Name => "BaseSystem";
    public string Version => "1.0.0";
    public string Description => "基础系统命令插件（关机、重启、状态、心跳）";

    public IEnumerable<ICommand> GetCommands()
    {
        return new List<ICommand>
        {
            new ShutdownCommand(),
            new RebootCommand(),
            new StatusCommand(),
            new PingCommand()
        };
    }

    public Task InitializeAsync()
    {
        Console.WriteLine($"[插件] {Name} v{Version} 初始化完成");
        return Task.CompletedTask;
    }

    public Task CleanupAsync()
    {
        Console.WriteLine($"[插件] {Name} 清理完成");
        return Task.CompletedTask;
    }
}