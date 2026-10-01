# Changelog

## 2.0.0-alpha.1 — 2026-10-01

- Rebuild the incomplete .NET Framework 2.0 project as separate .NET 10 Core, Windows and WinForms projects.
- Normalize optional `.exe` suffixes and use ordinal, case-insensitive exact/contains rules.
- Introduce stopped/preview startup, explicit termination confirmation and non-overlapping asynchronous monitoring.
- Validate process identity and critical status using the same native handle before termination.
- Skip protected/unverified processes, report per-process errors, and count confirmed exits only.
- Persist enabled rules and the check interval in versioned JSON under LocalAppData with temporary-file replacement.
- Preserve invalid settings and prevent failed saves from changing active rules.
- Add filtering, identity-based selection, an Activity tab, rule controls and a real notification-area icon.
- Add Core regressions, owned-child native Windows tests, UI smoke checks, screenshots and x64/x86 CI builds.
- Preserve the original source under `legacy/v1` and remove tracked user-specific project settings from the active tree.

## 1.0 — 2019-12-23

- Original process-name blacklist, exact/contains modes, a process table and timer-based termination.
- Historical source is retained in Git history and `legacy/v1`.
