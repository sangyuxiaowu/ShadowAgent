using System.Diagnostics;
using ShadowAgent.Commands;

namespace ShadowAgent.BasePlugins.Commands;

/// <summary>
/// 关机命令
/// </summary>
public class ShutdownCommand : ICommand
{
    public string Name => "shutdown";
    public string Description => "立即关闭系统";

    public async Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        Console.WriteLine($"[命令] 执行关机...");
        
        var shutdownCommands = new[]
        {
            ("sudo", "/usr/bin/systemctl poweroff"),
            ("sudo", "/sbin/shutdown -h now"),
            ("sudo", "/sbin/poweroff"),
            ("", "/usr/bin/systemctl poweroff"),
            ("", "/sbin/shutdown -h now"),
            ("", "/sbin/poweroff")
        };

        foreach (var (prefix, cmd) in shutdownCommands)
        {
            try
            {
                var fullCmd = string.IsNullOrEmpty(prefix) ? cmd : $"{prefix} {cmd}";
                Console.WriteLine($"尝试执行：{fullCmd}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = string.IsNullOrEmpty(prefix) ? cmd.Split(' ')[0] : prefix,
                    Arguments = string.IsNullOrEmpty(prefix) ? string.Join(" ", cmd.Split(' ').Skip(1)) : cmd,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync(ct);
                    
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"✓ 关机命令执行成功：{fullCmd}");
                        return CommandResult.Ok("系统正在关闭...");
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync(ct);
                        Console.WriteLine($"✗ 失败：{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 异常：{ex.Message}");
            }
        }

        return CommandResult.Fail("所有关机命令均失败，请检查权限配置");
    }
}

/// <summary>
/// 重启命令
/// </summary>
public class RebootCommand : ICommand
{
    public string Name => "reboot";
    public string Description => "立即重启系统";

    public async Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        Console.WriteLine($"[命令] 执行重启...");
        
        var rebootCommands = new[]
        {
            ("sudo", "/usr/bin/systemctl reboot"),
            ("sudo", "/sbin/reboot"),
            ("", "/usr/bin/systemctl reboot"),
            ("", "/sbin/reboot")
        };

        foreach (var (prefix, cmd) in rebootCommands)
        {
            try
            {
                var fullCmd = string.IsNullOrEmpty(prefix) ? cmd : $"{prefix} {cmd}";
                Console.WriteLine($"尝试执行：{fullCmd}");

                var startInfo = new ProcessStartInfo
                {
                    FileName = string.IsNullOrEmpty(prefix) ? cmd.Split(' ')[0] : prefix,
                    Arguments = string.IsNullOrEmpty(prefix) ? string.Join(" ", cmd.Split(' ').Skip(1)) : cmd,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync(ct);
                    
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"✓ 重启命令执行成功：{fullCmd}");
                        return CommandResult.Ok("系统正在重启...");
                    }
                    else
                    {
                        var error = await process.StandardError.ReadToEndAsync(ct);
                        Console.WriteLine($"✗ 失败：{error}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ 异常：{ex.Message}");
            }
        }

        return CommandResult.Fail("所有重启命令均失败，请检查权限配置");
    }
}

/// <summary>
/// 状态命令
/// </summary>
public class StatusCommand : ICommand
{
    public string Name => "status";
    public string Description => "查看服务状态";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        var status = $"""
            👻 墨影代理服务状态
            
            运行时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
            系统: {Environment.OSVersion}
            用户: {Environment.UserName}
            进程ID: {Environment.ProcessId}
            工作目录: {Environment.CurrentDirectory}
            
            服务正常 ✓
            """;
        
        return Task.FromResult(CommandResult.Ok(status));
    }
}

/// <summary>
/// 心跳命令
/// </summary>
public class PingCommand : ICommand
{
    public string Name => "ping";
    public string Description => "心跳检测";

    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct = default)
    {
        return Task.FromResult(CommandResult.Ok("pong 👻"));
    }
}