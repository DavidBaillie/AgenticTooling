# Defensive CSS

Design for intrinsic minimums, variable content, missing media, and intentional scroll regions.

## Sizing And Tracks

```css
*,
*::before,
*::after {
  box-sizing: border-box;
}

.flex-or-grid-child {
  min-width: 0;
  min-height: 0;
}

.content-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 2fr);
}
```

`border-box` keeps padding and borders inside declared dimensions. Flex and grid children otherwise have automatic minimums that can block shrinking.

## Overflow And Text

```css
.page-shell {
  inline-size: 100%;
  max-inline-size: 100%;
}

.scroll-region {
  max-inline-size: 100%;
  overflow: auto;
  scrollbar-gutter: stable;
}

.prose {
  max-inline-size: 65ch;
}

.long-text {
  overflow-wrap: anywhere;
  hyphens: auto;
}
```

Avoid `100vw` in normal document flow because it can include the scrollbar and create horizontal overflow. Make scrolling intentional. Use ellipsis or line clamping only when the full value remains available through a label, expansion, or disclosure; never truncate required instructions or errors.

```css
.excerpt {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}
```

## Images

```css
img {
  display: block;
  max-inline-size: 100%;
  block-size: auto;
}

.card-media {
  aspect-ratio: 4 / 3;
  overflow: hidden;
}

.card-media img {
  inline-size: 100%;
  block-size: 100%;
  object-fit: cover;
  object-position: 50% 50%;
}

.thumbnail {
  min-inline-size: 48px;
  min-block-size: 48px;
}
```

Use `srcset` and `sizes` for resolution-aware images, or `picture` when art direction changes. Choose the crop position deliberately.

## Variable Heights And Empty States

```css
.card {
  min-block-size: 12rem;
}

.container {
  block-size: auto;
  min-block-size: 100vh;
}

.description {
  max-block-size: 12rem;
  overflow: auto;
}

.section[data-empty] {
  min-block-size: 6rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}
```

Prefer `min-height` or `min-block-size` when content can grow. Use a definite height only for a deliberate viewport or scroll frame. Render empty-state text in the DOM, not in pseudo-elements.
