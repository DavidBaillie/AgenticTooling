# Token Naming

Expose [palette roles](./palette-structure.md) through names that survive theme changes. The CSS examples illustrate the layers; preserve an established token source or build pipeline.

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

Themes repoint the semantic layer. Small systems with direct semantic values need not gain primitives unless that solves a real problem.

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
| `--color-blue-button` | A shared semantic name fixes hue and component | `--color-accent-solid` |
| `--color-light-gray` | Appearance label becomes misleading across themes | A hue-step primitive or a semantic role |
| `--color-gray-hover` | Hue and state lack an explicit job | `--color-bg-surface-hover` |
| A raw primitive in a component with an established semantic layer | Component bypasses theme mapping | The appropriate semantic token |

These are reasons to inspect meaning and scope, not instructions to rename every occurrence without permission.

## Tailwind Projects

Tailwind v4 generates utilities from the `--color-*` namespace in `@theme`. This example defines its own sRGB literals; it does not assert current default palette values. Both primitive and semantic utilities remain reachable:

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

When instead exposing an existing variable through `@theme inline`, generated utilities reference that variable directly. Choose the pattern based on which variable changes at runtime; do not mechanically switch between them. Custom-property aliases resolve where declared, so check nested theme scopes rather than assuming a root alias tracks every descendant override.

Templates use semantic utilities when the project has that layer. Raw `bg-blue-600` is a candidate for role mapping, not permission for an unrequested architecture migration.

Opacity modifiers such as `bg-accent-solid/50` change the rendered pair. Measure the composite on its actual background, or choose a solid token when variable backgrounds would make text contrast unreliable.

Sources: [Tailwind theme variables and inline aliases](https://tailwindcss.com/docs/theme) and [Tailwind color values](https://tailwindcss.com/docs/colors).
