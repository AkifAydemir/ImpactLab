# ImpactLab source package

Run `scripts/package-source.ps1` from PowerShell to create `ImpactLab-source-latest.zip`
beside the repository. The archive preserves the `ImpactLab/` directory layout and includes
the current working tree, including uncommitted source files.

The package intentionally excludes Git internals, IDE metadata, build output, test output,
temporary Codex folders, and generated simulation artifacts. Restore the archive into any
workspace, then run the normal .NET restore, build, and test commands.
