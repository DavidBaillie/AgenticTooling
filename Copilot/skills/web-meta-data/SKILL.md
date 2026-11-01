---
name: web-meta-data
description: 'Build and validate public website metadata. Use when creating or fixing HTML head tags, SEO descriptions, canonical URLs, robots directives, Open Graph previews, Twitter/X Cards, JSON-LD (application/ld+json) structured data, Schema.org markup, or browser and PWA metadata.'
license: Apache-2.0
---

# Public Website Metadata

Build the smallest accurate metadata set for each public page. Use the article's distinction between useful, conditional, and obsolete tags; do not copy every tag into every website.

## When to Use

- Create page metadata for a public website or a new route.
- Fix missing, duplicate, misleading, or outdated head tags.
- Configure search indexing, social link previews, or browser presentation.
- Configure or fix JSON-LD structured data for page content and site entities.

## Workflow

1. Inspect the existing head, layout, route metadata API, JSON-LD output, deployment headers, and indexing configuration. Reuse the framework's native metadata mechanism and existing structured-data owner; do not add another head manager.
2. Establish the production origin, page path, language, site name, page-specific title and summary, content type, and public preview image. Confirm indexing intent, verified structured-data facts, and any PWA or platform requirements. Ask for missing facts that affect behavior; never invent domains, verification tokens, author handles, image assets, or business details.
3. Implement the baseline below, then add only justified conditional tags and JSON-LD types. Share site-wide defaults while preserving route-specific titles, descriptions, canonical URLs, previews, and structured data. Escape dynamic metadata using the framework or template engine; serialize JSON-LD safely for a script context as described below.
4. Validate the delivered HTML, headers, URLs, and representative routes using the checks below. Correct conflicts at their source, not by appending more tags.
5. Report changed surfaces, indexing decisions, checks performed, and unresolved inputs. Distinguish local validation from deployed verification; never claim indexing or social previews are guaranteed.

## Baseline for Public, Indexable Pages

- Put `<meta charset="UTF-8">` early in the head, entirely within the document's first 1024 bytes.
- Set `<meta name="viewport" content="width=device-width, initial-scale=1">`. Do not disable zoom with `user-scalable=no` or a restrictive `maximum-scale`.
- Supply one meaningful `<title>` and one accurate, page-specific `description`. Search engines may rewrite titles and snippets; character-count targets are editorial guidance, not guaranteed display limits or ranking improvements.
- Set `<html lang="...">` to the actual content language, not a `Content-Language` meta tag.
- Include one canonical `<link>` pointing to the intended absolute production HTTPS URL. Keep the route and meaningful query parameters; remove tracking parameters only when they identify the same content. Do not canonicalize every route to the homepage.
- Omit `robots="index, follow"` unless the project intentionally spells out defaults. It is already the default and cannot override a restrictive directive elsewhere.
- For social sharing, use `og:title`, `og:type`, `og:url`, and a real `og:image`; add `og:description` and descriptive `og:image:alt`. Use `website` for general pages and `article` only for actual articles.
- Use `x:card="summary_large_image"` when a suitable image exists. Twitter/X supports Open Graph fallbacks for title, description, and image; add explicit `x:title`, `x:description`, and `x:image` when needed for platform-specific copy or existing project conventions. Include `x:image:alt` when supplying a x image.
- Keep `og:url` aligned with the canonical URL. Preview images must be absolute HTTPS URLs accessible without authentication. Confirm supported image format, size, and dimensions for the target platforms; do not advertise dimensions that differ from the actual file.

### Example Head

This example is for an indexable general page with an existing share image. Replace every illustrative value before deployment. A title and canonical link are head metadata, not `<meta>` elements.

```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Trail Maps | Example Outdoors</title>
  <meta name="description" content="Find trail maps, route distances, and practical planning advice for your next walking trip.">
  <link rel="canonical" href="https://example.com/trails/">
  <meta property="og:title" content="Trail Maps | Example Outdoors">
  <meta property="og:description" content="Find trail maps, route distances, and practical planning advice for your next walking trip.">
  <meta property="og:type" content="website">
  <meta property="og:url" content="https://example.com/trails/">
  <meta property="og:image" content="https://example.com/images/trail-map-share.jpg">
  <meta property="og:image:alt" content="A walking trail marked across a regional map">
  <meta name="x:card" content="summary_large_image">
</head>
</html>
```

