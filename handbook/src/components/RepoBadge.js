import React, { useEffect, useMemo, useState } from "react";
import classes from "./RepoBadge.module.css";
import repoDefaults from "@site/src/config/repoDefaults";
import GitHubIcon from "@site/src/components/icons/GitHubIcon";
import GiteeIcon from "@site/src/components/icons/GiteeIcon";

const PrIcon = ({ className }) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor" className={className}>
    <path d="M7.177 3.073L9.573.677A.25.25 0 0110 .854v4.792a.25.25 0 01-.427.177L7.177 3.427a.25.25 0 010-.354zM3.75 2.5a.75.75 0 100 1.5.75.75 0 000-1.5zm-2.25.75a2.25 2.25 0 113 2.122v5.256a2.251 2.251 0 11-1.5 0V5.372A2.25 2.25 0 011.5 3.25zM11 2.5h-1V4h1a1 1 0 011 1v5.628a2.251 2.251 0 101.5 0V5A2.5 2.5 0 0011 2.5zm1 10.25a.75.75 0 111.5 0 .75.75 0 01-1.5 0zM3.75 12a.75.75 0 100 1.5.75.75 0 000-1.5z" />
  </svg>
);

const IssueIcon = ({ className }) => (
  <svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor" className={className}>
    <path d="M8 9.5a1.5 1.5 0 100-3 1.5 1.5 0 000 3z" />
    <path fillRule="evenodd" d="M8 0a8 8 0 100 16A8 8 0 008 0zM1.5 8a6.5 6.5 0 1113 0 6.5 6.5 0 01-13 0z" />
  </svg>
);

const TYPE_METADATA = {
  pr: {
    label: "PR",
    className: classes.typePr,
    icon: PrIcon,
  },
  issue: {
    label: "Issue",
    className: classes.typeIssue,
    icon: IssueIcon,
  },
};

const PLATFORM_LABELS = {
  github: "GitHub",
  gitee: "Gitee",
};

const PLATFORM_ICONS = {
  github: GitHubIcon,
  gitee: GiteeIcon,
};

const PLATFORM_AVATAR_BUILDERS = {
  github: (username) => `https://github.com/${username}.png`,
  gitee: (username) => `https://gitee.com/${username}.png`,
};

const PLATFORM_API_BUILDERS = {
  github: ({ owner, repo }, type, id) => {
    const safeOwner = encodeURIComponent(owner);
    const safeRepo = encodeURIComponent(repo);
    const safeId = encodeURIComponent(String(id));
    const resource = type === "pr" ? "pulls" : "issues";
    return `https://api.github.com/repos/${safeOwner}/${safeRepo}/${resource}/${safeId}`;
  },
  gitee: ({ owner, repo }, type, id) => {
    if (type !== "issue") {
      return undefined;
    }

    const safeOwner = encodeURIComponent(owner);
    const safeRepo = encodeURIComponent(repo);
    const safeId = encodeURIComponent(String(id));

    return `https://gitee.com/api/v5/repos/${safeOwner}/${safeRepo}/issues/${safeId}`;
  },
};

const trimTrailingSlash = (value) => value?.replace(/\/$/, "") ?? "";
const resolveRepoCoordinates = (urlString) => {
  if (!urlString) {
    return undefined;
  }

  try {
    const url = new URL(urlString);
    const segments = url.pathname.split("/").filter(Boolean);
    if (segments.length >= 2) {
      return {
        owner: segments[0],
        repo: segments[1],
      };
    }
  } catch (error) {
    // ignore parsing issues
  }

  return undefined;
};
const buildPlatformAvatarUrl = (platform, username) => {
  if (typeof username !== "string") {
    return undefined;
  }

  const normalized = username.trim();
  if (!normalized) {
    return undefined;
  }

  const builder = PLATFORM_AVATAR_BUILDERS[platform];
  return builder ? builder(encodeURIComponent(normalized)) : undefined;
};

