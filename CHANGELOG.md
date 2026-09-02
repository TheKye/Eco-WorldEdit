# Changelog

## [3.0.3] - Unreleased

### Added

- Added support for preserving installed plugin modules on crafting stations through copy, paste, stack, move, undo/redo, and blueprint serialization. ([#97](https://github.com/TheKye/Eco-WorldEdit/issues/97))

### Changed

- Updated the required Eco reference assemblies to 0.14.0.3 beta release-1060.
- Centralized WorldEdit logging so player-visible errors and command exceptions are recorded in the server log, while successful command messages are also logged in debug builds.

### Fixed

- Added an Eco-version blueprint migration that corrects stair and roof block rotations when importing blueprints created before Eco 0.10.0.0. Missing rotated variants are logged and left unchanged. ([#89](https://github.com/TheKye/Eco-WorldEdit/issues/89))
- Fixed paste, rotation validation, and undo restoration for freely placed world objects whose exact positions are not aligned to the block grid.

## [3.0.2] - 2026-09-01

### Added

- Added an in-world visual highlight for completed selections, supporting dimensions of up to 8192 blocks per axis. Highlight objects are updated with the selection and cleaned up when users log out or after an unclean server shutdown. ([#6](https://github.com/TheKye/Eco-WorldEdit/issues/6))

### Fixed

- Added world-height validation throughout capture, restore, and batched placement. `paste`, `move`, and `stack` now reject destinations outside the valid world height before changing the world. ([#93](https://github.com/TheKye/Eco-WorldEdit/issues/93))
- Corrected unset-selection detection and ensured internal selection-highlight objects are ignored by capture, clearing, occupancy checks, and paid-item accounting.
- Made blueprint header detection handle short streams safely and restored the original stream position when the input is not LZ4-compressed.

### Changed

- Consolidated block-grid alignment and plant-block classification in `BlockUtils`, while retaining payload-based classification for blueprint and snapshot restoration.
- Standardized the help text and attribute formatting for all WorldEdit chat commands. Renamed `WEversion` and `WELooking` shortcuts to `weversion` and `welooking`.
- Corrected nullable contracts for history stack operations and removed the remaining compiler and analyzer warnings.

## [3.0.1] - 2026-08-31

### Fixed

- Restored wand interactions by explicitly targeting supported block tags, including liquids. ([#99](https://github.com/TheKye/Eco-WorldEdit/issues/99))
- Prevented the wand from being treated as a placeable surface item despite inheriting from `ToolItem`. ([#98](https://github.com/TheKye/Eco-WorldEdit/issues/98))

## [3.0.0] - 2026-08-30

### Highlights

- Reworked the command pipeline around isolated command contexts, consistent change tracking, execution timing, and protection from running two WorldEdit commands for the same user at once.
- Rebuilt `undo`/`redo` around captured world snapshots instead of re-running the original command. Multi-step history now records reverse changes directly and preserves a recovery snapshot when an operation fails after partially changing the world.
- Added selective `World.BatchApply` batching for ordinary block changes. Plants, water, world objects, occupied blocks, paid-item accounting, and other high-risk cases still use the safer specialized paths; this is intentionally not a full parallel execution model.
- Reworked clipboard and blueprint handling: format 1.4, explicit migrations from formats 1.1-1.3, typed block/component data, compatibility filtering for unavailable game types, and safer validation during import, rotation, and restore.
- Improved capture and restoration of world objects, including attached object hierarchies, exact positions, colors, and storage, custom text, mint, door, and store data. Claim stakes remain excluded for safety.
- Updated the project to Eco 0.14.0.2, .NET 10, nullable reference types, and version 3.0.0.

### Performance

`BatchApply` was chosen as the lower-risk optimization after evaluating broader parallel execution. In the measured cold scenario it produced the following results:

| Operation | Before | With `BatchApply` | Time reduction | Speedup |
| --- | ---: | ---: | ---: | ---: |
| First `/set` | 1736.96 ms | 1305.51 ms | 24.8% | 1.33x |
| `/undo` | 1132.13 ms | 727.15 ms | 35.8% | 1.56x |
| `/redo` | 1131.86 ms | 673.57 ms | 40.5% | 1.68x |

First-run `/set` throughput increased from approximately 79.7K to 106.0K blocks/s (+33%). `undo` increased from 122.2K to 190.3K blocks/s, and `redo` from 122.3K to 205.4K blocks/s. Repeated `set`, `undo`, and `redo` operations remained around 0.66-0.70 seconds in the same benchmark.

### Related issues

- [#35: parallel execute](https://github.com/TheKye/Eco-WorldEdit/issues/35) — partially addressed by selective batching. General `Parallel.ForEach` execution was not adopted because its limited expected benefit did not justify the correctness and thread-safety risks.
- [#94: Support for new placement system](https://github.com/TheKye/Eco-WorldEdit/issues/94) — `WorldObjectManyBlock` support is preserved and integrated into the new capture, restore, clearing, history, and clipboard transformation paths.
- [#93: World height validation](https://github.com/TheKye/Eco-WorldEdit/issues/93) — initial selection normalization landed in 3.0.0; complete capture, restore, and placement validation followed in 3.0.2.
