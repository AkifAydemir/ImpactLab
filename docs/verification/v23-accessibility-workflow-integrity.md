# v23 portable WPF accessibility/workflow integrity

Vol23 adds a Python-standard-library source-contract gate for the canonical WPF accessibility workflow while the .NET/Windows UI runtime remains unavailable.

## Repair discovered by the gate

The v16 sample and `AccessibilityWorkflowCatalog` both require automation name `Engineering viewport`, but the current `MainWindow.xaml` did not expose that automation name. Vol23 adds the existing required automation name to the Simulation workspace surface. This is a source-contract repair, not executed UI Automation evidence.

## Canonical gate

`eng/accessibility_integrity.py` verifies:

- v16 sample schema/required-step identity against `AccessibilityWorkflowCatalog`;
- all required sample automation names are present in `MainWindow.xaml`;
- cyclic keyboard tab navigation and polite live-region wiring remain present;
- shell construction/injection of announcement and traversal services;
- viewport selection/clear and large-result load announcements;
- keyboard traversal, shortcut and command-routing source contracts;
- architecture wording remains explicitly bounded to future compiled UI verification.

The audit is wired into `.github/workflows/verify.yml` before the .NET toolchain stage and emits `artifacts/verification/accessibility-integrity.json`.

## Claim boundary

PASS means canonical source/sample accessibility contracts are internally consistent. It does **not** mean Windows UI Automation, focus order, high-contrast rendering, keyboard-only task completion, screen-reader narration, accessibility-tree semantics or WPF runtime behavior have been executed or validated.
