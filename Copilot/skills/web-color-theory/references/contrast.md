# Contrast

Measure a foreground against the background it actually renders on, usually the nearest ancestor that paints a surface. A card's text is not necessarily on the page background.

Use the project's required standard. Otherwise use WCAG 2.2 AA as the baseline; APCA may supplement text evaluation when suitable tooling is available. Label the metric and do not substitute one metric's score for another's requirement.

## WCAG Contrast Thresholds

WCAG 2 contrast ratios support formal WCAG conformance checks.

| Content | AA | AAA |
| --- | --- | --- |
| Normal text | 4.5:1 | 7:1 |
| Large text, at least `24px` regular or approximately `18.67px` bold | 3:1 | 4.5:1 |
| Non-text UI or graphical information required to identify controls, states, or meaning | 3:1 against adjacent colors | No separate enhanced ratio |

Apply requirements to the information needed to identify a component or state, not indiscriminately to every decorative border.

Inactive controls and logos have relevant exemptions. Placeholder text is not a blanket exemption: apply the text requirement when it conveys information. Disabled presentation can still be inspected for usability without inventing a required pass.

## Supplementary APCA Targets

APCA's signed Lc value models text contrast differently from WCAG 2. These are working targets, not WCAG conformance thresholds or a replacement for APCA's font-size and weight guidance:

| Content | Starting target |
| --- | --- |
| Body text in columns or blocks | Lc 75, with Lc 90 preferred |
| Labels and headlines | Lc 60 |
| Large text, at least `36px` regular or `24px` bold | Lc 45 |

For a project explicitly using APCA-like non-text heuristics, Lc 45 for fine outlines, Lc 30 for solid icons or controls, and Lc 15 for merely discernible dividers can guide inspection. They do not replace applicable WCAG non-text requirements.

Positive Lc means dark text on a light background; negative means light text on a dark background. Compare magnitude against the chosen target and retain polarity in the report. Measure both appearances independently.

Inactive text may use a legibility target such as Lc 30 without being reported as a WCAG requirement. If APCA is required but its tooling is unavailable, state `Not verified`; do not estimate a value.

## Fix a Failing Pair Within Authorized Scope

During an audit, report the pair, measured result, required threshold, and proposed correction without editing colors. A request to improve or fix colors authorizes relevant corrections without a separate permission step. Broad palette consolidation or notation migration still needs its own scope.

When changes are authorized:

1. Adjust lightness first, keeping the intended hue and chroma where possible.
2. If the foreground cannot meet the target on that surface, consider the background or another treatment rather than assuming black or white must pass.
3. Reduce chroma if the adjustment leaves the target gamut.
4. Remeasure every affected pair and inspect related states and themes.

Mid-lightness backgrounds can limit achievable text contrast. Test alternatives instead of treating a fixed lightness delta as a guarantee.

## Choose Text Polarity by Measurement

Compare black and white using the required metric; the higher score does not necessarily meet the target. APCA and WCAG can prefer different treatments.

For a neutral background at `oklch(65% 0 0)`, WCAG black contrast is about 6.49:1 and white contrast about 3.23:1. Only black passes AA for normal text. These values do not establish a crossover rule for colored backgrounds; measure the actual pair.

## What to Verify

- **Every affected pair and appearance:** body text, controls, focus, hover, error, success, and other meaningful states within scope.
- **Translucent surfaces:** composite against the actual background. For overlays or `backdrop-filter`, test the lightest and darkest content they can cover, or provide a sufficiently stable surface.
- **Computed colors:** resolve `color-mix()`, relative colors, inherited values, and opacity before calculating contrast.
- **Gradients and images:** measure the worst region behind the foreground, or establish a guaranteed pairing with a scrim or solid surface.
- **Gamut:** check the value that reaches the target display, including fallbacks and wider-gamut enhancements.

Without a browser, declared solid pairs can be computed if their values resolve. That does not verify layout, actual background selection, image content, or compositing. Report those limits explicitly as `Not verified`.
