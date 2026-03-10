# Technology Stack

## Framework & Language

- **Language**: C# 
- **Target Framework**: .NET Framework 4.8
- **UI Framework**: WPF (Windows Presentation Foundation)
- **Build System**: MSBuild (Visual Studio 2015+)

## Data Access

- **ORM**: Entity Framework 6.x (Database-First approach)
- **Database**: Microsoft SQL Server
- **Connection**: ADO.NET with Entity Framework DbContext
- **Code Generation**: T4 Templates (.tt files) for entity and enum generation

## Key Libraries & Dependencies

### Core Dependencies
- **EntityFramework** 6.2.0+ - ORM for database operations
- **System.Reactive** 3.0.0 - Reactive Extensions for event-driven programming
- **DeviceId** 6.2.0 - Hardware identification for licensing
- **Newtonsoft.Json** 13.0.3 - JSON serialization

### UI & Input
- **WPFTabTip** 1.2.2 - Touch keyboard support
- **WPFControls** - Custom control library (local reference)

### Printing
- **Seagull.BarTender.Print.dll** - Label printing integration

### Security
- **System.Security.Cryptography** - RSA and AES encryption for licensing

## Project Structure

The solution follows a multi-project architecture:

```
MyChairPos.sln
├── POS                      # Main POS application
├── POSAdmin                 # Admin interface
├── POS-C                    # Customer display
├── OrdersMonitor            # Kitchen/service monitoring
├── PosDbForAll              # POS database entities (EF)
├── PosDbCentralForAll       # Central database entities
├── OmasDbForAll             # OMAS database entities (EF)
├── ApplicationsSecurity     # Licensing & encryption
├── Security                 # Security utilities
├── ShipData                 # Data sync utility
└── PosServerCommands        # Server command processor
```

## Build Configurations

- **Debug**: Development build with full symbols
- **Release**: Production build with optimizations
- **DEVICE**: Device-specific configuration for deployment

## Common Commands

### Building
```cmd
# Build entire solution
msbuild MyChairPos.sln /p:Configuration=Release

# Build specific project
msbuild POS\POS.csproj /p:Configuration=Release

# Clean and rebuild
msbuild MyChairPos.sln /t:Clean,Build /p:Configuration=Release
```

### Package Restore
```cmd
# Restore NuGet packages
nuget restore MyChairPos.sln
```

### Database Operations
- Entity models are generated from database using EF Designer (.edmx files)
- T4 templates auto-generate entity classes and enums from database schema
- Connection strings configured in App.config files

## Development Environment

- **IDE**: Visual Studio 2015 or later
- **Required SDKs**: .NET Framework 4.8 SDK
- **Database Tools**: SQL Server Management Studio (recommended)
- **Version Control**: Git

## Deployment

- ClickOnce deployment configured for OrdersMonitor (OPS)
- FTP deployment to: ftp://163.123.183.156/setup.ops/
- Web install URL: http://mcp.3inventors.com/ops/
