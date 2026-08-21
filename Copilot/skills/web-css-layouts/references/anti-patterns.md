# Anti-Patterns

## Component Breakpoints

Avoid making a reusable component depend on viewport width:

```css
/* Avoid. */
@media (min-width: 768px) {
  .card {
    flex-direction: row;
  }
}
```

Prefer a named container query:

```css
.card-region {
  container: card / inline-size;
}

@container card (min-width: 400px) {
  .card {
    flex-direction: row;
  }
}
```

Viewport media queries remain valid for page-level concerns.

## Magic Numbers

Avoid unrelated arbitrary values:

```css
/* Avoid. */
.element {
  margin-block-start: 23px;
  padding: 17px;
}
```

Use semantic tokens:

```css
.element {
  margin-block-start: var(--space-md);
  padding: var(--space-sm);
}
```

## Fixed Or Unbounded Type

Avoid breakpoint jumps and raw viewport units:

```css
/* Avoid. */
.title { font-size: 2vw; }
```

Use bounded fluid scaling:

```css
.title {
  font-size: clamp(1.5rem, 2vw, 3rem);
}
```

## Variant Explosion

Avoid combinatorial classes such as `.card--featured-compact`. Use one explicit data attribute fallback and a style query when the browser matrix supports it:

```css
.card[data-featured] {
  background: var(--color-featured);
}

@container card style(--featured: true) {
  .card {
    background: var(--color-featured);
  }
}
```

## Other Failure Modes

- Bare `1fr` tracks can refuse to shrink; use `minmax(0, 1fr)` where appropriate.
- `100vw` in document flow can cause scrollbar-width overflow.
- Fixed heights can clip variable content; prefer `min-height`.
- Truncation can hide required information; provide the full value another way.
- Dense grid packing can change visual order; use it only when order is irrelevant.
- Generated content is not an accessible empty, loading, or error state.

## Resources

- Clamp calculator: https://min-max-calculator.9elements.com/
- Quantity queries: https://css-tip.com/quantity-queries/
- Container query support: https://caniuse.com/css-container-queries
- Every Layout: https://every-layout.dev/
- Utopia fluid design: https://utopia.fyi/
