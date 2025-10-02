export const paths = {
  home: {
    path: "/",
    getHref: () => "/",
  },

  auth: {
    register: {
      path: "/auth/register",
      getHref: (redirectTo?: string | null | undefined) =>
        `/auth/register${
          redirectTo ? `?redirectTo=${encodeURIComponent(redirectTo)}` : ""
        }`,
    },
    login: {
      path: "/auth/login",
      getHref: (redirectTo?: string | null | undefined) =>
        `/auth/login${
          redirectTo ? `?redirectTo=${encodeURIComponent(redirectTo)}` : ""
        }`,
    },
  },

  app: {
    root: {
      path: "/app",
      getHref: () => "/app",
    },
    dashboard: {
      path: "/app/dashboard",
      getHref: () => "/app/dashboard",
    },
    calendar: {
      path: "/app/calendar",
      getHref: () => "/app/calendar",
    },
    clients: {
      path: "/app/clients",
      getHref: () => "/app/clients",
    },
    questionnaires: {
      path: "/app/questionnaires",
      getHref: () => "/app/questionnaires",
    },
    settings: {
      path: "/app/settings",
      getHref: () => "/app/settings",
    },
  },
} as const;
