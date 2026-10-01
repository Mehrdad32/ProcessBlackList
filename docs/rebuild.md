# Rebuild decisions

The first rebuild step preserves the utility's purpose: see running processes, create name rules, and optionally terminate matching processes. The original upload targets .NET Framework 2.0 and omits referenced `Properties`/resource files, so it cannot serve as a reproducible build.

## Changes in this step

| Original behavior | Rebuilt behavior |
| --- | --- |
| UI controls contain the monitoring logic | An independent Core returns snapshots and structured per-process actions |
| Manual input strips `.exe`, but context-menu rules retain it | Rule input is normalized once; snapshots retain their canonical executable base name |
| Exact mode is case-sensitive | Exact/contains use ordinal case-insensitive matching |
| UI timer refreshes the full grid repeatedly during termination | Each asynchronous scan reads one snapshot and updates the UI once |
| One error stops all monitoring | Per-process failures are reported and remaining matches continue |
| Several matches overwrite the same saved counter value | Each confirmed exit contributes once to the responsible rule's session count |
| PID/selection can become stale | Creation time and image path are rechecked; termination uses the verified native handle |
| Row indices are restored without bounds checks | Selection follows process identity; a vanished selection is cleared |
| Rules live only in UI rows | Validated, versioned JSON stores rules and the polling interval |
| Elevation can launch a second instance without closing the first | Run with current privileges; elevation is an explicit Windows launch choice |
| A minimize button only minimizes the window | A real tray icon provides Show, Stop and Exit |

## Matching and enforcement

Rules target the executable **base name**, not a full path, user account or PID. Optional `.exe` is removed from rule input only. The first enabled matching rule wins; overlapping rules do not duplicate termination. Processes that cannot be verified are still visible, with their inspection status.

The Windows platform opens a handle with query/terminate/synchronize rights, validates creation time and full image path, checks native critical status, and invokes `TerminateProcess` using that handle. It waits up to 1.5 seconds for confirmed exit. No process trees, services, scheduled tasks or startup entries are changed.

Native references: [GetProcessTimes](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getprocesstimes), [IsProcessCritical](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-isprocesscritical), [TerminateProcess](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-terminateprocess).

## Validation and remaining work

Core tests use a fake platform to cover matching, overlap, protection, identity failures, cancellation, serialization and JSON persistence. Native Windows tests use owned test children to check identity mismatches and real termination confirmation. UI smoke checks use simulated processes and produce screenshots.

Before a stable release, manually verify the Windows app, tray behavior, standard/elevated access and display scaling. File-path rules, export/import, persistent logs and alternative actions such as graceful closing can be considered after this foundation is tested; they are not claimed as current features.
