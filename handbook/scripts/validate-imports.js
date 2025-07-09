const fs = require('fs');
const path = require('path');

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

// 验证单个文件的导入
function validateFile(filePath) {
    const content = fs.readFileSync(filePath, 'utf8');
    
    // 查找文件中使用的 Definition 组件
    const usedComponents = new Set();
    const componentRegex = /<(\w+Definition)\s*\/>/g;
    let componentMatch;
    
    while ((componentMatch = componentRegex.exec(content)) !== null) {
        usedComponents.add(componentMatch[1]);
    }
    
    if (usedComponents.size === 0) {
        return { valid: true, reason: '没有使用Definition组件' };
    }
    
    // 检查导入语句
    const importRegex = /import\s+{\s*([^}]+)\s*}\s+from\s+["']@site\/src\/components\/Definition\.js["'];?/;
    const importMatch = content.match(importRegex);
    
    if (!importMatch) {
        return { 
            valid: false, 
            reason: '缺少Definition导入语句',
            usedComponents: Array.from(usedComponents)
        };
    }
    
    // 解析导入的组件
    const importedComponents = importMatch[1].split(',').map(c => c.trim());
    const missingComponents = Array.from(usedComponents).filter(c => !importedComponents.includes(c));
    const unusedComponents = importedComponents.filter(c => !usedComponents.has(c));
    
    if (missingComponents.length > 0 || unusedComponents.length > 0) {
        return {
            valid: false,
            reason: '导入和使用不匹配',
            missingComponents,
            unusedComponents,
            usedComponents: Array.from(usedComponents),
            importedComponents
        };
    }
    
    return { valid: true, components: Array.from(usedComponents) };
}

// 主函数
function main() {
    const docsDir = path.join(__dirname, '..', 'docs');
    const versionedDocsDir = path.join(__dirname, '..', 'versioned_docs');
    
    const allFiles = [
        ...findMdxFiles(docsDir),
        ...findMdxFiles(versionedDocsDir)
    ];
    
    console.log(`验证 ${allFiles.length} 个 mdx 文件`);
    
    let validFiles = 0;
    let invalidFiles = 0;
    const errors = [];
    
    for (const file of allFiles) {
        const relativePath = path.relative(path.join(__dirname, '..'), file);
        const result = validateFile(file);
        
        if (result.valid) {
            validFiles++;
            if (result.components) {
                console.log(`✓ ${relativePath}: ${result.components.join(', ')}`);
            }
        } else {
            invalidFiles++;
            console.log(`✗ ${relativePath}: ${result.reason}`);
            if (result.missingComponents?.length > 0) {
                console.log(`  缺少导入: ${result.missingComponents.join(', ')}`);
            }
            if (result.unusedComponents?.length > 0) {
                console.log(`  多余导入: ${result.unusedComponents.join(', ')}`);
            }
            if (result.usedComponents?.length > 0) {
                console.log(`  使用的组件: ${result.usedComponents.join(', ')}`);
            }
            if (result.importedComponents?.length > 0) {
                console.log(`  导入的组件: ${result.importedComponents.join(', ')}`);
            }
            errors.push({ file: relativePath, ...result });
        }
    }
    
    console.log(`\n验证完成：`);
    console.log(`✓ 有效文件：${validFiles}`);
    console.log(`✗ 无效文件：${invalidFiles}`);
    
    if (errors.length > 0) {
        console.log('\n错误详情：');
        errors.forEach(error => {
            console.log(`- ${error.file}: ${error.reason}`);
        });
    } else {
        console.log('\n🎉 所有文件的导入都正确！');
    }
}

if (require.main === module) {
    main();
}
