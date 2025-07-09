const fs = require('fs');
const path = require('path');

// 从 Definition.js 中提取预定义的程序集映射
const definitionJsPath = path.join(__dirname, '..', 'src', 'components', 'Definition.js');
const definitionJsContent = fs.readFileSync(definitionJsPath, 'utf8');

// 解析程序集到 type 的映射
const assemblyToTypeMap = {};
const typeDefRegex = /export const (\w+)Definition = \([^)]*\) =>\s*<Definition type="([^"]+)"/g;
let match;
while ((match = typeDefRegex.exec(definitionJsContent)) !== null) {
    const [, componentName, type] = match;
    assemblyToTypeMap[type] = componentName + 'Definition';
}

console.log('解析到的程序集映射：', assemblyToTypeMap);

// 递归查找所有 mdx 文件
function findMdxFiles(dir) {
    const files = [];
    const items = fs.readdirSync(dir);
    
    for (const item of items) {
        const fullPath = path.join(dir, item);
        const stat = fs.statSync(fullPath);
        
        if (stat.isDirectory()) {
            files.push(...findMdxFiles(fullPath));
        } else if (item.endsWith('.mdx')) {
            files.push(fullPath);
        }
    }
    
    return files;
}

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

// 生成 Definition 组件
function generateDefinitionComponent(namespace, assemblies) {
    // 将程序集名称转换为类型名称 (去掉.dll并转换格式)
    const assemblyTypes = assemblies.map(assembly => {
        const type = assembly.name.replace('.dll', '');
        // 去掉点，转换为驼峰式
        return type.replace(/\./g, '');
    });
    
    // 检查是否有对应的预定义组件
    if (assemblies.length === 1) {
        // 单个程序集
        const type = assemblyTypes[0];
        if (assemblyToTypeMap[type]) {
            return `<${assemblyToTypeMap[type]} />`;
        }
    } else if (assemblies.length === 2) {
        // 双程序集组合，检查是否有 pro 版本的组合
        const regularAssembly = assemblies.find(a => !a.isPro);
        const proAssembly = assemblies.find(a => a.isPro);
        
        if (regularAssembly && proAssembly) {
            const regularType = regularAssembly.name.replace('.dll', '').replace(/\./g, '');
            const proType = proAssembly.name.replace('.dll', '').replace(/\./g, '');
            
            // 检查是否有 pro 版本的预定义组件
            if (assemblyToTypeMap[proType]) {
                return `<${assemblyToTypeMap[proType]} />`;
            }
            // 或者检查是否有常规版本的预定义组件
            else if (assemblyToTypeMap[regularType]) {
                return `<${assemblyToTypeMap[regularType]} />`;
            }
        }
    }
    
    // 如果没有找到预定义组件，使用自定义参数
    const props = [`namespace="${namespace}"`];
    
    if (assemblies.length === 1) {
        props.push(`assembly="${assemblies[0].name}"`);
        if (assemblies[0].isPro) {
            props.push(`isPro={true}`);
        }
    } else {
        const assemblyNames = assemblies.map(a => a.name);
        props.push(`assemblies={${JSON.stringify(assemblyNames)}}`);
        
        const proAssemblies = assemblies.filter(a => a.isPro).map(a => a.name);
        if (proAssemblies.length > 0) {
            props.push(`proAssemblies={${JSON.stringify(proAssemblies)}}`);
        }
    }
    
    return `<Definition ${props.join(' ')} />`;
}

