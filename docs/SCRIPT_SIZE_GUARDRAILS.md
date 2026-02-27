# Script Size Guardrails
Last Synced: 2026-02-27


## Purpose
- Keep runtime scripts readable and reviewable.
- Force responsibility boundaries before files turn into "god files".

## Thresholds
- Target size: `<= 300` lines per behavior script.
- Early warning: `>= 220` lines triggers split review.
- Hard review: `> 300` lines requires explicit reason in PR/commit notes.

## Split Rules
- Prefer splitting by responsibility, not by arbitrary line blocks.
- Good split examples:
  - lifecycle / state machine
  - apply/effect logic
  - eligibility/policy logic
  - ui binding / ui state / presenter
  - spawn scheduling / spawn placement / debug helpers

## Do Not Split For
- tiny helpers where extraction increases coupling.
- pure data/DTO files that are already stable and short.

## Refactor Checklist
- Build green: `dotnet build ProjectGenesis.sln`.
- No behavior drift in smoke flow for affected module.
- Update docs (`log.md`, `docs/TODO.md`) when boundaries change.
- Keep naming explicit (for example: `*.Lifecycle.cs`, `*.Apply.cs`, `*.Binding.cs`).

## Quick Audit Command
```powershell
Get-ChildItem Scripts -Recurse -Filter *.cs |
  ForEach-Object {
    [PSCustomObject]@{
      Path = $_.FullName
      Lines = (Get-Content $_.FullName | Measure-Object -Line).Lines
    }
  } | Sort-Object Lines -Descending | Select-Object -First 30
```
