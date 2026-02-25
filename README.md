# 墨影代理服务 (Shadow Agent Service)

可扩展的系统代理服务，通过 Unix Socket 接收指令并执行系统操作。

## 项目结构

```
ShadowAgent/
├── src/
│   └── ShadowAgent/                    # 主项目
│       ├── Commands/                   # 命令处理器
│       │   ├── ICommand.cs             # 命令接口
│       │   ├── CommandRegistry.cs      # 命令注册表
│       │   ├── ShutdownCommand.cs      # 关机命令
│       │   ├── RebootCommand.cs        # 重启命令
│       │   ├── StatusCommand.cs        # 状态查询
│       │   ├── PingCommand.cs          # 心跳检测
│       │   └── HelpCommand.cs          # 帮助命令
│       ├── Program.cs                  # 主程序
│       ├── config.json                 # 配置文件
│       └── ShadowAgent.csproj          # 项目文件
├── script/                             # 脚本目录
│   ├── install-service.sh              # 安装脚本
│   ├── test-client.sh                  # 测试客户端
│   └── shadow-agent.service            # systemd 服务模板
├── publish/                            # 发布目录（自动生成）
├── ShadowAgent.sln                     # 解决方案文件
├── .gitignore                          # Git 忽略文件
├── LICENSE                             # 许可证
└── README.md                           # 本文档
```

## 架构设计

采用**命令处理器模式**，易于扩展新功能：

1. **命令接口** (`ICommand`) - 定义命令标准
2. **命令注册表** (`CommandRegistry`) - 管理所有命令
3. **命令处理器** - 实现具体功能
4. **Socket 服务** - Unix Domain Socket 通信

## 快速开始

### 1. 安装依赖

```bash
# 安装 .NET SDK (10.0+)
# 参考：https://dotnet.microsoft.com/download
```

### 2. 一键安装

```bash
# 以 root 用户运行
sudo ./script/install-service.sh
```

### 3. 手动安装

```bash
# 发布项目
cd src/ShadowAgent
source /etc/profile.d/dotnet.sh
dotnet publish -c Release -o ../../publish

# 安装服务
sudo cp script/shadow-agent.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable shadow-agent
sudo systemctl start shadow-agent
```

## 使用方法

### 命令格式

```
SHADOW <command> [args]
```

### 可用命令

| 命令 | 描述 | 示例 |
|------|------|------|
| `ping` | 心跳检测 | `SHADOW ping` |
| `status` | 服务状态 | `SHADOW status` |
| `shutdown` | 关机 | `SHADOW shutdown` |
| `reboot` | 重启 | `SHADOW reboot` |
| `help` | 帮助信息 | `SHADOW help` |

### 发送指令

```bash
# 使用 socat
echo "SHADOW ping" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 使用测试脚本
./script/test-client.sh

# 使用 Python
python3 -c "
import socket
s = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
s.connect('/tmp/shadow-agent.sock')
s.send(b'SHADOW status\n')
print(s.recv(4096).decode())
s.close()
"
```

## 扩展新命令

1. 在 `src/ShadowAgent/Commands/` 目录创建新类，实现 `ICommand` 接口
2. 在 `Program.cs` 的 `Main` 方法中注册新命令

示例：
```csharp
public class MyCommand : ICommand
{
    public string Name => "mycmd";
    public string Description => "我的命令";
    
    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct)
    {
        // 实现逻辑
        return Task.FromResult(CommandResult.Ok("执行成功"));
    }
}
```

## 配置文件

`src/ShadowAgent/config.json`:
```json
{
  "SocketPath": "/tmp/shadow-agent.sock",
  "MagicToken": "SHADOW",
  "LogLevel": "Info"
}
```

## 查看日志

```bash
# 实时日志
sudo journalctl -u shadow-agent -f

# 最近 100 行
sudo journalctl -u shadow-agent -n 100
```

## 安全说明

- Socket 文件位于 `/tmp/shadow-agent.sock`
- 需要 Magic Token 认证（默认：`SHADOW`）
- 建议在生产环境中：
  - 修改默认 Token
  - 限制 socket 访问权限（组权限）
  - 添加 IP 白名单或用户认证

## 停止服务

```bash
sudo systemctl stop shadow-agent
sudo systemctl disable shadow-agent
```

## 许可证

MIT License - 详见 [LICENSE](LICENSE)

## 贡献

欢迎提交 Issue 和 Pull Request！

---

👻 **墨影代理服务** - 代码中的幽灵，文字间的旅人
