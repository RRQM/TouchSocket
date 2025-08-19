import React, { useState, useEffect } from 'react';
import CodeBlock from '@theme/CodeBlock';
import { extractCodeRegion, getAvailableRegions } from './codesData';

/**
 * CodeBlock组件 - 根据region名称显示对应的代码块
 * @param {Object} props
 * @param {string} props.region - region的名称
 * @param {string} props.language - 代码语言，默认为'csharp'
 * @param {string} props.title - 代码块标题，默认使用region名称
 * @param {boolean} props.showLineNumbers - 是否显示行号，默认为true
 * @param {boolean} props.showAvailableRegions - 是否在错误时显示可用的regions，默认为true
 * @param {boolean} props.showSourceFile - 是否在标题中显示源文件，默认为false
 */
const CustomCodeBlock = ({
    region,
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

    useEffect(() =>
    {
        const loadCode = () =>
        {
            try
            {
                setLoading(true);
                setError(null);

                if (!region)
                {
                    setError('请提供region参数');
                    setCodeInfo(null);
                    return;
                }

                const extractedInfo = extractCodeRegion(region);

                if (extractedInfo === null)
                {
                    let errorMessage = `找不到名为 "${region}" 的代码区域`;

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
    }, [region, showAvailableRegions]);

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
                <div>⚠️ 代码区域 "{region}" 为空</div>
            </div>
        );
    }

    // 生成标题
    let codeBlockTitle = title;
    if (!codeBlockTitle)
    {
        codeBlockTitle = showSourceFile
            ? `${region} (来自: ${codeInfo.sourceFile})`
            : `代码区域: ${region}`;
    }

    return (
        <CodeBlock
            language={language}
            title={codeBlockTitle}
            showLineNumbers={showLineNumbers}
            {...props}
        >
            {codeInfo.code}
        </CodeBlock>
    );
};

export default CustomCodeBlock;