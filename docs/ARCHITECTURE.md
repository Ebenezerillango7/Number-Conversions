# Number Conversions Paratext Plugin - Architecture & Design Specification

**Target Platform**: Paratext 9.5.110.11 (.NET Framework 4.8 / Windows x64)  
**Target Language**: Tamil (`ta` / தமிழ்)  
**Plugin Identifier**: `NumberConversions`  

---

## 1. Executive Summary

The **Number Conversions** plugin is a scripture editing assistant designed for Paratext 9.5. It automates the bidirectional normalization of numbers in Tamil Bible texts based on established biblical translation orthography rules:

1. **Range 0 – 199**: Convert numeric digits into Tamil text words (both cardinal and ordinal).
   - *Example*: `175` $\rightarrow$ `நூற்று எழுபத்தைந்து`; `2nd` / `2-ஆம்` $\rightarrow$ `இரண்டாம்`.
2. **Range 200 and above**: Convert Tamil number words into numeric digits.
   - *Example*: `இருநூறு` $\rightarrow$ `200`; `ஐயாயிரம்` $\rightarrow$ `5,000`; `ஒரு லட்சத்து நாற்பத்து நான்காயிரம்` $\rightarrow$ `1,44,000`.
3. **Thousand Separator**: Indian numbering system formatting (`1,23,456,789`).
4. **Strict Structural Immunity**: Absolute preservation of USFM structural tokens, reference passages (`\r`), footnotes (`\f`), cross-references (`\x`), figures (`\fig`), verse markers (`\v`), chapter markers (`\c`), and book headers (`\id`, `\h`, `\toc1-3`, `\mt1-3`).
5. **Translator Agency**: Non-destructive, interactive review window with verse-by-verse diff comparison and Paratext history rollback safety points.

---

## 2. High-Level Architecture

The project is structured into modular layers adhering to separation of concerns:

```
+-------------------------------------------------------------------------+
|                        Paratext 9.5 Host                                |
|   (PluginInterfaces.dll, CorePluginInterfaces.dll, Paratext.exe)        |
+-------------------------------------------------------------------------+
                                   |
                                   v
+-------------------------------------------------------------------------+
|                  Paratext Plugin Adapter Layer                          |
|  - NumberConversionsPlugin : IParatextStandalonePlugin                   |
|  - PluginMenuEntry integration under Tools Menu                         |
|  - Paratext Host context & project resolver                             |
+-------------------------------------------------------------------------+
                                   |
         +-------------------------+-------------------------+
         |                                                   |
         v                                                   v
+----------------------------------+     +----------------------------------+
|      USFM Token Pipeline         |     |     Interactive UI Layer         |
| - USFM Token Stream Reader       |     | - WinForms / WPF Review Window   |
| - Strict Marker Filter           |     | - Verse-by-verse Diff Grid       |
| - Safe Token Replacement Engine  |     | - Accept / Reject / Skip actions |
| - WriteLock & History Point Mgr  |     | - Progress & Status Reporter     |
+----------------------------------+     +----------------------------------+
                 |
                 v
+-------------------------------------------------------------------------+
|                 Tamil Number Conversion Core Engine                     |
|  +-----------------------------------+-------------------------------+  |
|  |     Digit-to-Text (0 - 199)       |      Text-to-Digit (200+)     |  |
|  | - Cardinal Table & Builder        | - Tokenizer & Compound Parser |  |
|  | - Ordinal Pattern Matcher         | - Sandhi Resolver (புணர்ச்சி) |  |
|  | - Sandhi Euphonic Rules           | - Multiplier Engine           |  |
|  +-----------------------------------+-------------------------------+  |
|  |               Indian Numbering Formatter (1,23,456,789)           |  |
|  +-------------------------------------------------------------------+  |
|  |                Linguistic Corpus & Lexicon Database               |  |
|  |  (Tamil Virtual Academy, Open-Tamil, BSI/IRV Tamil Scripture)      |  |
+-------------------------------------------------------------------------+
```

---

## 3. Paratext 9.5 Plugin Lifecycle & Interfaces

### 3.1 Plugin Manifest & Interface Implementation

The entry point implements `Paratext.PluginInterfaces.IParatextStandalonePlugin`:

