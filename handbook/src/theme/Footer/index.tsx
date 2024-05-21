import React from 'react';

import { useThemeConfig } from '@docusaurus/theme-common';
import FooterLinks from '@theme/Footer/Links';
import FooterLogo from '@theme/Footer/Logo';
import FooterCopyright from '@theme/Footer/Copyright';
import FooterLayout from '@theme/Footer/Layout';

function Footer(): JSX.Element | null
{
  const { footer } = useThemeConfig();
  if (!footer)
  {
    return null;
  }
  const { copyright, links, logo, style } = footer;

  return (
    <span>
      <FooterLayout
        style={style}
        links={links && links.length > 0 && <FooterLinks links={links} />}
        logo={logo && <FooterLogo logo={logo} />}
        copyright={copyright && <FooterCopyright copyright={copyright} />}
      />
      <script defer src="https://umami.touchsocket.net:10086/script.js" data-website-id="9a4065f2-8c32-4727-9b5f-bc6a92843fa5"></script>
    </span>
  );
}

export default React.memo(Footer);
