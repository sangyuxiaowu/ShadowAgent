#!/bin/bash
# 墨影代理 - 测试客户端

SOCKET_PATH="/tmp/shadow-agent.sock"
TOKEN="SHADOW"

# 检查 socket 是否存在
if [ ! -S "$SOCKET_PATH" ]; then
    echo "❌ Socket 不存在：$SOCKET_PATH"
    echo "请确保服务正在运行：sudo systemctl status shadow-agent"
    exit 1
fi

send_command() {
    local cmd=$1
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "📤 发送：$TOKEN $cmd"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "$TOKEN $cmd" | socat - UNIX-CONNECT:$SOCKET_PATH 2>/dev/null
    echo ""
}

echo "👻 墨影代理 - 测试客户端"
echo "Socket: $SOCKET_PATH"
echo ""

# 测试各命令
send_command "ping"
send_command "status"
send_command "help"

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "⚠️  下面是关机命令（不会真正执行）"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "如果要测试关机，取消下面注释："
echo "# send_command \"shutdown\""
