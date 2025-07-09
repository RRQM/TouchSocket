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

// 修复单个文件的导入
function fixImportInFile(filePath) {
    const content = fs.readFileSync(filePath, 'utf8');
    
    // 查找文件中使用的 Definition 组件
    const usedComponents = new Set();
    const componentRegex = /<(\w+Definition)\s*\/>/g;
    let componentMatch;
    
    while ((componentMatch = componentRegex.exec(content)) !== null) {
        usedComponents.add(componentMatch[1]);
    }
    
    if (usedComponents.size === 0) {
        return { processed: false, reason: '没有使用Definition组件' };
    }
    
    // 检查当前的导入语句
    const currentImportRegex = /import\s+(?:{\s*([^}]+)\s*}|(\w+))\s+from\s+["']@site\/src\/components\/Definition\.js["'];?/;
    const importMatch = content.match(currentImportRegex);
    
    if (!importMatch) {
        return { processed: false, reason: '没有找到Definition导入语句' };
    }
    
    const componentsArray = Array.from(usedComponents);
    const newImportStatement = `import { ${componentsArray.join(', ')} } from "@site/src/components/Definition.js";`;
    
    // 替换导入语句
    const newContent = content.replace(currentImportRegex, newImportStatement);
    
    // 写回文件
    fs.writeFileSync(filePath, newContent);
    
    return {
        processed: true,
        components: componentsArray,
        oldImport: importMatch[0],
        newImport: newImportStatement
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
    
    for (const file of allFiles) {
        const relativePath = path.relative(path.join(__dirname, '..'), file);
        const result = fixImportInFile(file);
        
        if (result.processed) {
            processed++;
            console.log(`✓ ${relativePath}: ${result.components.join(', ')}`);
        } else {
            skipped++;
            // console.log(`- ${relativePath}: ${result.reason}`);
        }
    }
    
    console.log(`\n修复完成：`);
    console.log(`修复了 ${processed} 个文件`);
    console.log(`跳过了 ${skipped} 个文件`);
}

if (require.main === module) {
    main();
}

module.exports = { fixImportInFile };
