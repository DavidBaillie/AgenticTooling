# Container Queries

Use container queries when a component changes based on its available space. Name the container for clarity.

## Size Queries

```css
.card-region {
  container: card / inline-size;
}

.card {
  display: flex;
  flex-direction: column;
}

@container card (min-width: 400px) {
  .card {
    flex-direction: row;
  }
}
```

Typical starting points are 300px for a minimum horizontal card, 400px for a comfortable horizontal card, and 600px for internal multi-column layouts. Derive thresholds from content, not device labels.

Use `cqw`, `cqh`, `cqi`, `cqb`, `cqmin`, and `cqmax` for container-relative values. Always bound them:

```css
.card-title {
  font-size: clamp(1rem, 1rem + 2cqw, 1.75rem);
}

.card {
  gap: clamp(0.5rem, 2cqw, 1.5rem);
}
```

For an adaptive repeated grid, keep the intrinsic baseline and tune the minimum only when the container is large enough:

```css
.grid-region {
  container: grid / inline-size;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(min(250px, 100%), 1fr));
  gap: clamp(1rem, 2cqw, 2rem);
}

@container grid (min-width: 800px) {
  .grid {
    grid-template-columns: repeat(auto-fit, minmax(min(300px, 100%), 1fr));
  }
}
```

## Style Queries

Use a custom property as an explicit variant contract. Keep a class or data attribute fallback before the enhancement:

```css
.card-region[data-featured] .card {
  background: var(--color-featured);
}

.card-region {
  container: card / inline-size;
  --featured: true;
}

@container card style(--featured: true) {
  .card {
    background: var(--color-featured);
  }
}
```

Keyword-like values use plain colon syntax, such as `style(--layout: compact)`. Numeric comparisons can use range syntax, such as `style(--columns >= 3)`. Do not apply the queried variant to the container itself. Register a property with `@property` when equivalent typed values must compare consistently.

## Progressive Enhancement

Build the baseline first. Add enhancements inside feature checks when the browser matrix requires it:

```css
.card {
  display: flex;
  flex-direction: column;
}

@supports (container-type: inline-size) {
  .card-region {
    container: card / inline-size;
  }

  @container card (min-width: 400px) {
    .card {
      flex-direction: row;
    }
  }
}

@supports (font-size: 1cqw) {
  .card-title {
    font-size: clamp(1rem, 1rem + 2cqw, 1.75rem);
  }
}
```

Do not assume support for size queries implies support for style queries. Preserve the same meaning and controls without any enhancement.
