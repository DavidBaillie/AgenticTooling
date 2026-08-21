# Layout Primitives

Choose the smallest layout primitive that matches the relationship between elements. Let the container own spacing with `gap`.

## Stack

Use for vertical flow with consistent spacing:

```css
.stack {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}
```

Use margins only when prose flow or external spacing requires them. Prefer semantic spacing custom properties.

## Cluster

Use for inline items that wrap naturally:

```css
.cluster {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  align-items: center;
}
```

This suits tags, buttons, metadata, and navigation controls.

## Sidebar

Use when one child has an intrinsic width and the other should grow:

```css
.sidebar {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-md);
}

.sidebar > * {
  flex-grow: 1;
  min-width: 0;
}

.sidebar > :first-child {
  flex-basis: 250px;
  flex-grow: 0;
}
```

The layout wraps when the available space cannot support the sidebar and content together. Use `inline-size` or `block-size` when a logical dimension is the real constraint.

## Adaptive Grid

Use for repeated items or two-dimensional alignment:

```css
.grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(250px, 100%), 1fr));
  gap: var(--space-md);
}
```

`auto-fit` collapses empty columns. The `min()` wrapper prevents the minimum from forcing overflow. Use `minmax(0, 1fr)` when a track must shrink below intrinsic content width.

Useful page-level tracks include:

```css
.page {
  display: grid;
  grid-template-columns: minmax(200px, 250px) minmax(0, 1fr);
}
```

## Frame

Use for fixed-format media:

```css
.frame {
  inline-size: 100%;
  min-block-size: 12rem;
  aspect-ratio: 16 / 9;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
}

.frame > img {
  inline-size: 100%;
  block-size: 100%;
  object-fit: cover;
  object-position: 50% 50%;
}
```

Set `object-position` deliberately because `cover` crops the source. Keep a minimum dimension so missing or unsupported aspect-ratio behavior does not collapse the frame.

## Grid Or Flexbox

Use Grid for rows and columns, named areas, precise placement, and aligned tracks. Use Flexbox for one-dimensional distribution, wrapping, and content-driven sizing.

Combining them is normal:

```css
.card {
  display: grid;
  grid-template-rows: auto 1fr auto;
}

.card-content {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}
```

Keep source order logical. Avoid dense packing when visual order could differ from keyboard or reading order.
