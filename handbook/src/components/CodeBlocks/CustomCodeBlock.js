import React, { useState, useEffect } from 'react';
import CodeBlock from '@theme/CodeBlock';
import { extractCodeRegion, getAvailableRegions } from './codesData';

/**
 * 解析高亮规则，例如 "1,2-3" 或 "{1,2-3}" 转换为数组 [1, 2, 3]
 * @param {string} highlightText - 高亮规则字符串
 * @returns {number[]} - 行号数组
 */
const parseHighlightRules = (highlightText) =>
{
    if (!highlightText || typeof highlightText !== 'string')
    {
        return [];
    }

    // 移除可能的花括号（兼容旧格式）
    const rulesText = highlightText.replace(/[{}]/g, '').trim();
    if (!rulesText)
    {
        return [];
    }

    const lines = [];
    const parts = rulesText.split(',');

    for (const part of parts)
    {
        const trimmedPart = part.trim();

        // 检查是否是范围（如 "2-5"）
        if (trimmedPart.includes('-'))
        {
            const [start, end] = trimmedPart.split('-').map(s => parseInt(s.trim(), 10));
            if (!isNaN(start) && !isNaN(end) && start <= end)
            {
                for (let i = start; i <= end; i++)
                {
                    lines.push(i);
                }
            }
        } else
        {
            // 单个行号
            const lineNumber = parseInt(trimmedPart, 10);
            if (!isNaN(lineNumber))
            {
                lines.push(lineNumber);
            }
        }
    }

    // 去重并排序
    return [...new Set(lines)].sort((a, b) => a - b);
};

/**
 * 将行号数组转换为Docusaurus支持的高亮字符串格式
 * @param {number[]} lines - 行号数组
 * @returns {string} - 高亮字符串，如 "1,3-5,8"
 */
const formatHighlightString = (lines) =>
{
    if (!lines || lines.length === 0) return '';

    // 确保行号已排序并去重
    const sortedLines = [...new Set(lines)].sort((a, b) => a - b);

    const ranges = [];
    let i = 0;

    while (i < sortedLines.length)
    {
        const start = sortedLines[i];
        let end = start;

        // 寻找连续的数字
        while (i + 1 < sortedLines.length && sortedLines[i + 1] === sortedLines[i] + 1)
        {
            i++;
            end = sortedLines[i];
        }

        // 如果是连续范围，用 start-end 格式，否则单独列出
        if (start === end)
        {
            ranges.push(start.toString());
        } else
        {
            ranges.push(`${start}-${end}`);
        }
        i++;
    }

    return ranges.join(',');
};

/**
 * CodeBlock组件 - 根据region名称显示对应的代码块
 * @param {Object} props
 * @param {string} props.region - region的名称
 * @param {string} props.highlight - 高亮规则，例如 "1,2-3,5" 或 "{1,2-3,5}"
 * @param {string} props.language - 代码语言，默认为'csharp'
 * @param {string} props.title - 代码块标题，默认使用region名称
 * @param {boolean} props.showLineNumbers - 是否显示行号，默认为true
 * @param {boolean} props.showAvailableRegions - 是否在错误时显示可用的regions，默认为true
 * @param {boolean} props.showSourceFile - 是否在标题中显示源文件，默认为false
 */
