import React, { useState, useEffect } from 'react';
import styles from './VotingModal.module.css';

const VotingModal: React.FC = () => {
  
  // eslint-disable-next-line no-unreachable
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    // 检查当前会话是否已经显示过弹窗（使用 sessionStorage 记录）
    const hasShownModal = sessionStorage.getItem('gitee-voting-modal-shown-2025');
    
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
    // 记录当前会话已显示过弹窗，刷新页面后会重新显示
    sessionStorage.setItem('gitee-voting-modal-shown-2025', 'true');
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