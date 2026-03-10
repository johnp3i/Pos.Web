# Multi-Root Workspace Setup Guide

## Quick Start

### Option 1: Copy Workspace File to Parent Directory (Recommended)

1. **Copy the workspace file**:
   ```bash
   # From Pos.Web directory
   cp .kiro/specs/web-based-pos-system/POS-Development.code-workspace ../../
   ```

2. **Open workspace in Kiro**:
   ```bash
   # From parent directory
   code POS-Development.code-workspace
   ```

   Or in Kiro:
   - File → Open Workspace from File
   - Navigate to parent directory
   - Select `POS-Development.code-workspace`

### Option 2: Open Directly from Spec Folder

```bash
# From Pos.Web directory
code .kiro/specs/web-based-pos-system/POS-Development.code-workspace
```

---

## What You'll See

After opening the workspace, your Kiro sidebar will show:

```
EXPLORER
├── 🌐 Pos.Web (New Web POS)
│   ├── .kiro/
│   │   ├── specs/
│   │   │   └── web-based-pos-system/
│   │   │       ├── requirements.md
│   │   │       ├── design.md
│   │   │       ├── tasks.md
│   │   │       ├── LEGACY-CODE-REFERENCE.md  ← Use this!
│   │   │       └── ...
│   │   └── steering/
│   ├── src/
│   │   ├── Pos.Web.Shared/
│   │   ├── Pos.Web.Infrastructure/
│   │   ├── Pos.Web.API/
│   │   ├── Pos.Web.Client/
│   │   └── Pos.Web.Tests/
│   └── Pos.Web.sln
│
└── 🖥️ MyChairPos (Legacy WPF POS)
    ├── .kiro/
    │   └── steering/
    ├── POS/
    │   ├── Helpers/
    │   │   ├── DbHelper.cs              ← Reference this
    │   │   ├── CalculationsHelper.cs    ← Reference this
    │   │   ├── PrintHelper.cs           ← Reference this
    │   │   └── ...
    │   ├── CheckoutHelpers/
    │   ├── Models/
    │   ├── MainWindow.xaml.cs           ← Reference this
    │   └── ...
    ├── OrdersMonitor/
    │   └── MainWindow.xaml.cs           ← Reference this
    ├── PosDbForAll/
    │   ├── Invoice.cs                   ← Reference this
    │   ├── Customer.cs                  ← Reference this
    │   └── ...
    └── MyChairPos.sln
```

---

## Daily Workflow

### 1. Open Workspace

```bash
code POS-Development.code-workspace
```

### 2. Navigate to Task List

- Open: `Pos.Web/.kiro/specs/web-based-pos-system/tasks.md`
- Select a task to implement

### 3. Find Legacy Code

- Open: `Pos.Web/.kiro/specs/web-based-pos-system/LEGACY-CODE-REFERENCE.md`
- Find the legacy file path for your task

### 4. Open Files Side-by-Side

**Example: Implementing Payment Service**

1. **Open legacy file** (left pane):
   - Navigate to: `MyChairPos/POS/Helpers/DbHelper.cs`
   - Find method: `ProcessPayment()` (line ~400)

2. **Open new file** (right pane):
   - Navigate to: `Pos.Web/src/Pos.Web.API/Services/Implementations/PaymentService.cs`
   - Create if doesn't exist