const CustomCodeBlock = ({
    region,
    highlight,
    language = 'csharp',
    title,
    showLineNumbers = true,
    showAvailableRegions = true,
    showSourceFile = false,
    ...props
}) =>
{
    const [codeInfo, setCodeInfo] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [highlightLines, setHighlightLines] = useState([]);

    useEffect(() =>
    {
        const loadCode = () =>
        {
            try
            {
                setLoading(true);
                setError(null);
                setHighlightLines([]);

                if (!region)
                {
                    setError('请提供region参数');
                    setCodeInfo(null);
                    return;
                }

                // 检查region是否包含旧格式的高亮规则（向下兼容）
                let regionName = region;
                let legacyHighlightRules = '';

                const legacyMatch = region.match(/^(.+?)\{([^}]+)\}$/);
                if (legacyMatch)
                {
                    regionName = legacyMatch[1].trim();
                    legacyHighlightRules = legacyMatch[2];
                    console.warn(`检测到旧格式的高亮规则: "${region}". 推荐使用独立的 highlight 参数: region="${regionName}" highlight="${legacyHighlightRules}"`);
                }

                const extractedInfo = extractCodeRegion(regionName);

                if (extractedInfo === null)
                {
                    let errorMessage = `找不到名为 "${regionName}" 的代码区域`;

                    if (showAvailableRegions)
                    {
                        const availableRegions = getAvailableRegions();
                        if (availableRegions.length > 0)
                        {
                            errorMessage += `\n\n可用的代码区域：\n${availableRegions.map(r => `• ${r.name} (${r.file})`).join('\n')}`;
                        } else
                        {
                            errorMessage += '\n\n当前文件中没有找到任何代码区域';
                        }
                    }

                    setError(errorMessage);
                    setCodeInfo(null);
                } else
                {
                    setCodeInfo(extractedInfo);

                    // 确定要使用的高亮规则（优先级：highlight参数 > 旧格式 > region定义中的高亮）
                    let highlightRulesToUse = highlight || legacyHighlightRules;

                    // 如果没有外部提供的高亮规则，使用从region定义中解析出的高亮信息
                    if (!highlightRulesToUse && extractedInfo.highlightLines && extractedInfo.highlightLines.length > 0)
                    {
                        highlightRulesToUse = extractedInfo.highlightLines.join(',');
                        console.log(`🎨 使用从region定义中解析的高亮规则: [${extractedInfo.highlightLines.join(',')}]`);
                    }

                    if (highlightRulesToUse)
                    {
                        const parsedHighlightLines = parseHighlightRules(highlightRulesToUse);
                        console.log('🎨 解析的高亮规则:', {
                            input: highlightRulesToUse,
                            parsed: parsedHighlightLines,
                            formatted: formatHighlightString(parsedHighlightLines)
                        });
                        setHighlightLines(parsedHighlightLines);
                    }
                }
            } catch (err)
            {
                setError(`处理代码失败: ${err.message}`);
                setCodeInfo(null);
            } finally
            {
                setLoading(false);
            }
        };

        loadCode();
    }, [region, highlight, showAvailableRegions]);

    if (loading)
    {
        return (
            <div style={{
                padding: '20px',
                textAlign: 'center',
                backgroundColor: '#f5f5f5',
                borderRadius: '4px',
                fontFamily: 'monospace'
            }}>
                <div>🔄 正在加载代码...</div>
            </div>
        );
    }

    if (error)
    {
        return (
            <div style={{
                padding: '20px',
                backgroundColor: '#ffebee',
                color: '#c62828',
                borderRadius: '4px',
                border: '1px solid #ffcdd2',
                fontFamily: 'monospace',
                whiteSpace: 'pre-line'
            }}>
                <div>❌ {error}</div>
            </div>
        );
    }

    if (!codeInfo || !codeInfo.code.trim())
    {
        return (
            <div style={{
                padding: '20px',
                textAlign: 'center',
                backgroundColor: '#fff3e0',
                color: '#ef6c00',
                borderRadius: '4px',
                border: '1px solid #ffcc02',
                fontFamily: 'monospace'
            }}>
                <div>⚠️ 代码区域 "{region.replace(/\{[^}]+\}$/, '')}" 为空</div>
            </div>
        );
    }

    // 生成标题
    let codeBlockTitle = title;
    if (!codeBlockTitle)
    {
        // 移除可能存在的旧格式高亮规则部分，只显示region名称
        const cleanRegionName = region.replace(/\{[^}]+\}$/, '');
        codeBlockTitle = showSourceFile
            ? `${cleanRegionName} (来自: ${codeInfo.sourceFile})`
            : `代码区域: ${cleanRegionName}`;
    }

    // 构建最终的代码内容和高亮信息
    let finalCode = codeInfo.code;
    let finalMetastring = '';

    if (highlightLines.length > 0)
    {
        const highlightString = formatHighlightString(highlightLines);
        // 使用 metastring 格式
        finalMetastring = `{${highlightString}}`;
        console.log('🎯 最终高亮字符串:', finalMetastring);
    }

    return (
        <>
            {process.env.NODE_ENV === 'development' && codeInfo && (
                <div style={{
                    padding: '8px',
                    backgroundColor: '#fff3e0',
                    border: '1px solid #ff9800',
                    borderRadius: '4px',
                    marginBottom: '8px',
                    fontSize: '12px',
                    fontFamily: 'monospace'
                }}>
                    📝 Region信息: region="{region}", highlight参数="{highlight || 'none'}",
                    region中的高亮="{codeInfo.highlightLines ? codeInfo.highlightLines.join(',') : 'none'}"
                </div>
            )}
            {process.env.NODE_ENV === 'development' && highlightLines.length > 0 && (
                <div style={{
                    padding: '8px',
                    backgroundColor: '#e3f2fd',
                    border: '1px solid #2196f3',
                    borderRadius: '4px',
                    marginBottom: '8px',
                    fontSize: '12px',
                    fontFamily: 'monospace'
                }}>
                    🔍 调试信息: 高亮行={JSON.stringify(highlightLines)}, 最终metastring="{finalMetastring}"
                </div>
            )}
            <CodeBlock
                language={language}
                title={codeBlockTitle}
                showLineNumbers={showLineNumbers}
                metastring={finalMetastring}
                {...props}
            >
                {finalCode}
            </CodeBlock>
        </>
    );
};

export default CustomCodeBlock;