namespace ShadowAgent.Commands;

/// <summary>
/// 心跳检测命令
/// </summary>
public class PingCommand : ICommand
{
    public string Name => "ping";
    public string Description => "心跳检测";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        return Task.FromResult(CommandResult.Ok($"pong - {DateTime.Now:yyyy-MM-dd HH:mm:ss}"));
    }
}
