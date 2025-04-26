// @ts-check
// `@type` JSDoc annotations allow editor autocompletion and type checking
// (when paired with `@ts-check`).
// There are various equivalent ways to declare your Docusaurus config.
// See: https://docusaurus.io/docs/api/docusaurus-config

import { themes as prismThemes } from 'prism-react-renderer';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: "TouchSocket",
  tagline: "网络开发",
  url: "https://touchsocket.net/",
  baseUrl: '/',
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "throw",
  favicon: "img/favicon.ico",
  projectName: "TouchSocket",
  scripts: [],
  i18n: {
    defaultLocale: 'zh',
    locales: ['zh'],
    localeConfigs: {
      zh: {
        htmlLang: 'zh-cn',
      },
    },
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: './sidebars.ts',
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://gitee.com/rrqm_home/touchsocket/tree/master/handbook/',
          showLastUpdateTime: true,
          showLastUpdateAuthor: true,
          sidebarCollapsible: true,
          sidebarCollapsed: true,
          lastVersion: 'current',
          versions: {
            current: {
              label: '3.1',
              path: '/current',
            },
            // next: {
            //   label: '2.1-rc',
            //   path: '/next',
            //   banner: 'none'
            // }
          },
        },
        blog: {
          showReadingTime: true,
          feedOptions: {
            type: ['rss', 'atom'],
            xslt: true,
          },
          onInlineTags: 'warn',
          onInlineAuthors: 'warn',
          onUntruncatedBlogPosts: 'warn',
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            'https://gitee.com/rrqm_home/touchsocket/tree/master/handbook/',
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
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
        additionalLanguages: ["powershell", "csharp", "sql", "json"],
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
            label: "博客",
            position: "left",
            to: "/blog"
          },
          {
            label: "视频",
            position: "left",
            to: "docs/current/video"
          },
          {
            label: "API手册",
            position: "left",
            to: "https://touchsocket.net/api/index"
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
                to: "https://touchsocket.net/api/index",
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
              {
                label: "统计",
                href: "https://touchsocket.net:10086/share/52srUBHSadfSOngf/touchsocket.net",
              },
            ],
          },
        ],
        copyright: `Copyright © 2020-${new Date().getFullYear()} 若汝棋茗  <a href="https://beian.miit.gov.cn/">赣ICP备2024022829号-1</a>`,
      },
      mermaid: {
        theme: { light: 'neutral', dark: 'forest' },
        options: {
          maxTextSize: 20000,
        },
      },
    }),

  markdown: {
    mermaid: true,
  },
  themes: [
    ['@docusaurus/theme-mermaid',
      {
        theme: { light: 'neutral', dark: 'forest' },
        options: {
          maxTextSize: 500,
        },
      }
    ],
    [
      "@easyops-cn/docusaurus-search-local",
      {
        hashed: true,
        language: ["zh", "en"],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ]
};

export default config;
