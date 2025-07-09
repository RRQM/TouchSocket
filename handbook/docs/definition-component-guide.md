# Definition 组件使用说明

## 概述

Definition 组件是一个专门用于显示 TouchSocket 文档中定义信息的 React 组件，它提供了统一的样式和更好的用户体验。支持预定义组件和自定义参数两种使用方式。

## 组件特性

- ✨ **现代化设计**：采用白百合蓝色主题，符合整站风格
- 🎨 **毛玻璃效果**：支持背景模糊和渐变效果
- 🌓 **暗色主题**：完美适配明暗主题切换
- 📱 **响应式设计**：在各种屏幕尺寸下都有良好表现
- 🔗 **智能链接**：NuGet 包链接支持悬停效果
- 📋 **一键复制**：支持复制 dotnet 安装命令
- 🚀 **预定义组件**：提供所有 TouchSocket 包的预定义组件
- ⚡ **轻量高效**：组件体积小，性能优秀

## 使用方法

### 1. 预定义组件使用（推荐）

针对 TouchSocket 各个包，我们提供了预定义组件，使用最简单：

```mdx
import { TouchSocketCoreDefinition } from '@site/src/components/Definition.js';

<!-- 默认不带版本号（安装最新版） -->
<TouchSocketCoreDefinition />

<!-- 带版本号 -->
<TouchSocketCoreDefinition withVersion={true} />
```

#### 可用的预定义组件

| 组件名称 | 包名称 | 说明 |
|---------|--------|------|
| `TouchSocketCoreDefinition` | TouchSocket.Core | 基础服务功能库 |
| `TouchSocketDmtpDefinition` | TouchSocket.Dmtp | 分布式消息传输协议 |
| `TouchSocketHttpDefinition` | TouchSocket.Http | HTTP服务器和客户端 |
| `TouchSocketRpcDefinition` | TouchSocket.Rpc | 远程过程调用框架 |
| `TouchSocketMqttDefinition` | TouchSocket.Mqtt | MQTT协议实现 |
| `TouchSocketModbusDefinition` | TouchSocket.Modbus | Modbus协议实现 |

### 2. 通过 type 参数使用

```mdx
import Definition from '@site/src/components/Definition.js';

<!-- 默认不带版本号 -->
<Definition type="TouchSocketCore" />

<!-- 带版本号 -->
<Definition type="TouchSocketCore" version="3.1.12" />
```

#### 可用的 type 参数

- `TouchSocketCore`
- `TouchSocketDmtp`
- `TouchSocketHttp`
- `TouchSocketRpc`
- `TouchSocketMqtt`
- `TouchSocketModbus`

### 3. 自定义参数使用

```mdx
import Definition from '@site/src/components/Definition.js';

<!-- 单个命名空间和程序集 -->
<Definition 
  namespace="MyCompany.MyLibrary" 
  assembly="MyCompany.MyLibrary.dll"
  packageName="MyCompany.MyLibrary"
  version="2.1.0"
  nugetUrl="https://www.nuget.org/packages/MyCompany.MyLibrary"
/>

<!-- 多个命名空间和程序集 -->
<Definition 
  namespace={['My.Core', 'My.Extensions']}
  assembly={['My.Core.dll', 'My.Extensions.dll']}
  packageName="MyPackage"
  nugetUrl={['https://www.nuget.org/packages/MyPackage', 'https://www.nuget.org/packages/MyPackage']}
/>
```

## 参数说明

| 参数 | 类型 | 默认值 | 描述 |
|------|------|--------|------|
| `type` | string | - | 预定义类型，如 `TouchSocketCore` |
| `withVersion` | boolean | `false` | 预定义组件的版本控制参数 |
| `namespace` | string \| string[] | `TouchSocket.Core` | 命名空间名称，支持数组 |
| `assembly` | string \| string[] | `TouchSocket.Core.dll` | 程序集文件名，支持数组 |
| `packageName` | string | `TouchSocket.Core` | NuGet 包名称 |
| `version` | string | `undefined` | 包版本号，默认不指定版本 |
| `nugetUrl` | string \| string[] | `https://www.nuget.org/packages/TouchSocket.Core` | NuGet 包链接，支持数组 |

## 新功能

### 1. dotnet 安装命令

每个 Definition 组件都会显示对应的 dotnet 安装命令，用户可以一键复制：

```bash
# 默认不带版本号（安装最新版）
dotnet add package TouchSocket.Core

# 带版本号
dotnet add package TouchSocket.Core --version 3.1.12
```

### 2. 多值支持

支持显示多个命名空间和程序集，适用于包含多个模块的库：

- **多个命名空间**：以数组形式传入多个命名空间
- **多个程序集**：以数组形式传入多个程序集文件
- **对应链接**：每个程序集可以对应不同的 NuGet 链接

### 3. 复制功能

点击复制按钮可以快速复制 dotnet 安装命令到剪贴板。

## 使用示例

### TouchSocket.Core 包
```mdx
import { TouchSocketCoreDefinition } from '@site/src/components/Definition.js';

<!-- 默认不带版本号 -->
<TouchSocketCoreDefinition />

<!-- 带版本号 -->
<TouchSocketCoreDefinition withVersion={true} />
```

### 自定义包（默认不带版本）
```mdx
import Definition from '@site/src/components/Definition.js';

<Definition 
  namespace="MyCompany.MyLibrary" 
  assembly="MyCompany.MyLibrary.dll"
  packageName="MyCompany.MyLibrary"
  nugetUrl="https://www.nuget.org/packages/MyCompany.MyLibrary"
  description="我的自定义库，安装最新版本"
/>
```

## 样式特性

- **渐变背景**：采用白百合蓝色渐变背景
- **毛玻璃效果**：支持 backdrop-filter 模糊效果
- **左侧装饰条**：蓝色渐变装饰条突出显示
- **悬停效果**：鼠标悬停时的微妙动画效果
- **代码字体**：命名空间和安装命令使用等宽字体显示
- **响应式布局**：在移动设备上自动调整布局
- **复制按钮**：带有复制成功反馈的交互式按钮

## 最佳实践

1. **优先使用预定义组件**：对于 TouchSocket 的包，优先使用预定义组件
2. **统一导入方式**：使用解构导入，保持代码简洁
3. **参数完整性**：自定义使用时，提供完整的参数信息
4. **位置规范**：将 Definition 组件放在文档开头，标题下方

## 迁移指南

### 从旧版本迁移：

**旧写法：**
```mdx
import Definition from '@site/src/components/Definition.js';

<Definition 
  namespace="TouchSocket.Core" 
  assembly="TouchSocket.Core.dll"
  nugetUrl="https://www.nuget.org/packages/TouchSocket.Core"
/>
```

**新写法（推荐）：**
```mdx
import { TouchSocketCoreDefinition } from '@site/src/components/Definition.js';

<TouchSocketCoreDefinition />
```

### 优势

- 更简洁的代码
- 自动包含最新版本号
- 自动包含包描述信息
- 自动生成 dotnet 安装命令
- 更好的维护性

## 版本信息

当前组件包含的包版本均为 `3.1.12`，这是 TouchSocket 的最新稳定版本。如需更新版本，只需修改组件内的预定义配置即可。
