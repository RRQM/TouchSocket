"use strict";(self.webpackChunktouchsocket=self.webpackChunktouchsocket||[]).push([[9769],{3905:(e,t,n)=>{n.d(t,{Zo:()=>u,kt:()=>k});var r=n(7294);function a(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function l(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,r)}return n}function c(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?l(Object(n),!0).forEach((function(t){a(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):l(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function o(e,t){if(null==e)return{};var n,r,a=function(e,t){if(null==e)return{};var n,r,a={},l=Object.keys(e);for(r=0;r<l.length;r++)n=l[r],t.indexOf(n)>=0||(a[n]=e[n]);return a}(e,t);if(Object.getOwnPropertySymbols){var l=Object.getOwnPropertySymbols(e);for(r=0;r<l.length;r++)n=l[r],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(a[n]=e[n])}return a}var p=r.createContext({}),i=function(e){var t=r.useContext(p),n=t;return e&&(n="function"==typeof e?e(t):c(c({},t),e)),n},u=function(e){var t=i(e.components);return r.createElement(p.Provider,{value:t},e.children)},d={inlineCode:"code",wrapper:function(e){var t=e.children;return r.createElement(r.Fragment,{},t)}},s=r.forwardRef((function(e,t){var n=e.components,a=e.mdxType,l=e.originalType,p=e.parentName,u=o(e,["components","mdxType","originalType","parentName"]),s=i(n),k=a,m=s["".concat(p,".").concat(k)]||s[k]||d[k]||l;return n?r.createElement(m,c(c({ref:t},u),{},{components:n})):r.createElement(m,c({ref:t},u))}));function k(e,t){var n=arguments,a=t&&t.mdxType;if("string"==typeof e||a){var l=n.length,c=new Array(l);c[0]=s;var o={};for(var p in t)hasOwnProperty.call(t,p)&&(o[p]=t[p]);o.originalType=e,o.mdxType="string"==typeof e?e:a,c[1]=o;for(var i=2;i<l;i++)c[i]=n[i];return r.createElement.apply(null,c)}return r.createElement.apply(null,n)}s.displayName="MDXCreateElement"},9882:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>p,contentTitle:()=>c,default:()=>d,frontMatter:()=>l,metadata:()=>o,toc:()=>i});var r=n(7462),a=(n(7294),n(3905));const l={id:"createtouchrpcclient",title:"\u521b\u5efaTouchRpc\u5ba2\u6237\u7aef"},c=void 0,o={unversionedId:"createtouchrpcclient",id:"createtouchrpcclient",title:"\u521b\u5efaTouchRpc\u5ba2\u6237\u7aef",description:"\u4e00\u3001\u8bf4\u660e",source:"@site/docs/createtouchrpcclient.mdx",sourceDirName:".",slug:"/createtouchrpcclient",permalink:"/touchsocket/docs/createtouchrpcclient",draft:!1,editUrl:"https://gitee.com/rrqm_home/touchsocket/tree/master/handbook/docs/createtouchrpcclient.mdx",tags:[],version:"current",lastUpdatedBy:"\u82e5\u6c5d\u68cb\u8317",lastUpdatedAt:1675660193,formattedLastUpdatedAt:"Feb 6, 2023",frontMatter:{id:"createtouchrpcclient",title:"\u521b\u5efaTouchRpc\u5ba2\u6237\u7aef"},sidebar:"docs",previous:{title:"\u521b\u5efaTouchRpc\u670d\u52a1\u5668",permalink:"/touchsocket/docs/createtouchrpcservice"},next:{title:"\u57fa\u7840\u529f\u80fd",permalink:"/touchsocket/docs/touchrpcbase"}},p={},i=[{value:"\u4e00\u3001\u8bf4\u660e",id:"\u4e00\u8bf4\u660e",level:2},{value:"\u4e8c\u3001\u53ef\u914d\u7f6e\u9879",id:"\u4e8c\u53ef\u914d\u7f6e\u9879",level:2},{value:"SetVerifyTimeout",id:"setverifytimeout",level:4},{value:"SetVerifyToken",id:"setverifytoken",level:4},{value:"SetHeartbeatFrequency",id:"setheartbeatfrequency",level:4},{value:"SetSerializationSelector",id:"setserializationselector",level:4},{value:"SetResponseType",id:"setresponsetype",level:4},{value:"SetRootPath",id:"setrootpath",level:4},{value:"\u4e09\u3001\u652f\u6301\u63d2\u4ef6\u63a5\u53e3",id:"\u4e09\u652f\u6301\u63d2\u4ef6\u63a5\u53e3",level:2},{value:"\u56db\u3001\u521b\u5efa",id:"\u56db\u521b\u5efa",level:2},{value:"4.1 TcpTouchRpcClient",id:"41-tcptouchrpcclient",level:3},{value:"4.2 HttpTouchRpcClient",id:"42-httptouchrpcclient",level:3},{value:"4.3 UdpTouchRpc",id:"43-udptouchrpc",level:3},{value:"4.4 WSTouchRpcClient",id:"44-wstouchrpcclient",level:3}],u={toc:i};function d(e){let{components:t,...n}=e;return(0,a.kt)("wrapper",(0,r.Z)({},u,n,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("h2",{id:"\u4e00\u8bf4\u660e"},"\u4e00\u3001\u8bf4\u660e"),(0,a.kt)("p",null,"TouchRpc\u5ba2\u6237\u7aef\u5bf9\u5e94\u7684\uff0c\u4e5f\u6709\u56db\u79cd\u4e0d\u540c\u534f\u8bae\u7684\u7248\u672c\u3002"),(0,a.kt)("h2",{id:"\u4e8c\u53ef\u914d\u7f6e\u9879"},"\u4e8c\u3001\u53ef\u914d\u7f6e\u9879"),(0,a.kt)("details",null,(0,a.kt)("summary",null,"\u53ef\u914d\u7f6e\u9879"),(0,a.kt)("div",null,(0,a.kt)("h4",{id:"setverifytimeout"},"SetVerifyTimeout"),(0,a.kt)("p",null,"\u8bbe\u7f6e\u9a8c\u8bc1\u8d85\u65f6\u65f6\u95f4\uff0c\u9ed8\u8ba43000ms\u3002\uff08\u4ec5TcpTouchRpc\u53ef\u7528\uff09 \u3002"),(0,a.kt)("h4",{id:"setverifytoken"},"SetVerifyToken"),(0,a.kt)("p",null,"\u8bbe\u7f6e\u9a8c\u8bc1\u53e3\u4ee4\u3002 "),(0,a.kt)("h4",{id:"setheartbeatfrequency"},"SetHeartbeatFrequency"),(0,a.kt)("p",null,"\u8bbe\u7f6e\u5fc3\u8df3\u3002\u9ed8\u8ba4\u4e3a\u95f4\u96942000ms\uff0c\u8fde\u7eed3\u6b21\u65e0\u54cd\u5e94\u5373\u89c6\u4e3a\u65ad\u5f00\u3002"),(0,a.kt)("h4",{id:"setserializationselector"},"SetSerializationSelector"),(0,a.kt)("p",null,"\u8bbe\u7f6e\u5e8f\u5217\u5316\u9009\u62e9\u5668\u3002"),(0,a.kt)("h4",{id:"setresponsetype"},"SetResponseType"),(0,a.kt)("p",null,"\u8bbe\u7f6e\u5141\u8bb8\u7684\u54cd\u5e94\u7c7b\u578b"),(0,a.kt)("h4",{id:"setrootpath"},"SetRootPath"),(0,a.kt)("p",null,"\u8bbe\u7f6e\u6839\u8def\u5f84"))),(0,a.kt)("h2",{id:"\u4e09\u652f\u6301\u63d2\u4ef6\u63a5\u53e3"},"\u4e09\u3001\u652f\u6301\u63d2\u4ef6\u63a5\u53e3"),(0,a.kt)("p",null,"\u58f0\u660e\u81ea\u5b9a\u4e49\u5b9e\u4f8b\u7c7b\uff0c\u7136\u540e\u5b9e\u73b0",(0,a.kt)("strong",{parentName:"p"},"ITouchRpcPlugin"),"\u63a5\u53e3\uff0c\u5373\u53ef\u5b9e\u73b0\u4e0b\u5217\u4e8b\u52a1\u7684\u89e6\u53d1\u3002\n\u6216\u8005\u7ee7\u627f\u81ea",(0,a.kt)("strong",{parentName:"p"},"TouchRpcPluginBase"),"\u7c7b\uff0c\u91cd\u5199\u76f8\u5e94\u65b9\u6cd5\u5373\u53ef\u3002"),(0,a.kt)("table",null,(0,a.kt)("thead",{parentName:"table"},(0,a.kt)("tr",{parentName:"thead"},(0,a.kt)("th",{parentName:"tr",align:null},"\u63d2\u4ef6\u65b9\u6cd5"),(0,a.kt)("th",{parentName:"tr",align:null},"\u529f\u80fd"))),(0,a.kt)("tbody",{parentName:"table"},(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnHandshaking"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5ba2\u6237\u7aef\u5728\u9a8c\u8bc1\u8fde\u63a5\u3002\u9ed8\u8ba4\u60c5\u51b5\u4e0b\uff0c\u6846\u67b6\u4f1a\u9996\u5148\u9a8c\u8bc1\u8fde\u63a5Token\u662f\u5426\u6b63\u786e\uff0c\u5982\u679c\u4e0d\u6b63\u786e\u5219\u76f4\u63a5\u62d2\u7edd\u3002\u4e0d\u4f1a\u6709\u4efb\u4f55\u6295\u9012\u3002\u7528\u6237\u4e5f\u53ef\u4ee5\u4f7f\u7528Metadata\u8fdb\u884c\u52a8\u6001\u9a8c\u8bc1\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnHandshaked"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5ba2\u6237\u7aef\u5b8c\u6210\u8fde\u63a5\u9a8c\u8bc1")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnFileTransfering"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5728\u6587\u4ef6\u4f20\u8f93\u5373\u5c06\u8fdb\u884c\u65f6\u89e6\u53d1\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnFileTransfered"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5f53\u6587\u4ef6\u4f20\u8f93\u7ed3\u675f\u4e4b\u540e\u3002\u5e76\u4e0d\u610f\u5473\u7740\u5b8c\u6210\u4f20\u8f93\uff0c\u8bf7\u901a\u8fc7e.Result\u5c5e\u6027\u503c\u8fdb\u884c\u5224\u65ad\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnLoadingStream"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5728\u8fdc\u7a0b\u8bf7\u6c42\u52a0\u8f7d\u6d41\u65f6\u89e6\u53d1\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnReceivedProtocolData"),(0,a.kt)("td",{parentName:"tr",align:null},"\u6536\u5230\u534f\u8bae\u6570\u636e")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnRemoteAccessing"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5728\u8fdc\u7a0b\u64cd\u4f5c\u8bbf\u95ee\u4e4b\u524d\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnRemoteAccessed"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5728\u8fdc\u7a0b\u64cd\u4f5c\u8bbf\u95ee\u4e4b\u540e\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnRouting"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5f53\u9700\u8981\u8f6c\u53d1\u8def\u7531\u5305\u65f6\u3002\u4e00\u822c\u6240\u6709\u7684",(0,a.kt)("strong",{parentName:"td"},"\u5ba2\u6237\u7aef\u4e4b\u95f4"),"\u7684\u6570\u636e\u4f20\u8f93\uff0c\u90fd\u9700\u8981\u7ecf\u8fc7\u8be5\u51fd\u6570\u7684\u8fd0\u884c\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnStreamTransfering"),(0,a.kt)("td",{parentName:"tr",align:null},"\u5373\u5c06\u63a5\u6536\u6d41\u6570\u636e\uff0c\u7528\u6237\u9700\u8981\u5728\u6b64\u4e8b\u4ef6\u4e2d\u5bf9e.Bucket\u521d\u59cb\u5316\u3002")),(0,a.kt)("tr",{parentName:"tbody"},(0,a.kt)("td",{parentName:"tr",align:null},"OnStreamTransfered"),(0,a.kt)("td",{parentName:"tr",align:null},"\u6d41\u6570\u636e\u5904\u7406\uff0c\u7528\u6237\u9700\u8981\u5728\u6b64\u4e8b\u4ef6\u4e2d\u5bf9e.Bucket\u624b\u52a8\u91ca\u653e\u3002 \u5f53\u6d41\u6570\u636e\u4f20\u8f93\u7ed3\u675f\u4e4b\u540e\u3002\u5e76\u4e0d\u610f\u5473\u7740\u5b8c\u6210\u4f20\u8f93\uff0c\u8bf7\u901a\u8fc7e.Result\u5c5e\u6027\u503c\u8fdb\u884c\u5224\u65ad\u3002")))),(0,a.kt)("h2",{id:"\u56db\u521b\u5efa"},"\u56db\u3001\u521b\u5efa"),(0,a.kt)("h3",{id:"41-tcptouchrpcclient"},"4.1 TcpTouchRpcClient"),(0,a.kt)("p",null,"\u57fa\u672c\u521b\u5efa\u5982\u4e0b\uff0c\u652f\u6301",(0,a.kt)("a",{parentName:"p",href:"/touchsocket/docs/createtcpclient"},"\u521b\u5efaTcpClient"),"\u7684\u6240\u6709\u914d\u7f6e\u3002"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-csharp"},'TcpTouchRpcClient client = new TcpTouchRpcClient();\nclient.Setup(new TouchSocketConfig()\n    .SetRemoteIPHost("127.0.0.1:7789")\n    .SetVerifyToken("TouchRpc"));\nclient.Connect();\n')),(0,a.kt)("h3",{id:"42-httptouchrpcclient"},"4.2 HttpTouchRpcClient"),(0,a.kt)("p",null,"\u57fa\u672c\u521b\u5efa\u5982\u4e0b\uff0c\u652f\u6301",(0,a.kt)("a",{parentName:"p",href:"/touchsocket/docs/createtcpclient"},"\u521b\u5efaTcpClient"),"\u7684\u6240\u6709\u914d\u7f6e\u3002"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-csharp"},'HttpTouchRpcClient client = new HttpTouchRpcClient();\nclient.Setup(new TouchSocketConfig()\n   .SetRemoteIPHost("127.0.0.1:7789")\n   .SetVerifyToken("TouchRpc"));\nclient.Connect();\n')),(0,a.kt)("h3",{id:"43-udptouchrpc"},"4.3 UdpTouchRpc"),(0,a.kt)("p",null,"\u57fa\u672c\u521b\u5efa\u5982\u4e0b\uff0c\u652f\u6301@\u7684\u6240\u6709\u914d\u7f6e\u3002"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-csharp"},'UdpTouchRpc client = new UdpTouchRpc();\nclient.Setup(new TouchSocketConfig()\n    .SetBindIPHost(7794)\n    .SetRemoteIPHost("127.0.0.1:7789"));//\u8bbe\u7f6e\u76ee\u6807\u5730\u5740\u3002\nclient.Start();\n')),(0,a.kt)("h3",{id:"44-wstouchrpcclient"},"4.4 WSTouchRpcClient"),(0,a.kt)("pre",null,(0,a.kt)("code",{parentName:"pre",className:"language-csharp"},' WSTouchRpcClient client = new WSTouchRpcClient();\n     client.Setup(new TouchSocketConfig()\n         .SetRemoteIPHost("ws://127.0.0.1:5000/wstouchrpc"));\n     client.ConnectAsync();\n')))}d.isMDXComponent=!0}}]);