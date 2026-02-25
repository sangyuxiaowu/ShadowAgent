#!/bin/bash
# 墨影代理服务安装脚本

set -e

echo "👻 墨影代理服务安装脚本"
echo "=========================="

# 检查是否以 root 运行
if [ "$EUID" -ne 0 ]; then
    echo "❌ 请以 root 用户运行此脚本"
    echo "   sudo $0"
    exit 1
fi

# 获取脚本所在目录
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
SRC_DIR="$PROJECT_ROOT/src/ShadowAgent"
SERVICE_FILE="$PROJECT_ROOT/script/shadow-agent.service"

echo "📁 项目根目录：$PROJECT_ROOT"
echo "📁 源代码目录：$SRC_DIR"

# 检查 dotnet 环境
if ! command -v dotnet &> /dev/null; then
    echo "❌ 未找到 dotnet 命令"
    echo "请先安装 .NET SDK：https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ 检测到 dotnet：$(dotnet --version)"

# 发布项目
echo "📦 发布项目..."
cd "$SRC_DIR"
source /etc/profile.d/dotnet.sh 2>/dev/null || true
dotnet publish -c Release -o "$PROJECT_ROOT/publish"

echo "✅ 发布完成：$PROJECT_ROOT/publish/ShadowAgent"

# 创建 systemd 服务文件
echo "🔧 配置 systemd 服务..."
cat > /etc/systemd/system/shadow-agent.service << EOF
[Unit]
Description=墨影代理服务 (Shadow Agent Service)
Documentation=https://github.com/sangyuxiaowu/ShadowAgent
After=network.target

[Service]
Type=exec
Environment="PATH=/usr/bin:/bin:/usr/sbin:/sbin:/usr/local/bin"
Environment="DOTNET_ROOT=/usr/share/dotnet"
ExecStart=/bin/bash -c "source /etc/profile.d/dotnet.sh && exec $PROJECT_ROOT/publish/ShadowAgent"
WorkingDirectory=$PROJECT_ROOT/publish
Restart=always
RestartSec=5
StandardOutput=journal
StandardError=journal
SyslogIdentifier=shadow-agent

# 安全设置
NoNewPrivileges=false
ProtectSystem=false
ProtectHome=read-only

[Install]
WantedBy=multi-user.target
EOF

echo "✅ 服务文件已创建：/etc/systemd/system/shadow-agent.service"

# 重新加载 systemd
echo "🔄 重新加载 systemd..."
systemctl daemon-reload

# 启用并启动服务
echo "🚀 启动服务..."
systemctl enable shadow-agent
systemctl start shadow-agent

# 检查状态
echo "📊 服务状态："
systemctl status shadow-agent --no-pager

echo ""
echo "🎉 安装完成！"
echo ""
echo "📝 使用说明："
echo "  测试服务：$PROJECT_ROOT/script/test-client.sh"
echo "  查看日志：sudo journalctl -u shadow-agent -f"
echo "  停止服务：sudo systemctl stop shadow-agent"
echo "  重启服务：sudo systemctl restart shadow-agent"
