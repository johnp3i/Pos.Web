# Pos.Web Setup Checklist

Use this checklist to track your progress setting up the new Pos.Web solution.

## Phase 1: Directory Structure ⏱️ 2 minutes

- [ ] Navigate to parent directory (where MyChairPos is located)
- [ ] Create `Pos.Web` directory
- [ ] Create `Pos.Web/.kiro` directory
- [ ] Create `Pos.Web/.kiro/specs` directory
- [ ] Create `Pos.Web/.kiro/steering` directory
- [ ] Create `Pos.Web/src` directory

**Commands**:
```bash
cd /path/to/parent-directory
mkdir Pos.Web
cd Pos.Web
mkdir -p .kiro/specs .kiro/steering src
```

## Phase 2: Move Spec Files ⏱️ 1 minute

- [ ] Move entire `web-based-pos-system` folder from MyChairPos to Pos.Web
- [ ] Verify all 13 files are present in Pos.Web

**Command**:
```bash
mv ../MyChairPos/.kiro/specs/web-based-pos-system .kiro/specs/
```

**Verify** (should see 13 files):
```bash
ls -la .kiro/specs/web-based-pos-system/
```

Expected files:
1. requirements.md
2. design.md
3. tasks.md
4. blazor-project-structure.md
5. blazor-pos-examples.md
6. mvc-vs-blazor-comparison.md
7. browser-compatibility.md
8. database-scripts.sql
9. pos-web-project-structure.md
10. .config.kiro
11. MIGRATION-GUIDE.md
12. QUICK-START.md
13. ORGANIZATION-SUMMARY.md
14. SETUP-CHECKLIST.md (this file)

## Phase 3: Copy Shared Steering Files ⏱️ 1 minute

- [ ] Copy `repository-standards.md` from MyChairPos to Pos.Web
- [ ] Copy `character-standards.md` from MyChairPos to Pos.Web

**Commands**:
```bash
cp ../MyChairPos/.kiro/steering/repository-standards.md .kiro/steering/
cp ../MyChairPos/.kiro/steering/character-standards.md .kiro/steering/
```

**Verify**:
```bash
ls -la .kiro/steering/
```

Should see:
- repository-standards.md
- character-standards.md

## Phase 4: Create New Steering Files ⏱️ 5 minutes

Create these 5 new files in `.kiro/steering/`. Content for each is in MIGRATION-GUIDE.md (Step 3).

### 4.1 product.md
- [ ] Create `.kiro/steering/product.md`
- [ ] Copy content from MIGRATION-GUIDE.md → Step 3.1
- [ ] Save file

**Content**: Web POS product overview (PWA, Blazor, offline support)

### 4.2 tech.md
- [ ] Create `.kiro/steering/tech.md`
- [ ] Copy content from MIGRATION-GUIDE.md → Step 3.2
- [ ] Save file

**Content**: Technology stack (.NET 8, Blazor WebAssembly, ASP.NET Core 8, EF Core 8)

### 4.3 structure.md
- [ ] Create `.kiro/steering/structure.md`
- [ ] Copy content from MIGRATION-GUIDE.md → Step 3.3
- [ ] Save file

**Content**: 5-project Clean Architecture (Shared, Infrastructure, API, Client, Tests)

### 4.4 blazor-patterns.md
- [ ] Create `.kiro/steering/blazor-patterns.md`
- [ ] Copy content from MIGRATION-GUIDE.md → Step 3.4
- [ ] Save file

**Content**: Blazor component patterns (Fluxor, MudBlazor, lifecycle, SignalR)

### 4.5 api-design.md
- [ ] Create `.kiro/steering/api-design.md`
- [ ] Copy content from MIGRATION-GUIDE.md → Step 3.5
- [ ] Save file

**Content**: RESTful API conventions (routes, status codes, validation, error handling)

**Verify**:
```bash
ls -la .kiro/steering/
```

