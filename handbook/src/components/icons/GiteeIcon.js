import React from 'react';

const GiteeIcon = ({ width = 16, height = 16, className = "" }) => (
    <svg
        width={width}
        height={height}
        viewBox="0 0 16 16"
        fill="currentColor"
        className={className}
    >
        <circle cx="8" cy="8" r="7" stroke="currentColor" strokeWidth="1" fill="none" />
        <path d="M5 6c0-1.5 1.5-3 3.5-3S12 4.5 12 6v1H9v1h3v1c0 1.5-1.5 3-3.5 3S5 10.5 5 9V6z" />
    </svg>
);

export default GiteeIcon;
