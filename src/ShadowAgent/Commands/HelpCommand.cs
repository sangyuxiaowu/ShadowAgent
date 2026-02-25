namespace ShadowAgent.Commands;

/// <summary>
/// 帮助命令
/// </summary>
public class HelpCommand : ICommand
{
    private readonly IEnumerable<ICommand> _commands;

    public HelpCommand(IEnumerable<ICommand> commands)
    {
        _commands = commands;
    }
    
    public string Name => "help";
    public string Description => "显示可用命令列表";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var commandList = _commands.Select(c => $"  {c.Name,-15} {c.Description}");
        var helpText = $"""
            👻 墨影代理 - 可用命令
            
            {string.Join("\n", commandList)}
            
            使用格式：<command> [args]
            """;
        
        return Task.FromResult(CommandResult.Ok(helpText));
    }
}