Should see 7 files:
1. product.md (NEW)
2. tech.md (NEW)
3. structure.md (NEW)
4. blazor-patterns.md (NEW)
5. api-design.md (NEW)
6. repository-standards.md (COPIED)
7. character-standards.md (COPIED)

## Phase 5: Create Solution ⏱️ 1 minute

- [ ] Create `Pos.Web.sln` solution file
- [ ] Verify solution created

**Command**:
```bash
dotnet new sln -n Pos.Web
```

**Verify**:
```bash
ls -la *.sln
```

Should see: `Pos.Web.sln`

## Phase 6: Create Projects ⏱️ 2 minutes

### 6.1 Create Class Libraries
- [ ] Create `Pos.Web.Shared` (Class Library)
- [ ] Create `Pos.Web.Infrastructure` (Class Library)

**Commands**:
```bash
dotnet new classlib -n Pos.Web.Shared -o src/Pos.Web.Shared
dotnet new classlib -n Pos.Web.Infrastructure -o src/Pos.Web.Infrastructure
```

### 6.2 Create API Project
- [ ] Create `Pos.Web.API` (ASP.NET Core Web API)

**Command**:
```bash
dotnet new webapi -n Pos.Web.API -o src/Pos.Web.API
```

### 6.3 Create Client Project
- [ ] Create `Pos.Web.Client` (Blazor WebAssembly)

**Command**:
```bash
dotnet new blazorwasm -n Pos.Web.Client -o src/Pos.Web.Client
```

### 6.4 Create Test Project
- [ ] Create `Pos.Web.Tests` (xUnit Test Project)

**Command**:
```bash
dotnet new xunit -n Pos.Web.Tests -o src/Pos.Web.Tests
```

**Verify**:
```bash
ls -la src/
```

Should see 5 directories:
1. Pos.Web.Shared/
2. Pos.Web.Infrastructure/
3. Pos.Web.API/
4. Pos.Web.Client/
5. Pos.Web.Tests/

## Phase 7: Add Projects to Solution ⏱️ 1 minute

- [ ] Add `Pos.Web.Shared` to solution
- [ ] Add `Pos.Web.Infrastructure` to solution
- [ ] Add `Pos.Web.API` to solution
- [ ] Add `Pos.Web.Client` to solution
- [ ] Add `Pos.Web.Tests` to solution

**Commands**:
```bash
dotnet sln add src/Pos.Web.Shared/Pos.Web.Shared.csproj
dotnet sln add src/Pos.Web.Infrastructure/Pos.Web.Infrastructure.csproj
dotnet sln add src/Pos.Web.API/Pos.Web.API.csproj
dotnet sln add src/Pos.Web.Client/Pos.Web.Client.csproj
dotnet sln add src/Pos.Web.Tests/Pos.Web.Tests.csproj
```

**Verify**:
```bash
dotnet sln list
```

Should see 5 projects listed.

## Phase 8: Add Project References ⏱️ 1 minute

### 8.1 Infrastructure References
- [ ] Infrastructure → Shared

**Command**:
```bash
dotnet add src/Pos.Web.Infrastructure reference src/Pos.Web.Shared
```

### 8.2 API References
- [ ] API → Infrastructure
- [ ] API → Shared

**Commands**:
```bash
dotnet add src/Pos.Web.API reference src/Pos.Web.Infrastructure
dotnet add src/Pos.Web.API reference src/Pos.Web.Shared
```

### 8.3 Client References
- [ ] Client → Shared

**Command**:
```bash
dotnet add src/Pos.Web.Client reference src/Pos.Web.Shared
```

### 8.4 Test References
- [ ] Tests → Shared
- [ ] Tests → Infrastructure
- [ ] Tests → API
- [ ] Tests → Client

**Commands**:
```bash
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Shared
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Infrastructure
dotnet add src/Pos.Web.Tests reference src/Pos.Web.API
dotnet add src/Pos.Web.Tests reference src/Pos.Web.Client
```

## Phase 9: Build and Verify ⏱️ 1 minute

- [ ] Build entire solution
- [ ] Verify no build errors

**Command**:
```bash
dotnet build
```

