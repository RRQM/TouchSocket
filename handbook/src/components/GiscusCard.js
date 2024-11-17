import Giscus from '@giscus/react';
import BrowserOnly from '@docusaurus/BrowserOnly';

export default function MyApp()
{
    return (

        <BrowserOnly>
            {() =>
            {
                return <Giscus
                    id="comments"
                    repo="RRQM/TouchSocket"
                    repoId="MDEwOlJlcG9zaXRvcnkzNTUwNzEyODM="
                    category="Announcements"
                    categoryId="DIC_kwDOFSn1M84Chi23"
                    mapping="pathname"
                    term="Welcome to @giscus/react component!"
                    reactionsEnabled="1"
                    emitMetadata="0"
                    inputPosition="top"
                    theme="preferred_color_scheme"
                    lang="zh-CN"
                    loading="lazy"
                />;
            }
            }
        </BrowserOnly>
    );
}