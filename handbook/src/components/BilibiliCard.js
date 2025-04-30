// src/BilibiliCard.js
import React from 'react';
import '../css/BilibiliCard.css';
import Bilibili from '../pages/bilibili.svg';

// 辅助函数：从URL中提取最后一个路径段
function getLastPathSegment(url) {
    const urlParts = new URL(url).pathname.split('/').filter(Boolean);
    if (urlParts.length > 0) {
        return urlParts[urlParts.length - 1];
    } else {
        return url; // 如果没有路径段，返回整个URL
    }
}

const BilibiliCard = ({ title, link, isPro }) => {
    const resolvedTitle = title || getLastPathSegment(link);

    return (
        <div className="card-link">
            <a href={link} target='_blank'>
                <div className="row card-content">
                    <Bilibili height="40" width="40" />
                    <h3>{resolvedTitle}</h3>
                </div>
                {isPro && (
                    <div className="pro-badge">
                        <span>付费</span>
                    </div>
                )}
            </a>
        </div>
    );
};

export default BilibiliCard;