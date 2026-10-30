# FontFace

record struct in `FuiCs`

A font the host has loaded or is loading, identified by its font id.

## Properties

- `IsLoaded`: Whether the host has loaded the font.

## Methods

- `IsFontLoaded(uint)`: Whether the host has loaded the font with `fontId`.
- `Load(string, uint)`: Loads a font from `url` under `id`.
- `Load(string)`: Loads a font from `url` under a newly allocated id.
- `OnLoaded(Action<FontFaceLoadedEventArgs>)`: Runs `handler` once the font has loaded, or now if it already has.
- `WhenFontsLoaded(IReadOnlyList<uint>, Action<FontsLoadedEventArgs>)`: Runs `handler` once every font in `fontIds` has loaded.
- `WhenLoaded(uint, Action<FontFaceLoadedEventArgs>)`: Runs `handler` once the font with `fontId` has loaded, or now if it already has.
