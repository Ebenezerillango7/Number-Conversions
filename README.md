# Number Conversions - Paratext 9.5 Plugin

[![Build & Test](https://github.com/Ebenezerillango7/Number-Conversions/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/Ebenezerillango7/Number-Conversions/actions/workflows/build-and-test.yml)
[![Publish Release](https://github.com/Ebenezerillango7/Number-Conversions/actions/workflows/publish-release.yml/badge.svg)](https://github.com/Ebenezerillango7/Number-Conversions/actions/workflows/publish-release.yml)
[![Paratext Version](https://img.shields.io/badge/Paratext-9.5.110.11-blue.svg)](https://paratext.org)
[![Target Script](https://img.shields.io/badge/Language-Tamil%20%28%E0%AE%A4%E0%AE%AE%E0%AE%BF%E0%AE%B4%E0%AF%8D%29-green.svg)](#tamil-orthography-rules)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

An intelligent scripture editing plugin for **Paratext 9.5** designed to standardize and convert numbers in **Tamil script** Bible translation projects (`ta` / தமிழ்).

---

## 🌟 Key Features

1. **Selective Bidirectional Number Conversion**:
   - **Range 0 – 199 (Digit $\rightarrow$ Text)**: Converts digits and ordinals into Tamil text words.
     - *Cardinals*: `175` $\rightarrow$ `நூற்று எழுபத்தைந்து`, `12` $\rightarrow$ `பன்னிரண்டு`
     - *Ordinals*: `2nd` / `2-ஆம்` / `2வது` $\rightarrow$ `இரண்டாம்`
   - **Range 200 and above (Text $\rightarrow$ Digit)**: Converts Tamil spelled-out numbers into numeric digits with Indian thousand separators.
     - *Example*: `இருநூறு` $\rightarrow$ `200`
     - *Example*: `ஐயாயிரம்` $\rightarrow$ `5,000`
     - *Example*: `ஒரு லட்சத்து நாற்பத்து நான்காயிரம்` $\rightarrow$ `1,44,000`
2. **Indian Numbering System Formatting**:
   - Formats large numbers using standard Indian grouping (`1,23,456,789` / $3, 2, 2\dots$ grouping).
3. **Typo & Spelling Mistake Detection**:
   - Automatically detects common spelling errors and missing pulli/sandhi (e.g. `அறுபதைந்து` instead of `அறுபத்தைந்து`).
   - Flagged with bug context and **never** auto-inserted (`IsAccepted = false` by default).
4. **Alternate Spellings Support**:
   - Seamlessly handles regional and dialectal variations:
     - `ஐயாயிரம்` and `ஐந்தாயிரம்` ($5,000$)
     - `நാലாயிரம்` and `நான்காயிரம்` ($4,000$)
     - `முந்நூறு` and `முன்னூறு` ($300$)
     - `ஐந்நூறு` and `ஐநூறு` ($500$)
     - `லட்சம்` and `இலட்சம்` ($1,00,000$)
5. **Malformed Separator Typo Detection**:
   - Identifies malformed separators (e.g. `5.673` period instead of comma, or `5, 673` spaced commas).
   - Resolves all interval overlaps and suppresses false sub-token matches.
6. **"Flag as Paratext Notes" for Manual Review**:
   - One-click action to create native **Paratext Project Notes** anchored to verses for flagged or suspicious items.
   - Translators can review notes directly in Paratext's standard Notes panel with full verse context.
7. **Strict Structural Immunity (USFM Protection)**:
   - **Ignores and preserves**:
     - Parallel references (`\r ...`)
     - Footnotes (`\f ... \f*`, `\fe ... \fe*`)
     - Cross-references (`\x ... \x*`)
     - Figures and illustrations (`\fig ... \fig*`)
     - Verse numbers and markers (`\v 1`)
     - Chapter numbers and markers (`\c 1`)
     - Translator remarks (`\rem ...`)
     - Book headers and names (`\id`, `\h`, `\toc1-3`, `\mt1-3`, leading book digits like `1 சாமுவேல்`, `2 அரசர்கள்`, `1 பேதுரு`)
   - Operates **exclusively** on narrative and poetic body text (`\p`, `\m`, `\q`, `\s`).
8. **Translator Agency & Safety**:
   - Interactive review window showing side-by-side diffs (Book, Chapter, Verse, Original, Proposed, Warning/Bug Context).
   - Amber row highlights for flagged/suspicious items.
   - Translator can accept, reject, or filter changes before committing.
   - Creates Paratext **History Points** prior to applying changes for 1-click rollback.
   - Uses Paratext `IWriteLock` to guarantee transactional integrity.

---

## 🏛️ Architecture & Paratext 9.5 Integration

The plugin targets **Paratext 9.5.110.11** (.NET Framework 4.8 / Windows x64) and references:
- `PluginInterfaces.dll` (`Paratext.PluginInterfaces`)
- `CorePluginInterfaces.dll`

### Solution Structure

```
Number Conversions/
├── .github/
│   ├── workflows/
│   │   ├── build-and-test.yml       # CI build & automated tests
│   │   └── publish-release.yml      # Release workflow for .ptxplg
│   └── ISSUE_TEMPLATE/
│       ├── bug_report.md
│       ├── feature_request.md
│       └── tamil_orthography_issue.md
├── docs/
│   ├── ARCHITECTURE.md              # Technical design & sequence flows
│   ├── PARATEXT_PLUGIN_GUIDE.md     # Paratext 9.5 plugin development guide
│   ├── TAMIL_NUMBER_RULES.md        # Tamil linguistic specifications & corpus
│   └── GITHUB_ISSUES_ROADMAP.md     # Detailed GitHub issues and milestones
├── .gitattributes
├── .gitignore
├── LICENSE
└── README.md
```

---

## 📖 Tamil Orthography Rules & Corpus Reference

The conversion algorithms are grounded in authoritative linguistic and scripture resources:
- **Tamil Virtual Academy** (தமிழ் இணையக் கல்விக்கழகம்)
- **Open-Tamil** Numeral Engine
- **BSI Tamil Bible** (Old Version / பரிசுத்த வேதாகமம்)
- **Indian Revised Version (IRV) Tamil**

For detailed specifications on sandhi rules (புணர்ச்சி), compound hundreds, and ordinals, refer to [docs/TAMIL_NUMBER_RULES.md](file:///d:/Projects/Number%20Conversions/docs/TAMIL_NUMBER_RULES.md).

---

## 🚀 Installation & Deployment

### Quick 1-Click Install (Recommended)
1. Download the latest `NumberConversions-Paratext9-Plugin-v1.0.5.zip` from [Releases](https://github.com/Ebenezerillango7/Number-Conversions/releases).
2. Extract the zip file.
3. Double-click **`install-plugin.bat`**.
   - The installer automatically removes Windows Mark of the Web (`Zone.Identifier`) security blocks on downloaded files.
   - If Paratext 9 is open, it prompts you to save and close Paratext, then automatically completes installation.
   - Rebuilds Paratext's plugin cache and shows an installation confirmation message.
4. Launch Paratext 9, open your project, and access **Tools $\rightarrow$ Tamil Number Conversions...**.

### Manual Install
1. Before extracting, right-click the downloaded `.zip` file $\rightarrow$ **Properties** $\rightarrow$ check **Unblock** $\rightarrow$ click **Apply** / **OK** (removes Windows download block).
2. Extract `NumberConversions.ptxplg`, `NumberConversions.Core.dll`, and `uninstall-plugin.ps1` into:
   ```
   D:\Transdata\Paratext 9\plugins\NumberConversions\
   ```
   *(or `C:\Program Files\Paratext 9\plugins\NumberConversions\` depending on your installation path).*
3. Double-click `UpdateParatextPluginCache.exe` in the Paratext 9 folder (or restart Paratext 9).

---

## 🗑️ Clean Uninstallation

Translators never need to reinstall Paratext 9 to remove the plugin. You can cleanly remove it in two ways:

### Method 1: In-App (From Within Paratext)
1. Open the plugin window via **Tools $\rightarrow$ Tamil Number Conversions...**.
2. Click the **"Uninstall Plugin..."** button in the top-right corner.
3. Confirm the prompt and close Paratext when asked.
4. The background assistant waits for Paratext to exit, deletes the plugin files, rebuilds the plugin cache, and notifies you when complete.

### Method 2: 1-Click Uninstaller Script
1. Ensure Paratext 9 is closed.
2. In the extracted folder, double-click **`uninstall-plugin.bat`** (or right-click `uninstall-plugin.ps1` and choose **Run with PowerShell**).
3. The script locates Paratext 9, removes `<ParatextDir>\plugins\NumberConversions\`, and refreshes `AddIns.store`.

> **Note on Scripture Safety**: Uninstallation only removes the plugin software. Your biblical text (`.sfm` / `.usfm` files), project settings, and project notes are **never** altered or deleted. If you ever wish to revert any text changes previously applied, you can do so at any time using Paratext's built-in **Project History**.

---

## 🗺️ Project Roadmap & Issues

The implementation plan is tracked in [docs/GITHUB_ISSUES_ROADMAP.md](file:///d:/Projects/Number%20Conversions/docs/GITHUB_ISSUES_ROADMAP.md):

- **Issue #1**: [Core] Paratext 9.5 Plugin Shell & `PluginInterfaces` Integration
- **Issue #2**: [USFM] Strict Token Filtering Engine (`\r`, `\x`, `\f`, `\fig`, `\v`, `\c`, Book Names)
- **Issue #3**: [Linguistics] Tamil Number Converter: 0–199 Digit-to-Text (Cardinals & Ordinals)
- **Issue #4**: [Linguistics] Tamil Number Converter: 200+ Text-to-Digit with Indian Separator (`1,23,456,789`)
- **Issue #5**: [UI] Interactive Review Window with Diff View & WriteLock Safety
- **Issue #6**: [Quality] Comprehensive Test Suite & Tamil Scripture Corpus Validation
- **Issue #7**: [DevOps] CI/CD Pipeline & GitHub Release Distribution (`.ptxplg`)

---

## 👤 Author & Maintainer

- **Developer**: Ebenezer Illango ([ebenezerillango7@gmail.com](mailto:ebenezerillango7@gmail.com))
- **License**: [MIT License](LICENSE)