**Expected output**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Phase 10: Open in Kiro ⏱️ 1 minute

- [ ] Open Kiro
- [ ] File → Open Folder
- [ ] Navigate to `Pos.Web` directory
- [ ] Open the folder
- [ ] Verify `.kiro` folder is recognized
- [ ] Navigate to `.kiro/specs/web-based-pos-system/tasks.md`

## Phase 11: Start Implementation 🚀

- [ ] Open `tasks.md`
- [ ] Review Task 1: "Set up solution structure and projects"
- [ ] Mark Task 1 as complete (already done!)
- [ ] Move to Task 2: "Create database schema (web schema)"
- [ ] Follow tasks sequentially

## Verification Checklist

### Directory Structure
```
Pos.Web/
├── .kiro/
│   ├── specs/
│   │   └── web-based-pos-system/  (14 files)
│   └── steering/                   (7 files)
├── src/
│   ├── Pos.Web.Shared/
│   ├── Pos.Web.Infrastructure/
│   ├── Pos.Web.API/
│   ├── Pos.Web.Client/
│   └── Pos.Web.Tests/
└── Pos.Web.sln
```

### File Counts
- [ ] `.kiro/specs/web-based-pos-system/`: 14 files
- [ ] `.kiro/steering/`: 7 files
- [ ] `src/`: 5 project directories
- [ ] Root: 1 solution file

### Build Status
- [ ] `dotnet build` succeeds with 0 errors
- [ ] All 5 projects compile successfully

### Kiro Integration
- [ ] Pos.Web folder opened in Kiro
- [ ] `.kiro` folder recognized
- [ ] Steering files accessible
- [ ] Spec files accessible
- [ ] `tasks.md` visible and readable

## Troubleshooting

### Issue: "dotnet command not found"
**Solution**: Install .NET 8 SDK from https://dotnet.microsoft.com/download

### Issue: Build errors after creating projects
**Solution**: Ensure all project references are added correctly (Phase 8)

### Issue: Kiro doesn't recognize .kiro folder
**Solution**: 
1. Close and reopen Kiro
2. Ensure you opened the `Pos.Web` folder (not a parent directory)
3. Verify `.kiro` folder exists in the root of opened folder

### Issue: Missing steering files
**Solution**: 
1. Check MIGRATION-GUIDE.md Step 3 for file content
2. Ensure files are in `.kiro/steering/` (not `.kiro/specs/`)
3. Verify file extensions are `.md` (not `.txt`)

## Time Estimate

| Phase | Time | Cumulative |
|-------|------|------------|
| 1. Directory Structure | 2 min | 2 min |
| 2. Move Spec Files | 1 min | 3 min |
| 3. Copy Shared Files | 1 min | 4 min |
| 4. Create Steering Files | 5 min | 9 min |
| 5. Create Solution | 1 min | 10 min |
| 6. Create Projects | 2 min | 12 min |
| 7. Add to Solution | 1 min | 13 min |
| 8. Add References | 1 min | 14 min |
| 9. Build & Verify | 1 min | 15 min |
| 10. Open in Kiro | 1 min | 16 min |
| **Total** | **16 min** | |

## Next Steps After Setup

1. ✅ Review `requirements.md` to understand business requirements
2. ✅ Review `design.md` to understand technical architecture
3. ✅ Review `tasks.md` to see implementation plan
4. ✅ Start with Task 2 (Task 1 is complete after this setup)
5. ✅ Follow tasks sequentially
6. ✅ Use steering files for guidance on patterns and conventions

## Related Documents

- **MIGRATION-GUIDE.md**: Detailed migration steps with file content
- **QUICK-START.md**: Quick setup guide (condensed version)
- **ORGANIZATION-SUMMARY.md**: Visual guide to folder organization
- **pos-web-project-structure.md**: Complete project structure documentation
- **tasks.md**: Implementation task list

---

**Completion Status**: _____ / 11 phases complete  
**Estimated Time Remaining**: _____ minutes  
**Ready to Code**: [ ] Yes [ ] No
