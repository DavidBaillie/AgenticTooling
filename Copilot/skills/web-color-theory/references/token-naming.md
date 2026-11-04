# Token Naming

Expose [palette roles](./palette-structure.md) through names that survive theme changes. Preserve an existing coherent vocabulary; renaming an entire system is a migration, not a side effect of a local improvement.

## Primitives and Semantics

For a new multi-role palette, separate values from jobs:

- **Primitives** name a hue and step, such as `--blue-600` or `--neutral-200`.
- **Semantics** name a role, such as `--color-accent-solid`, `--color-text-secondary`, or `--color-border-subtle`.
- Components consume semantic tokens rather than choosing a hue step themselves.

```css
:root {
  --blue-600: #2563eb;
  --neutral-200: #e5e7eb;
  --neutral-700: #374151;

  --color-accent-solid: var(--blue-600);
  --color-border: var(--neutral-200);
  --color-text-secondary: var(--neutral-700);
}
```

Themes repoint the semantic layer so components keep their roles. Small existing systems with direct semantic values need not gain a primitive layer unless it solves a real problem.

A third, component-level tier such as `--color-button-danger-bg` is justified when a component intentionally diverges from a shared role. Many such exceptions suggest missing semantic roles.

## Role Inventory

Use this inventory to inspect supported components, not to manufacture unused tokens.

| Group | Common roles |
| --- | --- |
| Surfaces | Page, surface, raised menus or popovers, sunken inputs or wells, overlay scrim |
| Text | Primary, secondary, disabled, inverse, on-accent |
| Borders | Subtle, default, strong, focus ring, separator |
| Accent | Subtle background, border, solid, solid hover, text |
| Status | For each shipped status: subtle background, border, solid, text |

Separate roles can point at the same primitive today. A separator divides content; a border encloses a control. Give them separate semantic names when they need independent evolution.

Never borrow a border token for a caption or a text token for a background just because the current value fits. Reuse an existing appropriate role or add the missing role.

## Naming Grammar

Follow the project's grammar. For a new system, a predictable shape is:

- Neutral roles: `--color-{bg|text|border}-{variant}-{state}`.
- Accent and status roles: `--color-{accent|danger|...}-{subtle|border|solid|text}-{state}`.

```css
--color-bg-surface
--color-text-secondary
--color-border-strong
--color-accent-solid-hover
--color-danger-subtle
--color-accent-text
```

Choose one term per concept rather than mixing `text`, `fg`, `foreground`, and `ink`, or `bg`, `background`, and `fill` for the same role.

For a new vocabulary, reserve `primary` for prominence within a group, such as `--color-text-primary`, and use `accent` for brand emphasis. Existing `--color-primary` is not inherently wrong; flag ambiguity only when meanings or usage are actually confused.

## Naming Problems

| Pattern | Risk | Suggested role |
| --- | --- | --- |
| `--color-blue-button` | Semantic name fixes both hue and component | `--color-accent-solid` |
| `--color-sidebar-gray` | A shared role is named after one location | `--color-bg-surface` |
| `--color-light-gray` | Lightness label becomes misleading across themes | A hue-step primitive or a semantic role |
| `--color-text-2` | Number does not explain the job | `--color-text-secondary` |
| `--color-gray-hover` | Hue and state are mixed without a role | `--color-bg-surface-hover` |
| A raw primitive in a component with an established semantic layer | Component bypasses theme mapping | The appropriate semantic token |

These are reasons to inspect meaning and scope, not instructions to rename every occurrence without permission.

## Tailwind Projects

Tailwind v4 generates utilities from the `--color-*` namespace in `@theme`. Both primitive and semantic utilities remain reachable:

```css
@theme {
  --color-blue-400: #60a5fa;
  --color-blue-600: #2563eb;
  --color-neutral-200: #e5e7eb;
  --color-neutral-700: #374151;

  --color-accent-solid: var(--color-blue-600);
  --color-text-secondary: var(--color-neutral-700);
}

.dark {
  --color-accent-solid: var(--color-blue-400);
  --color-text-secondary: var(--color-neutral-200);
}
```

This yields utilities such as `bg-accent-solid` and `text-text-secondary`. The doubled `text-` is valid; follow the project's naming convention rather than renaming it for aesthetics.

For this runtime semantic-override pattern, leave `@theme` without `inline` so generated utilities reference the semantic variable. Keep theme overrides in the established selector outside `@theme`; do not adopt this class example if the project uses another switching strategy.

Templates use semantic utilities when the project has that layer. Raw `bg-blue-600` is a candidate for role mapping, not permission for an unrequested architecture migration.

Opacity modifiers such as `bg-accent-solid/50` change the rendered pair. Measure the composite on its actual background, or choose a solid token when variable backgrounds would make text contrast unreliable.
