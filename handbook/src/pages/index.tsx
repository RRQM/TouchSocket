import Link from "@docusaurus/Link";
import { useColorMode } from "@docusaurus/theme-common";
import useBaseUrl from "@docusaurus/useBaseUrl";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import Layout from "@theme/Layout";
import components from "@theme/MDXComponents";
import React from "react";
import AndroidIcon from "./android.svg";
import DockerIcon from "./docker.svg";
import "./index.css";
import "./index.own.css";
import KubernetesIcon from "./kubernetes.svg";
import LinuxIcon from "./linux.svg";
import MacOSIcon from "./macos.svg";
import WindowIcon from "./windows.svg";

function Home() {
  const context = useDocusaurusContext();
  const { siteConfig = {} } = context;

  React.useEffect(() => { }, []);

  return (
    <Layout title={`TouchSocket说明文档。 ${siteConfig.title}`} description="TouchSocket说明文档">
      <Banner />
      <Gitee />
    </Layout>
  );
}

function Banner() {

  const { colorMode, setLightTheme, setDarkTheme } = useColorMode();
  const isDarkTheme = colorMode === "dark";

  return (
    <div className={"TouchSocket-banner" + (isDarkTheme ? " dark" : "")}>
      <div className="TouchSocket-banner-container">
        <div className="TouchSocket-banner-item">
          <div className="TouchSocket-banner-project">
            TouchSocket
          </div>
          <div style={{ color: "#4f7cb8", position: "relative", fontSize: 14, fontWeight: 500, marginTop: "0.5em" }}>
            一款简单易用的基础网络通讯组件库。
          </div>
          <div className={"TouchSocket-banner-description"+ (isDarkTheme ? " dark" : "")}>
            三十功名尘与土，八千里路云和月。
          </div>
          <ul className="TouchSocket-banner-spec">
            <li> Apache-2.0 宽松开源协议，商业免费授权</li>
            <li>
              支持 .NET Framework 4.5及以上，.NET Standard2.0及以上
            </li>
            <li>极少依赖</li>
            <li>极速上手，极简使用</li>
          </ul>
          <div className="TouchSocket-support-platform">受支持平台：</div>
          <div className="TouchSocket-support-icons">
            <span>
              <WindowIcon height="39" width="39" />
            </span>
            <span>
              <LinuxIcon height="39" width="39" />
            </span>
            <span>
              <AndroidIcon height="39" width="39" />
            </span>
            <span>
              <MacOSIcon height="39" width="39" />
            </span>
            <span>
              <DockerIcon height="39" width="39" />
            </span>
            <span>
              <KubernetesIcon height="39" width="39" />
            </span>
          </div>
          <div className="TouchSocket-get-start-btn">
            <Link className="TouchSocket-get-start" to={useBaseUrl("docs/current/")}>
              入门指南
              {/* <span className="TouchSocket-version">v1.0</span> */}
            </Link>
          </div>
        </div>
        <div className="TouchSocket-banner-item">
          <SystemWindow style={{ float: "right" }}>
            <CodeSection
              language="cs"
              // section="schema"
              source={`
// highlight-next-line
var service = new TcpService();
service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
service.Disconnected = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接
service.Received = (client, e) =>
{
    //从客户端收到信息
    string mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);
    client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
    return EasyTask.CompletedTask;
};

service.Setup(new TouchSocketConfig()//载入配置
    .SetListenIPHosts("tcp://127.0.0.1:7788", 7789)//同时监听两个地址
    .ConfigureContainer(a =>
    {
        a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
    })
    .ConfigurePlugins(a =>
    {
        //a.Add();//此处可以添加插件
    }));
service.Start();//启动
`}
            />
          </SystemWindow>
        </div>
      </div>
    </div>
  );
}

function Gitee() {
  const { colorMode, setLightTheme, setDarkTheme } = useColorMode();
  const isDarkTheme = colorMode === "dark";

  return (
    <div className="TouchSocket-content">
      <p className={"TouchSocket-small-title" + (isDarkTheme ? " dark" : "")}>
        开源免费/商业免费授权
      </p>
      <h1 className={"TouchSocket-big-title" + (isDarkTheme ? " dark" : "")}>
        ⭐️ Apache-2.0 开源协议，代码在 Gitee/Github 平台托管 ⭐️
      </h1>
      <div className="TouchSocket-gitee-log">
        <div
          className="TouchSocket-log-item"
          style={{ border: "3px solid #4f7cb8" }}
        >
          <div
            className={"TouchSocket-log-jiao" + (isDarkTheme ? " dark" : "")}
          ></div>
          <div className="TouchSocket-log-number">
            <div style={{ color: "#2d5aa0" }}>2000 +</div>
            <span className={isDarkTheme ? " dark" : ""}>Stars</span>
          </div>
        </div>
        <div
          className="TouchSocket-log-item"
          style={{ border: "3px solid #7fa8d3" }}
        >
          <div
            className={"TouchSocket-log-jiao" + (isDarkTheme ? " dark" : "")}
          ></div>
          <div className="TouchSocket-log-number">
            <div style={{ color: "#4f7cb8" }}>600 +</div>
            <span className={isDarkTheme ? " dark" : ""}>Forks</span>
          </div>
        </div>
        <div
          className="TouchSocket-log-item"
          style={{ border: "3px solid #a5c6e0" }}
        >
          <div
            className={"TouchSocket-log-jiao" + (isDarkTheme ? " dark" : "")}
          ></div>
          <div className="TouchSocket-log-number">
            <div style={{ color: "#6092b6" }}>202,125</div>
            <span className={isDarkTheme ? " dark" : ""}>Downloads</span>
          </div>
        </div>
      </div>
    </div>
  );
}

function CodeSection(props) {
  let { language, replace, section, source } = props;

  source = source.replace(/\/\/ <.*?\n/g, "");

  if (replace) {
    for (const [pattern, value] of Object.entries(replace)) {
      source = source.replace(new RegExp(pattern, "gs"), value);
    }
  }

  source = source.trim();
  if (!source.includes("\n")) {
    source += "\n";
  }

  return (
    <components.pre>
      <components.code
        children={source}
        className={`language-${language}`}
        mdxType="code"
        originalType="code"
        parentName="pre"
      />
    </components.pre>
  );
}

function SystemWindow(systemWindowProps) {
  const { children, className, ...props } = systemWindowProps;
  return (
    <div
      {...props}
      className={"system-window blue-accent preview-border " + className}
    >
      <div className="system-top-bar">
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#f5222d" }}
        />
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#faad14" }}
        />
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#52c41a" }}
        />
      </div>
      {children}
    </div>
  );
}

export default Home;
