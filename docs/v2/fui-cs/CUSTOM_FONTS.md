# FUI-CS Custom Fonts

FUI-CS models typography with three types:

- `FontFace`: one font file the host loads, known by its id;
- `FontStack`: a primary face with ordered fallbacks for the glyphs it lacks,
  such as emoji or other scripts;
- `FontFamily`: the stacks of one family by weight and style (`Regular`,
  `Bold`, `Italic`, `BoldItalic`, `Medium`, `Semibold`, ...), from which a
  weight and style resolve to the closest face.

Applications do not pick font ids; `Load` allocates them.

## Loading fonts

```csharp
var emoji = FontFace.Load("/fonts/NotoColorEmoji.ttf");
var body = FontStack.Load("/fonts/DejaVuSans.ttf").Fallback(emoji);
var bold = FontStack.Load("/fonts/DejaVuSans-Bold.ttf").Fallback(emoji);
var family = FontFamily.RegularBold(body, bold);

var heading = new Text("Release notes 🚀").FontFamily(family).FontWeight(FontWeight.Bold).FontSize(22.0f);
var note = new Text("Set in one stack").FontStack(body, 16.0f);
```

1. Publish each font file with the application, and load it by URL; a
   relative URL resolves against the page's address.
2. Load each face with `FontFace.Load(url)`, or a stack's primary face with
   `FontStack.Load(url)`.
3. Add fallbacks in order with `Fallback(face)`, `Fallback(stack)` or
   `FallbackLoaded(url)`.
4. Build a `FontFamily` from the stacks when weights or styles should resolve
   to different faces: `FontFamily.RegularBold(regular, bold)`, or the
   constructor for every style.
5. Apply it with `FontFamily(...)` on text nodes, spans and controls, or a
   single stack with `FontStack(stack, size)`, or put it in the theme's
   `Fonts` so every control uses it.

<!-- example: FontFamily family, FontStack body -->
```csharp
var theme = Theme.Active.Value;
Theme.UseCustom(theme with { Fonts = theme.Fonts with { BodyFamily = family, BodyStack = body } });
```

## The built-in fonts

The host ships Noto Sans (regular, bold, italic and bold italic), Noto Sans
Mono (regular and bold), and Noto Emoji and Noto Sans Symbols 2 as fallbacks
for all of them. The default theme uses them (`Theme.Active.Value.Fonts`:
`BodyFamily`, `HeadingFamily`, `MonoFamily`); there is no need to load them.
The sans-serif faces load with the page, and the monospace faces the first
time text uses them.

## Readiness

Fonts load asynchronously. Retained `Text` and `RichText` nodes reshape
themselves when a face they use arrives, so a page needs no loading screen or
manual refresh for them.

Text drawn outside the tree waits for its fonts:

- a `TextLayout` raises `OnReady` once its fonts are available; a
  `CustomDrawable` invalidates from it (see
  [Custom drawing](./CUSTOM_DRAWING_AND_BITMAPS.md#text-in-a-drawing));
- `Bitmap.WhenTextReady(node, ...)` waits for a node's fonts before rendering
  it into a bitmap.

To wait yourself, `FontFace.OnLoaded`, `FontStack.OnLoaded` and
`FontFace.WhenFontsLoaded(family.RequiredFontIds(), ...)` run a handler once
the fonts have loaded, or at once if they already have. `IsLoaded` reports it.

## Fallback coverage

One Latin face does not cover emoji, CJK, Arabic, Thai or symbols. Put the
faces you supply first and order the fallbacks by the scripts you expect. Use
a real monospaced face where text must line up in columns.

## Serving fonts

The browser fetches fonts as other assets: serve them with a font MIME type
(`font/ttf`, `font/woff2`), from the same origin or with CORS headers, and with
cache headers suited to files that rarely change.
