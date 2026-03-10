# Quick Start Guide: Setting Up Pos.Web Solution

## TL;DR - What You Need to Do

You need to manually create the Pos.Web directory structure outside of the current workspace, then move/create the necessary files.

## 5-Minute Setup

### 1. Create Directory Structure (2 minutes)

```bash
# Navigate to parent directory (where MyChairPos is located)
cd /path/to/parent-directory

# Create new solution directory
mkdir Pos.Web
cd Pos.Web

# Create .kiro folder structure
mkdir -p .kiro/specs
mkdir -p .kiro/steering
mkdir src
```

### 2. Move Spec Files (1 minute)

```bash
# Move the entire web-based-pos-system folder
mv ../MyChairPos/.kiro/specs/web-based-pos-system .kiro/specs/
```

### 3. Copy Shared Steering Files (1 minute)

```bash
# Copy shared standards that apply to both solutions
cp ../MyChairPos/.kiro/steering/repository-standards.md .kiro/steering/
cp ../MyChairPos/.kiro/steering/character-standards.md .kiro/steering/
```

### 4. Create New Steering Files (1 minute)

You'll need to create these 5 new steering files in `.kiro/steering/`:

1. **product.md** - Web POS product overview
2. **tech.md** - .NET 8, Blazor, ASP.NET Core 8 stack
3. **structure.md** - 5-project Clean Architecture
4. **blazor-patterns.md** - Blazor component patterns
5. **api-design.md** - RESTful API conventions

**Good news**: The content for all 5 files is already prepared in the MIGRATION-GUIDE.md file (Step 3). You can copy-paste from there.

### 5. Create Solution and Projects (2 minutes)

```bash
# Create solution
dotnet new sln -n Pos.Web

# Create projects
dotnet new classlib -n Pos.Web.Shared -o src/Pos.Web.Shared
dotnet new classlib -n Pos.Web.Infrastructure -o src/Pos.Web.Infrastructure
dotnet new webapi -n Pos.Web.API -o src/Pos.Web.API
dotnet new blazorwasm -n Pos.Web.Client -o src/Pos.Web.Client
dotnet new xunit -n Pos.Web.Tests -o src/Pos.Web.Tests

# Add projects to solution
dotnet sln add src/Pos.Web.Shared/Pos.Web.Shared.csproj
dotnet sln add src/Pos.Web.Infrastructure/Pos.Web.Infrastructure.csproj
dotnet sln add src/Pos.Web.API/Pos.Web.API.csproj
dotnet sln add src/Pos.Web.Client/Pos.Web.Client.csproj
dotnet sln add src/Pos.Web.Tests/Pos.Web.Tests.csproj

# Add project references
dotnet add src/Pos.Web.Infrastructure reference src/Pos.Web.Shared
dotnet add src/Pos.Web.API reference src/Pos.Web.Infrastructure
dotnet add src/Pos.Web.API reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Client reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Infrastructure
dotnet add src/Pos.Web.Tests reference src/Pos.Web.API
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Client

# Verify
dotnet build
```

### 6. Open in Kiro

1. Open Kiro
2. File → Open Folder
3. Navigate to `Pos.Web` directory
4. Open the folder

## What Gets Moved vs Created

### Moved from MyChairPos to Pos.Web
- ✅ `.kiro/specs/web-based-pos-system/` (entire folder with all files)

### Copied from MyChairPos to Pos.Web
- ✅ `repository-standards.md` (applies to both solutions)
- ✅ `character-standards.md` (applies to both solutions)

### Created New in Pos.Web
- 🆕 `product.md` (Web POS overview)
- 🆕 `tech.md` (.NET 8, Blazor stack)
- 🆕 `structure.md` (5-project architecture)
- 🆕 `blazor-patterns.md` (Blazor patterns)
- 🆕 `api-design.md` (API conventions)

### Stays in MyChairPos (Legacy-Specific)
- 🔒 `product.md` (Legacy WPF POS)
- 🔒 `tech.md` (.NET Framework 4.8, WPF)
- 🔒 `structure.md` (Legacy structure)
- 🔒 `pos-code-review.md`
- 🔒 `refactoring-strategy.md`
- 🔒 `pos-layout-design-pattern.md`
- 🔒 `pos-code-structure.md`

## Final Directory Structure

```
/parent-directory/
├── MyChairPos/          # Legacy WPF POS (keep as-is)
│   └── .kiro/
│       ├── specs/       # (empty after move)
│       └── steering/    # Legacy-specific files
│
└── Pos.Web/             # New Web POS (create this)
    ├── .kiro/
    │   ├── specs/
    │   │   └── web-based-pos-system/  # Moved from MyChairPos
    │   └── steering/
    │       ├── product.md              # NEW
    │       ├── tech.md                 # NEW
    │       ├── structure.md            # NEW
    │       ├── blazor-patterns.md      # NEW
    │       ├── api-design.md           # NEW
    │       ├── repository-standards.md # COPIED
    │       └── character-standards.md  # COPIED
    ├── src/
    │   ├── Pos.Web.Shared/
    │   ├── Pos.Web.Infrastructure/
    │   ├── Pos.Web.API/
    │   ├── Pos.Web.Client/
    │   └── Pos.Web.Tests/
    └── Pos.Web.sln
```

## Ready to Start Coding?

Once setup is complete:

1. Open `Pos.Web` folder in Kiro
2. Navigate to `.kiro/specs/web-based-pos-system/tasks.md`
3. Start with Task 1: "Set up solution structure and projects"
4. Follow the tasks sequentially

## Need More Details?

See `MIGRATION-GUIDE.md` for:
- Complete content for all 5 new steering files
- Detailed explanations of each step
- Rationale for the organization strategy
- Full project structure documentation

---

**Estimated Setup Time**: 10-15 minutes  
**Next Step**: Open Pos.Web in Kiro and start implementing tasks.md
