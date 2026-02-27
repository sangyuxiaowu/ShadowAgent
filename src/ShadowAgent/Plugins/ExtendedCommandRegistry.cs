using ShadowAgent.Commands;

namespace ShadowAgent.Plugins;

/// <summary>
/// 扩展的命令注册表 - 支持动态添加和移除命令
/// </summary>
public class ExtendedCommandRegistry : CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

    public ExtendedCommandRegistry(IEnumerable<ICommand> commands) : base(commands)
    {
        // 初始化时注册所有命令
        foreach (var command in commands)
        {
            _commands[command.Name] = command;
        }
    }

    /// <summary>
    /// 动态注册命令
    /// </summary>
    public bool RegisterCommand(ICommand command)
    {
        if (_commands.ContainsKey(command.Name))
        {
            Console.WriteLine($"[命令] 命令已存在：{command.Name}");
            return false;
        }

        _commands[command.Name] = command;
        Console.WriteLine($"[命令] ✓ 注册命令：{command.Name} - {command.Description}");
        return true;
    }

    /// <summary>
    /// 动态注册多个命令
    /// </summary>
    public void RegisterCommands(IEnumerable<ICommand> commands)
    {
        foreach (var command in commands)
        {
            RegisterCommand(command);
        }
    }

    /// <summary>
    /// 动态移除命令
    /// </summary>
    public bool UnregisterCommand(string commandName)
    {
        if (!_commands.ContainsKey(commandName))
        {
            Console.WriteLine($"[命令] 命令不存在：{commandName}");
            return false;
        }

        _commands.Remove(commandName);
        Console.WriteLine($"[命令] ✓ 移除命令：{commandName}");
        return true;
    }

    /// <summary>
    /// 动态移除多个命令
    /// </summary>
    public void UnregisterCommands(IEnumerable<string> commandNames)
    {
        foreach (var commandName in commandNames)
        {
            UnregisterCommand(commandName);
        }
    }

    /// <summary>
    /// 获取命令（重写基类方法）
    /// </summary>
    public new ICommand? GetCommand(string name)
    {
        return _commands.TryGetValue(name, out var command) ? command : null;
    }

    /// <summary>
    /// 获取所有命令（重写基类方法）
    /// </summary>
    public new IEnumerable<ICommand> GetAllCommands() => _commands.Values;

    /// <summary>
    /// 检查命令是否存在（重写基类方法）
    /// </summary>
    public new bool Contains(string name) => _commands.ContainsKey(name);
}