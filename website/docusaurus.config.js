// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'KafkaFlow Retry Extensions',
  tagline: 'KafkaFlow Extension for Apache Kafka consumer\'s resilience.',
  url: 'https://your-docusaurus-test-site.com',
  baseUrl: '/kafkaflow-retry-extensions/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: 'FARFETCH', // Usually your GitHub org/user name.
  projectName: 'KafkaFlow Retry Extensions', // Usually your repo name.

  // Even if you don't use internalization, you can use this field to set useful
  // metadata like html lang. For example, if your site is Chinese, you may want
  // to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          routeBasePath: '/', // Serve the docs at the site's root
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
          'https://github.com/farfetch/kafkaflow-retry-extensions/tree/main/website/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
        blog: false
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      colorMode: {
        defaultMode: 'light',
        disableSwitch: true,
      },
      navbar: {
        logo: {
          alt: 'KafkaFlow Retry Extensions',
          src: 'img/logo.svg',
          href: 'https://farfetch.github.io/kafkaflow-retry-extensions/',
          target: '_self',
          height: 32,
        },
        items: [
          {
            href: 'https://github.com/farfetch/kafkaflow-retry-extensions',
            label: 'GitHub',
            position: 'right',
          },
        ],
      },
      footer: {
        style: 'dark',
        links: [
          {
            title: 'Docs',
            items: [
              {
                label: 'Introduction',
                to: '/',
              },
              {
                label: 'Getting Started',
                to: '/category/getting-started',
              },
              {
                label: 'Guides',
                to: '/category/guides',
              },
            ],
          },
          {
            title: 'More',
            items: [
              {
                label: 'FARFETCH Blog',
                to: 'https://farfetchtechblog.com',
              },
              {
                label: 'GitHub',
                href: 'https://github.com/farfetch/kafkaflow-retry-extensions',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} FARFETCH UK Limited. Built with Docusaurus.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
        additionalLanguages: ['csharp']
      },
    }),
};

module.exports = config;
