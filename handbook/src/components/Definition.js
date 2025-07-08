import React, { useState } from 'react';
import './Definition.css';

// 预定义的TouchSocket包配置
const TOUCHSOCKET_PACKAGES = {
  // 基础包
  TouchSocket: {
    namespace: 'TouchSocket',
    assembly: 'TouchSocket.dll',
    packageName: 'TouchSocket',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket'
  },
  TouchSocketCore: {
    namespace: 'TouchSocket.Core',
    assembly: 'TouchSocket.Core.dll',
    packageName: 'TouchSocket.Core',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Core'
  },
  
  // 通信协议包
  TouchSocketDmtp: {
    namespace: 'TouchSocket.Dmtp',
    assembly: 'TouchSocket.Dmtp.dll',
    packageName: 'TouchSocket.Dmtp',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Dmtp'
  },
  TouchSocketHttp: {
    namespace: ['TouchSocket.Http', 'TouchSocket.Http.WebSockets'],
    assembly: ['TouchSocket.Http.dll', 'TouchSocket.Http.WebSockets.dll'],
    packageName: 'TouchSocket.Http',
    nugetUrl: ['https://www.nuget.org/packages/TouchSocket.Http', 'https://www.nuget.org/packages/TouchSocket.Http']
  },
  TouchSocketNamedPipe: {
    namespace: 'TouchSocket.NamedPipe',
    assembly: 'TouchSocket.NamedPipe.dll',
    packageName: 'TouchSocket.NamedPipe',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.NamedPipe'
  },
  TouchSocketSerialPorts: {
    namespace: 'TouchSocket.SerialPorts',
    assembly: 'TouchSocket.SerialPorts.dll',
    packageName: 'TouchSocket.SerialPorts',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.SerialPorts'
  },
  
  // RPC包
  TouchSocketRpc: {
    namespace: ['TouchSocket.Rpc', 'TouchSocket.Rpc.JsonRpc'],
    assembly: ['TouchSocket.Rpc.dll', 'TouchSocket.Rpc.JsonRpc.dll'],
    packageName: 'TouchSocket.Rpc',
    nugetUrl: ['https://www.nuget.org/packages/TouchSocket.Rpc', 'https://www.nuget.org/packages/TouchSocket.Rpc']
  },
  TouchSocketJsonRpc: {
    namespace: 'TouchSocket.JsonRpc',
    assembly: 'TouchSocket.JsonRpc.dll',
    packageName: 'TouchSocket.JsonRpc',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.JsonRpc'
  },
  TouchSocketXmlRpc: {
    namespace: 'TouchSocket.XmlRpc',
    assembly: 'TouchSocket.XmlRpc.dll',
    packageName: 'TouchSocket.XmlRpc',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.XmlRpc'
  },
  TouchSocketWebApi: {
    namespace: 'TouchSocket.WebApi',
    assembly: 'TouchSocket.WebApi.dll',
    packageName: 'TouchSocket.WebApi',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.WebApi'
  },
  TouchSocketWebApiSwagger: {
    namespace: 'TouchSocket.WebApi.Swagger',
    assembly: 'TouchSocket.WebApi.Swagger.dll',
    packageName: 'TouchSocket.WebApi.Swagger',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.WebApi.Swagger'
  },
  
  // 工业协议包
  TouchSocketModbus: {
    namespace: 'TouchSocket.Modbus',
    assembly: 'TouchSocket.Modbus.dll',
    packageName: 'TouchSocket.Modbus',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Modbus'
  },
  TouchSocketMqtt: {
    namespace: 'TouchSocket.Mqtt',
    assembly: 'TouchSocket.Mqtt.dll',
    packageName: 'TouchSocket.Mqtt',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Mqtt'
  },
  
  // 扩展包
  TouchSocketAspNetCore: {
    namespace: 'TouchSocket.AspNetCore',
    assembly: 'TouchSocket.AspNetCore.dll',
    packageName: 'TouchSocket.AspNetCore',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.AspNetCore'
  },
  TouchSocketHosting: {
    namespace: 'TouchSocket.Hosting',
    assembly: 'TouchSocket.Hosting.dll',
    packageName: 'TouchSocket.Hosting',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Hosting'
  },
  TouchSocketCoreDependencyInjection: {
    namespace: 'TouchSocket.Core.DependencyInjection',
    assembly: 'TouchSocket.Core.DependencyInjection.dll',
    packageName: 'TouchSocket.Core.DependencyInjection',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Core.DependencyInjection'
  },
  TouchSocketCoreAutofac: {
    namespace: 'TouchSocket.Core.Autofac',
    assembly: 'TouchSocket.Core.Autofac.dll',
    packageName: 'TouchSocket.Core.Autofac',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Core.Autofac'
  },
  TouchSocketRpcRateLimiting: {
    namespace: 'TouchSocket.Rpc.RateLimiting',
    assembly: 'TouchSocket.Rpc.RateLimiting.dll',
    packageName: 'TouchSocket.Rpc.RateLimiting',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Rpc.RateLimiting'
  },
  
  // 专业版包
  TouchSocketPro: {
    namespace: 'TouchSocketPro',
    assembly: 'TouchSocketPro.dll',
    packageName: 'TouchSocketPro',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocketPro'
  },
  TouchSocketProDmtp: {
    namespace: 'TouchSocketPro.Dmtp',
    assembly: 'TouchSocketPro.Dmtp.dll',
    packageName: 'TouchSocketPro.Dmtp',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocketPro.Dmtp'
  },
  TouchSocketProAspNetCore: {
    namespace: 'TouchSocketPro.AspNetCore',
    assembly: 'TouchSocketPro.AspNetCore.dll',
    packageName: 'TouchSocketPro.AspNetCore',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocketPro.AspNetCore'
  },
  TouchSocketProHosting: {
    namespace: 'TouchSocketPro.Hosting',
    assembly: 'TouchSocketPro.Hosting.dll',
    packageName: 'TouchSocketPro.Hosting',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocketPro.Hosting'
  },
  TouchSocketProModbus: {
    namespace: 'TouchSocketPro.Modbus',
    assembly: 'TouchSocketPro.Modbus.dll',
    packageName: 'TouchSocketPro.Modbus',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocketPro.Modbus'
  },
  TouchSocketProPlcBridges: {
    namespace: ['TouchSocketPro.PlcBridges', 'TouchSocketPro.Modbus'],
    assembly: ['TouchSocketPro.PlcBridges.dll', 'TouchSocketPro.Modbus.dll'],
    packageName: ['TouchSocketPro.PlcBridges', 'TouchSocketPro.Modbus'],
    nugetUrl: ['https://www.nuget.org/packages/TouchSocketPro.PlcBridges', 'https://www.nuget.org/packages/TouchSocketPro.Modbus']
  },
  
  // 实验性包
  TouchSocketPipelines: {
    namespace: 'TouchSocket.Pipelines',
    assembly: 'TouchSocket.Pipelines.dll',
    packageName: 'TouchSocket.Pipelines',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Pipelines'
  }
};

