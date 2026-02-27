#!/bin/bash

# 墨影代理服务构建脚本
# 用法: ./build.sh [clean|build|publish|all]

set -e

PROJECT_ROOT="/work/workspace/ShadowAgent"
SRC_DIR="$PROJECT_ROOT/src"
PUBLISH_DIR="$PROJECT_ROOT/publish"
PLUGINS_DIR="$PUBLISH_DIR/plugins"

# 颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}👻 墨影代理服务构建脚本${NC}"
echo -e "${BLUE}=========================${NC}"

clean() {
    echo -e "${YELLOW}清理构建输出...${NC}"
    
    # 清理主项目
    if [ -d "$SRC_DIR/ShadowAgent/bin" ]; then
        rm -rf "$SRC_DIR/ShadowAgent/bin"
        echo "✓ 清理主项目构建输出"
    fi
    
    # 清理基础插件库
    if [ -d "$SRC_DIR/ShadowAgent.BasePlugins/bin" ]; then
        rm -rf "$SRC_DIR/ShadowAgent.BasePlugins/bin"
        echo "✓ 清理基础插件库构建输出"
    fi
    
    # 清理发布目录
    if [ -d "$PUBLISH_DIR" ]; then
        rm -rf "$PUBLISH_DIR"
        echo "✓ 清理发布目录"
    fi
    
    echo -e "${GREEN}清理完成${NC}"
}

build_plugins() {
    echo -e "${YELLOW}构建基础插件库...${NC}"
    
    cd "$SRC_DIR/ShadowAgent.BasePlugins"
    
    echo "编译 Release 版本..."
    dotnet build -c Release
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 基础插件库构建成功${NC}"
    else
        echo -e "${RED}✗ 基础插件库构建失败${NC}"
        exit 1
    fi
}

build_main() {
    echo -e "${YELLOW}构建主服务...${NC}"
    
    cd "$SRC_DIR/ShadowAgent"
    
    echo "编译 Release 版本..."
    dotnet build -c Release
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 主服务构建成功${NC}"
    else
        echo -e "${RED}✗ 主服务构建失败${NC}"
        exit 1
    fi
}

publish() {
    echo -e "${YELLOW}发布应用程序...${NC}"
    
    # 创建发布目录
    mkdir -p "$PUBLISH_DIR"
    mkdir -p "$PLUGINS_DIR"
    
    # 发布主服务
    echo "发布主服务..."
    cd "$SRC_DIR/ShadowAgent"
    dotnet publish -c Release -o "$PUBLISH_DIR" --no-build
    
    # 复制基础插件库
    echo "复制基础插件库..."
    PLUGIN_DLL="$SRC_DIR/ShadowAgent.BasePlugins/bin/Release/net10.0/ShadowAgent.BasePlugins.dll"
    if [ -f "$PLUGIN_DLL" ]; then
        cp "$PLUGIN_DLL" "$PLUGINS_DIR/"
        echo -e "${GREEN}✓ 基础插件库已复制到 plugins/ 目录${NC}"
    else
        echo -e "${YELLOW}⚠ 基础插件库 DLL 未找到，请先构建插件库${NC}"
    fi
    
    # 复制配置文件
    if [ -f "$SRC_DIR/ShadowAgent/config.json" ]; then
        cp "$SRC_DIR/ShadowAgent/config.json" "$PUBLISH_DIR/"
        echo "✓ 配置文件已复制"
    fi
    
    # 设置执行权限
    chmod +x "$PUBLISH_DIR/ShadowAgent"
    
    echo -e "${GREEN}发布完成${NC}"
    echo -e "${BLUE}发布目录: $PUBLISH_DIR${NC}"
    echo -e "${BLUE}插件目录: $PLUGINS_DIR${NC}"
    
    # 显示文件列表
    echo -e "\n${YELLOW}发布内容:${NC}"
    ls -la "$PUBLISH_DIR"
    if [ -d "$PLUGINS_DIR" ]; then
        echo -e "\n${YELLOW}插件文件:${NC}"
        ls -la "$PLUGINS_DIR"
    fi
}

all() {
    clean
    build_plugins
    build_main
    publish
}

# 根据参数执行相应操作
case "$1" in
    "clean")
        clean
        ;;
    "build")
        build_plugins
        build_main
        ;;
    "publish")
        publish
        ;;
    "all"|"")
        all
        ;;
    *)
        echo -e "${RED}未知命令: $1${NC}"
        echo "用法: $0 [clean|build|publish|all]"
        exit 1
        ;;
esac

echo -e "\n${GREEN}👻 墨影代理服务构建完成！${NC}"
echo -e "${BLUE}运行服务: cd $PUBLISH_DIR && ./ShadowAgent${NC}"