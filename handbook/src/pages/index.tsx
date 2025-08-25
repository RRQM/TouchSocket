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

function Home()
{
  const context = useDocusaurusContext();
  const { siteConfig = {} } = context;

  React.useEffect(() => { }, []);

  return (
    <Layout title={`TouchSocket说明文档。 ${siteConfig.title}`} description="TouchSocket说明文档">
      <Banner />
      <Gitee />
      <Features />
      <UseCases />
      <QuickStart />
    </Layout>
  );
}

function Banner()
{

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
            知行合一，从理论到实践的C#网络通讯组件库。
          </div>
          <div className={"TouchSocket-banner-description" + (isDarkTheme ? " dark" : "")}>
            纸上得来终觉浅，绝知此事要躬行。
          </div>
          <ul className="TouchSocket-banner-spec">
            <li> Apache-2.0 宽松开源协议，商业免费授权</li>
            <li>
              支持 .NET Framework 4.5及以上，.NET Standard2.0及以上
            </li>
            <li>轻量级设计，最小化依赖包</li>
            <li>开箱即用，几行代码即可构建网络应用</li>
            <li>高性能异步通讯，支撑海量并发连接</li>
            <li>丰富的协议支持：TCP、UDP、HTTP、WebSocket、MQTT等</li>
            <li>内置断线重连、心跳检测、流量控制等企业级特性</li>
            <li>完善的插件体系，灵活扩展业务逻辑</li>
            <li>详细的中文文档，丰富的示例代码</li>
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
var service = new TcpService();
service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
service.Disconnected = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接
service.Received = (client, e) =>
{
    //从客户端收到信息
    string mes = Encoding.UTF8.GetString(e.Memory.Span);
    client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
    return EasyTask.CompletedTask;
};

await service.SetupAsync(new TouchSocketConfig()//载入配置
    .SetListenIPHosts("tcp://127.0.0.1:7788", 7789)//同时监听两个地址
    .ConfigureContainer(a =>
    {
        a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
    })
    .ConfigurePlugins(a =>
    {
        //a.Add();//此处可以添加插件
    }));
await service.StartAsync();//启动
`}
            />
          </SystemWindow>
        </div>
      </div>
    </div>
  );
}

function Gitee()
{
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
            <div style={{ color: "#2d5aa0" }}>4000 +</div>
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
            <div style={{ color: "#4f7cb8" }}>1000 +</div>
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
            <div style={{ color: "#6092b6" }}>524,288</div>
            <span className={isDarkTheme ? " dark" : ""}>Downloads</span>
          </div>
        </div>
      </div>
    </div>
  );
}

function CodeSection(props)
{
  let { language, replace, section, source } = props;

  source = source.replace(/\/\/ <.*?\n/g, "");

  if (replace)
  {
    for (const [pattern, value] of Object.entries(replace))
    {
      source = source.replace(new RegExp(pattern, "gs"), value);
    }
  }

  source = source.trim();
  if (!source.includes("\n"))
  {
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

function SystemWindow(systemWindowProps)
{
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

function Features()
{
  const { colorMode } = useColorMode();
  const isDarkTheme = colorMode === "dark";

  return (
    <div className="TouchSocket-content" style={{ paddingTop: "60px", paddingBottom: "60px" }}>
      <div style={{ textAlign: "center", marginBottom: "50px" }}>
        <h2 className={"TouchSocket-big-title" + (isDarkTheme ? " dark" : "")} style={{ marginBottom: "20px" }}>
          🚀 核心特性
        </h2>
        <p className={"TouchSocket-small-title" + (isDarkTheme ? " dark" : "")} style={{ fontSize: "16px", opacity: 0.8 }}>
          为现代C#应用程序提供全面的网络通讯解决方案
        </p>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(300px, 1fr))", gap: "30px", maxWidth: "1200px", margin: "0 auto" }}>
        <FeatureCard
          title="🔥 高性能架构"
          description="基于异步编程模型，支持数万并发连接，内存占用低，CPU效率高"
          isDark={isDarkTheme}
        />
        <FeatureCard
          title="🌐 协议丰富"
          description="支持TCP、UDP、HTTP、WebSocket、MQTT、Modbus等多种网络协议"
          isDark={isDarkTheme}
        />
        <FeatureCard
          title="🔧 企业级特性"
          description="内置断线重连、心跳检测、数据校验、流量控制等生产环境必备功能"
          isDark={isDarkTheme}
        />
        <FeatureCard
          title="🎯 易于使用"
          description="链式配置API，丰富的扩展点，几行代码即可构建复杂的网络应用"
          isDark={isDarkTheme}
        />
        <FeatureCard
          title="📚 完善文档"
          description="详细的中文文档，丰富的示例代码，活跃的社区支持"
          isDark={isDarkTheme}
        />
        <FeatureCard
          title="💡 插件生态"
          description="灵活的插件系统，支持自定义扩展，满足各种业务需求"
          isDark={isDarkTheme}
        />
      </div>
    </div>
  );
}

function FeatureCard({ title, description, isDark })
{
  return (
    <div style={{
      padding: "25px",
      borderRadius: "12px",
      border: isDark ? "1px solid #444" : "1px solid #e1e4e8",
      backgroundColor: isDark ? "#1a1a1a" : "#ffffff",
      transition: "all 0.3s ease",
      boxShadow: isDark ? "0 4px 12px rgba(0,0,0,0.3)" : "0 4px 12px rgba(0,0,0,0.1)"
    }}>
      <h3 style={{
        color: isDark ? "#ffffff" : "#2c3e50",
        marginBottom: "15px",
        fontSize: "18px",
        fontWeight: "600"
      }}>
        {title}
      </h3>
      <p style={{
        color: isDark ? "#cccccc" : "#666666",
        lineHeight: "1.6",
        fontSize: "14px",
        margin: 0
      }}>
        {description}
      </p>
    </div>
  );
}

function UseCases()
{
  const { colorMode } = useColorMode();
  const isDarkTheme = colorMode === "dark";

  return (
    <div className="TouchSocket-content" style={{ paddingTop: "60px", paddingBottom: "60px", backgroundColor: isDarkTheme ? "#0f0f0f" : "#f8f9fa" }}>
      <div style={{ textAlign: "center", marginBottom: "50px" }}>
        <h2 className={"TouchSocket-big-title" + (isDarkTheme ? " dark" : "")} style={{ marginBottom: "20px" }}>
          💼 应用场景
        </h2>
        <p className={"TouchSocket-small-title" + (isDarkTheme ? " dark" : "")} style={{ fontSize: "16px", opacity: 0.8 }}>
          TouchSocket广泛应用于各种行业和场景
        </p>
      </div>
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "25px", maxWidth: "1000px", margin: "0 auto" }}>
        <UseCaseCard
          icon="🏭"
          title="工业自动化"
          description="PLC通讯、设备监控、数据采集、Modbus协议通讯"
          isDark={isDarkTheme}
        />
        <UseCaseCard
          icon="🌐"
          title="物联网平台"
          description="设备接入、MQTT消息传递、实时数据推送"
          isDark={isDarkTheme}
        />
        <UseCaseCard
          icon="💬"
          title="即时通讯"
          description="聊天系统、消息推送、实时协作应用"
          isDark={isDarkTheme}
        />
        <UseCaseCard
          icon="🎮"
          title="游戏开发"
          description="多人在线游戏、实时对战、游戏服务器"
          isDark={isDarkTheme}
        />
        <UseCaseCard
          icon="💰"
          title="金融交易"
          description="高频交易系统、实时行情推送、风控系统"
          isDark={isDarkTheme}
        />
        <UseCaseCard
          icon="🚗"
          title="车联网"
          description="车载终端通讯、GPS定位、远程诊断"
          isDark={isDarkTheme}
        />
      </div>
    </div>
  );
}

function UseCaseCard({ icon, title, description, isDark })
{
  return (
    <div style={{
      padding: "30px 20px",
      borderRadius: "12px",
      backgroundColor: isDark ? "#1a1a1a" : "#ffffff",
      border: isDark ? "1px solid #333" : "1px solid #e1e4e8",
      textAlign: "center",
      transition: "all 0.3s ease",
      boxShadow: isDark ? "0 4px 12px rgba(0,0,0,0.3)" : "0 4px 12px rgba(0,0,0,0.1)"
    }}>
      <div style={{ fontSize: "48px", marginBottom: "15px" }}>{icon}</div>
      <h3 style={{
        color: isDark ? "#ffffff" : "#2c3e50",
        marginBottom: "15px",
        fontSize: "18px",
        fontWeight: "600"
      }}>
        {title}
      </h3>
      <p style={{
        color: isDark ? "#cccccc" : "#666666",
        lineHeight: "1.6",
        fontSize: "14px",
        margin: 0
      }}>
        {description}
      </p>
    </div>
  );
}

function QuickStart()
{
  const { colorMode } = useColorMode();
  const isDarkTheme = colorMode === "dark";

  return (
    <div className="TouchSocket-content" style={{ paddingTop: "60px", paddingBottom: "80px" }}>
      <div style={{ textAlign: "center", marginBottom: "50px" }}>
        <h2 className={"TouchSocket-big-title" + (isDarkTheme ? " dark" : "")} style={{ marginBottom: "20px" }}>
          ⚡ 快速开始
        </h2>
        <p className={"TouchSocket-small-title" + (isDarkTheme ? " dark" : "")} style={{ fontSize: "16px", opacity: 0.8 }}>
          三步即可开始你的网络编程之旅
        </p>
      </div>

      <div style={{ maxWidth: "900px", margin: "0 auto" }}>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "30px", marginBottom: "40px" }}>
          <StepCard
            step="1"
            title="安装包"
            description="通过NuGet包管理器安装TouchSocket"
            code="Install-Package TouchSocket"
            isDark={isDarkTheme}
          />
          <StepCard
            step="2"
            title="创建服务"
            description="几行代码创建TCP服务器"
            code="var service = new TcpService();"
            isDark={isDarkTheme}
          />
          <StepCard
            step="3"
            title="启动运行"
            description="配置并启动服务"
            code="await service.StartAsync();"
            isDark={isDarkTheme}
          />
        </div>

        <div style={{ textAlign: "center", marginTop: "40px" }}>
          <Link
            className="TouchSocket-get-start"
            to={useBaseUrl("docs/current/")}
            style={{
              display: "inline-block",
              padding: "15px 40px",
              fontSize: "18px",
              fontWeight: "600",
              borderRadius: "8px",
              textDecoration: "none",
              backgroundColor: "#4f7cb8",
              color: "#ffffff",
              border: "none",
              cursor: "pointer",
              transition: "all 0.3s ease",
              boxShadow: "0 4px 12px rgba(79, 124, 184, 0.3)"
            }}
          >
            开始使用 TouchSocket →
          </Link>
        </div>
      </div>
    </div>
  );
}

function StepCard({ step, title, description, code, isDark })
{
  return (
    <div style={{
      padding: "25px",
      borderRadius: "12px",
      border: isDark ? "1px solid #444" : "1px solid #e1e4e8",
      backgroundColor: isDark ? "#1a1a1a" : "#ffffff",
      textAlign: "center",
      position: "relative",
      boxShadow: isDark ? "0 4px 12px rgba(0,0,0,0.3)" : "0 4px 12px rgba(0,0,0,0.1)"
    }}>
      <div style={{
        position: "absolute",
        top: "-15px",
        left: "50%",
        transform: "translateX(-50%)",
        width: "30px",
        height: "30px",
        borderRadius: "50%",
        backgroundColor: "#4f7cb8",
        color: "#ffffff",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        fontSize: "14px",
        fontWeight: "bold"
      }}>
        {step}
      </div>
      <h3 style={{
        color: isDark ? "#ffffff" : "#2c3e50",
        marginBottom: "10px",
        marginTop: "15px",
        fontSize: "16px",
        fontWeight: "600"
      }}>
        {title}
      </h3>
      <p style={{
        color: isDark ? "#cccccc" : "#666666",
        fontSize: "14px",
        marginBottom: "15px",
        lineHeight: "1.5"
      }}>
        {description}
      </p>
      <code style={{
        backgroundColor: isDark ? "#2d2d2d" : "#f6f8fa",
        color: isDark ? "#79c0ff" : "#d73a49",
        padding: "8px 12px",
        borderRadius: "4px",
        fontSize: "12px",
        fontFamily: "Monaco, Consolas, monospace",
        display: "block",
        border: isDark ? "1px solid #444" : "1px solid #e1e4e8"
      }}>
        {code}
      </code>
    </div>
  );
}

export default Home;