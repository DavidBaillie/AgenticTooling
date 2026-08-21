# Testing Responsive Layouts

Test behavior, not only a single viewport screenshot.

## Content Matrix

Check:

- 0, 1, 2, 4, 6, and many items.
- A 100-character title and a 500-word description.
- A long unbroken URL or identifier.
- Missing image, missing description, and mixed media.
- Empty, loading, and error states.

## Container Matrix

Place the component in:

- A narrow sidebar around 200px.
- A medium container around 600px.
- A wide container around 1200px.
- A full-width region.
- A nested container.

Resize the container itself, not only the viewport. Verify every intended container-query state.

## Browser And Fallback Checks

- Test Chrome, Firefox, and Safari when the browser matrix matters.
- Verify `container-type`, `:has()`, style queries, query units, `clamp()`, logical properties, and `aspect-ratio` support.
- Disable one enhancement at a time and confirm the baseline remains readable and exposes the same controls.
- Tab through every interactive element. Check focus visibility and DOM-order keyboard navigation.
- Test 200% zoom, narrow widths, forced colors, and reduced motion.

## Automated Check

For a representative page, assert that unintended horizontal overflow is absent:

```js
expect(await page.evaluate(() =>
  document.documentElement.scrollWidth <= document.documentElement.clientWidth
)).toBe(true);
```

Treat intentional scroll regions such as data tables as explicit exceptions and test their keyboard access separately. Run the project's CSS linter or style checker.

Use [the standalone fixture](../examples/container-aware-card.html) for quick manual browser checks.
