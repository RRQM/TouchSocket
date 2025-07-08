// src/BilibiliCard.js
import React from 'react';
import '../css/BilibiliCard.css';

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
        <div className="bilibili-card">
            <a href={link} target='_blank' rel='noopener noreferrer' className="card-link">
                <div className="card-content">
                    <div className="icon-container">
                        <svg
                            viewBox="0 0 1024 1024"
                            className="bilibili-icon"
                            fill="currentColor"
                            height="40"
                            width="40"
                        >
                            <path d="M434.048 437.376l-153.6 31.104 13.12 60.8 152.128-30.976-11.648-60.928z m82.88 161.408c-37.568 85.056-71.232 20.864-71.232 20.864l-25.152 17.152s49.536 71.552 96.768 17.216c55.68 54.336 98.304-17.6 98.304-17.6l-22.848-15.552c0-0.384-39.872 60.48-75.84-22.08z m75.84-100.48l152.512 31.04 12.8-60.928-153.28-31.04-12.032 60.928z m302.848-119.616c-2.432-113.408-96.128-135.168-96.128-135.168s-73.152-0.448-168.064-1.28l69.12-70.4s10.88-14.464-7.68-30.656c-18.56-16.192-19.84-8.96-26.24-4.672-5.76 4.224-88.512 89.984-103.04 105.344-37.568 0-76.8-0.448-114.752-0.448h13.376S363.2 138.176 356.736 133.504c-6.4-4.672-7.232-11.52-26.24 4.672-18.56 16.256-7.68 30.72-7.68 30.72L393.6 241.408c-76.8 0-143.04 0.448-173.696 1.664-99.392 30.336-91.712 135.68-91.712 135.68s1.28 225.92 0 340.288c10.88 114.304 94.08 132.608 94.08 132.608s33.152 0.832 57.792 0.832c2.432 7.296 4.48 43.072 42.432 43.072 37.568 0 42.432-43.072 42.432-43.072s276.672-1.28 299.712-1.28c1.216 12.352 6.848 45.248 44.8 44.8 37.568-0.832 40.384-47.36 40.384-47.36s12.928-1.28 51.328 0C890.752 831.168 896 720.32 896 720.32s-1.6-228.16-0.384-341.632z m-77.184 361.6c0 17.92-13.312 32.448-29.888 32.448h-545.28c-16.576 0-29.888-14.464-29.888-32.384V358.656c0-17.92 13.312-32.384 29.888-32.384h545.28c16.576 0 29.888 14.464 29.888 32.384v381.696z" />
                        </svg>
                    </div>
                    <div className="card-text">
                        <h3>{resolvedTitle}</h3>
                        <p className="card-description">点击观看视频教程</p>
                    </div>
                </div>
                {isPro && (
                    <div className="pro-badge">
                        <a href="/docs/current/video">
                            <span>课程</span>
                        </a>
                    </div>
                )}
            </a>
        </div>
    );
};

export default BilibiliCard;