# GitHub Issues Roadmap

This document outlines the GitHub issues planned for the `Number Conversions` repository. These issues represent discrete, actionable work items for building the Paratext 9.5 plugin.

---

### Issue #1: [Core] Project Scaffolding & Paratext 9.5 `PluginInterfaces` Integration
- **Labels**: `setup`, `paratext`, `backend`
- **Milestone**: v1.0.0-alpha
- **Description**:
  Create the .NET solution and projects targeting .NET Framework 4.8.
  - Setup `NumberConversions.sln`
  - Setup `NumberConversions.Core` (Class Library for USFM parsing and Tamil linguistic logic, testable outside Paratext)
  - Setup `NumberConversions.Plugin` (.NET Framework 4.8 class library referencing Paratext 9.5 `PluginInterfaces.dll` and `CorePluginInterfaces.dll`)
  - Setup `NumberConversions.Tests` (xUnit/NUnit test suite)
  - Implement `IParatextStandalonePlugin` manifest class (`Name`, `Version`, `Publisher`, `PluginMenuEntries`)
  - Hook menu action into Paratext Tools menu.

---

### Issue #2: [USFM] Strict Token Filtering Engine (\r, \x, \f, \fig, \v, \c, Book Names)
- **Labels**: `usfm`, `parser`, `safety`
- **Milestone**: v1.0.0-alpha
- **Description**:
  Develop the USFM token reader and filter that strictly guarantees structural scripture markers are untouched.
  - Implement token stream traversal using `IUSFMToken`, `IUSFMMarkerToken`, `IUSFMTextToken`.
  - Filter out all metadata: `\id`, `\h`, `\toc1`, `\toc2`, `\toc3`, `\mt1`.
  - Filter out chapter (`\c`) and verse (`\v`) numbers.
  - Filter out parallel references (`\r`).
  - Filter out footnote bodies (`\f ... \f*`, `\fe ... \fe*`).
  - Filter out cross-references (`\x ... \x*`).
  - Filter out figure captions and references (`\fig ... \fig*`).
  - Prevent accidental conversion of leading digits in book titles (e.g. `1 சாமுவேல்`, `2 கொரிந்தியர்`).
  - Unit tests covering complex USFM samples with nested footnotes and poetry markers (`\q1`, `\q2`).

---

### Issue #3: [Linguistics] Tamil Number Converter: 0–199 Digit-to-Text
- **Labels**: `linguistics`, `tamil-script`, `engine`
- **Milestone**: v1.0.0-beta
- **Description**:
  Implement the Tamil digit-to-word conversion engine for range 0–199.
  - Support cardinal numbers:
    - 0–9 (`பூஜ்யம்`, `ஒன்று`, `இரண்டு`...)
    - 10–19 (`பத்து`, `பதினொன்று`, `பன்னிரண்டு`...)
    - 20–99 tens and compound sandhi (`இருபது`, `இருபத்தொன்று`...)
    - 100–199 hundreds and compound sandhi (`நூறு`, `நூற்று எழுபத்தைந்து` / `நூற்றெழுபத்தைந்து`)
  - Support ordinal numbers:
    - `1-ஆம்`, `1ஆம்`, `1வது`, `1ம்` $\rightarrow$ `முதலாம்` / `முதலாவது`
    - `2nd`, `2-ஆம்`, `2ஆம்`, `2வது`, `2ம்` $\rightarrow$ `இரண்டாம்` / `இரண்டாவது`
    - Suffix matching for all ordinals up to `199-ஆம்`.
  - Configurable sandhi style preference (spaced words vs euphonic combination).

---

### Issue #4: [Linguistics] Tamil Number Converter: 200+ Text-to-Digit with Indian Separator
- **Labels**: `linguistics`, `tamil-script`, `engine`
- **Milestone**: v1.0.0-beta
- **Description**:
  Implement the text-to-digit parser for Tamil number words $\ge 200$.
  - Parse hundreds: `இருநூறு` (200), `முந்நூறு` (300), ..., `தொள்ளாயிரம்` (900).
  - Parse multipliers: `ஆயிரம்` (1,000), `பத்தாயிரம்` (10,000), `லட்சம்` / `இலட்சம்` (1,00,000), `கோடி` (1,00,00,000).
  - Resolve compound phrases:
    - `இருநூற்று ஐம்பது` $\rightarrow$ `250`
    - `ஐயாயிரத்து அறுநூறு` $\rightarrow$ `5,600`
    - `ஒரு லட்சத்து நாற்பத்து நான்காயிரம்` $\rightarrow$ `1,44,000`
  - Implement Indian numbering system formatter (`1,23,456,789`):
    - Format $\ge 1,000$ with $3,2,2$ comma grouping.
  - Support ordinal conversions for $\ge 200$ (e.g. `இருநூறாம்` $\rightarrow$ `200-ஆம்`).

---

### Issue #5: [UI] Interactive Review Window with Diff View & WriteLock Safety
- **Labels**: `ui`, `winforms`, `paratext`
- **Milestone**: v1.0.0-rc
- **Description**:
  Build the translator-facing UI inside Paratext for previewing and approving changes.
  - Scope selection: Entire Project, Selected Books, or Current Chapter.
  - Review Grid displaying:
    - Book / Chapter / Verse reference
    - Original Scripture Text
    - Proposed Converted Text
    - Detected Number & Conversion Type (0–199 Digit-to-Text vs 200+ Text-to-Digit)
    - Checkbox for per-item Accept / Reject
  - Action buttons: "Scan", "Accept Selected", "Accept All", "Reject Selected".
  - Paratext Safety & History Integration:
    - Call `project.MakeHistoryPoint(...)` before applying changes.
    - Acquire `IWriteLock` with proper error handling and release.
    - Refresh Paratext editor view on completion.

---

### Issue #6: [Quality] Comprehensive Test Suite & Tamil Scripture Corpus Validation
- **Labels**: `testing`, `corpus`, `qa`
- **Milestone**: v1.0.0-rc
- **Description**:
  Validate the engine against real-world Tamil scripture data.
  - Benchmarks using BSI Tamil Old Version and Indian Revised Version (IRV) Tamil USFM books.
  - Exhaustive boundary tests: 0, 1, 199, 200, 201, 999, 1000, 99999, 100000, 10000000.
  - Verification that references (`\r`), footnotes (`\f`), cross-references (`\x`), figures (`\fig`), verse tags (`\v`), and chapter tags (`\c`) are 100% untouched.

---

### Issue #7: [DevOps] CI/CD Pipeline & GitHub Release Distribution (.ptxplg)
- **Labels**: `devops`, `ci-cd`, `packaging`
- **Milestone**: v1.0.0
- **Description**:
  Configure automated continuous integration and release packaging.
  - GitHub Actions workflow for automated build and unit test execution on Windows runner.
  - Release workflow triggered on version tags (`v*`).
  - Packaging script producing `NumberConversions.ptxplg` and installation zip bundle.
  - Automated release notes generation.
