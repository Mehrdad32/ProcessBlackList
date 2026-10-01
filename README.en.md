# ProcessBlacklist

[فارسی](README.md) · [Test builds](https://github.com/Mehrdad32/ProcessBlackList/actions/workflows/ci.yml) · [Issues](https://github.com/Mehrdad32/ProcessBlackList/issues)

> The rebuilt **2.0.0-alpha.1** is on the main `master` branch. It remains an alpha for testing and feedback; the historical 2019 sources are preserved under `legacy/v1`.

ProcessBlacklist monitors process-name rules on Windows. Inspect matches in **Preview**, then explicitly enable automatic termination when the rules produce the intended results.

## Download and run

Open the latest successful `master` run in [Actions](https://github.com/Mehrdad32/ProcessBlackList/actions/workflows/ci.yml) and download an artifact:

| System | Artifact |
| --- | --- |
| 64-bit Windows | `ProcessBlacklist-win-x64` |
| 32-bit Windows | `ProcessBlacklist-win-x86` |
| Native UI screenshots with sample data | `ProcessBlacklist-ui-preview` |

Extract the ZIP and run `ProcessBlacklist.exe`. The packages include .NET. The app targets Windows versions supported by [.NET 10](https://learn.microsoft.com/en-us/dotnet/core/install/windows).

The app runs with the user's current privileges. Use **Run as Administrator** if Windows denies access to a process you intend to manage. It does not automatically relaunch another elevated instance.

## Usage

1. Filter the process list and click **Use selected name**, or enter a process name manually.
2. Choose **Exact name** or **Name contains text** and click **Add rule**. The `.exe` suffix is optional and matching ignores case.
3. Click **Start preview** to inspect Matches and Activity.
4. Stop preview, select **Terminate matching processes**, then confirm **Start termination** to enable enforcement.

Exact name matches every instance with that name, not one PID. Contains matches a name fragment: `pad` can match both `notepad` and `phonepad`. Paths, wildcards and regex are unsupported. If rules overlap, the first enabled matching rule handles the process.

**Preview does not terminate anything. Termination is forced and can discard unsaved work in the target app.** Inspect the preview before enabling it.

## Controls

| Control | Behavior |
| --- | --- |
| Start preview | Monitor rules and report matches without requesting termination |
| Start termination | Monitor and terminate eligible matches after confirmation |
| Stop | Prevent further actions and wait for the current operation |
| Refresh | Reload processes while monitoring is stopped |
| Add rule | Validate and persist a rule without starting monitoring |
| Enable / disable | Persist the selected rule's enabled state |
| Remove rule | Remove and persist the selected rule |
| End selected… | Confirm and terminate the selected process identity |
| Reset counters | Reset session termination counts |
| Minimize to tray | Hide the window while active monitoring continues |
| Settings folder | Open the rule storage directory |

Stop monitoring before editing rules, the mode or check interval. Every launch starts stopped, with Preview selected. Closing the window stops monitoring; minimizing to tray does not.

## Process identity and results

- PID, creation time and image path are rechecked before termination, using the same native handle for verification and the write. Recycled PIDs cannot redirect termination to another process.
- The app itself, reserved IDs, protected Windows process names and native critical processes are excluded.
- Unverifiable identities or critical status are skipped. One process's access error does not stop the remaining rules.
- **Closed** counts confirmed exits only. `Pending` means termination was requested but exit was not confirmed within 1.5 seconds.
- Stop cannot undo a termination request already issued. This is a polling desktop utility, not a service, driver or execution-prevention mechanism.

## Storage and Activity

Rules and the check interval live in `%LOCALAPPDATA%\ProcessBlacklist\settings.json`. Writes use a temporary file followed by replacement. Invalid or unsupported settings are preserved; the app displays recovery instructions instead of silently overwriting them.

Counters and Activity are session-only. Activity keeps at most 300 rows and suppresses unchanged results for the same process across consecutive scans.

## Build and test

Full builds require Windows and the **.NET 10 SDK**:

```powershell
dotnet build ProcessBlacklist.slnx -c Release
dotnet run --project tests/ProcessBlacklist.Core.Tests -c Release
dotnet run --project tests/ProcessBlacklist.Windows.Tests -c Release
dotnet run --project tests/ProcessBlacklist.Ui.Smoke -c Release -- ui-preview
```

The runners use `dotnet run`, not `dotnet test`. Core and UI tests use simulated processes. Native Windows tests create and terminate only their own child processes.

| Location | Responsibility |
| --- | --- |
| `src/ProcessBlacklist.Core` | Rules, validation, JSON storage, preview and monitoring execution |
| `src/ProcessBlacklist.Windows` | Native process identity reads and handle-based termination |
| `src/ProcessBlacklist.App` | WinForms UI, lists, Activity and tray |
| `tests` | Core, native and UI regression checks |
| `legacy/v1` | Historical 2019 sources, excluded from the new build |

[Manual Windows checks (Persian)](docs/testing.fa.md) · [Rebuild decisions](docs/rebuild.md) · [Changelog](CHANGELOG.md)
