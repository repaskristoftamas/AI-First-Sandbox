# 8-Week React Learning Roadmap

**Time budget:** 22 hrs/week (10 weekday + 12 weekend) = **176 hours total**

---

## Week 1 — JavaScript Fundamentals (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Fri | `let`/`const`, arrow functions, template literals, destructuring, spread/rest, modules (`import`/`export`) | 10 |
| Sat | Array methods deep dive: `.map()`, `.filter()`, `.find()`, `.reduce()`, `.some()`, `.every()` — do exercises mapping these to LINQ equivalents | 6 |
| Sun | Closures, higher-order functions, callback patterns, short-circuit evaluation (`&&`, `??`, `?.`) | 6 |

**Resource:** [javascript.info](https://javascript.info) Parts 1.1–1.6
**Milestone:** Solve 10+ small exercises using array methods and destructuring without looking up syntax.

---

## Week 2 — Async JS + TypeScript Basics (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Wed | Promises, `async`/`await`, `fetch()`, error handling with `.catch()` / `try-catch`, `AbortController` | 6 |
| Thu–Fri | Reference vs. value equality, immutability patterns (`...spread` to copy/update objects and arrays) | 4 |
| Sat | TypeScript: types, interfaces, union types, optional properties, generics, type narrowing | 6 |
| Sun | TypeScript: utility types (`Partial`, `Pick`, `Omit`, `Record`), discriminated unions, `as const`, strict mode | 6 |

**Resource:** [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/) (skip Decorators, Mixins, Namespaces)
**Milestone:** Rewrite 3–4 Week 1 exercises in TypeScript with strict mode, no `any`.

---

## Week 3 — React Core Concepts (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Fri | Official React tutorial ([react.dev/learn](https://react.dev/learn)): JSX, components, props, conditional rendering, rendering lists, responding to events | 10 |
| Sat | `useState` deep dive: primitives, objects, arrays, immutable updates, batching, updater functions | 6 |
| Sun | `useEffect`: side effects, dependency arrays, cleanup functions, fetching data on mount | 6 |

**Milestone:** Build a simple interactive app (e.g., a todo list or a bookshelf tracker) using only `useState` and `useEffect`.

---

## Week 4 — React Intermediate Patterns (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Tue | `useRef`, `useMemo`, `useCallback` — when and why (not premature optimization) | 4 |
| Wed–Fri | Component composition: children, render patterns, lifting state up, controlled vs. uncontrolled inputs, forms | 6 |
| Sat | Context API (`useContext`) for shared state — compare to DI in .NET; custom hooks (extracting reusable logic) | 6 |
| Sun | Error boundaries, React DevTools, debugging re-renders | 6 |

**Milestone:** Refactor the Week 3 app — extract 2+ custom hooks, add a theme/auth context, add a form with validation.

---

## Week 5 — Tooling & Ecosystem Essentials (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Tue | Project setup: Vite + React + TypeScript, project structure conventions, ESLint, Prettier | 4 |
| Wed–Fri | React Router: routes, nested routes, dynamic params, navigation, loaders | 6 |
| Sat | TanStack Query (React Query): fetching, caching, mutations, invalidation — the React equivalent of CQRS read/write split mindset | 6 |
| Sun | State management options: when Context is enough, Zustand for global state (lightweight, no boilerplate) | 6 |

**Milestone:** Start a new project from scratch with Vite — multi-page app with routing, data fetching via TanStack Query, and global state.

---

## Week 6 — Styling + UI Libraries + Testing (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Tue | Styling approaches: CSS Modules, Tailwind CSS (most popular in React ecosystem now) | 4 |
| Wed–Fri | Component libraries: pick one (shadcn/ui recommended — composable, Tailwind-based, not a black box) | 6 |
| Sat | Testing with Vitest + React Testing Library: rendering, querying, user events, async assertions | 6 |
| Sun | Testing patterns: mocking API calls (MSW), testing custom hooks, snapshot testing (when/when not) | 6 |

**Milestone:** Add Tailwind + shadcn/ui to the Week 5 project. Write tests for 2–3 key components and a custom hook.

---

## Week 7 — Build a Real Project (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Sun | Build a **Bookstore Frontend** that consumes the Bookstore API | 22 |

**Feature targets:**

- Auth flow (login page, token storage, protected routes)
- Authors list + detail page (with pagination)
- Books list + detail page (with search/filter)
- Create/Edit forms with client-side validation
- Error handling (network errors, 404s, validation errors from API)
- Responsive layout with Tailwind

This is where everything clicks. You'll hit real problems (CORS, token refresh, loading/error states, form state management) and solve them.

---

## Week 8 — Polish, Patterns & Next Steps (22 hrs)

| Day | Focus | Hours |
|---|---|---|
| Mon–Tue | Performance: lazy loading (`React.lazy`/`Suspense`), code splitting, memoization audit | 4 |
| Wed–Thu | Accessibility basics: semantic HTML, ARIA, keyboard navigation, focus management | 4 |
| Fri | Deployment: build for production, deploy to Vercel or Netlify (free tier), environment variables | 2 |
| Sat | Review & refactor the Bookstore Frontend — clean up, improve types, add missing tests | 6 |
| Sun | Explore what's next: Next.js/Remix (SSR frameworks), React Server Components, monorepo setup with .NET backend | 6 |

**Milestone:** Bookstore Frontend deployed and usable. Code is typed, tested, and structured cleanly.

---

## Summary

| Week | Theme | Hours |
|---|---|---|
| 1 | JavaScript fundamentals | 22 |
| 2 | Async JS + TypeScript | 22 |
| 3 | React core (components, state, effects) | 22 |
| 4 | React intermediate (hooks, context, patterns) | 22 |
| 5 | Tooling & ecosystem (Router, TanStack Query) | 22 |
| 6 | Styling, UI libraries, testing | 22 |
| 7 | Full project build (Bookstore Frontend) | 22 |
| 8 | Polish, performance, deployment | 22 |
| **Total** | | **176** |

## Tips (from a C# background)

- **Weeks 1–2** will feel fast — a lot maps directly from C#/LINQ/async.
- **Week 3** is the real paradigm shift — lean into "UI = f(state)", don't fight it.
- **Week 7** is the most valuable week. Building against your own API removes the "toy project" feeling and surfaces real integration challenges.
