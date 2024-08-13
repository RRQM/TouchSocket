import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import BrowserOnly from '@docusaurus/BrowserOnly';

const MyApp = () => {
  const location = useLocation();
  const [likeCount, setLikeCount] = useState(0); // 使用 useState 来管理计数

  useEffect(() => {
    console.log(`Component rendered at ${location.pathname}`);
  }, [location.pathname]); // 添加依赖项以确保只在路由变化时重新运行

  const handleLikeClick = () => {
    setLikeCount(likeCount + 1); // 使用 setState 方法更新计数
    console.log(`User liked the page. New like count: ${likeCount + 1}`);
  };

  return (
    <BrowserOnly>
      {() => (
        <button className="like-button" onClick={handleLikeClick}>
          👍 {likeCount}
        </button>
      )}
    </BrowserOnly>
  );
};

export default MyApp;