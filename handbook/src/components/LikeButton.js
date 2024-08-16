import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import BrowserOnly from '@docusaurus/BrowserOnly';

const MyApp = () =>
{
  const location = useLocation();
  const [likeCount, setLikeCount] = useState(0);
  const [canLike, setCanLike] = useState(true); // 新增的状态用于控制是否可以点赞

  function getLastRouteName() {
    // 获取pathname并分割为数组
    const pathArray = location.pathname.split('/');
  
    // 获取最后一个非空的路径片段
    let lastRouteName = pathArray[pathArray.length - 1];
  
    // 如果最后一个路径片段为空，则返回"any"
    if (lastRouteName === '') {
      lastRouteName = 'any';
    }
  
    return lastRouteName;
  }
  
  useEffect(() =>
  {
    //let timer;
    if (location.pathname)
    {
      // timer = setTimeout(() =>
      // {
      //   setCanLike(true); // 10秒后允许点赞
      // }, 10000); // 10000毫秒 = 10秒
    }

    // // 清理函数
    // return () => clearTimeout(timer);
  }, [location.pathname]);

  const handleLikeClick = async () =>
  {
    try
    {
      const response = await fetch('https://touchsocket.net/likebuttonserver/linkclick', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          "link": getLastRouteName(),
        }),
      });

      if (!response.ok)
      {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const responseData = await response.json();

      if (responseData.IsSuccess)
      {
        setLikeCount(responseData.Count);
      }
      else
      {
        alert('哎呀，你这是要上天啊！歇会儿，让火箭模式冷却一下吧！');
      }
    } catch (error)
    {
      console.error('Error:', error);
    }
  };

  const getLikeClick = async () =>
  {
    try
    {
      const response = await fetch('https://touchsocket.net/likebuttonserver/getLinkCount', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          "link":  getLastRouteName(),
        }),
      });

      if (!response.ok)
      {
        throw new Error(`HTTP error! Status: ${response.status}`);
      }

      const responseData = await response.json();
      setLikeCount(responseData.Count);
    } catch (error)
    {
      console.error('Error:', error);
    }
  };

  useEffect(() =>
  {
    getLikeClick();
  }, [location.pathname]);

  return (
    <BrowserOnly>
      {() => (
        <button
          className="like-button"
          onClick={handleLikeClick}
          disabled={!canLike} // 如果不可以点赞，则禁用按钮
          title={!canLike ? '请先阅读10秒后再点赞' : undefined}
        >
          👍 {likeCount}
        </button>
      )}
    </BrowserOnly>
  );
};

export default MyApp;