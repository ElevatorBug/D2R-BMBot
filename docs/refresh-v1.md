# Desktop refresh, first increment

Base: `bouletmarc/D2R-BMBot`, commit
`4ec5d63213c5f668e2545c29e746e2da2461220e`.

## Changes

- Shared light palette for the main window and five settings/tools dialogs.
- Flat buttons with hover/pressed feedback, white input and log surfaces,
  alternating grid rows and clearer selection contrast.
- System high-contrast settings take priority over the custom palette.
- Existing control sizes, fonts, diagnostic collision rendering and native
  tabs remain in place. The transparent overlay uses its existing appearance.
- Main log handling moved to `Form1.Logging.cs`. Existing `method_1` callers
  remain compatible; new callers can use the descriptive `AppendLog` entry point.
- The background-thread dispatch now preserves `LogTime = false`. Previously
  it called the method without that argument, adding an unwanted timestamp.

## Build and review

Use Windows with Visual Studio and the .NET Framework 4.7.2 targeting pack.
Restore NuGet dependencies and build `app.sln` in `Release | x64`.
This increment does not change the target framework or dependencies.

Source-level checks: project XML parses; new sources are explicitly included;
the patch passes `git -c core.whitespace=cr-at-eol diff --check`
(preserving upstream CRLF files) and can be applied to the stated base.
A Windows build and visual check are still required. This work was prepared
on Linux without a .NET compiler or Windows desktop runtime; it is not a
verified runnable release.

For the Windows review, check every settings dialog for clipped labels and
button states, log colors and diagnostic map alignment. Check the OS
high-contrast mode. Exercise `AppendLog` from the UI and a background thread
with `includeTime` both true and false; false must not add a timestamp.

## Next increments

1. Establish a reproducible Windows build and CI before migrating frameworks.
2. Extract settings loading/validation from the main form.
3. Separate UI presentation from session orchestration through narrow interfaces.
4. Add recorded-state fixtures to check components without a running game.
5. Rework navigation and layout after the existing behavior has been verified.

The first increment preserves the existing gameplay and process-access code.
UI and code organization changes make no claim about anti-cheat detection.
