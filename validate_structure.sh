#!/bin/bash

echo "🔍 验证墨影代理项目结构..."

PROJECT_ROOT="/work/workspace/ShadowAgent"
ERRORS=0

check_file() {
    local file="$1"
    local description="$2"
    
    if [ -f "$file" ]; then
        echo "✅ $description"
        return 0
    else
        echo "❌ $description - 文件不存在: $file"
        ERRORS=$((ERRORS + 1))
        return 1
    fi
}

check_directory() {
    local dir="$1"
    local description="$2"
    
    if [ -d "$dir" ]; then
        echo "✅ $description"
        return 0
    else
        echo "❌ $description - 目录不存在: $dir"
        ERRORS=$((ERRORS + 1))
        return 1
    fi
}

echo ""
echo "📁 主服务项目结构检查:"
check_directory "$PROJECT_ROOT/src/ShadowAgent" "主服务目录"
check_file "$PROJECT_ROOT/src/ShadowAgent/Program.cs" "主程序文件"
check_file "$PROJECT_ROOT/src/ShadowAgent/ShadowAgent.csproj" "主项目文件"
check_file "$PROJECT_ROOT/src/ShadowAgent/config.json" "配置文件"

echo ""
echo "📦 插件系统检查:"
check_directory "$PROJECT_ROOT/src/ShadowAgent/Plugins" "插件系统目录"
check_file "$PROJECT_ROOT/src/ShadowAgent/Plugins/IPlugin.cs" "插件接口"
check_file "$PROJECT_ROOT/src/ShadowAgent/Plugins/PluginManager.cs" "插件管理器"
check_file "$PROJECT_ROOT/src/ShadowAgent/Plugins/ExtendedCommandRegistry.cs" "扩展命令注册表"

echo ""
echo "⚙️ 命令系统检查:"
check_directory "$PROJECT_ROOT/src/ShadowAgent/Commands" "命令目录"
check_file "$PROJECT_ROOT/src/ShadowAgent/Commands/PingCommand.cs" "Ping命令"
check_file "$PROJECT_ROOT/src/ShadowAgent/Commands/StatusCommand.cs" "Status命令"
check_file "$PROJECT_ROOT/src/ShadowAgent/Commands/HelpCommand.cs" "Help命令"
check_file "$PROJECT_ROOT/src/ShadowAgent/Commands/PluginCommands.cs" "插件管理命令"

# 检查基础命令是否已移除
if [ -f "$PROJECT_ROOT/src/ShadowAgent/Commands/ShutdownCommand.cs" ]; then
    echo "❌ ShutdownCommand.cs 应该已移除"
    ERRORS=$((ERRORS + 1))
else
    echo "✅ ShutdownCommand.cs 已正确移除"
fi

if [ -f "$PROJECT_ROOT/src/ShadowAgent/Commands/RebootCommand.cs" ]; then
    echo "❌ RebootCommand.cs 应该已移除"
    ERRORS=$((ERRORS + 1))
else
    echo "✅ RebootCommand.cs 已正确移除"
fi

echo ""
echo "🔌 基础插件库检查:"
check_directory "$PROJECT_ROOT/src/ShadowAgent.BasePlugins" "基础插件库目录"
check_file "$PROJECT_ROOT/src/ShadowAgent.BasePlugins/ShadowAgent.BasePlugins.csproj" "基础插件库项目文件"
check_directory "$PROJECT_ROOT/src/ShadowAgent.BasePlugins/Plugins" "基础插件目录"
check_file "$PROJECT_ROOT/src/ShadowAgent.BasePlugins/Plugins/BaseSystemPlugin.cs" "基础系统插件"
check_directory "$PROJECT_ROOT/src/ShadowAgent.BasePlugins/Commands" "基础命令目录"
check_file "$PROJECT_ROOT/src/ShadowAgent.BasePlugins/Commands/BaseCommands.cs" "基础命令实现"

echo ""
echo "📋 构建脚本检查:"
check_file "$PROJECT_ROOT/build.sh" "构建脚本"
check_file "$PROJECT_ROOT/test_plugin_system.md" "测试文档"

echo ""
echo "📊 代码文件统计:"
echo "主服务 C# 文件: $(find "$PROJECT_ROOT/src/ShadowAgent" -name "*.cs" -type f | wc -l)"
echo "基础插件库 C# 文件: $(find "$PROJECT_ROOT/src/ShadowAgent.BasePlugins" -name "*.cs" -type f | wc -l)"
echo "总 C# 文件: $(find "$PROJECT_ROOT/src" -name "*.cs" -type f | wc -l)"

echo ""
if [ $ERRORS -eq 0 ]; then
    echo "🎉 项目结构验证通过！"
    echo ""
    echo "下一步:"
    echo "1. 安装 .NET SDK"
    echo "2. 运行构建脚本: ./build.sh all"
    echo "3. 测试插件系统功能"
else
    echo "⚠️  发现 $ERRORS 个问题需要修复"
    exit 1
fi