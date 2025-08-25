import type { SidebarsConfig } from '@docusaurus/plugin-content-docs';

/**
 * Creating a sidebar enables you to:
 - create an ordered group of docs
 - render a sidebar for each doc of that group
 - provide next/previous navigation

 The sidebars can be generated from the filesystem, or explicitly defined here.

 Create as many sidebars as you want. 
 */

module.exports =
{
  "docs": [
    {
      "type": "doc",
      "id": "description",
      "label": "01、说明（使用前必要阅读）"
    },
    {
      "type": "category",
      "label": "02、支持作者及商业运营",
      "items": [
        {
          "type": "doc",
          "id": "donate",
          "label": "2.1 支持作者"
        },
        {
          "type": "doc",
          "id": "enterprise",
          "label": "2.2 Pro相关"
        },
        {},
        {
          "type": "category",
          "label": "2.3 使用者项目",
          "items": [
            {
              "type": "doc",
              "id": "thingsgateway",
              "label": "a.ThingsGateway"
            }
          ]
        }
      ]
    },
    {
      "type": "doc",
      "id": "startguide",
      "label": "03、入门指南"
    },
    {
      "type": "category",
      "label": "04、疑难解答",
      "items": [
        {
          "type": "doc",
          "id": "troubleshootsourcecode",
          "label": "4.1 源码相关"
        },
        {
          "type": "doc",
          "id": "troubleshootunity3d",
          "label": "4.2 Unity3D相关"
        },
        {
          "type": "doc",
          "id": "troubleshootissue",
          "label": "4.3 Issue解答"
        }
      ]
    },
    {
      "type": "category",
      "label": "05、Core",
      "items": [
        {
          "type": "doc",
          "id": "bytepool",
          "label": "5.1 内存池"
        },
        {
          "type": "doc",
          "id": "consoleaction",
          "label": "5.2 控制台行为"
        },
        {
          "type": "doc",
          "id": "touchsocketbitconverter",
          "label": "5.3 大小端转换器"
        },
        {
          "type": "doc",
          "id": "datasecurity",
          "label": "5.4 数据加密"
        },
        {
          "type": "doc",
          "id": "ilog",
          "label": "5.5 日志记录器"
        },
        {
          "type": "doc",
          "id": "appmessenger",
          "label": "5.6 应用信使"
        },
        {
          "type": "doc",
          "id": "fastbinaryformatter",
          "label": "5.7 高性能二进制序列化"
        },
        {
          "type": "doc",
          "id": "jsonserialize",
          "label": "5.8 Json序列化"
        },
        {
          "type": "doc",
          "id": "ioc",
          "label": "5.9 依赖注入容器(IOC)"
        },
        {
          "type": "doc",
          "id": "dependencyproperty",
          "label": "5.10 依赖属性"
        },
        {
          "type": "doc",
          "id": "filepool",
          "label": "5.11 文件流池"
        },
        {
          "type": "doc",
          "id": "pluginsmanager",
          "label": "5.12 插件系统"
        },
        {
          "type": "doc",
          "id": "ipackage",
          "label": "5.13 包序列化模式"
        },
        {
          "type": "doc",
          "id": "dynamicmethod",
          "label": "5.14 动态方法调用"
        },
        {
          "type": "doc",
          "id": "othercore",
          "label": "5.15 其他相关功能类"
        }
      ]
    },
    {
      "type": "category",
      "label": "06、Tcp组件",
      "items": [
        {
          "type": "doc",
          "id": "tcpintroduction",
          "label": "6.1 Tcp入门基础"
        },
        {
          "type": "doc",
          "id": "tcpservice",
          "label": "6.2 创建TcpService"
        },
        {
          "type": "doc",
          "id": "tcpclient",
          "label": "6.3 创建TcpClient"
        },
        {
          "type": "doc",
          "id": "natservice",
          "label": "6.4 Tcp端口转发"
        },
        {
          "type": "doc",
          "id": "resetid",
          "label": "6.5 服务器重置Id"
        },
        {
          "type": "doc",
          "id": "tcpcommandlineplugin",
          "label": "6.6 命令行执行插件"
        },
        {
          "type": "doc",
          "id": "tcpcommonplugins",
          "label": "6.7 其他常用插件"
        },
        {
          "type": "doc",
          "id": "tcpaot",
          "label": "6.8 AOT模式"
        }
      ]
    },
    {
      "type": "category",
      "label": "07、NamedPipe组件",
      "items": [
        {
          "type": "doc",
          "id": "namedpipedescription",
          "label": "7.1 命名管道描述"
        },
        {
          "type": "doc",
          "id": "namedpipeservice",
          "label": "7.2 创建NamedPipeService"
        },
        {
          "type": "doc",
          "id": "namedpipeclient",
          "label": "7.2 创建NamedPipeClient"
        }
      ]
    },
    {
      "type": "category",
      "label": "08、Udp组件",
      "items": [
        {
          "type": "doc",
          "id": "udpsession",
          "label": "8.1 创建UdpSession"
        },
        {
          "type": "doc",
          "id": "udpwaitingclient",
          "label": "8.2 同步请求数据"
        },
        {
          "type": "doc",
          "id": "udptransmitbigdata",
          "label": "8.3 传输大于64K的数据"
        },
        {
          "type": "doc",
          "id": "udpbroadcast",
          "label": "8.4 组播、广播"
        }
      ]
    },
    {
      "type": "doc",
      "id": "serialportclient",
      "label": "09、串口组件"
    },
    {
      "type": "doc",
      "id": "waitingclient",
      "label": "10、等待响应组件"
    },
    {
      "type": "category",
      "label": "11、数据处理适配器",
      "items": [
        {
          "type": "doc",
          "id": "adapterdescription",
          "label": "11.1 介绍及使用"
        },
        {
          "type": "doc",
          "id": "singlethreadstreamadapter",
          "label": "11.2 单线程流式适配器"
        },
        {
          "type": "category",
          "label": "11.3 多线程非流式适配器",
          "items": [
            {
              "type": "doc",
              "id": "udpdatahandlingadapter",
              "label": "a.原始自定义适配器"
            }
          ]
        },
        {
          "type": "category",
          "label": "11.4 适配器案例赏析",
          "items": [
            {
              "type": "doc",
              "id": "adapterdemodescription",
              "label": "a.说明"
            },
            {
              "type": "doc",
              "id": "stategridtransmission",
              "label": "b.国网输电i1标准版"
            },
            {
              "type": "doc",
              "id": "adaptersiemenss7",
              "label": "d.西门子S7"
            }
          ]
        },
        {
          "type": "doc",
          "id": "independentusedatahandlingadapter",
          "label": "11.5 独立使用适配器"
        },
        {
          "type": "doc",
          "id": "adaptererrorcorrection",
          "label": "11.6 适配器纠错"
        },
        {
          "type": "doc",
          "id": "dataadaptertester",
          "label": "11.7 适配器完整性、性能测试"
        },
        {
          "type": "doc",
          "id": "adapterbuilder",
          "label": "11.8 适配器消息构建器"
        }
      ]
    },
    {
      "type": "category",
      "label": "12、Http组件",
      "items": [
        {
          "type": "doc",
          "id": "httpservice",
          "label": "12.1 创建HttpService"
        },
        {
          "type": "doc",
          "id": "httpclient",
          "label": "12.2 创建HttpClient"
        },
        {
          "type": "doc",
          "id": "httpstaticpageplugin",
          "label": "12.3 静态页面插件"
        },
        {
          "type": "doc",
          "id": "cors",
          "label": "12.4 Cors跨域"
        }
      ]
    },
    {
      "type": "category",
      "label": "13、WebSocket组件",
      "items": [
        {
          "type": "doc",
          "id": "websocketdescription",
          "label": "13.1 产品及架构介绍"
        },
        {
          "type": "doc",
          "id": "websocketservice",
          "label": "13.2 创建WebSocket服务器"
        },
        {
          "type": "doc",
          "id": "websocketclient",
          "label": "13.3 创建WebSocket客户端"
        },
        {
          "type": "doc",
          "id": "websocketheartbeat",
          "label": "13.4 心跳设置"
        },
        {
          "type": "doc",
          "id": "wscommandlineplugin",
          "label": "13.5 快捷事务命令行"
        }
      ]
    },
    {
      "type": "category",
      "label": "14、Rpc组件",
      "items": [
        {
          "type": "doc",
          "id": "rpcdescription",
          "label": "14.1 Rpc描述"
        },
        {
          "type": "doc",
          "id": "rpcregister",
          "label": "14.2 注册服务"
        },
        {
          "type": "doc",
          "id": "rpcgenerateproxy",
          "label": "14.3 生成调用代理"
        },
        {
          "type": "doc",
          "id": "generateproxysourcegeneratordemo",
          "label": "14.4 源生成代理推荐写法"
        },
        {
          "type": "doc",
          "id": "rpcactionfilter",
          "label": "14.5 Rpc服务AOP"
        },
        {
          "type": "doc",
          "id": "rpcallcontext",
          "label": "14.6 调用上下文"
        },
        {
          "type": "doc",
          "id": "rpcratelimiting",
          "label": "14.7 Rpc访问速率限制"
        },
        {
          "type": "doc",
          "id": "rpcdispatcher",
          "label": "14.8 Rpc执行调度器"
        },
        {
          "type": "doc",
          "id": "rpcauthorization",
          "label": "14.9 Rpc鉴权授权策略"
        }
      ]
    },
    {
      "type": "category",
      "label": "15、Dmtp组件",
      "items": [
        {
          "type": "doc",
          "id": "dmtpdescription",
          "label": "15.1 产品及架构介绍"
        },
        {
          "type": "doc",
          "id": "dmtpservice",
          "label": "15.2 创建Dmtp服务器"
        },
        {
          "type": "doc",
          "id": "dmtplient",
          "label": "15.3 创建Dmtp客户端"
        },
        {
          "type": "doc",
          "label": "15.4 基础功能",
          "id": "dmtpbase"
        },
        {
          "type": "category",
          "label": "15.5 进阶功能",
          "items": [
            {
              "type": "doc",
              "label": "a.自定义DmtpActor",
              "id": "dmtpcustomactor"
            }
          ]
        },
        {
          "type": "doc",
          "label": "15.6 Rpc功能",
          "id": "dmtprpc"
        },
        {
          "type": "doc",
          "label": "15.7 文件传输",
          "id": "dmtptransferfile"
        },
        {
          "type": "doc",
          "id": "dmtpremoteaccess",
          "label": "15.8 远程文件系统"
        },
        {
          "type": "doc",
          "id": "dmtpremotestream",
          "label": "15.9 远程流映射"
        },
        {
          "type": "doc",
          "id": "dmtprouterpackage",
          "label": "15.10 路由包传输"
        },
        {
          "type": "doc",
          "id": "dmtpredis",
          "label": "15.11 Redis缓存"
        }
      ]
    },
    {
      "type": "category",
      "label": "16、WebApi组件",
      "items": [
        {
          "type": "doc",
          "label": "16.1 WebApi",
          "id": "webapi"
        },
        {
          "type": "doc",
          "label": "16.2 Swagger页面",
          "id": "swagger"
        }
      ]
    },
    {
      "type": "doc",
      "label": "17、JsonRpc组件",
      "id": "jsonrpc"
    },
    {
      "type": "doc",
      "label": "18、XmlRpc组件",
      "id": "xmlrpc"
    },
    {
      "type": "category",
      "label": "19、Modbus组件",
      "items": [
        {
          "type": "doc",
          "label": "19.1 Modbus主站",
          "id": "modbusmaster"
        },
        {
          "type": "doc",
          "label": "19.2 Modbus从站",
          "id": "modbusslave"
        }
      ]
    },
    {
      "type": "doc",
      "label": "20、通用主机(Hosting)",
      "id": "generichost"
    },
    {
      "type": "category",
      "label": "21、Mqtt组件",
      "items": [
        {
          "type": "doc",
          "label": "21.1 Mqtt服务端",
          "id": "mqttservice"
        },
        {
          "type": "doc",
          "label": "21.2 Mqtt客户端",
          "id": "mqttclient"
        }
      ]
    },
    {
      "type": "category",
      "label": "22、PlcBridge组件",
      "items": [
        {
          "type": "doc",
          "label": "22.1 PlcBridge说明",
          "id": "plcbridgedescription"
        },
        {
          "type": "doc",
          "label": "22.2 PlcBridge服务",
          "id": "plcbridgeservice"
        },
        {
          "type": "doc",
          "label": "22.3 PlcBridgeModbus",
          "id": "plcbridgemodbus"
        }
      ]
    }
  ]
}
  ;
