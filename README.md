# Number Conversions - Paratext 9.5 Plugin

[![Build & Test](https://github.com/ebenezerillango/Number-Conversions/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/ebenezerillango/Number-Conversions/actions/workflows/build-and-test.yml)
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
3. **Strict Structural Immunity (USFM Protection)**:
   - **Ignores and preserves**:
     - Parallel references (`\r ...`)
     - Footnotes (`\f ... \f*`, `\fe ... \fe*`)
     - Cross-references (`\x ... \x*`)
     - Figures and illustrations (`\fig ... \fig*`)
     - Verse numbers and markers (`\v 1`)
     - Chapter numbers and markers (`\c 1`)
     - Book headers and names (`\id`, `\h`, `\toc1-3`, `\mt1-3`, leading book digits like `1 சாமுவேல்`, `2 கொரிந்தியர்`)
   - Operates **exclusively** on narrative and poetic body text (`\p`, `\m`, `\q`, `\s`).
4. **Translator Agency & Safety**:
   - Interactive review window showing side-by-side diffs (Book, Chapter, Verse, Original, Proposed, Rule matched).
   - Translator can accept, reject, or edit individual changes before committing.
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

1. Download the latest `NumberConversions-Paratext9-Plugin-vX.Y.Z.zip` from [Releases](https://github.com/ebenezerillango/Number-Conversions/releases).
2. Extract the archive into your Paratext plugins directory:
   ```
   D:\Transdata\Paratext 9\plugins\NumberConversions\
   ```
   *(or `C:\Program Files\Paratext 9\plugins\NumberConversions\` depending on your installation path).*
3. Verify that `NumberConversions.ptxplg` is present in that folder.
4. Restart Paratext (or run `UpdateParatextPluginCache.exe`).
5. Open Paratext, navigate to any Tamil project, and click **Tools $\rightarrow$ Tamil Number Conversions...**.

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
