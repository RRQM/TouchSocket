import React from "react";
import IconFont from "./iconfonts";
import classes from "./Tag.module.css";

export default function (props) {
  const { children } = props;
  const operates = {
    新增: {
      icon: "xinzeng",
      bgColor: "#52c41a", // 绿色 - 新增功能
    },
    修复: {
      icon: "bug",
      bgColor: "#f5222d", // 红色 - 修复问题
    },
    文档: {
      icon: "wendang",
      bgColor: "#2d5aa0", // 深蓝 - 文档相关
    },
    更新: {
      icon: "gengxin",
      bgColor: "#1890ff", // 蓝色 - 功能更新
    },
    调整: {
      icon: "tiaozheng",
      bgColor: "#722ed1", // 紫色 - 调整优化
    },
    升级: {
      icon: "shengji",
      bgColor: "#eb2f96", // 粉红 - 版本升级
    },
    移除: {
      icon: "shanchu",
      bgColor: "#8c8c8c", // 灰色 - 移除功能
    },
    答疑: {
      icon: "dayi",
      bgColor: "#13c2c2", // 青色 - 答疑解惑
    },
    优化: {
      icon: "youhua",
      bgColor: "#faad14", // 橙色 - 性能优化
    },
    重构: {
      icon: "gengxin",
      bgColor: "#fa541c", // 橙红 - 代码重构
    },

    推荐: {
      bgColor: "#52c41a", // 绿色 - 推荐使用
    },

    Pro: {
      bgColor: "#2d5aa0", // 深蓝 - 专业版本
    }
  };
  return (
    <label
      className={classes.label}
      title={children}
      style={{ backgroundColor: operates[children].bgColor }}
    >
      <IconFont
        name={operates[children].icon}
        color="white"
        size={14}
        className={classes.icon}
      />{" "}
      {children}
    </label>
  );
}
