// /**
//  * Creating a sidebar enables you to:
//  - create an ordered group of docs
//  - render a sidebar for each doc of that group
//  - provide next/previous navigation

//  The sidebars can be generated from the filesystem, or explicitly defined here.

//  Create as many sidebars as you want.
//  */

// // @ts-check

// /** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
// const sidebars = {

//   // But you can create a sidebar manually
  
//   tutorialSidebar: [
//     'intro',
//     'hello',
//     {
//       type: 'category',
//       label: 'Tutorial',
//       items: ['tutorial-basics/create-a-document'],
//     },
//   ],
   
// };

// module.exports = sidebars;


module.exports = {
  docs: [
    {
      type: "doc",
      id: "description",
      label: "01、说明（使用前必要阅读）"
    },
    {
      type: "doc",
      id: "upgrade",
      label: "02、历史更新"
    },
    {
      type: "category",
      label: "03、支持作者及商业运营",
      items: [
        {
          type: "doc",
          id: "donate",
          label: "3.1 支持作者"
        },
        {
          type: "doc",
          id: "enterprise",
          label: "3.2 企业版相关"
        },
        {
          // type: "category",
          // label: "3.3 商业项目",
          // items: [
          //   {
          //     type: "doc",
          //     id: "cooperation",
          //     label: "a.商业合作"
          //   },
          //   {
          //     type: "doc",
          //     id: "wpfuifiletransfer",
          //     label: "b.WPF界面、文件传输项目"
          //   },
          //   {
          //     type: "doc",
          //     id: "remotemonitoring",
          //     label: "c.远程监测、控制项目"
          //   },
          //   {
          //     type: "doc",
          //     id: "filesynchronization",
          //     label: "d.文件同步系统"
          //   },
          //   {
          //     type: "doc",
          //     id: "dataforwarding",
          //     label: "e.数据转发项目"
          //   },
          //   {
          //     type: "doc",
          //     id: "webdataforwarding",
          //     label: "f.Web数据转发Winform项目"
          //   },
          //],
        },
        {
          type: "category",
          label: "3.3 使用者项目",
          items: [
            {
              type: "doc",
              id: "fpsgame",
              label: "a.FPS实时游戏"
            },
            {
              type: "doc",
              id: "engineertoolbox",
              label: "b.工程师软件工具箱"
            },
            {
              type: "doc",
              id: "thingsgateway",
              label: "c.ThingsGateway"
            }
          ],
        }
      ],
    },
    {
      type: "doc",
      id: "startguide",
      label: "04、入门指南",
    },
    {
      type: "category",
      label: "05、疑难解答",
      items:
        [
          {
            type: "doc",
            id: "troubleshootsourcecode",
            label: "5.1 源码相关",
          },
          {
            type: "doc",
            id: "troubleshootunity3d",
            label: "5.2 Unity3D相关",
          },
        ]
    },
    {
      type: "category",
      label: "06、Core",
      items:
        [
          {
            type: "doc",
            id: "bytepool",
            label: "6.1 内存池"
          },
          {
            type: "doc",
            id: "consoleaction",
            label: "6.2 控制台行为"
          },
          {
            type: "doc",
            id: "touchsocketbitconverter",
            label: "6.3 大小端转换器"
          },
          {
            type: "doc",
            id: "datasecurity",
            label: "6.4 数据加密"
          },
          {
            type: "doc",
            id: "ilog",
            label: "6.5 日志记录器"
          },
          {
            type: "doc",
            id: "appmessenger",
            label: "6.6 应用信使"
          },
          {
            type: "doc",
            id: "fastbinaryformatter",
            label: "6.7 高性能二进制序列化"
          },
          {
            type: "doc",
            id: "jsonserialize",
            label: "6.8 Json序列化"
          },
          {
            type: "doc",
            id: "ioc",
            label: "6.9 依赖注入容器(IOC)"
          },
          {
            type: "doc",
            id: "dependencyproperty",
            label: "6.10 依赖属性"
          },
          {
            type: "doc",
            id: "filepool",
            label: "6.11 文件流池"
          },
          {
            type: "doc",
            id: "pluginsmanager",
            label: "6.12 插件系统"
          },
          {
            type: "doc",
            id: "ipackage",
            label: "6.13 包序列化模式"
          },
          {
            type: "doc",
            id: "othercore",
            label: "6.14 其他相关功能类"
          },
        ]
    },
    {
      type: "category",
      label: "07、Tcp组件",
      items:
        [
          {
            type: "doc",
            id: "tcpintroduction",
            label: "7.1 Tcp入门基础"
          },
          {
            type: "doc",
            id: "tcpservice",
            label: "7.2 创建TcpService"
          },
          {
            type: "doc",
            id: "tcpclient",
            label: "7.3 创建TcpClient"
          },
          {
            type: "doc",
            id: "tcpwaitingclient",
            label: "7.4 同步请求"
          },
          {
            type: "doc",
            id: "natservice",
            label: "7.5 Tcp端口转发"
          },
          {
            type: "doc",
            id: "resetid",
            label: "7.6 服务器重置Id"
          },
          {
            type: "doc",
            id: "reconnection",
            label: "7.7 断线重连"
          },
          {
            type: "doc",
            id: "tcpcommandlineplugin",
            label: "7.8 命令行执行插件"
          },
          {
            type: "doc",
            id: "tcpaot",
            label: "7.9 AOT模式"
          },
        ]
    },
    {
      type: "category",
      label: "08、NamedPipe组件",
      items:
        [
          {
            type: "doc",
            id: "namedpipedescription",
            label: "8.1 命名管道描述"
          },
          {
            type: "doc",
            id: "namedpipeservice",
            label: "8.2 创建NamedPipeService"
          },
          {
            type: "doc",
            id: "namedpipeclient",
            label: "8.2 创建NamedPipeClient"
          },
        ]
    },
    {
      type: "category",
      label: "09、Udp组件",
      items:
        [
          {
            type: "doc",
            id: "udpsession",
            label: "9.1 创建UdpSession"
          },
          {
            type: "doc",
            id: "udpwaitingclient",
            label: "9.2 同步请求数据"
          },
          {
            type: "doc",
            id: "udptransmitbigdata",
            label: "9.3 传输大于64K的数据"
          },
          {
            type: "doc",
            id: "udpbroadcast",
            label: "9.4 组播、广播"
          },
        ]
    },
    {
      type: "category",
      label: "10、数据处理适配器",
      items:
        [
          {
            type: "doc",
            id: "adapterdescription",
            label: "10.1 介绍及使用"
          },
          {
            type: "category",
            label: "10.2 Tcp适配器",
            items:
              [
                {
                  type: "doc",
                  id: "datahandleadapter",
                  label: "a.原始适配器"
                },
                {
                  type: "doc",
                  id: "packageadapter",
                  label: "b.内置包适配器"
                },
                {
                  type: "doc",
                  id: "customdatahandlingadapter",
                  label: "c.用户自定义适配器"
                },
                {
                  type: "doc",
                  id: "customfixedheaderdatahandlingadapter",
                  label: "d.模板解析固定包头适配器"
                },
                {
                  type: "doc",
                  id: "customunfixedheaderdatahandlingadapter",
                  label: "e.模板解析非固定包头适配器"
                },
                {
                  type: "doc",
                  id: "bigfixedheadercustomdatahandlingadapter",
                  label: "f.模板解析大数据固定包头适配器"
                },
                {
                  type: "doc",
                  id: "custombetweenanddatahandlingadapter",
                  label: "g.模板解析区间数据适配器"
                },
                {
                  type: "doc",
                  id: "pipelinedatahandlingadapter",
                  label: "h.Pipeline数据适配器"
                },
                {
                  type: "doc",
                  id: "tlvdatahandlingadapter",
                  label: "i.三元组编码TLV适配器"
                },
              ]
          },
          {
            type: "category",
            label: "10.3 Udp适配器",
            items:
              [
                {
                  type: "doc",
                  id: "udpdatahandlingadapter",
                  label: "a.原始自定义适配器"
                },
              ]
          },
          {
            type: "category",
            label: "10.4 适配器案例赏析",
            items:
              [
                {
                  type: "doc",
                  id: "adapterdemodescription",
                  label: "a.说明"
                },
                {
                  type: "doc",
                  id: "stategridtransmission",
                  label: "b.国网输电i1标准版"
                },
                {
                  type: "doc",
                  id: "adaptermodbus",
                  label: "c.Modbus系列"
                },
                {
                  type: "doc",
                  id: "adaptersiemenss7",
                  label: "d.西门子S7"
                },
              ]
          },
          {
            type: "doc",
            id: "independentusedatahandlingadapter",
            label: "10.5 独立使用适配器"
          },
          {
            type: "doc",
            id: "dataadaptertester",
            label: "10.6 适配器完整性、性能测试"
          },
        ]
    },
    {
      type: "category",
      label: "11、Http组件",
      items:
        [
          {
            type: "doc",
            id: "httpservice",
            label: "11.1 创建HttpService"
          },
          {
            type: "doc",
            id: "httpclient",
            label: "11.2 创建HttpClient"
          },
          {
            type: "doc",
            id: "httpstaticpageplugin",
            label: "11.3 静态页面插件"
          }
        ]
    },
    {
      type: "category",
      label: "12、WebSocket组件",
      items:
        [
          {
            type: "doc",
            id: "websocketdescription",
            label: "12.1 产品及架构介绍"
          },
          {
            type: "doc",
            id: "websocketservice",
            label: "12.2 创建WebSocket服务器"
          },
          {
            type: "doc",
            id: "websocketclient",
            label: "12.3 创建WebSocket客户端"
          },
          {
            type: "doc",
            id: "websocketheartbeat",
            label: "12.4 心跳设置"
          },
          {
            type: "doc",
            id: "wscommandlineplugin",
            label: "12.5 快捷事务命令行"
          }
        ]
    },
    {
      type: "category",
      label: "13、Rpc组件",
      items:
        [
          {
            type: "doc",
            id: "generateproxydescription",
            label: "13.1 为什么要生成代理"
          },
          {
            type: "doc",
            id: "generateproxyfromserver",
            label: "13.2 从服务端生成代理"
          },
          {
            type: "doc",
            id: "generateproxyfromsourcegenerator",
            label: "13.3 从SourceGenerator获取代理"
          },
          {
            type: "doc",
            id: "generateproxyfromdispatchproxy",
            label: "13.4 从DispatchProxy获取代理"
          },
          {
            type: "doc",
            id: "generateproxysourcegeneratordemo",
            label: "13.5 SG代理推荐写法"
          },
          {
            type: "doc",
            id: "rpcactionfilter",
            label: "13.6 Rpc服务AOP"
          }
        ]
    },
    {
      type: "category",
      label: "14、Dmtp组件",
      items:
        [
          {
            type: "doc",
            id: "dmtpdescription",
            label: "14.1 产品及架构介绍"
          },
          {
            type: "doc",
            id: "dmtpservice",
            label: "14.2 创建Dmtp服务器"
          },
          {
            type: "doc",
            id: "dmtplient",
            label: "14.3 创建Dmtp客户端"
          },
          {
            type: "doc",
            label: "14.4 基础功能",
            id: "dmtpbase"
          },
          {
            type: "category",
            label: "14.5 进阶功能",
            items: [
              {
                type: "doc",
                label: "a.自定义DmtpActor",
                id: "dmtpcustomactor"
              },
            ]
          },
          {
            type: "doc",
            label: "14.6 Rpc功能",
            id: "dmtprpc"
          },
          {
            type: "doc",
            label: "14.7 文件传输",
            id: "dmtptransferfile"
          },
          {
            type: "doc",
            id: "remotefilecontrol",
            label: "14.8 远程文件操作"
          },
          {
            type: "doc",
            id: "dmtpremotestream",
            label: "14.9 远程流映射"
          },
          {
            type: "doc",
            id: "dmtprouterpackage",
            label: "14.10 路由包传输"
          },
          {
            type: "doc",
            id: "dmtpredis",
            label: "14.11 Redis缓存"
          }
        ]
    },
    {
      type: "category",
      label: "15、WebApi组件",
      items:
        [
          {
            type: "doc",
            label: "15.1 WebApi",
            id: "webapi"
          },
          {
            type: "doc",
            label: "15.2 Swagger页面",
            id: "swagger"
          },
        ]
    },

    {
      type: "doc",
      label: "16、JsonRpc组件",
      id: "jsonrpc"
    },
    {
      type: "doc",
      label: "17、XmlRpc组件",
      id: "xmlrpc"
    }
  ]
};

