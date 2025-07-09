const { parseDefinition, generateDefinitionComponent } = require('./replace-definitions-precise.js');

const definitionText = `
命名空间：TouchSocket.Dmtp
程序集：[TouchSocket.Dmtp.dll](https://www.nuget.org/packages/TouchSocket.Dmtp)
程序集：[TouchSocketPro.Dmtp.dll](https://www.nuget.org/packages/TouchSocketPro.Dmtp) <Pro/>
`;

console.log('原始定义文本：');
console.log(definitionText);

const result = parseDefinition(definitionText);
console.log('\n解析结果：', result);

const component = generateDefinitionComponent(result.namespace, result.assemblies);
console.log('\n生成的组件：', component);
