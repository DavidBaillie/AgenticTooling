# Palette Generation

Generate values after deciding [palette structure](./palette-structure.md). A local fix may need only another existing role mapping, not a new ramp.

## Start from the Brand Color

Determine the seed's role before building around it. Pin a fixed brand value exactly, even if spacing becomes uneven. A flexible seed may move only when adjustment is allowed. If the seed cannot support its intended label, choose another role, fill, or label treatment.

## Useful Ramp Properties

These are starting criteria, not conformance thresholds:

- **Lightness follows the role convention.** OKLCH `L`, from `0` to `1`, is useful for perceptual ordering; HSL lightness is not perceptually uniform. A role-based scale is not necessarily one monotonic ramp.
- **Hue stays coherent within a ramp.** Start with a constant hue. Deliberate hue shifts may suit an established scale; inspect them rather than rejecting them automatically.
- **Chroma usually peaks near the middle.** Pale and dark extremes typically need less chroma to remain usable and in gamut.
- **Spacing serves a role.** Subtle surfaces may need finer differences than action fills. Judge useful distinctions at actual component sizes instead of enforcing a fixed lightness delta.
- **Tinted endpoints often stop short of black and white.** Pure black or white remain valid when the design or contrast requirements call for them.

## Generate, Inspect, Then Serialize

Use an available, verified tool such as `culori`, `colorjs.io`, or `chroma.js`; record the version when reporting results.

1. Assign consumed steps to roles and preserve fixed inputs.
2. Compute candidates in a suitable perceptual space, commonly OKLCH or OKLab. Interpolation and equally spaced samples produce candidates, not a validated role scale.
3. Inspect the unrounded candidates for spacing, hue coherence, and target gamut. Do not serialize to clamped hex before checking for out-of-gamut values.
4. Apply an intentional [gamut treatment](./color-formats.md#gamut-and-fallbacks) where needed, then emit the project's notation and precision.
5. Measure the final emitted pairs using the [measurement contract](./contrast.md#measurement-contract). Rounding or mapping can change a marginal result.

Decorative [gradients](./color-usage.md#gradients) are a different use of interpolation; their visual path need not satisfy ramp-role spacing.

## Several Hues at Once

Compare equivalent roles, not just matching numeric steps. Similar perceived lightness can help balance visual weight, but required contrast takes precedence. Available chroma depends on hue and lightness, so copying one chroma across families does not ensure equal vividness or a shared gamut.

## Dark Mode

A reversed light ramp is not a finished dark theme. Repoint semantic roles, then inspect:

- **Chroma:** an accent may look excessively vivid on a near-black surface. Reduce it when needed, not automatically.
- **Surface separation:** dark surface roles may need wider spacing than a mirrored light scale provides.
- **Contrast:** remeasure every affected pair. Passing in one appearance does not establish a pass in another.

## Theme Switching

- Use `prefers-color-scheme` alone when the interface follows the system and has no override.
- Use the established class or attribute strategy when users can choose an appearance. System preference may initialize the choice, but must not fight the explicit override.
- Use `light-dark()` when supported and appropriate. It follows the element's `color-scheme`, not a class name; a manual toggle must update that property too.

```css
:root {
  color-scheme: light dark;
  --color-bg-page: light-dark(#ffffff, #172554);
}
```

Do not let independent media-query and class-based token sets produce a half-themed interface. Check system, explicit light, explicit dark, and any increased-contrast variants that the product supports.

Sources: [CSS Color 4 perceptual spaces and gamut](https://www.w3.org/TR/css-color-4/) and [MDN light-dark()](https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Values/color_value/light-dark).
