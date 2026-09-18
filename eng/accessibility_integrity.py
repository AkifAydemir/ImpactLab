#!/usr/bin/env python3
from __future__ import annotations
import argparse, json, re, sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, asdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

@dataclass
class Check:
    name: str
    status: str
    detail: str
class Audit:
    def __init__(self): self.results=[]
    def ok(self,n,d): self.results.append(Check(n,'passed',d))
    def fail(self,n,d): self.results.append(Check(n,'failed',d))
    @property
    def passed(self): return all(x.status=='passed' for x in self.results)

def read(root, rel): return (root/rel).read_text(encoding='utf-8')

def missing_fragments(text, fragments):
    compact_text=re.sub(r'\s+','',text)
    return [f for f in fragments if f not in text and re.sub(r'\s+','',f) not in compact_text]

def parse_catalog(text:str):
    pat=re.compile(r'new\s*\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*"([^"]+)"\s*,\s*([^,\)]*?)(?:\s*,\s*(true|false))?\s*\)')
    out=[]
    for m in pat.finditer(text):
        out.append({'id':m.group(1),'name':m.group(2),'automationName':m.group(3),'shortcut':m.group(4).strip(),'required': m.group(5)!='false'})
    return out

def check_sample_catalog(root,audit):
    sample=json.loads(read(root,'samples/v16/accessibility-workflow.json'))
    catalog=parse_catalog(read(root,'src/ImpactLab.App/Accessibility/AccessibilityWorkflowCatalog.cs'))
    failures=[]
    if sample.get('schemaVersion')!=1: failures.append(f"sample schemaVersion={sample.get('schemaVersion')}, expected 1")
    ids=[x['id'] for x in catalog]
    if len(ids)!=len(set(x.casefold() for x in ids)): failures.append('catalog ids are not case-insensitively unique')
    byid={x['id']:x for x in catalog}
    for step in sample.get('steps',[]):
        c=byid.get(step.get('id'))
        if c is None: failures.append(f"sample step {step.get('id')!r} missing from catalog"); continue
        if step.get('automationName')!=c['automationName']: failures.append(f"{step['id']}: automationName drift")
        if bool(step.get('required'))!=bool(c['required']): failures.append(f"{step['id']}: required drift")
    if failures: audit.fail('sample-catalog-contract','; '.join(failures))
    else: audit.ok('sample-catalog-contract',f"schema v1; {len(sample['steps'])} sample steps resolve into {len(catalog)} unique catalog steps with matching automation names/required flags.")
    return sample,catalog

def check_xaml(root,sample,audit):
    failures=[]
    path=root/'src/ImpactLab.App/MainWindow.xaml'
    try: ET.parse(path)
    except Exception as e: failures.append(f'XAML XML parse failed: {e}')
    text=read(root,'src/ImpactLab.App/MainWindow.xaml')
    if 'KeyboardNavigation.TabNavigation="Cycle"' not in text: failures.append('root cyclic keyboard tab navigation missing')
    names=set(re.findall(r'AutomationProperties\.Name="([^"]+)"',text))
    for step in sample.get('steps',[]):
        if step.get('required') and step.get('automationName') not in names:
            failures.append(f"required automation name {step.get('automationName')!r} missing from MainWindow.xaml")
    if text.count('AutomationProperties.LiveSetting="Polite"')<2: failures.append('expected at least two polite live regions')
    if failures: audit.fail('wpf-automation-surface','; '.join(failures))
    else: audit.ok('wpf-automation-surface',f"All {sum(1 for x in sample['steps'] if x.get('required'))} required sample automation names exist in XAML; cyclic tab navigation and >=2 polite live regions remain wired.")

def check_shell_wiring(root,audit):
    text=read(root,'src/ImpactLab.App/ViewModels/ProductShellViewModel.cs')
    failures=[]
    for f in missing_fragments(text,['Announcements=new AccessibilityAnnouncementService()','Traversal=new KeyboardTraversalService()','Traversal.Configure(AccessibilityWorkflowCatalog.Default.Select(x=>x.AutomationName))','LargeResults=new LargeResultWorkspaceViewModel(Announcements)','Announcements.Announce($"Workspace changed to {value}.")']):
        failures.append(f'missing {f!r}')
    if failures: audit.fail('shell-accessibility-wiring','; '.join(failures))
    else: audit.ok('shell-accessibility-wiring','Shell constructs announcement/traversal services, configures catalog order, injects announcements into large-results and announces workspace changes.')

