const fs = require('fs');
const path = require('path');

// 精准匹配定义部分的正则表达式
const definitionRegex = /### 定义\s*([\s\S]*?)(?=\n## |$)/;

// 解析定义部分内容
function parseDefinition(definitionText) {
    const lines = definitionText.split('\n').map(line => line.trim()).filter(line => line);
    
    let namespace = '';
    const assemblies = [];
    
    for (const line of lines) {
        if (line.startsWith('命名空间：')) {
            namespace = line.replace('命名空间：', '').replace('<br/>', '').trim();
        } else if (line.startsWith('程序集：')) {
            // 解析程序集链接
            const assemblyMatch = line.match(/\[([^\]]+)\]/);
            if (assemblyMatch) {
                const assemblyName = assemblyMatch[1];
                const isPro = line.includes('<Pro/>');
                assemblies.push({ name: assemblyName, isPro });
            }
        }
    }
    
    return { namespace, assemblies };
}

const filePath = 'd:/CodeOpen/TouchSocket/handbook/docs/dmtpclient.mdx';
const content = fs.readFileSync(filePath, 'utf8');

console.log('文件内容：');
console.log(content.substring(0, 500));

const definitionMatch = content.match(definitionRegex);
if (definitionMatch) {
    console.log('\n匹配到的定义部分：');
    console.log(definitionMatch[1]);
    
    const result = parseDefinition(definitionMatch[1]);
    console.log('\n解析结果：', result);
} else {
    console.log('\n未找到定义部分');
}
