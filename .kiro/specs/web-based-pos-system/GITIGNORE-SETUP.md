# .gitignore Setup for Pos.Web Solution

## Quick Setup

1. **Copy the .gitignore file**:
   ```bash
   # From MyChairPos/.kiro/specs/web-based-pos-system/ directory
   # Copy Pos.Web.gitignore to Pos.Web/.gitignore
   
   # Windows (PowerShell)
   Copy-Item "Pos.Web.gitignore" "..\..\..\Pos.Web\.gitignore"
   
   # Or manually:
   # 1. Open Pos.Web.gitignore
   # 2. Copy all content
   # 3. Create new file: Pos.Web/.gitignore
   # 4. Paste content
   ```

2. **Verify it's working**:
   ```bash
   cd Pos.Web
   git status
   # Should NOT show .vs/, bin/, obj/, etc.
   ```

---

## What Gets Ignored

### Visual Studio Files
- ✅ `.vs/` - Visual Studio cache and settings
- ✅ `*.vsidx` - File content index
- ✅ `CopilotIndices/` - IntelliCode indices
- ✅ `*.suo`, `*.user` - User-specific settings

### Build Output
- ✅ `bin/` - Compiled binaries
- ✅ `obj/` - Intermediate build files
- ✅ `*.dll`, `*.exe` - Compiled assemblies
- ✅ `*.pdb` - Debug symbols

### NuGet
- ✅ `packages/` - NuGet packages (restored from packages.config)
- ✅ `*.nuget.props` - Auto-generated NuGet files
- ✅ `*.nuget.g.targets` - Auto-generated NuGet targets
- ✅ `*.nupkg` - NuGet package files

### Test Results
- ✅ `TestResults/` - Test output
- ✅ `*.trx` - Test result files
- ✅ `*.coverage` - Code coverage files

### Logs
- ✅ `*.log` - All log files
- ✅ `logs/` - Log directories

### Temporary Files
- ✅ `*.tmp`, `*.temp` - Temporary files
- ✅ `*.bak` - Backup files
- ✅ `*.swp` - Swap files

### Blazor WebAssembly
- ✅ `.blazor/` - Blazor debugging files
- ✅ `service-worker-assets.js` - Auto-generated service worker

### Database Files
- ✅ `*.mdf`, `*.ldf` - SQL Server database files
- ✅ `*.db`, `*.sqlite` - SQLite databases

### Secrets & Configuration
- ✅ `appsettings.Development.json` - Development settings (if contains secrets)
- ✅ `appsettings.Local.json` - Local settings
- ✅ `secrets.json` - User secrets
- ✅ `*.pfx`, `*.key` - Certificate files

### Legacy Project References
- ✅ `POS/`, `POSAdmin/`, `PosDbForAll/`, `OrdersMonitor/` - Legacy projects (tracked in MyChairPos repo)

---

## What Gets Tracked

### Solution & Project Files
- ✅ `Pos.Web.sln` - Solution file
- ✅ `*.csproj` - Project files
- ✅ `global.json` - SDK version
- ✅ `Directory.Build.props` - Build properties

### Source Code
- ✅ `*.cs` - C# source files
- ✅ `*.razor` - Blazor components
- ✅ `*.cshtml` - Razor views
- ✅ `*.css` - Stylesheets
- ✅ `*.js` - JavaScript files

### Configuration (Non-Secret)
- ✅ `appsettings.json` - Base configuration
- ✅ `appsettings.Production.json` - Production config (without secrets)

### Documentation
- ✅ `.kiro/` - All Kiro specs and steering files
- ✅ `README.md` - Documentation
- ✅ `*.md` - Markdown files

### Static Assets
- ✅ `wwwroot/` - Static web assets (images, fonts, etc.)
- ✅ `*.png`, `*.jpg`, `*.svg` - Images
- ✅ `*.woff`, `*.woff2` - Fonts

---

## Files You Mentioned

The .gitignore handles all the files you noticed:

1. **`.vs/Pos.Web/CopilotIndices/17.14.1584.41681/CodeChunks.db`**
   - ✅ Ignored by: `.vs/` and `*.db`

2. **`.vs/Pos.Web/FileContentIndex/458e1209-04e9-47d3-9d1c-d5891ba71ccb.vsidx`**
   - ✅ Ignored by: `.vs/` and `*.vsidx`

