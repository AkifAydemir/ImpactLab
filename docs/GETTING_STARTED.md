# First successful session

## Portable preview (no development tools on the recipient's PC)

The maintainer can run `powershell -NoProfile -File tools/package.ps1` to build,
test and create a new ZIP under `artifacts/` with a SHA-256 digest. Extract the
whole ZIP before launching the executable; do not run it inside the archive.
These are unsigned preview builds, not certified installers. Never disable OS
security checks to launch them. Native components may require Microsoft's
Visual C++ 2015–2022 x64 runtime. No clean-machine installation claim is made.

## Build from source

1. Install .NET 8 SDK on Windows 10/11.
2. From the repository root run: `powershell -NoProfile -File tools/check.ps1`.
3. Launch: `dotnet run --project src/ImpactLab.App/ImpactLab.App.csproj -c Release --no-build`.
4. Open Simulation → Quick Impact Sandbox. Start with the built-in preset; use 20 mm mesh cells and 1.5 µs time step for the documented demonstration.
5. Run Quick Impact, inspect the result and validity diagnostics before exporting.

## Understand the output

A completed run establishes that the software workflow completed, not that a real structure would behave identically. Read material assumptions, mesh resolution, time-step and energy diagnostics. Damage colors are model-dependent fields, not a measured probability of failure.

## If something goes wrong

- App does not launch: WPF requires Windows; verify the .NET SDK installation and build result.
- Time-step warning: investigate the stability limit; do not merely hide it to obtain a result.
- Imported geometry gives unexpected behavior: check units, watertightness, part placement and contact identity.
- MP4 unavailable: video export requires FFmpeg. Stored frames/GIF are not extra physics calculations.

See [validation](VALIDATION.md) for the evidence boundary and [physics architecture](PHYSICS_ENGINE_ARCHITECTURE.md) for model limitations.
