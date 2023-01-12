module.exports = {
  title: "TouchSocket",
  tagline: "让 .NET 开发更简单，更通用，更流行。",
  url: "https://rrqm_home.gitee.io",
  baseUrl: "/touchsocket",
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",
  favicon: "img/favicon.ico",
  organizationName: "Baiqian Co.,Ltd",
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
          to: "docs",
          activeBasePath: "docs",
          label: "文档",
          position: "left",
        },
        {
          label: "更新日志",
          position: "left",
          items: [
            {
              label: "📝 查看日志（v4.8.4.8）",
              href: "docs/upgrade",
            },
          ],
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
              href: "https://github.com/MonkSoul/TouchSocket",
            },
            {
              label: "Nuget",
              href: "https://www.nuget.org/profiles/monk.soul",
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
              to: "docs",
            },
            {
              label: "手册",
              to: "docs",
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
        language: ["en", "zh"],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ],
};
