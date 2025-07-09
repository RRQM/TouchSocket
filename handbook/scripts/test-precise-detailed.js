const { parseDefinition, generateDefinitionComponent } = require('./replace-definitions-precise.js');

// 测试不同的程序集组合
const testCases = [
    {
        name: 'TouchSocket.Dmtp + TouchSocketPro.Dmtp',
        text: `
命名空间：TouchSocket.Dmtp
程序集：[TouchSocket.Dmtp.dll](https://www.nuget.org/packages/TouchSocket.Dmtp)
程序集：[TouchSocketPro.Dmtp.dll](https://www.nuget.org/packages/TouchSocketPro.Dmtp) <Pro/>
`
    },
    {
        name: '单个 TouchSocket.Dmtp',
        text: `
命名空间：TouchSocket.Dmtp
程序集：[TouchSocket.Dmtp.dll](https://www.nuget.org/packages/TouchSocket.Dmtp)
`
    },
    {
        name: '单个 TouchSocketPro.Dmtp',
        text: `
命名空间：TouchSocket.Dmtp
程序集：[TouchSocketPro.Dmtp.dll](https://www.nuget.org/packages/TouchSocketPro.Dmtp) <Pro/>
`
    }
];

testCases.forEach(testCase => {
    console.log(`\n=== ${testCase.name} ===`);
    const result = parseDefinition(testCase.text);
    console.log('解析结果：', result);
    
    const component = generateDefinitionComponent(result.namespace, result.assemblies);
    console.log('生成的组件：', component);
    
    // 检查程序集类型
    const assemblyTypes = result.assemblies.map(a => a.name.replace('.dll', ''));
    console.log('程序集类型：', assemblyTypes);
    
    // 检查是否有对应的预定义组件
    const fs = require('fs');
    const path = require('path');
    const definitionJsPath = path.join(__dirname, '..', 'src', 'components', 'Definition.js');
    const definitionJsContent = fs.readFileSync(definitionJsPath, 'utf8');
    
    const assemblyToTypeMap = {};
    const typeDefRegex = /export const (\w+)Definition = \([^)]*\) =>\s*<Definition type="([^"]+)"/g;
    let match;
    while ((match = typeDefRegex.exec(definitionJsContent)) !== null) {
        const [, componentName, type] = match;
        assemblyToTypeMap[type] = componentName + 'Definition';
    }
    
    assemblyTypes.forEach(type => {
        if (assemblyToTypeMap[type]) {
            console.log(`  ${type} -> ${assemblyToTypeMap[type]}`);
        } else {
            console.log(`  ${type} -> 无预定义组件`);
        }
    });
});
