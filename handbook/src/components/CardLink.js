// src/CardLink.js
import React from 'react';
import '../css/CardLink.css';
import CodeDemo from '../pages/codedemo.svg';
import GiteeIcon from './icons/GiteeIcon';
import GitHubIcon from './icons/GitHubIcon';

// 辅助函数：从URL中提取最后一个路径段
function getLastPathSegment(path)
{
    if (!path) return path;

    // 如果是完整URL，提取路径
    if (path.startsWith('http'))
    {
        const urlParts = new URL(path).pathname.split('/').filter(Boolean);
        return urlParts.length > 0 ? urlParts[urlParts.length - 1] : path;
    }

    // 如果是相对路径，提取最后一段
    const pathParts = path.split('/').filter(Boolean);
    return pathParts.length > 0 ? pathParts[pathParts.length - 1] : path;
}

// 辅助函数：构建完整的仓库URL
function buildRepoUrls(repoPath)
{
    const giteeBase = 'https://gitee.com/RRQM_Home/TouchSocket/tree/master/';
    const githubBase = 'https://github.com/RRQM/TouchSocket/tree/master/';

    // 确保路径不以斜杠开头
    const cleanPath = repoPath.startsWith('/') ? repoPath.slice(1) : repoPath;

    return {
        gitee: `${giteeBase}${cleanPath}`,
        github: `${githubBase}${cleanPath}`
    };
}

const CardLink = ({ title, link, isPro }) =>
{
    const resolvedTitle = title || getLastPathSegment(link);
    const urls = buildRepoUrls(link);

    const handleLinkClick = (e, url) =>
    {
        e.preventDefault();
        e.stopPropagation();
        window.open(url, '_blank');
    };

    return (
        <div className="card-link">
            <div className="card-content">
                <CodeDemo height="30" width="30" />
                <h3>{resolvedTitle}</h3>
            </div>

            <div className="card-links">
                <a
                    href={urls.gitee}
                    className="link-button gitee"
                    onClick={(e) => handleLinkClick(e, urls.gitee)}
                    target="_blank"
                    rel="noopener noreferrer"
                >
                    <GiteeIcon className="link-icon" />
                    Gitee
                </a>
                <a
                    href={urls.github}
                    className="link-button github"
                    onClick={(e) => handleLinkClick(e, urls.github)}
                    target="_blank"
                    rel="noopener noreferrer"
                >
                    <GitHubIcon className="link-icon" />
                    GitHub
                </a>
            </div>

            {isPro && (
                <div className="pro-badge">
                    <a href="/docs/current/enterprise">Pro</a>
                </div>
            )}
        </div>
    );
};

export default CardLink;