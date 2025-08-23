# CodeBlock组件使用指南

这个自定义CodeBlock组件允许您通过region名称自动从`codes.cs`文件中提取和显示代码片段。

## 🚀 快速开始

### 1. 在codes.cs中定义代码区域

```csharp
class MyClass
{
    #region RegionName
    public void MyMethod()
    {
        // 您的代码
        Console.WriteLine("Hello World!");
    }
    #endregion

    #region AnotherRegion
    public int Calculate(int a, int b)
    {
        return a + b;
    }
    #endregion
}
```

### 2. 生成代码数据

在构建之前，运行以下命令生成代码数据：

```bash
npm run generate-codes
```

或者手动运行：

```bash
node src/components/CodeBlocks/generateCodesModule.js
```

### 3. 在MDX文件中使用组件

```mdx
import CustomCodeBlock from '@site/src/components/CodeBlocks/CustomCodeBlock';

# 我的文档

<CustomCodeBlock region="RegionName" />
```

## 📋 组件属性

| 属性                   | 类型      | 默认值               | 描述                      |
| ---------------------- | --------- | -------------------- | ------------------------- |
| `region`               | `string`  | **必需**             | 要显示的代码区域名称      |
| `language`             | `string`  | `'csharp'`           | 代码语言，用于语法高亮    |
| `title`                | `string`  | `代码区域: {region}` | 代码块标题                |
| `showLineNumbers`      | `boolean` | `true`               | 是否显示行号              |
| `showAvailableRegions` | `boolean` | `true`               | 错误时是否显示可用regions |

## 💡 使用示例

### 基本使用
```jsx
<CustomCodeBlock region="MyRegion" />
```

### 自定义标题和语言
```jsx
<CustomCodeBlock 
  region="MyRegion" 
  title="示例方法实现"
  language="java"
/>
```

### 不显示行号
```jsx
<CustomCodeBlock 
  region="MyRegion" 
  showLineNumbers={false}
/>
```

## 🔧 工作流程

1. **编写代码**: 在`src/codes/codes.cs`中用`#region`和`#endregion`包围代码块
2. **生成数据**: 运行`npm run generate-codes`将代码内容转换为JavaScript模块
3. **使用组件**: 在MDX文件中通过region名称引用代码块
4. **构建项目**: `npm run build`会自动运行代码生成

## ⚠️ 注意事项

1. **Region命名**: region名称区分大小写，确保在组件中使用正确的名称
2. **构建顺序**: 确保在构建前运行`generate-codes`脚本
3. **代码更新**: 每次修改`codes.cs`后都需要重新运行生成脚本
4. **文件路径**: 组件假设`codes.cs`文件位于`src/codes/codes.cs`

## 🎨 特性

- ✅ **自动提取**: 根据region名称自动提取代码
- ✅ **语法高亮**: 基于Docusaurus的CodeBlock组件
- ✅ **智能缩进**: 自动处理代码缩进
- ✅ **错误处理**: 友好的错误信息和region建议
- ✅ **复制功能**: 继承Docusaurus的代码复制功能
- ✅ **响应式**: 适应不同屏幕尺寸

## 📁 文件结构

```
src/
├── components/
│   └── CodeBlocks/
│       ├── CodeBlock.js          # 主组件文件
│       ├── codesData.js          # 自动生成的数据文件
│       ├── generateCodesModule.js # 生成脚本
│       └── usage-example.md      # 使用示例
└── codes/
    └── codes.cs                  # 源代码文件
```

## 🤝 扩展建议

1. **多文件支持**: 可以扩展为支持多个源代码文件
2. **语言检测**: 自动根据文件扩展名检测语言
3. **实时更新**: 在开发模式下监听文件变化
4. **缓存优化**: 添加构建缓存以提高性能
