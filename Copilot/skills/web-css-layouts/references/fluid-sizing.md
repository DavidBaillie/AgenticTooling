# Fluid Sizing

Use bounded fluid values for typography and spacing. Do not let viewport or container units grow without limits.

## Type Scale

```css
:root {
  --font-size-sm: clamp(0.875rem, 0.8rem + 0.375vw, 1rem);
  --font-size-base: clamp(1rem, 0.9rem + 0.5vw, 1.25rem);
  --font-size-md: clamp(1.125rem, 1rem + 0.625vw, 1.5rem);
  --font-size-lg: clamp(1.5rem, 1.3rem + 1vw, 2rem);
  --font-size-xl: clamp(2rem, 1.6rem + 2vw, 3rem);
}
```

Choose a mobile minimum, desktop maximum, and growth factor derived from the desired range. The basic formula is `clamp(min, min + growth, max)`.

## Spacing Scale

```css
:root {
  --space-3xs: clamp(0.25rem, 0.2rem + 0.25vw, 0.375rem);
  --space-2xs: clamp(0.5rem, 0.4rem + 0.5vw, 0.75rem);
  --space-xs: clamp(0.75rem, 0.6rem + 0.75vw, 1.125rem);
  --space-sm: clamp(1rem, 0.8rem + 1vw, 1.5rem);
  --space-md: clamp(1.5rem, 1.2rem + 1.5vw, 2.25rem);
  --space-lg: clamp(2rem, 1.6rem + 2vw, 3rem);
}
```

Keep names semantic and ratios consistent. Reference tokens instead of scattering arbitrary pixel values.

## Component Scaling

Use container units inside reusable components:

```css
.component {
  container-type: inline-size;
  padding: clamp(1rem, 2cqw, 2rem);
}

.component-title {
  font-size: clamp(1rem, 1rem + 2cqw, 1.75rem);
}
```

Use `vw` for page-level fluidity and `cqw` or `cqi` for component-level fluidity. Test both at very small and very large sizes.

Reference calculator: https://min-max-calculator.9elements.com/
