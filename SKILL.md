# 墨影代理服务调用技能

**技能名称**: shadow-agent-caller  
**技能类型**: 系统服务调用  
**适用场景**: 通过 Unix Socket 调用墨影代理服务执行系统操作  
**依赖要求**: 墨影代理服务已安装并运行

## 概述

墨影代理服务是一个可扩展的系统代理服务，通过 Unix Socket (`/tmp/shadow-agent.sock`) 接收指令并执行系统操作。采用命令处理器模式，易于扩展新功能。

## 服务状态检查

### 检查服务是否运行
```bash
# 检查 systemd 服务状态
sudo systemctl status shadow-agent

# 检查 socket 文件是否存在
ls -la /tmp/shadow-agent.sock

# 查看服务日志
sudo journalctl -u shadow-agent -f
```

### 启动/停止服务
```bash
# 启动服务
sudo systemctl start shadow-agent

# 停止服务
sudo systemctl stop shadow-agent

# 重启服务
sudo systemctl restart shadow-agent

# 启用开机自启
sudo systemctl enable shadow-agent

# 禁用开机自启
sudo systemctl disable shadow-agent
```

## 命令调用格式

### 基本格式
```
SHADOW <command> [args]
```

- **SHADOW**: Magic Token（可在 config.json 中修改）
- **<command>**: 命令名称
- **[args]**: 可选参数

### 可用命令

| 命令 | 描述 | 示例 |
|------|------|------|
| `ping` | 心跳检测，返回当前时间 | `SHADOW ping` |
| `status` | 查询服务状态（运行时间、内存占用等） | `SHADOW status` |
| `shutdown` | 立即关闭系统 | `SHADOW shutdown` |
| `reboot` | 立即重启系统 | `SHADOW reboot` |
| `help` | 显示可用命令列表 | `SHADOW help` |

## 调用方法

### 方法一：使用 socat（推荐）
```bash
# 基本调用
echo "SHADOW ping" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 保存响应到变量
RESPONSE=$(echo "SHADOW status" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock 2>/dev/null)
echo "响应：$RESPONSE"
```

### 方法二：使用 netcat (nc)
```bash
# 发送命令
echo "SHADOW ping" | nc -U /tmp/shadow-agent.sock

# 带超时设置
echo "SHADOW status" | timeout 2 nc -U /tmp/shadow-agent.sock
```

### 方法三：使用 Python
```python
import socket

def send_shadow_command(command):
    """发送命令到墨影代理服务"""
    try:
        s = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        s.connect('/tmp/shadow-agent.sock')
        s.send(f'SHADOW {command}\n'.encode())
        response = s.recv(4096).decode().strip()
        s.close()
        return response
    except Exception as e:
        return f"错误：{e}"

# 示例调用
print(send_shadow_command("ping"))
print(send_shadow_command("status"))
```

### 方法四：使用测试脚本
```bash
# 使用项目自带的测试脚本
cd /home/sangsq/workspace/ShadowAgent
./script/test-client.sh

# 只测试特定命令
echo "SHADOW ping" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock
```

## 高级调用示例

### 1. 批量执行命令
```bash
#!/bin/bash
# batch-commands.sh

SOCKET="/tmp/shadow-agent.sock"
TOKEN="SHADOW"

commands=("ping" "status" "help")

for cmd in "${commands[@]}"; do
    echo "执行命令：$cmd"
    echo "$TOKEN $cmd" | socat - UNIX-CONNECT:$SOCKET
    echo "---"
done
```

### 2. 带错误处理的调用
```bash
#!/bin/bash
# safe-call.sh

SOCKET="/tmp/shadow-agent.sock"
TOKEN="SHADOW"
COMMAND=$1

if [ ! -S "$SOCKET" ]; then
    echo "错误：Socket 不存在，服务可能未运行"
    exit 1
fi

if [ -z "$COMMAND" ]; then
    echo "用法：$0 <command>"
    exit 1
fi

RESPONSE=$(echo "$TOKEN $COMMAND" | socat - UNIX-CONNECT:$SOCKET 2>/dev/null)

if [[ $RESPONSE == OK:* ]]; then
    echo "✅ 成功：${RESPONSE#OK: }"
elif [[ $RESPONSE == ERROR:* ]]; then
    echo "❌ 错误：${RESPONSE#ERROR: }"
else
    echo "⚠️  未知响应：$RESPONSE"
fi
```

### 3. 集成到 OpenClaw 会话
```bash
# 在 OpenClaw 中调用墨影代理服务
# 可以通过 exec 工具执行命令

# 示例：检查服务状态
exec command="echo 'SHADOW status' | socat - UNIX-CONNECT:/tmp/shadow-agent.sock"

# 示例：关机（需要确认）
# exec command="echo 'SHADOW shutdown' | socat - UNIX-CONNECT:/tmp/shadow-agent.sock"
```

## 响应格式

### 成功响应
```
OK: <message>
```
示例：`OK: pong - 2026-02-25 13:46:22`

### 错误响应
```
ERROR: <error_message>
```
示例：`ERROR: 未知命令 'test'，使用 'help' 查看可用命令`

## 配置说明

### 修改 Magic Token
编辑 `src/ShadowAgent/config.json`：
```json
{
  "SocketPath": "/tmp/shadow-agent.sock",
  "MagicToken": "YOUR_NEW_TOKEN",  # 修改这里
  "LogLevel": "Info"
}
```

然后重新发布和安装服务：
```bash
cd /home/sangsq/workspace/ShadowAgent/src/ShadowAgent
dotnet publish -c Release -o ../../publish
sudo systemctl restart shadow-agent
```

### 修改 Socket 路径
同样在 `config.json` 中修改 `SocketPath`，然后重启服务。

## 故障排除

### 常见问题

1. **Socket 不存在**
   ```
   ❌ 错误：Socket 不存在
   ```
   解决方案：确保服务正在运行 `sudo systemctl start shadow-agent`

2. **权限被拒绝**
   ```
   ❌ 错误：Permission denied
   ```
   解决方案：检查 socket 文件权限，确保当前用户有访问权限

3. **命令不存在**
   ```
   ❌ 错误：未知命令 'xxx'
   ```
   解决方案：使用 `SHADOW help` 查看可用命令

4. **服务未响应**
   ```
   （无响应或超时）
   ```
   解决方案：检查服务日志 `sudo journalctl -u shadow-agent -n 20`

### 调试命令
```bash
# 查看 socket 信息
ls -la /tmp/shadow-agent.sock
stat /tmp/shadow-agent.sock

# 测试 socket 连接
timeout 1 socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 查看服务进程
ps aux | grep ShadowAgent
```

## 扩展命令

要添加新命令，参考以下步骤：

1. 在 `src/ShadowAgent/Commands/` 目录创建新类
2. 实现 `ICommand` 接口
3. 在 `Program.cs` 的 `Main` 方法中注册新命令
4. 重新发布和重启服务

## 安全建议

1. **修改默认 Token**：生产环境中不要使用默认的 `SHADOW`
2. **限制访问权限**：通过文件权限控制谁可以访问 socket
3. **监控日志**：定期检查服务日志
4. **网络隔离**：确保只有可信主机可以访问

---

**技能维护者**: [sangsq] (sang93@qq.com)  
**最后更新**: 2026-02-25  
**GitHub**: https://github.com/sangyuxiaowu/ShadowAgent

👻 **墨影代理服务** - 代码中的幽灵，文字间的旅人
