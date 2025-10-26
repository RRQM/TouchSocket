import React, { useState, useEffect } from 'react';
import styles from './VotingModal.module.css';

const VotingModal: React.FC = () => {
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    // 检查是否已经显示过弹窗（使用 localStorage 记录）
    const hasShownModal = localStorage.getItem('gitee-voting-modal-shown-2025');
    
    if (!hasShownModal) {
      // 延迟显示弹窗，让页面先加载完成
      const timer = setTimeout(() => {
        setIsVisible(true);
      }, 1000);

      return () => clearTimeout(timer);
    }
  }, []);

  const handleClose = () => {
    setIsVisible(false);
    // 记录已显示过弹窗，24小时内不再显示
    const expirationTime = Date.now() + 24 * 60 * 60 * 1000; // 24小时后过期
    localStorage.setItem('gitee-voting-modal-shown-2025', expirationTime.toString());
  };

  const handleVote = () => {
    // 在新窗口打开投票链接
    window.open('https://gitee.com/activity/2025opensource?ident=IYFARH', '_blank');
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
          <h3 className={styles.title}>🎉 支持 TouchSocket 参与 Gitee 2025 投票！</h3>
          <button className={styles.closeButton} onClick={handleClose}>
            ×
          </button>
        </div>
        
        <div className={styles.content}>
          <div className={styles.icon}>🏆</div>
          <p className={styles.message}>
            我正在参加 <strong>Gitee 2025 最受欢迎的开源软件投票活动</strong>，
            快来给 TouchSocket 投票吧！
          </p>
          <p className={styles.description}>
            您的每一票都是对开源社区的支持，感谢您的参与！
          </p>
        </div>
        
        <div className={styles.actions}>
          <button className={styles.voteButton} onClick={handleVote}>
            🚀 立即投票
          </button>
          <button className={styles.laterButton} onClick={handleClose}>
            稍后再说
          </button>
        </div>
      </div>
    </>
  );
};

export default VotingModal;