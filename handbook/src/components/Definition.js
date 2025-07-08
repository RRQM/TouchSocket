import React, { useState } from 'react';
import './Definition.css';

// 预定义的TouchSocket包配置
const TOUCHSOCKET_PACKAGES = {
  TouchSocketCore: {
    namespace: 'TouchSocket.Core',
    assembly: 'TouchSocket.Core.dll',
    packageName: 'TouchSocket.Core',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Core'
  },
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
  TouchSocketRpc: {
    namespace: ['TouchSocket.Rpc', 'TouchSocket.Rpc.JsonRpc'],
    assembly: ['TouchSocket.Rpc.dll', 'TouchSocket.Rpc.JsonRpc.dll'],
    packageName: 'TouchSocket.Rpc',
    nugetUrl: ['https://www.nuget.org/packages/TouchSocket.Rpc', 'https://www.nuget.org/packages/TouchSocket.Rpc']
  },
  TouchSocketMqtt: {
    namespace: 'TouchSocket.Mqtt',
    assembly: 'TouchSocket.Mqtt.dll',
    packageName: 'TouchSocket.Mqtt',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Mqtt'
  },
  TouchSocketModbus: {
    namespace: 'TouchSocket.Modbus',
    assembly: 'TouchSocket.Modbus.dll',
    packageName: 'TouchSocket.Modbus',
    nugetUrl: 'https://www.nuget.org/packages/TouchSocket.Modbus'
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
    ? `dotnet add package ${config.packageName} --version ${config.version}`
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
export const TouchSocketCoreDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketCore" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketDmtpDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketDmtp" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketHttpDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketHttp" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketRpcDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketRpc" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketMqttDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketMqtt" version={withVersion ? '3.1.12' : undefined} />;
export const TouchSocketModbusDefinition = ({ withVersion = false }) => 
  <Definition type="TouchSocketModbus" version={withVersion ? '3.1.12' : undefined} />;

export default Definition;
