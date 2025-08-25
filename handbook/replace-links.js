const fs = require('fs');
const path = require('path');

// 递归获取所有mdx文件
function getAllMdxFiles(dir, fileList = [])
{
    const files = fs.readdirSync(dir);

    files.forEach(file =>
    {
        const filePath = path.join(dir, file);
        const stat = fs.statSync(filePath);

        if (stat.isDirectory())
        {
            getAllMdxFiles(filePath, fileList);
        } else if (file.endsWith('.mdx'))
        {
            fileList.push(filePath);
        }
    });

    return fileList;
}

// 替换CardLink中的链接
function replaceCardLinks(content)
{
    // 使用正则表达式匹配CardLink中的完整gitee链接并替换为相对路径
    const regex = /CardLink\s+link="https:\/\/gitee\.com\/RRQM_Home\/TouchSocket\/tree\/master\/([^"]+)"/g;
    return content.replace(regex, 'CardLink link="$1"');
}

// 主函数
function main()
{
    const rootDir = __dirname; // 当前脚本所在目录
    console.log('开始处理目录:', rootDir);

    try
    {
        // 获取所有mdx文件
        const mdxFiles = getAllMdxFiles(rootDir);
        console.log(`找到 ${mdxFiles.length} 个mdx文件`);

        let updatedCount = 0;
        let totalReplacements = 0;

        // 处理每个文件
        mdxFiles.forEach(filePath =>
        {
            try
            {
                // 读取文件内容，使用UTF-8编码
                const originalContent = fs.readFileSync(filePath, 'utf8');

                // 替换链接
                const newContent = replaceCardLinks(originalContent);

                // 如果内容有变化，则写回文件
                if (originalContent !== newContent)
                {
                    fs.writeFileSync(filePath, newContent, 'utf8');

                    // 计算替换次数
                    const replacements = (originalContent.match(/CardLink\s+link="https:\/\/gitee\.com\/RRQM_Home\/TouchSocket\/tree\/master\//g) || []).length;
                    totalReplacements += replacements;
                    updatedCount++;

                    console.log(`✓ 已更新: ${path.relative(rootDir, filePath)} (${replacements} 个链接)`);
                }
            } catch (error)
            {
                console.error(`✗ 处理文件失败: ${filePath}`, error.message);
            }
        });

        console.log('\n处理完成!');
        console.log(`- 总文件数: ${mdxFiles.length}`);
        console.log(`- 已更新文件数: ${updatedCount}`);
        console.log(`- 总替换次数: ${totalReplacements}`);

    } catch (error)
    {
        console.error('处理过程中发生错误:', error.message);
        process.exit(1);
    }
}

// 运行脚本
if (require.main === module)
{
    main();
}

module.exports = { replaceCardLinks, getAllMdxFiles };
