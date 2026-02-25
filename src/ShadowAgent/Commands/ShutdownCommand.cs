using System.Diagnostics;

namespace ShadowAgent.Commands;

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
