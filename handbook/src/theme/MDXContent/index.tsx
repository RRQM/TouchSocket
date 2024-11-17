import React from 'react';
import { MDXProvider } from '@mdx-js/react';
import MDXComponents from '@theme/MDXComponents';
import type { Props } from '@theme/MDXContent';
import GiscusCard from '@site/src/components/GiscusCard.js';
import LikeButton from '@site/src/components/LikeButton.js';

export default function MDXContent({ children }: Props): JSX.Element {
  return <MDXProvider components={MDXComponents}>
    {children}
    {/* <GiscusCard/> */}
    <LikeButton />
  </MDXProvider>;
}
