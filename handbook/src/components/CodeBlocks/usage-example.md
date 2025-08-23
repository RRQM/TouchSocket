# CodeBlock组件使用说明

这个自定义的CodeBlock组件可以根据region标题自动从codes.cs文件中提取对应的代码片段并显示。

## 基本用法

```jsx
import CustomCodeBlock from '@site/src/components/CodeBlocks/CustomCodeBlock';

// 显示CodeRegion区域的代码
<CustomCodeBlock region="CodeRegion" />
```

## 组件参数

- `region` (必需): region的名称，对应codes.cs文件中#region和#endregion之间的代码块
- `language` (可选): 代码语言，默认为'csharp'
- `title` (可选): 代码块标题，默认使用"代码区域: {region}"
- `showLineNumbers` (可选): 是否显示行号，默认为true

## 使用示例

### 基本示例
```jsx
<CustomCodeBlock region="CodeRegion" />
```

### 自定义标题
```jsx
<CustomCodeBlock 
  region="CodeRegion" 
  title="示例方法实现" 
/>
```

### 不显示行号
```jsx
<CustomCodeBlock 
  region="CodeRegion" 
  showLineNumbers={false} 
/>
```

## 在MDX文件中使用

在你的.mdx文件中，你可以这样使用：

```mdx
import CustomCodeBlock from '@site/src/components/CodeBlocks/CustomCodeBlock';

# 我的文档

这里是一些文档内容。

<CustomCodeBlock region="CodeRegion" />

更多内容...
```

## codes.cs文件格式要求

确保你的codes.cs文件中的region格式如下：

```csharp
#region YourRegionName
// 你的代码
public void SomeMethod()
{
    // 实现
}
#endregion
```

组件会自动：
1. 查找匹配的region
2. 提取region内的代码（排除#region和#endregion行）
3. 处理缩进，使代码格式美观
4. 使用Docusaurus的代码高亮功能显示