3. **`Pos.Web.Client/obj/Pos.Web.Client.csproj.nuget.g.targets`**
   - ✅ Ignored by: `obj/` and `*.nuget.g.targets`

---

## Verify Ignored Files

After copying the .gitignore, check what Git sees:

```bash
cd Pos.Web

# See all tracked files
git ls-files

# See all untracked files (should be minimal)
git status --untracked-files=all

# Check if specific file is ignored
git check-ignore -v .vs/Pos.Web/CopilotIndices/17.14.1584.41681/CodeChunks.db
# Should output: .gitignore:XX:.vs/    .vs/Pos.Web/CopilotIndices/...
```

---

## Clean Up Already-Tracked Files

If you already committed files that should be ignored:

```bash
cd Pos.Web

# Remove from Git but keep locally
git rm -r --cached .vs/
git rm -r --cached bin/
git rm -r --cached obj/
git rm --cached **/*.vsidx
git rm --cached **/*.nuget.g.targets

# Commit the removal
git commit -m "Remove ignored files from tracking"

# Push changes
git push
```

---

## Team Collaboration

When team members clone the repository:

1. **They clone Pos.Web**:
   ```bash
   git clone <repo-url> Pos.Web
   cd Pos.Web
   ```

2. **They clone MyChairPos** (sibling directory):
   ```bash
   cd ..
   git clone <legacy-repo-url> MyChairPos
   ```

3. **Directory structure**:
   ```
   /projects/
   ├── MyChairPos/  (legacy repo)
   └── Pos.Web/     (new repo with .gitignore)
   ```

4. **Open Pos.Web.sln**:
   - Legacy projects load automatically (if in correct location)
   - .gitignore prevents tracking legacy files

---

## Common Issues

### Issue 1: Legacy Projects Showing as Modified

**Problem**: Git shows POS/, POSAdmin/, etc. as modified

**Solution**: The .gitignore already excludes these directories:
```gitignore
# Legacy Project References
POS/
POSAdmin/
PosDbForAll/
OrdersMonitor/
```

If still showing, they might be tracked. Remove them:
```bash
git rm -r --cached POS/ POSAdmin/ PosDbForAll/ OrdersMonitor/
git commit -m "Remove legacy projects from tracking"
```

### Issue 2: .vs/ Directory Still Tracked

**Problem**: `.vs/` directory shows in `git status`

**Solution**: Remove from tracking:
```bash
git rm -r --cached .vs/
git commit -m "Remove .vs directory from tracking"
```

### Issue 3: NuGet Files Tracked

**Problem**: `*.nuget.g.targets` files tracked

**Solution**: Remove from tracking:
```bash
git rm --cached **/*.nuget.g.targets
git rm --cached **/*.nuget.g.props
git commit -m "Remove auto-generated NuGet files"
```

---

## Best Practices

### 1. Never Commit Secrets
- ✅ Use User Secrets for development
- ✅ Use Azure Key Vault for production
- ✅ Never commit connection strings with passwords
- ✅ Never commit certificate files (.pfx, .key)

### 2. Keep .gitignore Updated
- ✅ Add new patterns as needed
- ✅ Review when adding new tools
- ✅ Document custom ignores

### 3. Review Before Committing
```bash
# Always review what you're committing
git status
git diff --staged

# Check for secrets
git diff --staged | grep -i "password\|secret\|key"
```

### 4. Use .gitattributes
Create `.gitattributes` for line endings:
```
# Auto detect text files and normalize line endings to LF
* text=auto

# Force LF for specific files
*.cs text eol=lf
*.razor text eol=lf
*.json text eol=lf
*.md text eol=lf

# Force CRLF for Windows-specific files
*.sln text eol=crlf
*.csproj text eol=crlf

# Binary files
*.png binary
*.jpg binary
*.gif binary
*.ico binary
*.dll binary
*.exe binary
```

---

## Summary

The provided .gitignore file:

✅ Ignores all Visual Studio temporary files  
✅ Ignores all build output (bin/, obj/)  
✅ Ignores all NuGet auto-generated files  
✅ Ignores all test results and logs  
✅ Ignores legacy project directories  
✅ Tracks all source code and documentation  
✅ Tracks solution and project files  
✅ Protects secrets and certificates  

**Next Steps**:
1. Copy `Pos.Web.gitignore` to `Pos.Web/.gitignore`
2. Run `git status` to verify
3. Clean up any already-tracked files
4. Commit and push

---

**Last Updated**: 2026-02-26  
**Maintained By**: Development Team
