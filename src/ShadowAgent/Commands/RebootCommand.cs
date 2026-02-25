using System.Diagnostics;

namespace ShadowAgent.Commands;

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