```csharp
namespace NumberConversions.ParatextPlugin
{
    public class NumberConversionsPlugin : IParatextStandalonePlugin
    {
        public string Name => "Number Conversions";
        public Version Version => new Version(1, 0, 0);
        public string VersionString => "1.0.0";
        public string Publisher => "Ebenezer Illango";

        public string GetDescription(string locale)
        {
            return "Converts Tamil scripture numbers between text and digits: " +
                   "0-199 to text words, 200+ to formatted digits (Indian numbering system).";
        }

        public IDataFileMerger GetMerger(IPluginHost host, string dataIdentifier) => null;

        public IEnumerable<PluginMenuEntry> PluginMenuEntries
        {
            get
            {
                yield return new PluginMenuEntry(
                    "Tamil Number Conversions...",
                    RunPlugin,
                    PluginMenuLocation.Tools,
                    imagePath: null,
                    customLocation: null);
            }
        }

        private void RunPlugin(IPluginHost host, IParatextChildState state)
        {
            // Open the Review & Conversion Window
            var mainWindow = new UI.Views.ConversionMainWindow(host, state);
            mainWindow.Show();
        }
    }
}
```

### 3.2 Compilation and Deployment

- **Assembly Name**: `NumberConversions.dll`
- **Plugin Artifact**: Renamed to `NumberConversions.ptxplg`
- **Installation Directory**:
  - `D:\Transdata\Paratext 9\plugins\NumberConversions\NumberConversions.ptxplg` (or `%LOCALAPPDATA%\Paratext9\plugins\NumberConversions\`)
- **Plugin Registration**:
  Paratext discovers plugins via `.ptxplg` in subdirectories of `plugins/` and manages the cache using `UpdateParatextPluginCache.exe`.

---

## 4. USFM Token Filtering Specification

Paratext represents scripture text as a sequence of `IUSFMToken` objects. Modifying raw text with regex without token awareness risks corrupting tags or scripture cross references. The plugin enforces strict token boundaries.

### 4.1 Token Classification Rules

| Token Type | USFM Marker | Action | Justification |
| :--- | :--- | :--- | :--- |
| **Chapter Marker** | `\c` | **IGNORE** | Chapter identifier must remain numeric. |
| **Verse Marker** | `\v` | **IGNORE** | Verse identifier must remain numeric. |
| **Parallel References** | `\r` | **IGNORE** | Reference citations must never be altered. |
| **Footnotes** | `\f ... \f*`, `\fe ... \fe*` | **IGNORE** | Footnote body and apparatus preserved. |
| **Cross-references**| `\x ... \x*` | **IGNORE** | Cross-reference citations preserved. |
| **Figures / Media** | `\fig ... \fig*` | **IGNORE** | File references and captions preserved. |
| **Book Headers** | `\id`, `\h`, `\toc1-3`, `\mt1-3` | **IGNORE** | Canonical book metadata preserved. |
| **Book Names in Body**| e.g. "1 சாமுவேல்", "2 கொரிந்தியர்" | **IGNORE** | Leading digits in book titles must not be replaced as standalone numbers. |
| **Body Paragraphs** | `\p`, `\m`, `\q`, `\q1-4`, `\s`, `\s1` | **PROCESS** | Standard narrative and poetic scripture text. |

### 4.2 Safe Token Traversal Algorithm

```
FOR each Book selected IN Project:
  FOR each Chapter IN Book:
    tokens = project.GetUSFMTokens(bookNum, chapterNum)
    FOR each token IN tokens:
      IF token.IsMetadata == TRUE THEN SKIP
      IF token.IsFigure == TRUE THEN SKIP
      IF token.IsFootnoteOrCrossReference == TRUE THEN SKIP
      IF token IS IUSFMMarkerToken markerToken THEN
        IF markerToken.Marker IN ["c", "v", "r", "x", "f", "fe", "fig", "id", "h", "toc1", "toc2", "toc3", "mt1", "mt2", "mt3"] THEN
          SKIP
      IF token IS IUSFMTextToken textToken THEN
        ProcessScriptureText(textToken.Text, verseRef)
```

---

## 5. Tamil Number Conversion Rules & Linguistic Specification

### 5.1 Conversion Range 0 – 199 (Digit to Text)

#### Cardinals
- Standard Arabic digits `0` to `199` are converted to standard Tamil words:
  - `0` $\rightarrow$ `பூஜ்யம்`
  - `1` $\rightarrow$ `ஒன்று` (or `ஒரு` when preceding a noun)
  - `2` $\rightarrow$ `இரண்டு`
  - `10` $\rightarrow$ `பத்து`
  - `12` $\rightarrow$ `பன்னிரண்டு`
  - `20` $\rightarrow$ `இருபது`
  - `99` $\rightarrow$ `தொண்ணூற்றொன்பது`
  - `100` $\rightarrow$ `நூறு`
  - `175` $\rightarrow$ `நூற்று எழுபத்தைந்து` (or `நூற்றெழுபத்தைந்து` per project preference)
  - `199` $\rightarrow$ `நூற்று தொண்ணூற்றொன்பது`

#### Ordinals
- Digit followed by ordinal suffix:
  - Patterns: `2nd`, `2-ஆம்`, `2ஆம்`, `2-வது`, `2வது`, `2ம்` $\rightarrow$ `இரண்டாம்` / `இரண்டாவது`
  - `1st` / `1-ஆம்` $\rightarrow$ `முதலாம்` / `முதலாவது`
  - `3rd` / `3-ஆம்` $\rightarrow$ `மூன்றாம்` / `மூன்றாவது`
  - `12th` / `12-ஆம்` $\rightarrow$ `பன்னிரண்டாம்`

### 5.2 Conversion Range 200 and Above (Text to Digit)

- Any Tamil number word or phrase evaluating to $\ge 200$ is converted into digits with Indian comma grouping.
- Examples:
  - `இருநூறு` $\rightarrow$ `200`
  - `இருநூற்று ஐம்பது` $\rightarrow$ `250`
  - `ஐந்நூறு` $\rightarrow$ `500`
  - `ஆயிரம்` $\rightarrow$ `1,000`
  - `இரண்டாயிரம்` $\rightarrow$ `2,000`
  - `ஐயாயிரத்து அறுநூற்று எழுபது` $\rightarrow$ `5,670`
  - `பத்தாயிரம்` $\rightarrow$ `10,000`
  - `ஒரு லட்சம்` / `இலட்சம்` $\rightarrow$ `1,00,000`
  - `ஒரு லட்சத்து நாற்பத்து நான்காயிரம்` $\rightarrow$ `1,44,000`
  - `கோடி` $\rightarrow$ `1,00,00,000`

### 5.3 Indian Numbering System Formatter

The plugin formats integers $\ge 1,000$ using the Indian / South Asian grouping format:
- The rightmost three digits form the first group.
- Every preceding group consists of two digits.

```csharp
public static string FormatIndianNumber(long value)
{
    if (value < 1000) return value.ToString();
    string str = value.ToString();
    string lastThree = str.Substring(str.Length - 3);
    string remaining = str.Substring(0, str.Length - 3);
    
    // Group remaining digits by 2 from right to left
    var groups = new List<string>();
    while (remaining.Length > 2)
    {
        groups.Insert(0, remaining.Substring(remaining.Length - 2));
        remaining = remaining.Substring(0, remaining.Length - 2);
    }
    if (remaining.Length > 0)
        groups.Insert(0, remaining);
        
    return string.Join(",", groups) + "," + lastThree;
}
```

---

## 6. Safety, Write Locks & Undo Protection

Bible translations in Paratext are mission-critical. Automated replacements must follow Paratext's concurrency and history safety guidelines:

1. **Paratext WriteLock Protocol**:
   - The plugin must request an `IWriteLock` before writing changes via `project.RequestWriteLock(...)`.
   - Lock scope is localized to book/chapter or acquired during the user-confirmed batch execution.
2. **Paratext History Point**:
   - Prior to committing any batch replacement, the plugin invokes:
     ```csharp
     project.MakeHistoryPoint("Number Conversions (0-199 words, 200+ digits)", force: true);
     ```
   - This allows translators to instantly roll back changes using Paratext's built-in **Project History / Revert** feature.
3. **Interactive Review Table**:
   - Grid columns: `Verse Reference`, `Original Scripture`, `Proposed Revision`, `Matched Token`, `Conversion Rule`, `Accept (Checkbox)`.
   - Users can review every proposed change, uncheck unwanted modifications, or edit the replacement directly before applying.

---

## 7. Quality Assurance & Verification Corpus

The engine will be validated against:
1. **Tamil Scripture Benchmarks**:
   - Indian Revised Version (IRV) Tamil.
   - Tamil Old Version (Tamil OV - BSI).
2. **Linguistic Authorities**:
   - Tamil Virtual Academy (தமிழ் இணையக் கல்விக்கழகம்) number standards.
   - Open-Tamil NLP library test cases (`open-tamil` numeral suite).
3. **Automated Unit Tests**:
   - Complete boundary testing: `0`, `1`, `199`, `200`, `201`, `999`, `1000`, `100000`, `10000000`.
   - Ordinal variants (`2-ஆம்`, `2ஆம்`, `2வது`, `2ம்`).
   - USFM token isolation tests proving zero modification to `\v`, `\c`, `\r`, `\x`, `\f`, `\fig`.
