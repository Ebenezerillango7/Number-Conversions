# Paratext 9.5 Plugin Implementation Guide

This guide describes how to develop, build, and deploy plugins for **Paratext 9.5.110.11**.

---

## 1. Paratext Plugin Ecosystem Overview

Paratext (developed by SIL International and United Bible Societies) is the industry-standard software for Bible translation and revision. Plugins extend Paratext's capabilities by interacting with:
- Project scripture data (USFM streams, USX, Versification, Key Terms)
- User interface (menu entries, standalone tool windows, synchronized child windows, annotations)
- User settings and access controls (roles, permissions, write locks)

---

## 2. Core Dependencies & Location

For Paratext 9.5.110.11 on Windows, the primary plugin reference libraries are located in the Paratext installation directory:

- Primary: `D:\Transdata\Paratext 9\` (or default `C:\Program Files\Paratext 9\`)
- Assemblies:
  - `PluginInterfaces.dll` (Namespace: `Paratext.PluginInterfaces`, Assembly Version: `2.0.100.0`, Runtime: `v4.0.30319` / .NET Framework 4.8)
  - `CorePluginInterfaces.dll`
  - `EmbeddedUiPluginInterfaces.dll` (optional for embedded web controls)

### Project Configuration (`.csproj`) Reference:

```xml
<ItemGroup>
  <Reference Include="PluginInterfaces">
    <HintPath>$(ParatextDir)\PluginInterfaces.dll</HintPath>
    <Private>False</Private>
  </Reference>
  <Reference Include="CorePluginInterfaces">
    <HintPath>$(ParatextDir)\CorePluginInterfaces.dll</HintPath>
    <Private>False</Private>
  </Reference>
</ItemGroup>
```
*(Note: `<Private>False</Private>` prevents copying the Paratext host DLLs into the plugin directory, preventing assembly binding conflicts).*

---

## 3. Implementing `IParatextStandalonePlugin`

A standalone plugin runs on demand from the Paratext menu. It must implement `Paratext.PluginInterfaces.IParatextStandalonePlugin`:

```csharp
using System;
using System.Collections.Generic;
using Paratext.PluginInterfaces;

namespace NumberConversions
{
    public class NumberConversionsPlugin : IParatextStandalonePlugin
    {
        // Identity Metadata
        public string Name => "Number Conversions";
        public Version Version => new Version(1, 0, 0);
        public string VersionString => "1.0.0";
        public string Publisher => "Ebenezer Illango";

        public string GetDescription(string locale)
        {
            return "Tamil Scripture Number Conversion Tool (0-199 Words, 200+ Digits)";
        }

        // XML Data Merging (Return null if no custom project XML data is merged)
        public IDataFileMerger GetMerger(IPluginHost host, string dataIdentifier) => null;

        // Menu Integration
        public IEnumerable<PluginMenuEntry> PluginMenuEntries
        {
            get
            {
                yield return new PluginMenuEntry(
                    "Tamil Number Conversions...",
                    (host, childState) =>
                    {
                        var form = new MainWindow(host, childState);
                        form.Show();
                    },
                    PluginMenuLocation.Tools,
                    imagePath: null,
                    customLocation: null
                );
            }
        }
    }
}
```

---

## 4. Reading and Writing Scripture Data Safely

### 4.1 Accessing the Active Project

```csharp
IProject project = childState?.Project;
if (project == null)
{
    // Fallback: prompt user to pick from host.GetAllProjects()
    var projects = host.GetAllProjects();
}
```

### 4.2 Reading USFM Tokens

```csharp
// Iterate chapters in a book
foreach (int bookNum in project.AvailableBooks)
{
    int chapterCount = project.Versification.GetLastChapter(bookNum);
    for (int chapter = 1; chapter <= chapterCount; chapter++)
    {
        IEnumerable<IUSFMToken> tokens = project.GetUSFMTokens(bookNum, chapter);
        foreach (IUSFMToken token in tokens)
        {
            // Verify token is publishable scripture body text
            if (!token.IsScripture || token.IsFootnoteOrCrossReference || token.IsFigure || token.IsMetadata)
                continue;

            if (token is IUSFMTextToken textToken)
            {
                string originalText = textToken.Text;
                // Run conversion engine...
            }
        }
    }
}
```

### 4.3 Safe Modification with WriteLock & History Point

Paratext strictly guards against unauthorized or unsynchronized data corruption. Writing scripture requires:

```csharp
// 1. Check editing permissions
if (!project.CanEdit(this, bookNum, chapterNum))
{
    throw new InvalidOperationException("Project cannot be edited by current user.");
}

// 2. Create a Paratext History Point so changes can be reverted cleanly
project.MakeHistoryPoint("Batch Tamil Number Conversion (0-199 words, 200+ digits)", force: true);

// 3. Request Write Lock
IWriteLock writeLock = project.RequestWriteLock(this, onRelease => {
    // Handle lock release request if Paratext needs the lock back
}, bookNum, chapterNum);

try
{
    // Write modified USFM tokens or USFM text
    project.PutUSFMTokens(writeLock, updatedTokens, bookNum);
}
finally
{
    // Dispose or release write lock
    writeLock.Dispose();
}
```

---

## 5. Deployment and Discovery

Paratext looks for plugins in the `plugins/` directory:
- Path: `<ParatextInstallDir>\plugins\<PluginName>\<PluginName>.ptxplg`
- For Paratext 9:
  `D:\Transdata\Paratext 9\plugins\NumberConversions\NumberConversions.ptxplg`

### UpdateParatextPluginCache.exe
Paratext maintains a cache of detected plugins in `AddIns.store`. Whenever a plugin is added, updated, or removed, Paratext or the installer can trigger:
```powershell
& "D:\Transdata\Paratext 9\UpdateParatextPluginCache.exe"
```
Or Paratext refreshes its plugin cache upon restarting.
