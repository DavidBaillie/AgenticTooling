# Accessibility

Layout must preserve meaning, reading order, keyboard access, and readable content at narrow widths and high zoom.

## Structure And Order

- Use semantic `header`, `nav`, `main`, `section`, `article`, and `footer` elements where appropriate.
- Add ARIA only when native HTML does not provide the required meaning.
- Keep DOM order logical. Do not use `order`, visual grid placement, or dense packing to create a different reading order.
- Keep empty, loading, and error messages in the DOM and expose dynamic status changes with an appropriate live region.

## Focus And Reflow

```css
:focus-visible {
  outline: 3px solid currentColor;
  outline-offset: 3px;
}
```

Test at 200% zoom and narrow widths. Content should reflow without two-dimensional scrolling, except for content that genuinely requires it such as a data table. Keep line lengths readable and do not clip headings, errors, instructions, or controls with fixed heights.

## Modes And Motion

Avoid communicating state through color alone. Check forced-colors mode:

```css
@media (forced-colors: active) {
  .card,
  .section {
    border: 1px solid CanvasText;
  }
}
```

Respect reduced motion:

```css
@media (prefers-reduced-motion: reduce) {
  *,
  *::before,
  *::after {
    scroll-behavior: auto;
    transition-duration: 0.01ms;
    animation-duration: 0.01ms;
    animation-iteration-count: 1;
  }
}
```
