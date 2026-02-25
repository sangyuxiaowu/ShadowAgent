namespace ShadowAgent.Commands;

/// <summary>
/// 命令注册表 - 管理所有可用命令
/// </summary>
public class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public CommandRegistry(IEnumerable<ICommand> commands)
    {
        foreach (var command in commands)
        {
            _commands[command.Name] = command;
            Console.WriteLine($"[注册] 命令：{command.Name} - {command.Description}");
        }
    }

    public ICommand? GetCommand(string name)
    {
        return _commands.TryGetValue(name, out var command) ? command : null;
    }

    public IEnumerable<ICommand> GetAllCommands() => _commands.Values;

    public bool Contains(string name) => _commands.ContainsKey(name);
}