3. **Split editor**:
   - Right-click on file tab → "Split Right"
   - Or use: `Ctrl+\` (Windows/Linux) or `Cmd+\` (Mac)

### 5. Port Logic

- Read legacy code to understand business rules
- Implement in new file with modern patterns
- Add improvements (DI, async/await, transactions, tests)

### 6. Update Progress

- Update `LEGACY-CODE-REFERENCE.md`:
  - Change status from ⏳ to ✅
  - Add notes about changes made
  - Document any business rules discovered

---

## Useful Keyboard Shortcuts

### File Navigation

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Quick Open | `Ctrl+P` | `Cmd+P` |
| Go to Symbol | `Ctrl+Shift+O` | `Cmd+Shift+O` |
| Go to Definition | `F12` | `F12` |
| Peek Definition | `Alt+F12` | `Option+F12` |

### Search

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Search in Files | `Ctrl+Shift+F` | `Cmd+Shift+F` |
| Find in File | `Ctrl+F` | `Cmd+F` |
| Replace in File | `Ctrl+H` | `Cmd+H` |

### Editor

| Action | Windows/Linux | Mac |
|--------|---------------|-----|
| Split Editor | `Ctrl+\` | `Cmd+\` |
| Close Editor | `Ctrl+W` | `Cmd+W` |
| Toggle Sidebar | `Ctrl+B` | `Cmd+B` |

---

## Search Across Both Solutions

### Example: Find all references to "ProcessPayment"

1. Press `Ctrl+Shift+F` (or `Cmd+Shift+F` on Mac)
2. Type: `ProcessPayment`
3. Results will show matches from BOTH solutions:
   - `MyChairPos/POS/Helpers/DbHelper.cs` (legacy)
   - `Pos.Web/src/Pos.Web.API/Services/PaymentService.cs` (new)

### Example: Find all discount calculations

1. Press `Ctrl+Shift+F`
2. Type: `discount`
3. Filter by file type: `*.cs`
4. Review results from both solutions

---

## File Comparison

### Compare Legacy vs New Implementation

1. Open legacy file: `MyChairPos/POS/Helpers/DbHelper.cs`
2. Open new file: `Pos.Web/src/Pos.Web.API/Services/PaymentService.cs`
3. Right-click on one file tab → "Select for Compare"
4. Right-click on other file tab → "Compare with Selected"
5. Side-by-side diff view appears

---

## Tips & Tricks

### 1. Use Breadcrumbs

Enable breadcrumbs to see file path:
- View → Show Breadcrumbs
- Shows: `Pos.Web > src > Pos.Web.API > Services > PaymentService.cs`

### 2. Use Outline View

See class/method structure:
- View → Open View → Outline
- Shows all methods in current file
- Click to jump to method

### 3. Use Minimap

See file overview:
- View → Show Minimap
- Shows entire file structure
- Click to jump to section

### 4. Use Bookmarks

Mark important locations:
- Install "Bookmarks" extension
- `Ctrl+Alt+K` to toggle bookmark
- `Ctrl+Alt+L` to list bookmarks

### 5. Use TODO Comments

Track work in progress:
```csharp
// TODO: Port discount calculation from DbHelper.cs line 450
// FIXME: Add transaction management
// NOTE: Legacy uses multiple SaveChanges() - need to fix
```

Search for TODOs:
- Press `Ctrl+Shift+F`
- Type: `TODO:|FIXME:|NOTE:`
- Use regex mode

---

## Troubleshooting

### Issue: Workspace file not found

**Solution**: Copy workspace file to parent directory:
```bash
cp Pos.Web/.kiro/specs/web-based-pos-system/POS-Development.code-workspace .
```

### Issue: One solution not showing

**Solution**: Check workspace file paths are correct:
```json
{
  "folders": [
    {
      "name": "🌐 Pos.Web (New Web POS)",
      "path": "Pos.Web"  // ← Relative to workspace file location
    },
    {
      "name": "🖥️ MyChairPos (Legacy WPF POS)",
      "path": "MyChairPos"  // ← Relative to workspace file location
    }
  ]
}
```

### Issue: Search not finding files in legacy solution

**Solution**: Check search exclusions in workspace settings:
- File → Preferences → Settings (Workspace)
- Search for: `search.exclude`
- Ensure legacy folders are not excluded

### Issue: Too many files in search results

**Solution**: Use file filters:
- In search box, click "..." → "files to include"
- Enter: `**/*.cs` (only C# files)
- Or: `**/Helpers/*.cs` (only Helper files)

---

## Workspace Settings

The workspace file includes these settings:

### File Exclusions
```json
"files.exclude": {
  "**/bin": true,      // Hide build output
  "**/obj": true,      // Hide intermediate files
  "**/.vs": true,      // Hide Visual Studio files
  "**/node_modules": true  // Hide npm packages
}
```

### Search Exclusions
```json
"search.exclude": {
  "**/bin": true,
  "**/obj": true,
  "**/node_modules": true
}
```

### Watcher Exclusions
```json
"files.watcherExclude": {
  "**/bin/**": true,
  "**/obj/**": true,
  "**/node_modules/**": true
}
```

---

## Recommended Extensions

The workspace recommends these extensions:

1. **C# Dev Kit** (`ms-dotnettools.csdevkit`)
   - IntelliSense for C#
   - Debugging support
   - Project management

2. **C#** (`ms-dotnettools.csharp`)
   - C# language support
   - Syntax highlighting
   - Code navigation

3. **TypeScript** (`ms-vscode.vscode-typescript-next`)
   - For Blazor JavaScript interop
   - TypeScript support

Install all recommended extensions:
- Open workspace
- Click "Install Recommended Extensions" notification
- Or: Extensions → "Show Recommended Extensions"

---

## Example Workflow: Porting Payment Processing

### Step 1: Open Workspace
```bash
code POS-Development.code-workspace
```

### Step 2: Open Task List
- Navigate to: `Pos.Web/.kiro/specs/web-based-pos-system/tasks.md`
- Find: "Task 10: Implement payment processing service"

### Step 3: Open Legacy Reference
- Open: `Pos.Web/.kiro/specs/web-based-pos-system/LEGACY-CODE-REFERENCE.md`
- Find: "Payment Processing" section
- Note: Legacy file is `MyChairPos/POS/Helpers/DbHelper.cs`, method `ProcessPayment()`

### Step 4: Open Legacy File
- Press `Ctrl+P`
- Type: `DbHelper.cs`
- Select: `MyChairPos/POS/Helpers/DbHelper.cs`
- Navigate to line ~400 (method `ProcessPayment()`)

### Step 5: Create New File
- Right-click: `Pos.Web/src/Pos.Web.API/Services/Implementations/`
- Select: "New File"
- Name: `PaymentService.cs`

### Step 6: Split Editor
- Right-click on `PaymentService.cs` tab
- Select: "Split Right"
- Now you have legacy code on left, new code on right

### Step 7: Port Logic
- Read legacy code
- Understand business rules
- Implement in new file with improvements
- Add tests

### Step 8: Update Progress
- Open: `LEGACY-CODE-REFERENCE.md`
- Update status: ⏳ → ✅
- Add notes about changes

---

## Summary

✅ **Multi-Root Workspace** gives you access to both solutions simultaneously  
✅ **LEGACY-CODE-REFERENCE.md** maps legacy code to new implementation  
✅ **Side-by-side editing** makes porting easy  
✅ **Search across both solutions** helps find related code  
✅ **File comparison** shows differences between legacy and new  

**You now have full access to legacy code while working in the new solution!** 🎉

---

**Next Steps**:
1. Copy workspace file to parent directory
2. Open workspace in Kiro
3. Start implementing tasks from `tasks.md`
4. Reference legacy code as needed
5. Update `LEGACY-CODE-REFERENCE.md` as you port
