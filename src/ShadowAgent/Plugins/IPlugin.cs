using ShadowAgent.Commands;

namespace ShadowAgent.Plugins;

/// <summary>
/// 插件接口 - 所有插件必须实现此接口
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// 插件名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 插件版本
    /// </summary>
    string Version { get; }
    
    /// <summary>
    /// 插件描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 获取插件提供的命令
    /// </summary>
    IEnumerable<ICommand> GetCommands();
    
    /// <summary>
    /// 插件初始化（可选）
    /// </summary>
    Task InitializeAsync();
    
    /// <summary>
    /// 插件清理（可选）
    /// </summary>
    Task CleanupAsync();
}