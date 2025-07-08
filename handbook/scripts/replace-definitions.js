// 批量替换定义部分的脚本
// 使用说明：这个脚本可以帮助您批量替换文档中的定义部分

const fs = require('fs');
const path = require('path');

// 定义不同程序集的映射
const assemblyMapping = {
  'TouchSocket.Core': {
    namespace: 'TouchSocket.Core',
    assembly: 'TouchSocket.Core.dll',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Core'
  },
  'TouchSocket.Dmtp': {
    namespace: 'TouchSocket.Dmtp',
    assembly: 'TouchSocket.Dmtp.dll',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Dmtp'
  },
  'TouchSocket.Http': {
    namespace: 'TouchSocket.Http',
    assembly: 'TouchSocket.Http.dll',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Http'
  },
  'TouchSocket.Rpc': {
    namespace: 'TouchSocket.Rpc',
    assembly: 'TouchSocket.Rpc.dll',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Rpc'
  },
  'TouchSocket.Mqtt': {
    namespace: 'TouchSocket.Mqtt',
    assembly: 'TouchSocket.Mqtt.dll',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Mqtt'
  },
  'TouchSocket.Modbus': {
    namespace: 'TouchSocket.Modbus',
    assembly: 'TouchSocket.Modbus.dll',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Modbus'
  }
};

// 函数：替换文件中的定义部分
function replaceDefinitionInFile(filePath, assemblyKey = 'TouchSocket.Core') {
  const config = assemblyMapping[assemblyKey];
  if (!config) {
    console.log(`Unknown assembly: ${assemblyKey}`);
    return;
  }

  try {
    let content = fs.readFileSync(filePath, 'utf8');
    
    // 匹配模式：### 定义\n\n命名空间：xxx <br/>\n程序集：[xxx](xxx)
    const definitionPattern = /### 定义\s*\n\s*命名空间：[^\n<]+<br\/>\s*\n\s*程序集：\[[^\]]+\]\([^)]+\)/g;
    
    // 检查是否已经导入了Definition组件
    const hasDefinitionImport = content.includes("import Definition from '@site/src/components/Definition.js';");
    
    if (definitionPattern.test(content)) {
      // 添加导入语句（如果还没有）
      if (!hasDefinitionImport) {
        // 查找现有的import语句
        const importLines = content.match(/^import .+;$/gm) || [];
        if (importLines.length > 0) {
          // 在最后一个import语句后添加Definition import
          const lastImport = importLines[importLines.length - 1];
          content = content.replace(lastImport, `${lastImport}\nimport Definition from '@site/src/components/Definition.js';`);
        } else {
          // 如果没有import语句，在frontmatter后添加
          content = content.replace(/---\s*\n/, `---\n\nimport Definition from '@site/src/components/Definition.js';\n`);
        }
      }
      
      // 替换定义部分
      const definitionComponent = `<Definition 
  namespace="${config.namespace}" 
  assembly="${config.assembly}"
  nugetUrl="${config.nugetUrl}"
/>`;
      
      content = content.replace(definitionPattern, definitionComponent);
      
      // 写回文件
      fs.writeFileSync(filePath, content, 'utf8');
      console.log(`✅ Updated: ${filePath}`);
    } else {
      console.log(`ℹ️  No definition section found in: ${filePath}`);
    }
  } catch (error) {
    console.error(`❌ Error processing ${filePath}:`, error.message);
  }
}

// 使用示例：
// replaceDefinitionInFile('./docs/example.mdx', 'TouchSocket.Core');

console.log('Definition replacement script ready!');
console.log('Available assemblies:', Object.keys(assemblyMapping));
console.log('Usage example: replaceDefinitionInFile("./docs/filename.mdx", "TouchSocket.Core");');

module.exports = { replaceDefinitionInFile, assemblyMapping };
