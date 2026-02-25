namespace ShadowAgent.Commands;

/// <summary>
/// 状态查询命令
/// </summary>
public class StatusCommand : ICommand
{
    public string Name => "status";
    public string Description => "查询服务状态";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var memory = GC.GetTotalMemory(false) / 1024 / 1024;
        
        var status = $"""
            👻 墨影代理服务状态
            ├─ 运行时间：{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s
            ├─ 内存占用：{memory} MB
            ├─ 系统平台：{Environment.OSVersion.Platform}
            ├─ 处理器数：{Environment.ProcessorCount}
            └─ .NET 版本：{Environment.Version}
            """;
        
        return Task.FromResult(CommandResult.Ok(status));
    }
}
