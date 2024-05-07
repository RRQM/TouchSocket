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
      <script defer src="https://us.umami.is/script.js" data-website-id="b77ef5ce-12e7-4637-95cb-c606ff14f188"></script>
    </span>
  );
}

export default React.memo(Footer);