// 主Definition组件
const Definition = ({ 
  type,
  namespace, 
  assembly,
  packageName,
  version,
  nugetUrl,
  description
}) => {
  const [copied, setCopied] = useState(false);

  // 如果指定了type，使用预定义配置，但允许参数覆盖
  const config = type && TOUCHSOCKET_PACKAGES[type] 
    ? {
        ...TOUCHSOCKET_PACKAGES[type],
        // 允许参数覆盖预定义值
        ...(namespace && { namespace }),
        ...(assembly && { assembly }),
        ...(packageName && { packageName }),
        ...(version !== undefined && { version }),
        ...(nugetUrl && { nugetUrl }),
        ...(description && { description })
      }
    : {
        namespace: namespace || 'TouchSocket.Core',
        assembly: assembly || 'TouchSocket.Core.dll',
        packageName: packageName || 'TouchSocket.Core',
        version: version !== undefined ? version : undefined,
        nugetUrl: nugetUrl || 'https://www.nuget.org/packages/TouchSocket.Core'
      };

  const dotnetCommand = config.version 
    ? Array.isArray(config.packageName) 
      ? config.packageName.map(pkg => `dotnet add package ${pkg} --version ${config.version}`).join(' && ')
      : `dotnet add package ${config.packageName} --version ${config.version}`
    : Array.isArray(config.packageName)
      ? config.packageName.map(pkg => `dotnet add package ${pkg}`).join(' && ')
      : `dotnet add package ${config.packageName}`;

  const copyCommand = async () => {
    try {
      await navigator.clipboard.writeText(dotnetCommand);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      console.error('Failed to copy command:', err);
    }
  };

  return (
    <div className="definition-container">
      <h3 className="definition-title">定义</h3>
      <div className="definition-content">
        <div className="definition-item">
          <span className="definition-label">命名空间：</span>
          <div className="definition-values">
            {Array.isArray(config.namespace) ? (
              config.namespace.map((ns, index) => (
                <code key={index} className="definition-value namespace">{ns}</code>
              ))
            ) : (
              <code className="definition-value namespace">{config.namespace}</code>
            )}
          </div>
        </div>
        <div className="definition-item">
          <span className="definition-label">程序集：</span>
          <div className="definition-values">
            {Array.isArray(config.assembly) && Array.isArray(config.nugetUrl) ? (
              config.assembly.map((asm, index) => (
                <a 
                  key={index}
                  href={config.nugetUrl[index] || config.nugetUrl[0]} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  className="definition-value assembly-link"
                >
                  {asm}
                </a>
              ))
            ) : Array.isArray(config.assembly) ? (
              config.assembly.map((asm, index) => (
                <a 
                  key={index}
                  href={config.nugetUrl} 
                  target="_blank" 
                  rel="noopener noreferrer"
                  className="definition-value assembly-link"
                >
                  {asm}
                </a>
              ))
            ) : (
              <a 
                href={Array.isArray(config.nugetUrl) ? config.nugetUrl[0] : config.nugetUrl} 
                target="_blank" 
                rel="noopener noreferrer"
                className="definition-value assembly-link"
              >
                {config.assembly}
              </a>
            )}
          </div>
        </div>
        <div className="definition-item install-item">
          <span className="definition-label">安装：</span>
          <div className="install-command-container">
            <code className="install-command">{dotnetCommand}</code>
            <button 
              className={`copy-button ${copied ? 'copied' : ''}`}
              onClick={copyCommand}
              title="复制安装命令"
            >
              {copied ? (
                <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M9 16.17L4.83 12l-1.42 1.41L9 19 21 7l-1.41-1.41z"/>
                </svg>
              ) : (
                <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M16 1H4c-1.1 0-2 .9-2 2v14h2V3h12V1zm3 4H8c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h11c1.1 0 2-.9 2-2V7c0-1.1-.9-2-2-2zm0 16H8V7h11v14z"/>
                </svg>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

// 预定义组件导出
// 基础包
export const TouchSocketDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocket" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketCoreDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketCore" version={withVersion ? '3.1.12' : undefined} />;

// 通信协议包
export const TouchSocketDmtpDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketDmtp" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketHttpDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketHttp" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketNamedPipeDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketNamedPipe" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketSerialPortsDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketSerialPorts" version={withVersion ? '3.1.12' : undefined} />;

// RPC包
export const TouchSocketRpcDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketRpc" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketJsonRpcDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketJsonRpc" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketXmlRpcDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketXmlRpc" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketWebApiDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketWebApi" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketWebApiSwaggerDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketWebApiSwagger" version={withVersion ? '3.1.12' : undefined} />;

// 工业协议包
export const TouchSocketModbusDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketModbus" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketMqttDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketMqtt" version={withVersion ? '3.1.12' : undefined} />;

// 扩展包
export const TouchSocketAspNetCoreDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketAspNetCore" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketHostingDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketHosting" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketCoreDependencyInjectionDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketCoreDependencyInjection" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketCoreAutofacDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketCoreAutofac" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketRpcRateLimitingDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketRpcRateLimiting" version={withVersion ? '3.1.12' : undefined} />;

// 专业版包
export const TouchSocketProDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketPro" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketProDmtpDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketProDmtp" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketProAspNetCoreDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketProAspNetCore" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketProHostingDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketProHosting" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketProModbusDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketProModbus" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketProPlcBridgesDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketProPlcBridges" version={withVersion ? '3.1.12' : undefined} />;

// 实验性包
export const TouchSocketPipelinesDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketPipelines" version={withVersion ? '2.1.0-alpha.31' : undefined} />;

export default Definition;
