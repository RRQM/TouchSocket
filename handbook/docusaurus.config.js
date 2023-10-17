
module.exports = {
  title: "TouchSocket",
  tagline: "网络开发",
  url: "https://rrqm_home.gitee.io",
  baseUrl: "/touchsocket/",
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",
  favicon: "img/favicon.ico",
  projectName: "TouchSocket",
  scripts: [],
  themeConfig: {
    zoom: {
      selector:
        ".markdown :not(em) > img,.markdown > img, article img[loading]",
      background: {
        light: "rgb(255, 255, 255)",
        dark: "rgb(50, 50, 50)",
      },
      // options you can specify via https://github.com/francoischalifour/medium-zoom#usage
      config: {},
    },
    docs: {
      sidebar: {
        hideable: true,
        autoCollapseCategories: true,
      },
    },
    prism: {
      additionalLanguages: ["powershell", "csharp", "sql"],
    },
    navbar: {
      title: "TouchSocket",
      logo: {
        alt: "TouchSocket Logo",
        src: "img/TouchSocketlogo.png",
      },
      hideOnScroll: true,
      items: [
        {
          label: "更新日志",
          position: "left",
          to: "docs/current/upgrade"
        },
        {
          label: "版本",
          type: 'docsVersionDropdown',
          position: 'right',
          dropdownItemsAfter: [],
          dropdownActiveClassDisabled: true,
        },
        {
          label: "源码",
          position: "right",
          items: [
            {
              label: "Gitee（主库）",
              href: "https://gitee.com/rrqm_home/touchsocket",
            },
            {
              label: "GitHub",
              href: "https://github.com/RRQM/TouchSocket",
            },
            {
              label: "Nuget",
              href: "https://www.nuget.org/profiles/rrqm",
            },
          ],
        },
        {
          label: "社区",
          position: "right",
          href: "https://gitee.com/dotnetchina",
        },
      ],
    },
    footer: {
      style: "dark",
      links: [
        {
          title: "文档",
          items: [
            {
              label: "入门",
              to: "docs/current",
            },
            {
              label: "手册",
              to: "docs/current",
            },
          ],
        },
        {
          title: "社区",
          items: [
            {
              label: "讨论",
              href: "https://gitee.com/rrqm_home/touchsocket/issues",
            },
            {
              label: "看板",
              href: "https://gitee.com/rrqm_home/touchsocket/board",
            },
          ],
        },
        {
          title: "更多",
          items: [
            {
              label: "仓库",
              href: "https://gitee.com/rrqm_home/touchsocket",
            },
          ],
        },
      ],
      copyright: `Copyright © 2020-${new Date().getFullYear()} 若汝棋茗.`,
    },
  },
  presets: [
    [
      "@docusaurus/preset-classic",
      {
        docs: {
          sidebarPath: require.resolve("./sidebars.js"),
          editUrl:
            "https://gitee.com/rrqm_home/touchsocket/tree/master/handbook/",
          showLastUpdateTime: true,
          showLastUpdateAuthor: true,
          sidebarCollapsible: true,
          sidebarCollapsed: true,
          lastVersion: 'current',
          versions: {
            current: {
              label: '最新版本',
              path: '/current',
            }
          },
          // sidebarCollapsible: true,
        },
        // blog: {
        //   showReadingTime: true,
        //   editUrl:
        //     "https://gitee.com/rrqm_home/touchsocket/tree/master/handbook/",
        // },
        theme: {
          customCss: require.resolve("./src/css/custom.css"),
        },
      },
    ],
  ],
  plugins: [require.resolve("docusaurus-plugin-image-zoom")],
  themes: [
    [
      "@easyops-cn/docusaurus-search-local",
      {
        hashed: true,
        language: ["zh"],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ],
};
