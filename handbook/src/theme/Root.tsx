import React from 'react';
import VotingModal from '../components/VotingModal';

// 包装原始的 Root 组件并添加我们的弹窗
export default function Root({ children }) {
  return (
    <>
      {children}
      <VotingModal />
    </>
  );
}