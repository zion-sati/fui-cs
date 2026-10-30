# FontStack

class in `FuiCs`

A primary font with fallbacks for glyphs it lacks.

## Properties

- `Face`: The primary font.
- `Id`: The primary font's id, which the stack is known by.
- `IsLoaded`: Whether the primary font and every fallback have loaded.

## Methods

- `Fallback(FontFace)`: Adds `face` as a fallback, tried after the fonts already in the stack.
- `Fallback(FontStack)`: Adds `stack`'s primary font as a fallback, tried after the fonts already in the stack.
- `FallbackLoaded(string, uint)`: Loads a font from `url` under `fontId` and adds it as a fallback.
- `FallbackLoaded(string)`: Loads a font from `url` and adds it as a fallback.
- `Load(string, uint)`: A stack of the font at `url`, loaded under `id`.
- `Load(string)`: A stack of the font at `url`, loaded under a newly allocated id.
- `OnLoaded(Action<FontStackLoadedEventArgs>)`: Runs `handler` once the primary font and every fallback have loaded.
- `RequiredFontIds()`: The primary font and its fallbacks, without duplicates.
