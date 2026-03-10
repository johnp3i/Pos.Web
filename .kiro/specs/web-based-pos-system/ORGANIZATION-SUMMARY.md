# .kiro Folder Organization Summary

## Visual Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                         PARENT DIRECTORY                            │
└─────────────────────────────────────────────────────────────────────┘
                                 │
                 ┌───────────────┴───────────────┐
                 │                               │
                 ▼                               ▼
┌────────────────────────────┐  ┌────────────────────────────────────┐
│      MyChairPos/           │  │         Pos.Web/                   │
│   (Legacy WPF POS)         │  │    (New Web POS)                   │
└────────────────────────────┘  └────────────────────────────────────┘
                 │                               │
                 ▼                               ▼
        ┌────────────────┐            ┌────────────────────┐
        │   .kiro/       │            │      .kiro/        │
        └────────────────┘            └────────────────────┘
                 │                               │
         ┌───────┴────────┐           ┌─────────┴──────────┐
         │                │           │                    │
         ▼                ▼           ▼                    ▼
    ┌────────┐      ┌──────────┐  ┌────────┐        ┌──────────┐
    │ specs/ │      │steering/ │  │ specs/ │        │steering/ │
    │(empty) │      │          │  │        │        │          │
    └────────┘      └──────────┘  └────────┘        └──────────┘
                         │              │                  │
                         │              │                  │
                         ▼              ▼                  ▼
              ┌──────────────────┐  ┌──────────┐  ┌──────────────────┐
              │ Legacy-Specific  │  │web-based-│  │  Web-Specific    │
              │   Steering       │  │pos-system│  │    Steering      │
              │                  │  │  (moved) │  │                  │
              │ • product.md     │  │          │  │ • product.md     │
              │ • tech.md        │  │ • req.md │  │ • tech.md        │
              │ • structure.md   │  │ • des.md │  │ • structure.md   │
              │ • pos-code-*.md  │  │ • tasks  │  │ • blazor-*.md    │
              │ • refactoring-*  │  │ • *.sql  │  │ • api-design.md  │
              │                  │  │ • *.md   │  │                  │
              │ Shared Standards │  └──────────┘  │ Shared Standards │
              │ • repo-stds.md   │                │ • repo-stds.md   │
              │ • char-stds.md   │                │ • char-stds.md   │
              └──────────────────┘                └──────────────────┘
```

## File Movement Map

### Before Migration

```
MyChairPos/
└── .kiro/
    ├── specs/
    │   └── web-based-pos-system/  ← All web POS spec files here
    │       ├── requirements.md
    │       ├── design.md
    │       ├── tasks.md
    │       └── ... (8 more files)
    └── steering/
        ├── product.md              ← Legacy WPF POS
        ├── tech.md                 ← .NET Framework 4.8
        ├── structure.md            ← Legacy structure
        ├── repository-standards.md ← Shared
        ├── character-standards.md  ← Shared
        └── ... (4 legacy-specific files)
```

### After Migration

```
MyChairPos/                         Pos.Web/
└── .kiro/                          └── .kiro/
    ├── specs/                          ├── specs/
    │   └── (empty)                     │   └── web-based-pos-system/  ← Moved here
    │                                   │       ├── requirements.md
    └── steering/                       │       ├── design.md
        ├── product.md                  │       ├── tasks.md
        ├── tech.md                     │       └── ... (8 more files)
        ├── structure.md                │
        ├── repository-standards.md     └── steering/
        ├── character-standards.md          ├── product.md              ← NEW (Web POS)
        └── ... (4 legacy files)            ├── tech.md                 ← NEW (.NET 8)
                                            ├── structure.md            ← NEW (5-project)
                                            ├── blazor-patterns.md      ← NEW
                                            ├── api-design.md           ← NEW
                                            ├── repository-standards.md ← COPIED
                                            └── character-standards.md  ← COPIED
```

## File Categories

### 📦 Moved Files (1 folder)
```
Source: MyChairPos/.kiro/specs/web-based-pos-system/
Target: Pos.Web/.kiro/specs/web-based-pos-system/

Files:
✓ requirements.md
✓ design.md
✓ tasks.md
✓ blazor-project-structure.md
✓ blazor-pos-examples.md
✓ mvc-vs-blazor-comparison.md
✓ browser-compatibility.md
✓ database-scripts.sql
✓ pos-web-project-structure.md
✓ .config.kiro
✓ MIGRATION-GUIDE.md (new)
✓ QUICK-START.md (new)
✓ ORGANIZATION-SUMMARY.md (new - this file)
```

### 📋 Copied Files (2 files)
```
Source: MyChairPos/.kiro/steering/
Target: Pos.Web/.kiro/steering/

Files:
✓ repository-standards.md (applies to both solutions)
✓ character-standards.md (applies to both solutions)
```

### 🆕 New Files in Pos.Web (5 files)
```
Location: Pos.Web/.kiro/steering/

Files to create:
□ product.md              (Web POS product overview)
□ tech.md                 (.NET 8, Blazor, ASP.NET Core 8)
□ structure.md            (5-project Clean Architecture)
□ blazor-patterns.md      (Blazor component patterns)
□ api-design.md           (RESTful API conventions)

