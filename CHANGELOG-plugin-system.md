# 插件系统变更摘要

## 分支：feature/plugin-system
**提交ID**: 770540197987708e971a596ee8ec8d892af11df6  
**时间**: 2026-02-27 19:08:39 +0800  
**作者**: sangsq <sang93@qq.com>

## 变更概述

本次提交实现了完整的插件系统架构，使墨影代理服务支持插件化扩展。

### 🎯 主要特性

1. **插件系统核心**
   - `IPlugin` 接口：定义插件标准
   - `PluginManager`：插件加载、管理、卸载
   - `ExtendedCommandRegistry`：支持动态命令注册

2. **基础插件库**
   - 新建 `ShadowAgent.BasePlugins` 类库项目
   - `BaseSystemPlugin`：包含关机、重启等基础命令
   - 命令实现从主项目迁移到插件库

3. **插件管理命令**
   - `load`：动态加载DLL插件
   - `unload`：卸载指定插件
   - `plugins`：列出已加载插件
   - `reload-plugins`：重新加载所有插件

4. **主程序更新**
   - 集成插件系统，自动加载插件目录中的DLL
   - 移除 `ShutdownCommand` 和 `RebootCommand` 到插件库
   - 保留 `PingCommand` 在主服务
   - 更新 `StatusCommand` 显示插件信息

5. **工具脚本**
   - `build.sh`：一键构建脚本
   - `validate_structure.sh`：项目结构验证
   - `test_plugin_system.md`：测试指南

### 📁 文件变更统计

```
新增文件 (10):
  build.sh
  src/ShadowAgent.BasePlugins/Commands/BaseCommands.cs
  src/ShadowAgent.BasePlugins/Plugins/BaseSystemPlugin.cs
  src/ShadowAgent.BasePlugins/ShadowAgent.BasePlugins.csproj
  src/ShadowAgent/Commands/PluginCommands.cs
  src/ShadowAgent/Plugins/ExtendedCommandRegistry.cs
  src/ShadowAgent/Plugins/IPlugin.cs
  src/ShadowAgent/Plugins/PluginManager.cs
  test_plugin_system.md
  validate_structure.sh

修改文件 (3):
  src/ShadowAgent/Commands/StatusCommand.cs
  src/ShadowAgent/Program.cs
  src/ShadowAgent/config.json

删除文件 (2):
  src/ShadowAgent/Commands/RebootCommand.cs
  src/ShadowAgent/Commands/ShutdownCommand.cs

总计: 15个文件变更，1278行新增，165行删除
```

### 🔧 使用说明

#### 构建项目
```bash
./build.sh all
```

#### 运行服务
```bash
cd publish && ./ShadowAgent
```

#### 测试命令
```bash
# 基础功能
echo "SHADOW ping" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock
echo "SHADOW status" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 插件管理
echo "SHADOW plugins" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock
echo "SHADOW load /path/to/plugin.dll" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock
```

#### 插件开发
1. 创建类库项目，引用 `ShadowAgent.dll`
2. 实现 `IPlugin` 接口
3. 编译为DLL，放入插件目录或使用 `load` 命令动态加载

### 🎨 架构优势

1. **模块化**：基础功能与扩展功能分离
2. **可扩展**：轻松添加新功能，无需修改主程序
3. **私有化**：敏感插件可放在私有仓库
4. **热加载**：支持运行时动态加载/卸载
5. **向后兼容**：原有API和协议保持不变

### 📋 后续步骤

1. 安装 .NET SDK 进行实际编译测试
2. 创建示例插件项目演示开发流程
3. 完善插件开发文档和最佳实践
4. 添加单元测试确保系统稳定性

---

**分支已创建并提交**：`feature/plugin-system`
**状态**：✅ 代码变更已完成，等待 .NET SDK 安装后进行编译测试