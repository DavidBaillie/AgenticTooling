# Conditional Layouts

Use `:has()` when layout depends on descendants, siblings, or item count. Keep a useful baseline before enhanced selectors.

## Quantity Queries

These examples assume `.card` elements are the direct children being counted:

```css
.section {
  display: grid;
  gap: var(--space-md);
}

/* Exactly three cards. */
.section:has(> .card:nth-child(3)):not(:has(> .card:nth-child(4))) {
  grid-template-columns: repeat(3, 1fr);
}

/* Four or more cards. */
.section:has(> .card:nth-child(4)) {
  grid-template-columns: 1fr;
}

/* One to three cards. */
.section:has(> .card:nth-child(1)):not(:has(> .card:nth-child(4))) {
  grid-template-columns: 200px 1fr;
}
```

If a header or wrapper exists, apply the selector to the card list rather than the whole section. `nth-last-child()` can express ranges when other children are present.

Feature items by count only when the visual result remains understandable:

```css
.section:has(> .card:nth-child(6)) > .card:first-child {
  --featured: true;
  grid-column: 1 / -1;
}
```

For a section with a header, count cards in the card list or use `nth-last-child()` so the header is not counted:

```css
.section:has(> .card:nth-last-child(n + 4)) {
  grid-template-columns: 1fr;
}

.section:has(> .card:nth-last-child(n + 4)) .section-header {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  padding-block-end: 0.5rem;
  border-block-end: 1px solid var(--color-border);
}
```

## Content And Sibling State

```css
.card:not(:has(img)) {
  border-inline-start: 4px solid var(--color-accent);
  padding-inline-start: 1rem;
}

.card:has(img) {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
}

.card:has(+ .card) {
  border-block-end: 1px solid var(--color-border);
}
```

Keep empty, loading, and error states in the DOM. Do not rely on generated content or `:has()` for essential meaning.

## Dense Placement

Use `grid-auto-flow: dense` only when visual placement is independent of source order. Dense packing can move later items into earlier gaps and confuse keyboard and assistive technology users.

Feature detection may be appropriate for an optional enhancement:

```css
@supports selector(:has(*)) {
  .section:has(> .card:nth-child(4)) {
    grid-template-columns: 1fr;
  }
}
```
