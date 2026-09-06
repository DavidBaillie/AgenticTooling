---
name: web-color-theory
description: 'Use when choosing or reviewing accessible web color systems: palette relationships, semantic tokens, CSS colors, contrast, and component states.'
---

# Web Color Theory

Choose color as a system: establish hierarchy and semantic roles, then tune hue, lightness, chroma, and contrast. Hue relationships do not guarantee accessible contrast.

## When to Use This Skill

- Create or refine a website, web app, or design-system palette.
- Derive supporting colors from a brand or base color.
- Review color harmony, hierarchy, contrast, semantic roles, or component states.

## Core Workflow

1. Identify the product, audience, primary action, brand constraints, and supported themes.
2. Choose a base hue and one relationship. Read only its matching reference: [analogous](./references/analogous-colors.md), [complementary](./references/complementary-colors.md), [triadic](./references/triadic-colors.md), [split-complementary](./references/split-complementary-colors.md), or [monochromatic](./references/monochromatic-colors.md).
3. Start with three to seven colors. Use a neutral or near-neutral for large surfaces and long-form text.
4. Let one color or neutral dominate, use supporting colors for structure, and reserve the highest-contrast accent for actions and meaningful states.
5. Map colors to semantic roles, not hue names: `--color-background`, `--color-surface`, `--color-text`, `--color-primary`, `--color-accent`, `--color-success`, `--color-warning`, and `--color-danger`.
6. Tune lightness and chroma until surfaces, text, borders, actions, and feedback states remain distinct in context.

## Required Output

- Color values and their intended roles.
- Semantic token mapping or the CSS custom properties to add or revise.
- The foreground/background pairs used for body text, controls, focus, and meaningful states.
- Validation performed and any pairing that still needs verification.

## Web Rules

- Use CSS custom properties so palette decisions remain centralized.
- Never communicate meaning through color alone; pair it with text, icons, labels, patterns, position, or shape.
- For WCAG AA, normal text needs at least 4.5:1 contrast, large text 3:1, and distinguishable UI components or focus indicators 3:1.
- When rendered UI or browser tooling is available, check body text, controls, focus indicators, hover, disabled, error, and success states in every supported theme.
- Do not claim validation that cannot be performed from the available values or rendered output.
- Prefer OKLCH for perceptual tuning when project conventions and browser support allow it; provide fallbacks where needed.

## References

- [Analogous Colors](./references/analogous-colors.md)
- [Complementary Colors](./references/complementary-colors.md)
- [Triadic Colors](./references/triadic-colors.md)
- [Split-Complementary Colors](./references/split-complementary-colors.md)
- [Monochromatic Colors](./references/monochromatic-colors.md)