export default function RepoBadge(props) {
  const {
    id,
    type = "pr",
    platform = "github",
    author,
    authorHandle,
    authorAvatar,
  } = props;

  const normalizedType = typeof type === "string" ? type.toLowerCase() : "pr";
  const normalizedPlatform = typeof platform === "string" ? platform.toLowerCase() : "github";

  const typeMeta = TYPE_METADATA[normalizedType] ?? TYPE_METADATA.pr;
  const platformDefaults = repoDefaults[normalizedPlatform] ?? repoDefaults.github;

  const baseUrl = trimTrailingSlash(platformDefaults.baseUrl);
  const routeSegment = platformDefaults.routes?.[normalizedType];
  const repoCoordinates = useMemo(() => resolveRepoCoordinates(baseUrl), [baseUrl]);

  const computedHref = id && baseUrl && routeSegment ? `${baseUrl}/${routeSegment}/${id}` : undefined;

  const manualAuthorName = author ?? authorHandle;
  const manualAvatar = authorAvatar ?? buildPlatformAvatarUrl(normalizedPlatform, authorHandle ?? author);
  const [remoteMeta, setRemoteMeta] = useState();

  const shouldFetchMeta = Boolean(
    (!manualAuthorName || !manualAvatar) &&
      repoCoordinates &&
      id &&
      PLATFORM_API_BUILDERS[normalizedPlatform]
  );

  useEffect(() => {
    if (!shouldFetchMeta) {
      setRemoteMeta(undefined);
      return undefined;
    }

    const builder = PLATFORM_API_BUILDERS[normalizedPlatform];
    if (!builder) {
      setRemoteMeta(undefined);
      return undefined;
    }

    const apiUrl = builder(repoCoordinates, normalizedType, id);
    if (!apiUrl) {
      setRemoteMeta(undefined);
      return undefined;
    }

    const controller = new AbortController();
    let isMounted = true;
    setRemoteMeta(undefined);

    fetch(apiUrl, { signal: controller.signal })
      .then((response) => (response.ok ? response.json() : null))
      .then((data) => {
        if (!isMounted || !data) {
          return;
        }

        const user =
          data.user ??
          data.author ??
          data.creator ??
          data.sender ??
          data.owner ??
          data.assignee;

        if (!user) {
          return;
        }

        setRemoteMeta({
          name: user.name ?? user.login ?? user.username ?? user.nick_name,
          login: user.login ?? user.username ?? user.name ?? user.nick_name,
          avatar:
            user.avatar_url ??
            user.avatar ??
            user.portrait ??
            user.avatarUrl ??
            user.avatar_url_template,
        });
      })
      .catch(() => {});

    return () => {
      isMounted = false;
      controller.abort();
    };
  }, [shouldFetchMeta, normalizedPlatform, normalizedType, repoCoordinates, id]);

  const remoteAuthorName = remoteMeta?.name ?? remoteMeta?.login;
  const remoteAvatar = remoteMeta?.avatar;

  const resolvedAuthorName = manualAuthorName ?? remoteAuthorName;
  const avatarSource = manualAvatar ?? remoteAvatar;

  const badgeTitle = `${typeMeta.label} #${id ?? "?"} · ${PLATFORM_LABELS[normalizedPlatform] ?? normalizedPlatform}`;
  const TypeIcon = typeMeta.icon;

  const platformLabel = PLATFORM_LABELS[normalizedPlatform] ?? normalizedPlatform;
  const PlatformIcon = PLATFORM_ICONS[normalizedPlatform];

  const authorChip = avatarSource || resolvedAuthorName ? (
    <span className={classes.author}>
      {avatarSource ? (
        <img
          className={classes.avatar}
          src={avatarSource}
          alt={resolvedAuthorName ? `${resolvedAuthorName} avatar` : "Contributor avatar"}
          loading="lazy"
          width={20}
          height={20}
        />
      ) : null}
      {resolvedAuthorName ? <span className={classes.authorName}>{resolvedAuthorName}</span> : null}
    </span>
  ) : null;

  return (
    <a
      className={`${classes.badge} ${typeMeta.className}`}
      href={computedHref}
      target="_blank"
      rel="noopener noreferrer"
      title={badgeTitle}
    >
      <span className={classes.content}>
        {TypeIcon ? <TypeIcon className={classes.typeIcon} /> : <span>{typeMeta.label}</span>}
      </span>
      {authorChip}
      <span className={classes.platform}>
        {PlatformIcon ? <PlatformIcon width={14} height={14} className={classes.platformIcon} /> : platformLabel}
      </span>
    </a>
  );
}
