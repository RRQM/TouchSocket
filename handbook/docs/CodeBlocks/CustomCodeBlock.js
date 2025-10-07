import React, { useState, useEffect } from 'react';
import CodeBlock from '@theme/CodeBlock';
import { extractCodeRegion, getAvailableRegions } from './codesData';

/**
 * 检测是否为构建环境
 * @returns {boolean} - 是否为构建环境
 */
const isBuildEnvironment = () =>
{
    // 安全地访问 process 对象
    if (typeof process === 'undefined') 
    {
        return typeof window === 'undefined'; // SSR环境，视为构建环境
    }

    // 更激进的构建环境检测 - 在所有可能的构建场景下都返回 true
    const isProduction = process.env.NODE_ENV === 'production';
    const isDocusaurusBuild = !!(process.env.DOCUSAURUS_CURRENT_LOCALE ||
        process.env.BUILD_PHASE === 'build' ||
        process.env.npm_lifecycle_event === 'build');
    const isSSR = typeof window === 'undefined';

    // 强制在任何看起来像构建的环境中启用错误检测
    const isBuildMode = isProduction || isDocusaurusBuild || isSSR;

    return isBuildMode;
};

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
 * @param {boolean} props.showAvailableRegions - 是否在错误时显示可用的regions，默认为false
 * @param {boolean} props.showSourceFile - 是否在标题中显示源文件，默认为false
 */
const CustomCodeBlock = ({
    region,
    highlight,
    language = 'csharp',
    title,
    showLineNumbers = true,
    showAvailableRegions = false,
    showSourceFile = false,
    ...props
}) =>
{
    // 🚨 在组件渲染时立即进行验证 - 这会发生在服务端渲染阶段
    if (isBuildEnvironment())
    {
        if (!region)
        {
            const error = new Error(`[BUILD VALIDATION FAILED] CustomCodeBlock 缺少 region 参数`);
            throw error; // 直接抛出，不等待useEffect
        }

        // 检查region是否存在（支持旧格式）
        let regionName = region;
        const legacyMatch = region.match(/^(.+?)\{([^}]+)\}$/);
        if (legacyMatch)
        {
            regionName = legacyMatch[1].trim();
        }

        const extractedInfo = extractCodeRegion(regionName);
        if (extractedInfo === null)
        {
            const error = new Error(`[BUILD VALIDATION FAILED] 找不到名为 "${regionName}" 的代码区域`);
            throw error; // 直接抛出，不等待useEffect
        }
    }

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
                    const errorMessage = '请提供region参数';

                    // 在构建环境中抛出异常，阻止网站发布
                    if (isBuildEnvironment())
                    {
                        throw new Error(`[CodeBlock Build Error] ${errorMessage}`);
                    }

                    setError(errorMessage);
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
                }

                const extractedInfo = extractCodeRegion(regionName);

                if (extractedInfo === null)
                {
                    const errorMessage = `找不到名为 "${regionName}" 的代码区域`;

                    // 在构建环境中抛出异常，阻止网站发布
                    if (isBuildEnvironment())
                    {
                        throw new Error(`[CodeBlock Build Error] ${errorMessage} (region: "${regionName}")`);
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
                    }

                    if (highlightRulesToUse)
                    {
                        const parsedHighlightLines = parseHighlightRules(highlightRulesToUse);
                        setHighlightLines(parsedHighlightLines);
                    }
                }
            } catch (err)
            {
                const errorMessage = `处理代码失败: ${err.message}`;

                // 在构建环境中抛出异常，阻止网站发布
                if (isBuildEnvironment())
                {
                    throw new Error(`[CodeBlock Build Error] ${errorMessage} (region: "${region}")`);
                }

                setError(errorMessage);
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
    // 去除尾随的所有空白字符（包括 \r\n、\n、空格、制表符等），防止显示时多一行
    let finalCode = codeInfo.code.replace(/\s+$/, '');
    let finalMetastring = '';

    if (highlightLines.length > 0)
    {
        const highlightString = formatHighlightString(highlightLines);
        // 使用 metastring 格式
        finalMetastring = `{${highlightString}}`;
    }

    // 安全地检查是否为开发环境
    const isDevelopment = typeof process !== 'undefined' && process.env.NODE_ENV === 'development';

    return (
        <CodeBlock
            language={language}
            title={codeBlockTitle}
            showLineNumbers={showLineNumbers}
            metastring={finalMetastring}
            {...props}
        >
            {finalCode}
        </CodeBlock>
    );
};

export default CustomCodeBlock;