If no appropriate image exists, report that gap rather than ship a broken URL. A text-oriented Twitter `summary` card can be appropriate, but it does not supply the missing image required for a complete Open Graph object.

## Conditional Tags

### Search and Content Controls

- Use `robots` with `noindex` only for pages intentionally excluded from search. Use `googlebot` only for a deliberate Google-specific policy. Check both meta tags and `X-Robots-Tag` headers; restrictive rules can win.
- Let crawlers access a page that must expose `noindex`; blocking it in `robots.txt` can prevent them from seeing the directive. Neither mechanism protects private content: use authentication and authorization.
- Add `google-site-verification` only with the owner's supplied verification value and chosen verification method.
- Add `googlebot` with `notranslate` only when the owner intentionally opts out of Google's translated results. Check current Google documentation rather than assuming the article's `name="google"` spelling is the preferred form.
- Add `rating="adult"` only when the content actually requires adult-content labeling.
- Add `author` only with verified attribution. Treat `publisher` and `generator` as consumer-specific metadata, not SEO requirements; omit them without a concrete need.

### Browser Presentation and Installed Apps

- Add `theme-color` using the actual brand color and supported light/dark variants.
- Declare `color-scheme` only for schemes the UI genuinely supports. Do not claim dark-mode support on a light-only site.
- Use `format-detection="telephone=no"` only when automatic phone linking is undesirable; preserve explicit usable `tel:` links.
- For an actual PWA, use a web app manifest and suitable icons first. Add `application-name` and Apple-specific app title, capability, or status-bar tags only where target-device behavior requires them.
- Treat `msapplication-TileImage` and `msapplication-TileColor` as legacy Windows integration, not modern website defaults. Do not add `mobile-web-app-capable` as a substitute for a manifest; retain compatibility tags only for a verified target-browser requirement.

### JSON-LD Structured Data

- Emit structured data as `<script type="application/ld+json">`, not a `<meta>` tag. Use the existing server-rendered or static page path so the initial HTML contains it; JSON-LD may live in the head or body.
- Choose Schema.org types that describe the actual page: `WebPage` for a general page, `Article` for editorial content, `Product` for a product detail page, or the appropriate `LocalBusiness` subtype for a real business. Add `Organization`, `WebSite`, or `BreadcrumbList` only when supported by verified site identity or actual navigation.
- Set `@context` to `https://schema.org`. Use stable absolute production `@id` values, such as a canonical page URL plus `#webpage`, and reference shared entities by `@id`. Use `@graph` when related entities need linking; do not emit conflicting copies from both layouts and routes.
- Keep page `url` aligned with its canonical URL. Use absolute public image URLs and accurate ISO 8601 dates where applicable. Source names, authors, offers, prices, currencies, availability, addresses, and ratings from real content or verified data; omit unsupported optional properties and ask for missing required facts.
- Match markup to visible page content and update it when that content changes. Never fabricate reviews, ratings, credentials, or FAQs to pursue rich results. Check current Google requirements for the selected feature; Schema.org validity alone does not establish rich-result eligibility.
- Serialize an object with a JSON serializer rather than string concatenation. Use the framework's script-safe serializer, or escape literal `<` as `\u003c` in serialized JSON before embedding it, preventing content such as `</script>` from ending the element. Ordinary HTML entity escaping is not JSON-LD serialization. Preserve CSP and use an existing nonce mechanism where required; do not weaken policy.

This general-page example complements the head example above. Replace illustrative values with verified page data; extend it only when the content warrants additional types.

```html
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "WebPage",
  "@id": "https://example.com/trails/#webpage",
  "url": "https://example.com/trails/",
  "name": "Trail Maps | Example Outdoors",
  "description": "Find trail maps, route distances, and practical planning advice for your next walking trip.",
  "inLanguage": "en"
}
</script>
```

### Security and Privacy

