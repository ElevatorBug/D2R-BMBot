# Runtime separation

This increment keeps the existing process memory reader and game routines while
introducing independently testable seams for memory access, run selection and
player presentation. It builds on the desktop theme and logging refresh.

## Memory access

- `IProcessMemoryReader` isolates native reads from decoders and the main form.
  `WindowsProcessMemoryReader` uses pointer-sized process handles, read lengths
  and byte counts, and preserves the Windows error code.
- `Mem` receives process-handle and base-address providers at construction. Its
  existing typed/relative/raw helpers retain their names and zero-default return
  values. Each read owns its buffer; the form no longer shares `bufferRead`.
- Typed values and structures require a successful, complete read. Incomplete
  reads return zero defaults and emit `ReadFailed` with the address, requested
  length, actual length and error code. The UI throttles warnings to one per five
  seconds to avoid flooding the log during game transitions.
- Bulk `ReadMemory` preserves a reported valid prefix for the existing pattern
  scanner and clears the unread suffix. This is deliberately different from the
  exact-read helpers. The startup sequence now aborts when process attachment,
  module lookup or the initial read yields no usable memory.
- Buffer decoding rejects invalid widths and overflowing bounds; monster-stat
  size arithmetic is checked and incomplete stat buffers return no stats.
- The legacy write helper remains for source compatibility with existing code;
  this change does not introduce write calls or change process access rights.

## Run selection

`RunSequence` executes the first currently eligible step and at most one step per
tick. `Form1.RunSequence.cs` composes the existing routines in exactly their
previous order: 26 normal steps and 18 rush steps. Enable flags and completion
flags are evaluated each tick, rather than captured at construction.

The existing battle priority, leave-game fallback after normal runs, and idle
behavior after rush runs remain at their original call sites. Exceptions still
propagate; a failed step does not cause another routine to execute in that tick.

## Player presentation

`PlayerScan` publishes an immutable `PlayerStateSnapshot` after completing a
position/stat scan. The snapshot contains name, coordinates, life, mana, area,
difficulty, map seed and a UTC capture time. `LatestState` is published atomically.

`Form1.Presentation.cs` renders the four existing player grid rows on the UI
thread. It coalesces pending updates to the latest snapshot so frequent scans
cannot create a queue of stale UI updates, and guards dispatch during shutdown.

The snapshot represents sequential memory reads, not an atomic view of the game.
Existing game routines still consume legacy fields, and other scanners retain
form dependencies. This is the first migration seam, not a complete replacement
of the legacy architecture. Zero defaults still cannot distinguish a real zero
value from a failed read; failures are also available through the diagnostic
event.

## Validation

The standalone tests use a fake reader and do not require D2R, WinForms, native
Windows calls, or extra NuGet packages:

```powershell
msbuild tests\RuntimeTests.csproj /t:Rebuild
.\tests\bin\RuntimeTests.exe
```

All 11 tests passed under Mono: complete/relative reads and 64-bit handles,
short/failed reads, bulk prefixes and stale suffixes, invalid counts, 1,999
parallel reads, bounded strings, decoding bounds, monster stats, dynamic run
priority, sequence edge cases and snapshot immutability.

A structural check also confirmed that all 44 predicates/actions have exactly
the old dispatch order and that the project includes every new source file.
All application C# sources compiled against Mono's .NET 4.7.2 reference assemblies
and available repository dependencies, with existing compiler warnings. That
source check used the framework `System.Net.Http` reference and omitted missing
package references; it did not run the MSBuild resource or Fody packaging steps.
An unused, unresolved `System.Text.Json` import/reference was removed.

The full Windows build, native reads, game-version offsets and desktop behavior
still need validation on the target machine. Build the application with its
restored NuGet dependencies and the .NET Framework 4.7.2 developer pack, then
check start/stop, transitions between games, player grid updates and normal/rush
run order. The memory backend and UI dispatch cannot be runtime-tested on Linux.
