# Color Usage

Apply the system through consistent meaning, intentional emphasis, and supported appearances. Use [palette generation](./palette-generation.md) for values, [token naming](./token-naming.md) for roles, and [contrast](./contrast.md) for pair measurements.

## Consistent Meaning and Roles

Use semantic colors consistently across the interface. If the accent communicates interaction, applying it indiscriminately to decorative headings weakens that signal. Neutral secondary controls are still valid when shape, labels, position, or other cues communicate their affordance.

```css
a {
  color: var(--color-accent-text);
}

.section-title {
  color: var(--color-text-primary);
}
```

Do not borrow tokens across roles:

```css
/* Avoid: a theme can lighten separators without intending to lighten text. */
.caption {
  color: var(--color-border);
}
```

Use an appropriate text token instead. Distinct semantic roles may share a value, but they must remain free to evolve independently.

Never let color carry meaning alone. Use labels, icons, position, pattern, or shape for status and interaction cues, especially when nearby hues could be confused.

## Primary Emphasis

When filled color encodes primary emphasis, prefer one filled primary action per view or task group. Keep peers neutral unless the established design communicates hierarchy another way.

```html
<button class="bg-accent-solid text-text-on-accent">Save</button>
<button class="text-text-secondary">Cancel</button>
```

A filled background usually communicates stronger emphasis than an accent-colored label. Use that distinction intentionally rather than forcing every primary action into a new style.

Selected tabs, checked segments, categories, and distinct statuses are state or classification, not necessarily competing primary actions. Several colored backgrounds can be valid in those contexts.

## Gradients

Interpolation space is a visual choice, not a correctness setting. Preserve an established gradient unless changing it is in scope.

```css
/* sRGB gives the familiar legacy interpolation. */
background: linear-gradient(in srgb, #3b82f6, #ec4899);

/* Oklab is a useful default for a new perceptual transition. */
background: linear-gradient(in oklab, #3b82f6, #ec4899);

/* OKLCH follows a hue-wheel path and can retain a vivid midpoint. */
background: linear-gradient(in oklch, #3b82f6, #ec4899);
```

sRGB and Oklab interpolate through a rectangular space. Opposing hues can pass near the neutral axis, producing a muted or gray midpoint. OKLCH follows a polar hue path instead, which can introduce intermediate hues the brief did not intend. Either choose that look or add an intermediate stop in the existing space.

```css
background: linear-gradient(in oklch shorter hue, #3b82f6, #ec4899);
background: linear-gradient(in oklch longer hue, #3b82f6, #ec4899);
```

The longer path sweeps more of the hue wheel. Check the actual result rather than assuming polar interpolation is always preferable.

Large gradients can show banding. Consider changing the stop range, reducing the area, or using a restrained noise overlay when that treatment suits the project.

Text over a gradient has no single background. Measure the worst region behind the text, not an average, or use a scrim or solid surface that guarantees the required pairing.

## Color Across Cultures

Verify semantic conventions in the locales the product ships to. For example, Chinese financial interfaces commonly show gains in red and losses in green, reversing the convention used by many English-language interfaces.

Where conventions differ, map gain and loss through locale-aware semantic tokens rather than hardcoded hues. Labels or icons must still communicate the meaning independently.

## Light, Dark, and Increased Contrast

Every rendered role needs a deliberate mapping in each supported appearance. Not every primitive needs a separate dark value if semantic remapping already supplies the correct result.

Adapt increased-contrast values per appearance. This media-query example assumes existing blue primitives and a system-driven theme:

```css
:root {
  --color-accent-solid: var(--blue-600);
}

@media (prefers-contrast: more) {
  :root {
    --color-accent-solid: var(--blue-900);
  }
}

@media (prefers-color-scheme: dark) {
  :root {
    --color-accent-solid: var(--blue-400);
  }
}

@media (prefers-color-scheme: dark) and (prefers-contrast: more) {
  :root {
    --color-accent-solid: var(--blue-200);
  }
}
```

Use the project's class or attribute selectors instead when users can override the system. Do not bolt this media-query strategy onto a conflicting toggle.

A shared increased-contrast override can improve one appearance and worsen another. Widening the lightness difference by about `0.15` in OKLCH `L` can be a starting experiment, not a required step or proof of a pass. Remeasure the actual pair and inspect on-accent text, focus, and state distinctions as well.

## Audit Checklist

Recommendations below apply within the requested change scope. Do not treat stylistic defaults or a different established convention as defects.

| Pattern | Check or proposed correction |
| --- | --- |
| A literal in a component where an appropriate token exists | Reuse the role token in the project's notation |
| One `oklch()` value in an otherwise hex system | Preserve the established notation unless a migration or documented exception is in scope |
| A primitive such as `--blue-600` or `bg-blue-600` bypasses an existing semantic layer | Map its actual role and consume the semantic token |
| A shared semantic name encodes a hue or one component | Prefer a role name; preserve intentional component-specific exceptions |
| `primary` ambiguously names both brand and text roles | Clarify the role vocabulary; `accent` is a useful brand name for new systems |
| A border token is used for text, or a text token for a background | Use or add the appropriate role token |
| An HSL ramp varies only numeric lightness | Inspect perceived spacing and hue coherence; regenerate only if the result is unsuitable |
| Pale steps have the same spacing as the middle | Inspect surface distinctions; finer light-end spacing is a default, not a requirement |
| Every hue uses the same chroma | Check each hue's available gamut and intended visual weight |
| A status hue is close to the accent | Inspect meaning and non-color cues; hue separation alone does not establish accessibility |
| Dark tokens simply reverse the light ramp | Check chroma, surface separation, and every affected pair |
| Media queries and a theme selector assign competing token sets | Make system preference and explicit overrides follow one coherent strategy |
| A contrast fix changes hue but leaves lightness nearly unchanged | Try a lightness adjustment first, then remeasure |
| One `prefers-contrast: more` override serves both appearances | Verify it increases required contrast in each; give appearances separate values where needed |
| Text sits on an alpha color, mixed color, overlay, image, or gradient | Measure the resolved or rendered worst-case pair; consider a solid surface |
| `text-white` sits on a Tailwind `500` fill | Measure it; a darker step may help, but no step number guarantees a pass |
| A declared color exceeds the target gamut without an intentional fallback or enhancement plan | Supply a measured baseline and gate wider-gamut enhancement when needed |