// 处理单个文件
function processFile(filePath) {
    const content = fs.readFileSync(filePath, 'utf8');
    
    // 检查是否包含定义部分
    const definitionMatch = content.match(definitionRegex);
    if (!definitionMatch) {
        return { processed: false, reason: '无定义部分' };
    }
    
    const definitionText = definitionMatch[1];
    const { namespace, assemblies } = parseDefinition(definitionText);
    
    if (!namespace || assemblies.length === 0) {
        return { processed: false, reason: '无法解析命名空间或程序集' };
    }
    
    // 生成 Definition 组件
    const definitionComponent = generateDefinitionComponent(namespace, assemblies);
    
    // 检查是否需要添加 import
    let newContent = content;
    
    // 确定需要导入的组件名称
    const componentName = definitionComponent.match(/<(\w+)/)[1];
    const hasCorrectImport = content.includes(`import { ${componentName} }`) || 
                           content.includes(`import {${componentName}}`) ||
                           (content.includes(`import { `) && content.includes(`${componentName}`));
    
    if (!hasCorrectImport) {
        // 检查是否已经有 Definition 相关的导入
        const existingImportRegex = /import\s+(?:{\s*([^}]+)\s*}|(\w+))\s+from\s+["']@site\/src\/components\/Definition\.js["'];?/;
        const existingImportMatch = content.match(existingImportRegex);
        
        if (existingImportMatch) {
            // 替换现有的导入
            const newImportLine = `import { ${componentName} } from "@site/src/components/Definition.js";`;
            newContent = content.replace(existingImportRegex, newImportLine);
        } else {
            // 添加新的导入
            const importLine = `import { ${componentName} } from "@site/src/components/Definition.js";`;
            
            // 查找现有的 import 语句
            const importMatches = content.match(/import .* from .*;/g) || [];
            
            if (importMatches.length > 0) {
                // 找到最后一个 import 语句的位置
                const lastImport = importMatches[importMatches.length - 1];
                const lastImportIndex = content.indexOf(lastImport);
                const afterLastImport = lastImportIndex + lastImport.length;
                newContent = content.slice(0, afterLastImport) + '\n' + importLine + content.slice(afterLastImport);
            } else {
                // 在文件开头添加 import（在 frontmatter 之后）
                const frontmatterMatch = content.match(/^---[\s\S]*?---/);
                if (frontmatterMatch) {
                    const frontmatterEnd = frontmatterMatch.index + frontmatterMatch[0].length;
                    newContent = content.slice(0, frontmatterEnd) + '\n\n' + importLine + content.slice(frontmatterEnd);
                } else {
                    newContent = importLine + '\n\n' + content;
                }
            }
        }
    }
    
    // 替换定义部分
    const newDefinitionSection = `### 定义\n\n${definitionComponent}\n\n`;
    newContent = newContent.replace(definitionRegex, newDefinitionSection);
    
    // 写回文件
    fs.writeFileSync(filePath, newContent);
    
    return { 
        processed: true, 
        namespace, 
        assemblies: assemblies.map(a => a.name), 
        component: definitionComponent 
    };
}

// 主函数
function main() {
    const docsDir = path.join(__dirname, '..', 'docs');
    const versionedDocsDir = path.join(__dirname, '..', 'versioned_docs');
    
    const allFiles = [
        ...findMdxFiles(docsDir),
        ...findMdxFiles(versionedDocsDir)
    ];
    
    console.log(`找到 ${allFiles.length} 个 mdx 文件`);
    
    let processed = 0;
    let skipped = 0;
    const results = [];
    
    for (const file of allFiles) {
        const relativePath = path.relative(path.join(__dirname, '..'), file);
        const result = processFile(file);
        
        if (result.processed) {
            processed++;
            console.log(`✓ ${relativePath}: ${result.component}`);
            results.push({ file: relativePath, ...result });
        } else {
            skipped++;
            console.log(`- ${relativePath}: ${result.reason}`);
        }
    }
    
    console.log(`\n处理完成：`);
    console.log(`处理了 ${processed} 个文件`);
    console.log(`跳过了 ${skipped} 个文件`);
    
    // 输出统计信息
    const typeStats = {};
    results.forEach(result => {
        const key = result.assemblies.join(',');
        if (!typeStats[key]) {
            typeStats[key] = 0;
        }
        typeStats[key]++;
    });
    
    console.log('\n程序集统计：');
    Object.entries(typeStats).forEach(([assemblies, count]) => {
        console.log(`  ${assemblies}: ${count} 个文件`);
    });
}

if (require.main === module) {
    main();
}

module.exports = { processFile, parseDefinition, generateDefinitionComponent };
