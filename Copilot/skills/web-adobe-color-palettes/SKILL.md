---
name: web-adobe-color-palettes
description: 'Use when searching Adobe Color palettes by keyword through the Adobe Color Search API, parsing theme and swatch JSON, extracting palette HEX values, or making every matching palette available to the current request. Uses the keyword as the unique search constraint and supports paginated API results.'
---

# Adobe Color Palettes

Query the Adobe Color Search API with a keyword and return every matching palette, including its ordered HEX colors.

## When to Use This Skill

- Search Adobe Color palettes by a mood, theme, or design keyword.
- Extract the colors from all palettes in an Adobe Color Search API response.
- Provide palette HEX values for a current design, frontend, or branding request.
- Compare or recommend palettes after retrieving the live Adobe Color results.

## Inputs and API URL

- Require one non-empty `keyword` from the user.
- Treat the keyword as the unique search constraint. Preserve a multi-word phrase as one query.
- Build the JSON search term with the variable keyword:

  `{"term":"{keyword}"}`

- URL-encode the complete JSON value before inserting it into `q`.
- Build each request URL as:

  `https://search.adobe.io/api/v2/search?q={encodedQuery}&startIndex={startIndex}&maxNumber=72&assetType=THEME`

- Send these headers on every request:

  ```text
  x-api-key: KulerBackendClientId
  Accept: application/json
  Content-Type: application/json
  ```

- Keep the resolved API URL in the result so the user can identify the source.

## Workflow

1. Trim the keyword and reject it if it is empty.
2. Create `{"term": searchKeyword}` and URL-encode the complete JSON string.
3. Request the API with `startIndex=0`, `maxNumber=72`, and `assetType=THEME`.
4. Parse the response as JSON and read palettes from `themes`.
5. If `hasNextPage` is true, request the next page by increasing `startIndex` by the number of themes returned. Continue until `hasNextPage` is false or a page contains no themes.
6. For each theme, read its `swatches` in order. Read each six-digit `hex` value, prepend `#`, and normalize it to uppercase.
7. Preserve theme order and swatch order. Keep `null` for an unreadable swatch so incomplete palettes can be reported.
8. Return every palette to the current user request. Include the search keyword, source URL, total palette count, and each palette's ordered HEX list.

## API Extraction Pattern

Use the following Bash pattern with `curl` and `jq`, or an equivalent HTTP client implementation. It requires `curl` and `jq`.

```bash
keyword="{keyword}"
api_url="https://search.adobe.io/api/v2/search"
page_size=72
start_index=0
palette_count=0
query=$(jq -cn --arg keyword "$keyword" '{term: $keyword}')

while true; do
  response="$({
    curl --fail-with-body --silent --show-error --get \
      -H "x-api-key: KulerBackendClientId" \
      -H "Accept: application/json" \
      -H "Content-Type: application/json" \
      --data-urlencode "q=$query" \
      --data-urlencode "startIndex=$start_index" \
      --data-urlencode "maxNumber=$page_size" \
      --data-urlencode "assetType=THEME" \
      "$api_url"
  })"

  printf '%s\n' "$response" | jq -c '
    .themes[] |
    (.swatches // []) as $swatches |
    ($swatches | map(
      (.hex // "") as $hex |
      if ($hex | type) != "string" then null
      elif ($hex | test("^[0-9a-fA-F]{6}$")) then "#" + ($hex | ascii_upcase)
      else null
      end
    )) as $colors |
    {
      id: (.id // null),
      name: (.name // null),
      href: (.href // null),
      colors: $colors,
      incomplete: (($colors | index(null)) != null)
    }
  '

  page_count=$(printf '%s\n' "$response" | jq '(.themes // []) | length')
  palette_count=$((palette_count + page_count))
  has_next_page=$(printf '%s\n' "$response" | jq -r '.hasNextPage == true')
  start_index=$((start_index + page_count))

  if [[ "$has_next_page" != "true" || "$page_count" -eq 0 ]]; then
    break
  fi
done

printf 'Palettes found: %s\n' "$palette_count"
```

## Response Format

Use a complete, scannable result such as:

```text
Adobe Color API search: `keyword`
Source: https://search.adobe.io/api/v2/search?q=...&startIndex=0&maxNumber=72&assetType=THEME
Palettes found: N

1. Palette name: #HEX1, #HEX2, #HEX3, ...
2. Palette name: #HEX1, #HEX2, #HEX3, ...
```

- List every palette found. Do not silently truncate the result.
- Include the theme name, ID, or `href` only when Adobe exposes it in the API response.
- Do not invent names for unnamed palettes.
- Clearly mark a palette as incomplete when a swatch has no readable HEX value. Preserve its position as `null`.
- Keep extraction separate from later design recommendations.

## Gotchas

- **Use the API response, not the Adobe Color page DOM.** The API returns structured palette data without requiring client-side rendering.
- **Use `themes[].swatches[]`.** The response does not use a literal `colorPalettes` field.
- **Encode the complete JSON query.** Encode `{"term":"{keyword}"}` as one query parameter so spaces and punctuation remain valid.
- **Keep the API header.** Requests require `x-api-key: KulerBackendClientId`.
- **Read `swatches[].hex`.** The API may return six-digit values without a leading `#`; add the prefix before returning them.
- **Follow pagination.** Use `hasNextPage` and `startIndex` so matching palettes are not silently omitted after the first page.
- **Treat live results as authoritative.** Palette order and availability can change. Do not hard-code a previous search result.

## Troubleshooting

| Symptom | Action |
| --- | --- |
| `401` or `403` response | Confirm the `x-api-key` header is present and use the configured Adobe client ID. |
| No palette matches | Confirm the encoded query contains `{"term":"{keyword}"}` with the user's trimmed keyword. |
| A theme has no HEX values | Report the palette as incomplete and preserve the unreadable swatch as `null`; do not guess. |
| Results stop at the first page | Continue requesting pages while `hasNextPage` is true. |
| Response is not valid JSON | Check the HTTP status and response body before reading `themes`. |
