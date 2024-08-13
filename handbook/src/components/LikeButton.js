import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import BrowserOnly from '@docusaurus/BrowserOnly';

const MyApp = () => {
  const location = useLocation();
  const [likeCount, setLikeCount] = useState(0); // 使用 useState 来管理计数

  useEffect(() => {
    handleLikeClick();
  }, [location.pathname]); // 添加依赖项以确保只在路由变化时重新运行

  const handleLikeClick =async () => 
  {
    try {
    //   const response = await fetch('http://127.0.0.1:7789/likebuttonserver/linkclick', {
    //     method: 'POST', // 或者 'PUT'
    //     headers: {
    //       'Content-Type': 'application/json',
    //     },
    //     body: JSON.stringify(
    //       {
    //         "link": location.pathname,
    //       }), // 数据体必须是 JSON 格式
    //   });

    //   if (!response.ok) {
    //     throw new Error(`HTTP error! Status: ${response.status}`);
    //   }

    //   const responseData = await response.json();
    //   setLikeCount(responseData.Count); // 使用 setState 方法更新计数
    } 
    catch (error) 
    {
      console.error('Error:', error);
    }
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