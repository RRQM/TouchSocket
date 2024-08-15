import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import BrowserOnly from '@docusaurus/BrowserOnly';

const MyApp = () =>
{
  const location = useLocation();
  const [likeCount, setLikeCount] = useState(0);
  const [canLike, setCanLike] = useState(false); // 新增的状态用于控制是否可以点赞

  useEffect(() =>
  {
    let timer;
    if (location.pathname)
    {
      timer = setTimeout(() =>
      {
        setCanLike(true); // 10秒后允许点赞
      }, 10000); // 10000毫秒 = 10秒
    }

    // 清理函数
    return () => clearTimeout(timer);
  }, [location.pathname]);

  const handleLikeClick = async () =>
  {
    if (!canLike)
    {
      alert('Please read for at least 10 seconds before liking.');
      return;
    }

    try
    {
      const response = await fetch('https://touchsocket.net/likebuttonserver/linkclick', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          "link": location.pathname,
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
        alert('aaa，太快了，歇歇啊！');
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
          "link": location.pathname,
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