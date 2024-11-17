// src/CardLink.js
import React from 'react';
import '../css/CardLink.css';
import CodeDemo from '../pages/codedemo.svg';

// 辅助函数：从URL中提取最后一个路径段
function getLastPathSegment(url) {
    const urlParts = new URL(url).pathname.split('/').filter(Boolean);
    if (urlParts.length > 0) {
        return urlParts[urlParts.length - 1];
    } else {
        return url; // 如果没有路径段，返回整个URL
    }
}

const CardLink = ({ title, link, isPro }) => {
    const resolvedTitle = title || getLastPathSegment(link);

    return (
        <div className="card-link">
            <a href={link}>
                <div className="row card-content">
                    <CodeDemo height="30" width="30" />
                    <h3>{resolvedTitle}</h3>
                </div>
                {isPro && (
                    <div className="pro-badge">
                        <a href="/docs/current/enterprise">Pro</a>
                    </div>
                )}
            </a>
        </div>
    );
};

export default CardLink;