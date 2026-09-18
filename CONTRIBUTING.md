# Contributing

ImpactLab is an experimental engineering workbench. Changes should keep numerical assumptions explicit, preserve deterministic evidence, and avoid presenting an unverified model as physically validated.

## Local quality gate

Requirements: Windows, PowerShell, Python 3, and the .NET 8 SDK.

```powershell
dotnet tool restore
dotnet tool run csharpier check .
python .\eng\source_integrity.py
python .\eng\artifact_integrity.py
python .\eng\external_mesher_integrity.py
python .\eng\process_ipc_integrity.py
python .\eng\accessibility_integrity.py
python .\eng\numerical_reference_integrity.py
.\eng\verify.ps1
```

To apply the repository formatting rules before rerunning the checks:

```powershell
dotnet tool run csharpier format .
```

## Pull requests

- Keep a change focused and explain the engineering assumption or contract it affects.
- Add or update tests for behavior changes, especially solver, migration, IPC, and security boundaries.
- Record new numerical reference data with its units, tolerance, provenance, and claim boundary.
- Do not commit `bin`, `obj`, test results, or generated verification evidence.
