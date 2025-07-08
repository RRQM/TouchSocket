import React from "react";
import IconFont from "./iconfonts";
import classes from "./Tag.module.css";

export default function (props) {
  const { children } = props;
  const operates = {
    新增: {
      icon: "xinzeng",
      bgColor: "#52c41a",
    },
    修复: {
      icon: "bug",
      bgColor: "#722ed1",
    },
    文档: {
      icon: "wendang",
      bgColor: "#1890ff",
    },
    更新: {
      icon: "gengxin",
      bgColor: "#13c2c2",
    },
    调整: {
      icon: "tiaozheng",
      bgColor: "#595959",
    },
    升级: {
      icon: "shengji",
      bgColor: "#eb2f96",
    },
    移除: {
      icon: "shanchu",
      bgColor: "#f5222d",
    },
    答疑: {
      icon: "dayi",
      bgColor: "#8c8c8c",
    },
    优化: {
      icon: "youhua",
      bgColor: "#52c41a",
    },
    重构: {
      icon: "gengxin",
      bgColor: "#fa8c16",
    },

    推荐: {
      bgColor: "#52c41a",
    },

    Pro: {
      bgColor: "#1890ff",
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