def check_selection_results(root,audit):
    failures=[]
    picker=read(root,'src/ImpactLab.App/Authoring/ViewportPickingCoordinator.cs')
    large=read(root,'src/ImpactLab.App/Visualization/LargeResultWorkspaceViewModel.cs')
    narrator=read(root,'src/ImpactLab.App/Accessibility/AccessibleSelectionNarrator.cs')
    workflow_audit=read(root,'src/ImpactLab.App/Accessibility/AccessibilityWorkflowAudit.cs')
    for f in missing_fragments(picker,['_a11y.Announce("Viewport selection cleared.")','_a11y.Announce($"Selected {hit.ObjectId} using {hit.Backend} picking.")']):
        failures.append(f'viewport picker missing {f!r}')
    if '_a11y.Announce($"Loaded {_field} frame {_frame}, range {Minimum:G4} to {Maximum:G4}.")' not in large: failures.append('large-result load announcement missing')
    for f in ['"No objects selected."','objects selected. Primary selection']:
        if f not in narrator: failures.append(f'selection narrator missing {f!r}')
    for f in ['Automation name is missing.','Required workflow step has no accessible name.','Duplicate accessibility workflow ids.']:
        if f not in workflow_audit: failures.append(f'workflow audit missing {f!r}')
    if failures: audit.fail('announcement-and-workflow-contract','; '.join(failures))
    else: audit.ok('announcement-and-workflow-contract','Viewport selection/clear and large-result loading remain announced; selection narration and workflow blocking checks remain present.')

def check_keyboard(root,catalog,audit):
    failures=[]
    traversal=read(root,'src/ImpactLab.App/Accessibility/KeyboardTraversalService.cs')
    profile=read(root,'src/ImpactLab.App/Input/KeyboardShortcutProfile.cs')
    router=read(root,'src/ImpactLab.App/Input/WorkspaceCommandRouter.cs')
    for f in missing_fragments(traversal,['Distinct(StringComparer.OrdinalIgnoreCase)','Current=_order.FirstOrDefault()','reverse?-1:1']):
        failures.append(f'traversal missing {f!r}')
    for f in missing_fragments(profile,['ModifierKeys.Control,"Save"','ModifierKeys.Control,"Undo"','ModifierKeys.Control,"Redo"','Key.Delete,ModifierKeys.None','Key.F,ModifierKeys.None,"Fit view"']):
        failures.append(f'shortcut profile missing {f!r}')
    for f in missing_fragments(router,['Keyboard.Modifiers','e.Handled=true','return true']):
        failures.append(f'command router missing {f!r}')
    if failures: audit.fail('keyboard-navigation-contract','; '.join(failures))
    else: audit.ok('keyboard-navigation-contract',f"Traversal, shortcut profile and handled command routing source contracts remain present across {len(catalog)} catalog workflow steps.")

def check_docs(root,audit):
    text=read(root,'docs/architecture/v16-wpf-large-result-accessibility.md')
    failures=[]
    for f in ['accessibility announcements','async result-store contract','keyboard traversal service','WPF AutomationProperties','ready for compiled UI verification']:
        if f not in text: failures.append(f'architecture doc missing {f!r}')
    if failures: audit.fail('architecture-claim-boundary','; '.join(failures))
    else: audit.ok('architecture-claim-boundary','Architecture doc keeps source-level workflow evidence explicitly bounded to future compiled UI verification.')

def write(path,audit,summary):
    payload={'schemaVersion':1,'generatedUtc':datetime.now(timezone.utc).isoformat().replace('+00:00','Z'),'status':'passed' if audit.passed else 'failed','summary':summary,'checks':[asdict(x) for x in audit.results]}
    path.parent.mkdir(parents=True,exist_ok=True); path.write_text(json.dumps(payload,indent=2)+"\n",encoding='utf-8')

def main():
    ap=argparse.ArgumentParser(); ap.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[1]); ap.add_argument('--output',type=Path,default=Path('artifacts/verification/accessibility-integrity.json')); a=ap.parse_args()
    root=a.root.resolve(); out=a.output if a.output.is_absolute() else root/a.output; audit=Audit()
    try:
        sample,catalog=check_sample_catalog(root,audit); check_xaml(root,sample,audit); check_shell_wiring(root,audit); check_selection_results(root,audit); check_keyboard(root,catalog,audit); check_docs(root,audit)
    except Exception as e: audit.fail('audit-execution',repr(e)); sample={'steps':[]}; catalog=[]
    summary={'sampleSteps':len(sample.get('steps',[])),'catalogSteps':len(catalog),'requiredSampleSteps':sum(1 for x in sample.get('steps',[]) if x.get('required'))}
    write(out,audit,summary)
    for r in audit.results: print(f'[{r.status.upper()}] {r.name}: {r.detail}')
    print(f"[{'PASS' if audit.passed else 'FAIL'}] accessibility-integrity -> {out}")
    return 0 if audit.passed else 2
if __name__=='__main__': sys.exit(main())
