import React, { useState, useEffect } from 'react';
import styles from './CoursePromotionModal.module.css';

const CoursePromotionModal: React.FC = () => {
  const [isVisible, setIsVisible] = useState(false);
  const [timeLeft, setTimeLeft] = useState('');

  useEffect(() => {
    // 检查当前会话是否已经显示过弹窗（使用 sessionStorage 记录）
    const hasShownModal = sessionStorage.getItem('course-promotion-modal-shown-2025-11');
    
    if (!hasShownModal) {
      // 延迟显示弹窗，让页面先加载完成
      const timer = setTimeout(() => {
        setIsVisible(true);
      }, 1000);

      return () => clearTimeout(timer);
    }
  }, []);

  useEffect(() => {
    // 计算倒计时
    const updateCountdown = () => {
      const endDate = new Date('2025-11-26T23:59:59');
      const now = new Date();
      const diff = endDate.getTime() - now.getTime();

      if (diff <= 0) {
        setTimeLeft('活动已结束');
        return;
      }

      const days = Math.floor(diff / (1000 * 60 * 60 * 24));
      const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);

      setTimeLeft(`${days}天 ${hours}小时 ${minutes}分 ${seconds}秒`);
    };

    updateCountdown();
    const interval = setInterval(updateCountdown, 1000);

    return () => clearInterval(interval);
  }, []);

  const handleClose = () => {
    setIsVisible(false);
    // 记录当前会话已显示过弹窗，刷新页面后会重新显示
    sessionStorage.setItem('course-promotion-modal-shown-2025-11', 'true');
  };

  const handleViewCourse = () => {
    // 在新窗口打开课程链接
    window.open('https://www.bilibili.com/cheese/play/ss489296905', '_blank');
    handleClose();
  };

  if (!isVisible) return null;

  return (
    <>
      {/* 遮罩层 */}
      <div className={styles.overlay} onClick={handleClose} />
      
      {/* 弹窗内容 */}
      <div className={styles.modal}>
        <div className={styles.header}>
          <h3 className={styles.title}>🎉 TouchSocket 4.0 正式版发布！</h3>
          <button className={styles.closeButton} onClick={handleClose}>
            ×
          </button>
        </div>
        
        <div className={styles.content}>
          <div className={styles.badge}>限时优惠</div>
          <div className={styles.icon}>🎓</div>
          <p className={styles.message}>
            为庆祝 <strong>TouchSocket 4.0 正式版</strong>发布，
            官方课程现已开启限时特惠！
          </p>
          
          <div className={styles.priceSection}>
            <div className={styles.originalPrice}>原价: ¥358</div>
            <div className={styles.promotionPrice}>限时优惠价: <span className={styles.price}>¥198</span></div>
          </div>

          <div className={styles.countdown}>
            <div className={styles.countdownLabel}>⏰ 距离活动结束还剩：</div>
            <div className={styles.countdownTime}>{timeLeft}</div>
          </div>

          <p className={styles.description}>
            活动时间：2025.11.23 - 2025.11.26
          </p>
        </div>
        
        <div className={styles.actions}>
          <button className={styles.viewButton} onClick={handleViewCourse}>
            🚀 立即查看课程
          </button>
          <button className={styles.laterButton} onClick={handleClose}>
            稍后再说
          </button>
        </div>
      </div>
    </>
  );
};

export default CoursePromotionModal;
