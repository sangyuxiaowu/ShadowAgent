# 墨影代理插件系统测试指南

## 当前状态总结

✅ **已完成的功能：**

1. **插件系统架构**
   - `IPlugin` 接口定义
   - `PluginManager` 插件管理器
   - `ExtendedCommandRegistry` 扩展命令注册表

2. **插件管理命令**
   - `load` - 加载DLL插件
   - `unload` - 卸载插件
   - `plugins` - 列出已加载插件
   - `reload-plugins` - 重新加载所有插件

3. **代码结构调整**
   - 移除了主项目中的 `ShutdownCommand.cs` 和 `RebootCommand.cs`
   - 保留了 `PingCommand.cs` 在主服务中
   - 更新了 `StatusCommand.cs` 显示插件信息
   - 创建了基础插件库项目

4. **构建脚本**
   - 创建了自动化构建脚本 `build.sh`

## 文件结构

```
/work/workspace/ShadowAgent/
├── src/
│   ├── ShadowAgent/                    # 主服务
│   │   ├── Commands/
│   │   │   ├── PingCommand.cs          # 心跳命令（保留在主服务）
│   │   │   ├── StatusCommand.cs        # 状态命令（已更新，显示插件信息）
│   │   │   ├── HelpCommand.cs          # 帮助命令
│   │   │   └── PluginCommands.cs       # 插件管理命令
│   │   ├── Plugins/                    # 插件系统
│   │   │   ├── IPlugin.cs
│   │   │   ├── PluginManager.cs
│   │   │   └── ExtendedCommandRegistry.cs
│   │   ├── Program.cs                  # 主程序（已集成插件系统）
│   │   ├── config.json                 # 配置文件
│   │   └── ShadowAgent.csproj          # 项目文件
│   └── ShadowAgent.BasePlugins/        # 基础插件库
│       ├── Plugins/
│       │   └── BaseSystemPlugin.cs     # 基础系统插件
│       ├── Commands/
│       │   └── BaseCommands.cs         # 基础命令（关机、重启）
│       └── ShadowAgent.BasePlugins.csproj
├── build.sh                            # 构建脚本
└── README.md                           # 文档
```

## 使用流程

### 1. 安装 .NET SDK
```bash
# 需要安装 .NET 10.0 SDK
# 参考：https://dotnet.microsoft.com/download
```

### 2. 构建项目
```bash
cd /work/workspace/ShadowAgent
./build.sh all
```

### 3. 运行服务
```bash
cd /work/workspace/ShadowAgent/publish
./ShadowAgent
```

### 4. 测试命令
```bash
# 测试ping命令
echo "SHADOW ping" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 查看状态（包含插件信息）
echo "SHADOW status" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 列出插件
echo "SHADOW plugins" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock

# 查看帮助
echo "SHADOW help" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock
```

## 插件开发指南

### 1. 创建插件项目
```bash
dotnet new classlib -n MyCustomPlugin
cd MyCustomPlugin
```

### 2. 添加引用
```xml
<ItemGroup>
  <Reference Include="ShadowAgent">
    <HintPath>path/to/ShadowAgent.dll</HintPath>
  </Reference>
</ItemGroup>
```

### 3. 实现插件
```csharp
using ShadowAgent.Commands;
using ShadowAgent.Plugins;

namespace MyCustomPlugin;

public class MyPlugin : IPlugin
{
    public string Name => "MyPlugin";
    public string Version => "1.0.0";
    public string Description => "我的自定义插件";
    
    public IEnumerable<ICommand> GetCommands()
    {
        return new List<ICommand>
        {
            new MyCustomCommand()
        };
    }
    
    public Task InitializeAsync() => Task.CompletedTask;
    public Task CleanupAsync() => Task.CompletedTask;
}

public class MyCustomCommand : ICommand
{
    public string Name => "mycommand";
    public string Description => "我的自定义命令";
    
    public Task<CommandResult> ExecuteAsync(string[] args, CancellationToken ct)
    {
        return Task.FromResult(CommandResult.Ok("Hello from custom plugin!"));
    }
}
```

### 4. 使用插件
```bash
# 编译插件
dotnet build -c Release

# 复制到插件目录
cp bin/Release/net10.0/MyCustomPlugin.dll /work/workspace/ShadowAgent/publish/plugins/

# 动态加载（如果需要）
echo "SHADOW load plugins/MyCustomPlugin.dll" | socat - UNIX-CONNECT:/tmp/shadow-agent.sock
```

## 优势

1. **模块化**：基础功能与自定义功能分离
2. **可扩展**：轻松添加新插件，无需修改主程序
3. **私有化**：敏感插件可以放在私有仓库
4. **热加载**：支持动态加载和卸载插件
5. **向后兼容**：原有API保持不变

## 下一步

1. **安装 .NET SDK** 以进行实际编译测试
2. **创建示例插件** 演示完整开发流程
3. **完善文档** 包括插件开发最佳实践
4. **添加单元测试** 确保插件系统稳定性