Note: Content for all 5 files is in MIGRATION-GUIDE.md Step 3
```

### 🔒 Kept in MyChairPos (9 files)
```
Location: MyChairPos/.kiro/steering/

Legacy-specific files (do not move):
✓ product.md                   (Legacy WPF POS overview)
✓ tech.md                      (.NET Framework 4.8, WPF, EF6)
✓ structure.md                 (Legacy multi-project structure)
✓ repository-standards.md      (Keep original, copy to Pos.Web)
✓ character-standards.md       (Keep original, copy to Pos.Web)
✓ pos-code-review.md           (Legacy code review)
✓ refactoring-strategy.md      (Legacy V2 refactoring)
✓ pos-layout-design-pattern.md (Legacy WPF patterns)
✓ pos-code-structure.md        (Legacy code structure)
```

## Steering File Comparison

### MyChairPos Steering Files (Legacy Focus)

| File | Purpose | Technology |
|------|---------|------------|
| product.md | WPF POS overview | Desktop application |
| tech.md | .NET Framework 4.8, WPF, EF6 | Windows-only |
| structure.md | Multi-project WPF structure | Desktop architecture |
| pos-code-review.md | Legacy code review | WPF patterns |
| refactoring-strategy.md | V2 refactoring with feature flags | Incremental modernization |
| pos-layout-design-pattern.md | WPF layout patterns | XAML, code-behind |
| pos-code-structure.md | Legacy code structure | Static helpers, God objects |
| repository-standards.md | Repository patterns | Applies to both |
| character-standards.md | Engineering standards | Applies to both |

### Pos.Web Steering Files (Modern Focus)

| File | Purpose | Technology |
|------|---------|------------|
| product.md | Web POS overview | Progressive Web App |
| tech.md | .NET 8, Blazor, ASP.NET Core 8 | Cross-platform |
| structure.md | 5-project Clean Architecture | Modern layered architecture |
| blazor-patterns.md | Blazor component patterns | Razor components, Fluxor |
| api-design.md | RESTful API conventions | HTTP, SignalR |
| repository-standards.md | Repository patterns | Applies to both |
| character-standards.md | Engineering standards | Applies to both |

## Why This Organization?

### Separation of Concerns
- **Legacy files** stay with legacy solution (MyChairPos)
- **Web files** move to new solution (Pos.Web)
- **Shared standards** copied to both (repository patterns, engineering standards)

### Benefits
1. ✅ Clear context for each solution
2. ✅ No confusion about which technology stack to use
3. ✅ Kiro provides relevant guidance based on current workspace
4. ✅ Both solutions can evolve independently
5. ✅ Shared standards ensure consistency across both

### Database Strategy
- Both solutions share same SQL Server database
- Legacy uses `dbo` schema (existing tables)
- New Web POS uses:
  - `dbo` schema (read-only access to legacy data)
  - `web` schema (new web-specific tables)

## Checklist

### Pre-Migration
- [ ] Understand the organization strategy
- [ ] Review MIGRATION-GUIDE.md for detailed steps
- [ ] Review QUICK-START.md for quick setup

### Migration Steps
- [ ] Create Pos.Web directory structure
- [ ] Move `.kiro/specs/web-based-pos-system/` to Pos.Web
- [ ] Copy `repository-standards.md` to Pos.Web
- [ ] Copy `character-standards.md` to Pos.Web
- [ ] Create 5 new steering files in Pos.Web (use content from MIGRATION-GUIDE.md)
- [ ] Create solution and projects using dotnet CLI
- [ ] Verify build: `dotnet build`

### Post-Migration
- [ ] Open Pos.Web in Kiro
- [ ] Verify `.kiro` folder is recognized
- [ ] Open `tasks.md` and start implementing
- [ ] Keep MyChairPos workspace for legacy work

## Quick Reference

### File Counts

| Location | Specs | Steering | Total |
|----------|-------|----------|-------|
| MyChairPos (before) | 1 folder (10 files) | 9 files | 19 files |
| MyChairPos (after) | 0 | 9 files | 9 files |
| Pos.Web (after) | 1 folder (13 files) | 7 files | 20 files |

### Commands Summary

```bash
# Create structure
mkdir -p Pos.Web/.kiro/specs Pos.Web/.kiro/steering Pos.Web/src

# Move specs
mv MyChairPos/.kiro/specs/web-based-pos-system Pos.Web/.kiro/specs/

# Copy shared
cp MyChairPos/.kiro/steering/repository-standards.md Pos.Web/.kiro/steering/
cp MyChairPos/.kiro/steering/character-standards.md Pos.Web/.kiro/steering/

# Create solution
cd Pos.Web
dotnet new sln -n Pos.Web
# ... (see QUICK-START.md for full commands)
```

---

**Document Purpose**: Visual guide to understand the .kiro folder organization  
**Related Documents**:
- MIGRATION-GUIDE.md (detailed steps with file content)
- QUICK-START.md (quick setup guide)
- pos-web-project-structure.md (complete project structure)
