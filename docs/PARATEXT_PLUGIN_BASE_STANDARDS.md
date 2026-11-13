# Paratext Plugin Development Standards & Architecture Guidelines

> **Purpose**: This document serves as the master architectural specification and operating standard for developing, packaging, installing, securing, and maintaining plugins for **SIL/UBS Paratext 9.x**. It is derived from the real-world engineering, safety, and end-user requirements established during the development of the *Number Conversions* plugin.

---

## Table of Contents
1. [Critical Safety & Security Measures](#1-critical-safety--security-measures)
2. [Turnkey Installation Architecture (End-User Experience)](#2-turnkey-installation-architecture-end-user-experience)
3. [Clean Uninstallation & Lifecycle Management](#3-clean-uninstallation--lifecycle-management)
4. [Paratext Plugin Technical Architecture](#4-paratext-plugin-technical-architecture)
5. [Scripture Tokenization & USFM Handling](#5-scripture-tokenization--usfm-handling)
6. [UI, WinForms & Review Grid Best Practices](#6-ui-winforms--review-grid-best-practices)
7. [GitHub, Versioning & CI/CD Release Automation](#7-github-versioning--cicd-release-automation)
8. [Linguistic, Sandhi & Orthographic Engineering](#8-linguistic-sandhi--orthographic-engineering)
9. [Automated Testing & Regression Invariants](#9-automated-testing--regression-invariants)

---

## 1. Critical Safety & Security Measures

### 1.1 Complete Isolation of Paratext Project Directories
* **Strict Off-Limits Rule**: Never allow automated scripts, batch tools, or background workers to write, overwrite, move, or delete files directly inside **`My Paratext 9 Projects`** (e.g. `C:\My Paratext 9 Projects`, `D:\My Paratext 9 Projects`).
* **Use Official Paratext APIs Only**: All scripture modifications must pass strictly through the sanctioned `PluginInterfaces` APIs (`IProject.PutUSFMToken`, `IProject.CreateUndoPoint`, `IProject.WriteText`, etc.). This ensures Paratext maintains internal integrity, version histories, and project locking.

### 1.2 Protection of Paratext Core System Files
* **Never Touch Core Folders**: Never alter files in `C:\Program Files\Paratext 9` or `D:\Transdata\Paratext 9` (such as `Paratext.exe`, core SIL DLLs, dictionaries, or configuration files).
* **Restricted Plugin Boundary**: Plugins must reside strictly within their own designated subfolder:
  `{ParatextPath}\plugins\{PluginName}\`

### 1.3 Zero Autonomous Scripture Modification
* **Translator Agency is Paramount**: An automated tool must **never** perform unattended, autonomous, or batch scripture revisions in the background without explicit human translator review.
* **Review Grid Mandate**: Every single proposed revision must be displayed in a review grid where translators can inspect, select, or uncheck individual items before committing.
* **Suspicious/Ambiguous Items Unchecked by Default**: Any item involving spelling errors, missing letters, or malformed punctuation must be highlighted (e.g., light amber) and **unchecked by default**, forcing deliberate human translator approval.

### 1.4 Automatic Paratext History / Rollback Checkpoints
* Before committing any approved batch revisions to a book, the plugin must call `project.CreateUndoPoint(description, true)`:
  ```csharp
  project.CreateUndoPoint($"Apply Number Conversions ({approved.Count} items)", true);
  ```
* This guarantees that a translator can immediately rollback the entire batch revision using Paratext's native *Project History* (*Undo / Project History*) without data loss.

---

## 2. Turnkey Installation Architecture (End-User Experience)

### 2.1 The Non-Technical End-User Principle
* End-users are field Bible translators, mother-tongue speakers, and translation consultants—**not software developers**.
* They will not use terminal consoles, package managers, development IDEs, or AI agent harnesses.
* The installation experience must be a **1-click, completely automated process**.

### 2.2 Dual-Layer Batch + PowerShell Execution
Windows default settings often restrict running `.ps1` scripts directly on double-click. To guarantee 1-click execution:
* Provide an `install-plugin.bat` wrapper that automatically calls `install-plugin.ps1` with bypassed execution policy:
  ```cmd
  @echo off
  cd /d "%~dp0"
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-plugin.ps1"
  ```

### 2.3 Windows "Mark of the Web" / Zone.Identifier Security Bypass
* When end-users download a `.zip` archive from GitHub Releases or email, Windows NTFS automatically attaches a hidden security stream:
  `filename.dll:Zone.Identifier:$DATA`
* Because .NET Framework blocks assemblies with this stream from running, Paratext will **silently ignore** the plugin without displaying any error message!
* **Mandatory Installer Rule**: The installation script must recursively run `Unblock-File` on every binary before and after copying:
  ```powershell
  Get-ChildItem -Path $PSScriptRoot -Recurse | Unblock-File
  Get-ChildItem -Path $targetDir -Recurse | Unblock-File
  ```

### 2.4 Paratext Process Detection & File-Locking Prevention
* Windows locks `.dll` and `.ptxplg` files when `Paratext.exe` has loaded them into memory. Attempting to copy over them results in `IOException: Access Denied`.
* The installer must check if `Paratext.exe` is running:
  - If running, display a clear prompt asking the user to close Paratext.
  - Automatically wait for the process to exit before attempting file operations.

### 2.5 Dual Path Discovery (Registry + Custom Paths)
Never hardcode `C:\Program Files\Paratext 9`. Discover the installation path using a fallback cascade:
1. Windows Registry: `HKLM:\SOFTWARE\ScrChecks\9.0\Settings_Directory` or `Paratext_Directory`.
2. Standard installation paths:
   - `D:\Transdata\Paratext 9`
   - `C:\Program Files\Paratext 9`
   - `C:\Program Files (x86)\Paratext 9`
   - Other drive letters (`E:`, `F:`)

### 2.6 Dual Cache Invalidation
Paratext caches installed add-ins in `%APPDATA%\Paratext9\AddIns.store`. The installer must:
* Touch or clear the cache store file so Paratext registers the plugin immediately on launch.
* Copy the standalone uninstaller script into the plugin directory alongside the binaries.

### 2.7 Native Windows Completion Dialogue
* End-users must never be left wondering if an installation finished.
* Show a native Windows GUI `MessageBox` on success:
  ```powershell
  Add-Type -AssemblyName System.Windows.Forms
  [System.Windows.Forms.MessageBox]::Show(
      "Plugin installed successfully!`n`nYou can now launch Paratext 9.",
      "Installation Complete",
      [System.Windows.Forms.MessageBoxButtons]::OK,
      [System.Windows.Forms.MessageBoxIcon]::Information
  )
  ```

---

## 3. Clean Uninstallation & Lifecycle Management

### 3.1 Tools Menu Cleanliness
* **Never Clutter Paratext Menus**: Do **not** register an "Uninstall Plugin" menu item in the main Paratext `Tools` or `Custom tools` menu.
* Placing an uninstaller in the main menu risks accidental uninstallation and adds permanent clutter to the translator's workspace.

### 3.2 In-App Dedicated Uninstallation Button
* Place the uninstaller button inside the plugin's own window (e.g., top-right corner of the plugin header) styled distinctly (e.g. muted red/coral border).
* Show a double confirmation prompt before triggering removal.

### 3.3 The In-Memory Self-Deletion Problem & Detached Process Solution
* A running .NET process cannot delete its own executing `.dll` or `.ptxplg` assembly.
* **The Detached Background Worker Pattern**:
  1. The in-app button triggers `PluginUninstaller.InitiateUninstall()`.
  2. It launches an independent, detached background PowerShell process (`-WindowStyle Hidden`):
     ```csharp
     var psi = new ProcessStartInfo
     {
         FileName = "powershell.exe",
         Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{uninstallScript}\" -PluginDir \"{pluginDir}\"",
         WindowStyle = ProcessWindowStyle.Hidden,
         UseShellExecute = true
     };
     Process.Start(psi);
     ```
  3. The plugin window closes itself immediately.
  4. The background worker waits for `Paratext.exe` to close, deletes the plugin directory, clears the cache store, and displays a clean completion confirmation.

### 3.4 Standalone Uninstaller Scripts
* Always bundle `uninstall-plugin.bat` and `uninstall-plugin.ps1` inside the release ZIP.
* Allows administrators or users to cleanly wipe the plugin if Paratext fails to launch.

---

## 4. Paratext Plugin Technical Architecture

### 4.1 Target Framework Standards
* **Paratext 9 Host**: Paratext 9.x is built on **.NET Framework 4.8** (`net48`).
* Any assembly loaded directly by Paratext must target **`net48`**.

### 4.2 Multi-Targeting for High-Performance Unit Testing
* **Problem**: Testing `.NET Framework 4.8` assemblies directly with modern testing tooling on non-Windows CI runners is painful.
* **Solution**: Split the solution into two projects:
  1. **`{PluginName}.Core`**: Contains all business logic, linguistic regex, converters, parsers, and sandhi algorithms.
     - Multi-targets: `<TargetFrameworks>netstandard2.0;net48</TargetFrameworks>`
  2. **`{PluginName}.Plugin`**: Contains Paratext `IPluginHost`, `PluginInterfaces`, WinForms UI, and host lifecycle.
     - Targets: `<TargetFramework>net48</TargetFramework>`
  3. **`{PluginName}.Tests`**: Contains unit and regression tests.
     - Targets: `<TargetFramework>net6.0</TargetFramework>` (or `net8.0`)
     - References `.Core` via `netstandard2.0`.
* **Benefit**: The test suite runs in sub-seconds (~80ms for 150 tests) using modern `dotnet test`, while the plugin binary compiles into clean .NET Framework 4.8 for Paratext.

### 4.3 The `.ptxplg` Binary Packaging
* A `.ptxplg` file is simply a standard .NET `.dll` renamed to `.ptxplg`.
* The MSBuild file should automate this rename in `AfterBuild`:
  ```xml
  <Target Name="GeneratePtxPlg" AfterTargets="Build">
    <Copy SourceFiles="$(TargetPath)" DestinationFiles="$(TargetDir)$(TargetName).ptxplg" />
  </Target>
  ```

---

## 5. Scripture Tokenization & USFM Handling

### 5.1 Strict Structural Exclusion Filter
Scripture contains footnotes, cross-references, introduction outlines, section titles, figures, and technical metadata. Number conversion must strictly exclude these.
* **Exclude Non-Scripture Tokens**:
  ```csharp
  if (!token.IsScripture || token.IsFootnoteOrCrossReference || token.IsFigure || token.IsMetadata)
      continue;
  ```
* **Exclude Section & Book Headings**:
  Tokens starting with `\s`, `\s1`, `\mt`, `\h`, `\toc` must be ignored.

### 5.2 Protection of Canonical Numbered Titles
* Never convert numbers in canonical book titles (e.g. `1 சாமுவேல்`, `2 சாமுவேல்`, `1 நாளாகமம்`, `2 கொரிந்தியர்`, `1 யோவான்`).
* Maintain a lookup `ScriptureProtectedEntities.NumberedBookTitles` to immediately skip any digit matching a canonical title prefix.

### 5.3 Token-Boundary Index Matching
* Always bind each `ConversionProposal` to its specific `BookNum`, `ChapterNum`, `VerseNum`, and exact token offset (`StartIndex`, `Length`).
* When applying replacements, match against the exact text slice in that verse to prevent accidental cross-verse substitutions.

---

## 6. UI, WinForms & Review Grid Best Practices

### 6.1 Avoiding WinForms Dynamic Layout Anchoring Bugs
* **The Bug**: Setting `Anchor = AnchorStyles.Top | AnchorStyles.Right` on a dynamically created control inside a panel before the parent is fully docked causes WinForms to calculate the right-offset against initial 200px width, pushing the control off-screen (e.g. `X=2270`).
* **The Rule**: Always position top-right or bottom-right action buttons dynamically on `Resize` and `Layout` events:
  ```csharp
  void PositionControls() {
      int right = topPanel.ClientSize.Width - button.Width - 15;
      button.Location = new Point(right, 13);
  }
  topPanel.Resize += (s, e) => PositionControls();
  ```

### 6.2 DataGridView Horizontal Scrollbar & Column Squeezing Prevention
* **The Bug**: Setting `AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill` forces the grid to cram all columns into the current viewport. Wide headers like `Type (வகை)` get squeezed into `Digit ➔ Wor...`, and horizontal scrollbars fail to appear.
* **The Standard**:
  ```csharp
  _grid.ScrollBars = ScrollBars.Both;
  _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
  ```
  Set explicit, generous column widths (e.g., Verse: 115px, Type: 185px, Preview: 280px) so the horizontal scrollbar activates naturally on smaller screens.

### 6.3 The 4-Rule Dynamic Impact Preview Standard
Instead of displaying lengthy sentence paragraphs in the review grid, use a smart, 4-rule **Impact Preview (மாற்ற முன்னோட்டம்)**:
1. **Rule 1 (Standard Number Only)**:
   - When only the number changes, display **only the replacement number** (e.g., `நூற்று முப்பது`, `44,760`).
2. **Rule 2 (Preceding Word Sandhi Changed)**:
   - Display the modified preceding word + number (e.g., `ஏறக்குறைய 10,000`).
3. **Rule 3 (Number Word Sandhi Changed)**:
   - Display the number with sandhi + following word (e.g., `பத்துப் பேர்`).
4. **Rule 4 (Compound Join with Next Word)**:
   - Display the fusion clearly using an arrow (e.g., `13 ஆம் ➔ பதின்மூன்றாம்`).

### 6.4 Bilingual Column Headers
Always provide bilingual (English + Vernacular) headers to accommodate both local native translators and international translation consultants:
- `Apply (ஏற்க)`
- `Verse (வசனம்)`
- `Type (வகை)`
- `Original (மூலம்)`
- `Proposed (மாற்றம்)`
- `Impact Preview (மாற்ற முன்னோட்டம்)`
- `Notes (குறிப்பு)`

---

## 7. GitHub, Versioning & CI/CD Release Automation

### 7.1 Semantic Version Tagging
* Every public release must have a semantic tag: `v1.0.0`, `v1.0.1`, etc.
* Always tag annotated tags with release notes:
  ```bash
  git tag -a v1.0.8 -m "Release v1.0.8 - Fused ordinal compounding"
  git push origin main --tags
  ```

### 7.2 Fully Automated GitHub Actions Release Workflow (`publish-release.yml`)
* **Trigger**: Automatic on `push: tags: ["v*.*.*"]`.
* **CI Steps**:
  1. Checkout code (`actions/checkout@v4`).
  2. Setup .NET (`6.0.x` and `8.0.x`).
  3. Restore solution dependencies (`dotnet restore`).
  4. Compile in Release mode with tag version (`dotnet build -c Release -p:Version=${{ env.VERSION }}`).
  5. Run test suite (`dotnet test -c Release --no-build`).
  6. Package distribution folder:
     - Plugin binaries (`NumberConversions.ptxplg`, `NumberConversions.Core.dll`)
     - 1-Click scripts (`install-plugin.bat`, `install-plugin.ps1`, `uninstall-plugin.bat`, `uninstall-plugin.ps1`)
     - Documentation (`README.md`, `LICENSE`)
  7. Compress into `NumberConversions-Paratext9-Plugin-v{VERSION}.zip`.
  8. Publish GitHub Release with assets using `softprops/action-gh-release@v2`.

### 7.3 Zero Untested Releases Invariant
* Never commit or push a release tag without verifying that **100% of unit tests pass**.
* If a single unit test fails, the CI/CD pipeline aborts immediately and creates no release assets.

---

## 8. Linguistic, Sandhi & Orthographic Engineering

### 8.1 Word-Boundary Invariant Protection
* **The Partial-Slicing Problem**: Slicing compound words mid-word creates ungrammatical monstrosities (e.g. `நாற்பத்துநான்காயிரத்து...` sliced into `நாற்பத்து4,760`).
* **The Invariant Rule**: A vernacular number scan must **never** start at a character immediately preceded by another vernacular letter:
  ```csharp
  if (startPos > 0 && IsVernacularCharacter(text[startPos - 1]))
      continue; // Skip mid-word candidate
  ```

### 8.2 Boundary Sandhi Phonology (புணர்ச்சி)
* In Dravidian languages (Tamil), consonants double (`க், ச், த், ப்`) across word boundaries.
* Native Tamil words never end with an isolated stop consonant (`க், ச், த், ப்`).
* When a following number word is replaced by digits (`10,000`), the preceding doubled stop is an orphan artifact that causes spelling squiggles.
* **The Rule**: The engine must detect and strip obsolete boundary stops before digits (e.g. `ஏறக்குறையப்` $\rightarrow$ `ஏறக்குறைய 10,000`).

### 8.3 Compounding / Postposition Joining (ஒட்டுச் சேர்க்கை)
* Ordinals (`-ஆம்`, `-ஆவது`) and case markers (`-இல்`, `-க்கு`, `-இலிருந்து`, `-ஐ`) written with space after digits (`13 ஆம்`, `14 ஆம்`) must not be left orphan as `பதின்மூன்று ஆம்`.
* The digit scanner must consume the number + postposition as a single unit and generate the correctly inflected, fused grammatical form:
  `13 ஆம்` $\longrightarrow$ **`பதின்மூன்றாம்`**

---

## 9. Automated Testing & Regression Invariants

### 9.1 Unit Test Hierarchy
Every Paratext plugin should maintain comprehensive tests organized into clear categories:
1. **Lexical Tests**: Verification of direct word mappings, compound tens-thousands decomposition, ordinals, and multipliers.
2. **Boundary Sandhi Tests**: Verification of preceding stop removal before digits and stop addition before words.
3. **Ordinal & Suffix Compounding Tests**: Multi-format verification (`13 ஆம்`, `13ஆம்`, `13-ஆம்`, `13 - ஆம்`, `13 ஆவது`, `13 இல்`, `13 க்கு`).
4. **Scripture Verse Regressions**: Real full-verse regression tests covering known complex cases (e.g., Genesis 5:3, Genesis 14:4–5, Judges 3:29, 1 Chronicles 5:18).
5. **USFM Structural Filter Tests**: Verification that footnotes, canonical book titles, and section headers are ignored.
6. **Typo / Variant Detection Tests**: Verification that ambiguous spellings are flagged as suspicious and never auto-applied.

---

### Summary Checklist for Future Paratext Plugins

- [ ] Core logic lives in a separate multi-targeted library (`netstandard2.0;net48`).
- [ ] Plugin assembly compiles into `.net48` and outputs `.ptxplg`.
- [ ] 1-Click `install-plugin.bat` + `install-plugin.ps1` with automatic `Unblock-File`.
- [ ] Paratext process detection prevents file-locking crashes.
- [ ] Clean in-app uninstaller button with detached background cleanup worker.
- [ ] No clutter in Paratext's main Tools menu.
- [ ] `My Paratext 9 Projects` and Paratext core installations are strictly off-limits.
- [ ] Every change requires human translator review in a dedicated UI grid.
- [ ] Paratext Undo / History checkpoint created before committing revisions.
- [ ] Grid features horizontal scrolling, bilingual headers, and smart Impact Previews.
- [ ] 100% test pass rate verified before every tag and GitHub Release publish.
