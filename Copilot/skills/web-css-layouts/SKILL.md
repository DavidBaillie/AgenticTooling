---
name: web-css-layouts
description: 'Use this skill when designing responsive CSS components with container queries, intrinsic layouts, fluid CSS, :has(), and modern grid or flexbox patterns.'
---

# Responsive CSS Layouts

Use this skill for resilient responsive components built with intrinsic sizing, container queries, fluid values, modern grid and flexbox, and conditional selectors.

## Use This Skill When

- Designing responsive CSS components or page layouts.
- Choosing between Grid, Flexbox, intrinsic sizing, or container queries.
- Handling fluid type, adaptive spacing, `:has()`, quantity queries, or layout fallbacks.

## Core Rules

- Choose the layout primitive first: stack, cluster, sidebar, grid, or frame.
- Make reusable components respond to their container; reserve viewport media queries for page-level layout.
- Prefer intrinsic tracks such as `repeat(auto-fit, minmax(min(250px, 100%), 1fr))`.
- Bound fluid typography and spacing with `clamp()`.
- Build a readable baseline, then layer `@container`, `:has()`, style queries, and query units with fallbacks.
- Keep grid and flex children shrinkable with `min-width: 0` or `minmax(0, 1fr)` where needed.
- Preserve semantic DOM order, visible focus, readable wrapping, and access to full content.
- Use `min-height` for growing content; make scrolling and truncation intentional.

## Workflow

1. Identify the primitive and whether the component is reusable.
2. Build the baseline with logical properties, intrinsic sizing, and stable frames.
3. Add container queries for component adaptation and `clamp()` for bounded fluid values.
4. Add `:has()` or style queries only when they express real content or variant state.
5. Validate content extremes, container sizes, fallback behavior, keyboard access, zoom, and overflow.

## Quick Decisions

| Need | Prefer |
| --- | --- |
| One-dimensional flow | Flexbox |
| Two-dimensional alignment | Grid |
| Wrapping inline items | Flexbox with `flex-wrap` |
| Adaptive repeated columns | Grid with `auto-fit` and `minmax()` |
| Component width changes | Named size container query |
| Component variant state | Data attribute fallback plus style query |
| Content-count behavior | `:has()` quantity query |
| Fixed-format media | `aspect-ratio` frame with `object-fit` |

## References

- [Layout primitives](./references/layout-primitives.md) - stack, cluster, sidebar, grid, frame, and Grid/Flexbox choices.
- [Container queries](./references/container-queries.md) - size queries, style queries, query units, and fallbacks.
- [Fluid sizing](./references/fluid-sizing.md) - `clamp()`, type scales, spacing scales, and component-level scaling.
- [Conditional layouts](./references/conditional-layouts.md) - `:has()`, quantity queries, and state-aware styling.
- [Defensive CSS](./references/defensive-css.md) - intrinsic minimums, overflow, text, images, and variable content.
- [Accessibility](./references/accessibility.md) - source order, focus, zoom, forced colors, motion, and status content.
- [Testing](./references/testing.md) - content/container matrices and the validation workflow.
- [Anti-patterns](./references/anti-patterns.md) - brittle breakpoints, magic numbers, and unbounded values.
- [Container-aware card fixture](./examples/container-aware-card.html) - standalone browser fixture.

## Required Validation

- Test 0, 1, 2, 4, 6, and many items where quantity changes layout.
- Test missing media, missing descriptions, long titles, long identifiers, and mixed content.
- Test roughly 200px, 600px, 1200px, full-width, and nested containers.
- Check unsupported-enhancement baselines for container queries, `:has()`, style queries, and query units.
- Test keyboard order, visible focus, 200% zoom, reduced motion, forced colors, and narrow widths.
- Confirm no unintended horizontal overflow; keep intentional scroll regions explicit.