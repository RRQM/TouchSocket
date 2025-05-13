import React from "react";
import classes from "../css/Pro.css";

export default function (props) {
  const { children } = props;
 
  return (
    <label className="pro-badge2">
      <a href="/docs/current/enterprise">Pro</a>
    </label>
  );
}