- Prefer HTTP response headers for Content Security Policy and Referrer Policy. Preserve the existing policy unless a change is requested and tested.
- Use a meta CSP only when headers cannot be configured and its limitations are acceptable. Place it before affected resources; it is not retroactive, cannot supply `frame-ancestors`, and cannot provide report-only enforcement.
- Choose a referrer policy deliberately. Do not blindly use `no-referrer`: it removes referrer information even for same-origin navigation and can affect integrations and analytics.
- Configure `X-Content-Type-Options` and `Permissions-Policy` as HTTP headers, never `http-equiv` meta tags.

## Tags to Avoid by Default

- Omit `keywords`, `revisit-after`, `distribution`, `coverage`, and `abstract`. Do not promise SEO benefit; Google ignores the keywords meta tag, and support elsewhere is consumer-specific.
- Use server-side HTTP redirects rather than meta refresh, especially delayed redirects. Do not create timed navigation that disrupts accessibility.
- Set caching, expiry, and cookies through HTTP headers: do not use meta `expires`, `pragma`, `cache-control`, or `Set-Cookie`.
- Omit `X-UA-Compatible`, `HandheldFriendly`, and `MobileOptimized` unless maintaining an explicitly required legacy client.

## Validation

- Inspect the initial HTML response, not only the hydrated DOM. Ensure search and social crawlers receive the intended head without relying solely on client-side JavaScript. Use the framework's supported server-rendered or static metadata path.
- Check one effective title, description, viewport, charset, and canonical per page. Remove accidental duplicate Open Graph and Twitter fields; repeated Open Graph properties are valid only when intentional, with structured image properties immediately following their image.
- Verify distinct routes do not inherit the homepage canonical or unrelated description. Include a content detail page and any intentionally non-indexable route when available.
- Verify canonical and image URLs resolve to the intended resources; images must return an image content type and be available to intended preview crawlers. Check production URLs contain no localhost, staging origin, placeholder, or unintended tracking value.
- Inspect robots meta tags, `googlebot`, `X-Robots-Tag`, and `robots.txt` together. Check production has no accidental `noindex`, and preview environments retain their intended protection.
- Parse every delivered `application/ld+json` block as JSON. Check expected types, stable IDs and references, canonical alignment, verified property values, and absence of duplicate or conflicting entities across representative routes.
- For dynamic JSON-LD, add a regression case containing `<` and `</script>` in a text value. Verify the HTML retains one intact data block, its JSON parses back to the original value, and no executable markup is injected.
- Use the [Schema Markup Validator](https://validator.schema.org/) for vocabulary checks and [Google Rich Results Test](https://search.google.com/test/rich-results) for supported search features when public content submission is permitted. Distinguish syntax errors, required-property errors, optional recommendations, and unsupported rich-result types; neither validator guarantees search appearance.
- Check existing metadata tests or add the smallest regression check when implementing non-trivial route logic. Run the relevant build or static generation checks.
- For a deployed public page, use target-platform preview tools if appropriate and available. Expect cached previews; request refresh where supported. Never submit private URLs or unpublished content to external validators without permission.

## Troubleshooting

- **Wrong share preview:** Check fetched source, route defaults, absolute image URLs, crawler access, and platform caches.
- **Page absent from search:** Check indexing directives, response status, crawler access, and canonical target. Metadata alone does not guarantee indexing.
- **Changes disappear or duplicate:** Fix the framework metadata owner or layout merge rather than injecting extra tags.
- **JSON-LD parses but no rich result appears:** Check supported feature types, required properties, visible-content alignment, and indexing eligibility. Valid generic `WebPage` markup does not promise a rich result.

## Sources and Scope

Use current platform documentation when behavior or support matters:

- [Google-supported meta tags](https://developers.google.com/search/docs/crawling-indexing/special-tags) for search controls and default indexing behavior.
- [Open Graph protocol](https://ogp.me/) for required properties and image metadata.
- [HTML meta element](https://developer.mozilla.org/en-US/docs/Web/HTML/Reference/Elements/meta) for browser behavior.
- [Twitter/X Cards markup](https://developer.x.com/en/docs/x-for-websites/cards/overview/markup) for card fields and Open Graph fallbacks.
- [Schema.org](https://schema.org/docs/schemas.html) for structured-data types and properties.
- [Google structured-data introduction](https://developers.google.com/search/docs/appearance/structured-data/intro-structured-data) and [general guidelines](https://developers.google.com/search/docs/appearance/structured-data/sd-policies) for JSON-LD implementation and search eligibility.