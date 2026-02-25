namespace ShadowAgent.Commands;

/// <summary>
/// 命令接口 - 所有可执行命令都要实现此接口
/// </summary>
public interface ICommand
{
    /// <summary>
    /// 命令名称（用于匹配）
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 命令描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 执行命令
    /// </summary>
    Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default);
}

/// <summary>
/// 命令执行结果
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Error { get; set; }
    
    public static CommandResult Ok(string message) => new() { Success = true, Message = message };
    public static CommandResult Fail(string error) => new() { Success = false, Error = error };
}
