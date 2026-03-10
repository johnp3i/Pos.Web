# Web POS Membership System - Deployment Guide

**Version:** 1.0.0  
**Last Updated:** March 4, 2026

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Environment Setup](#environment-setup)
3. [Database Setup](#database-setup)
4. [Application Configuration](#application-configuration)
5. [Deployment Steps](#deployment-steps)
6. [Post-Deployment Verification](#post-deployment-verification)
7. [Rollback Procedures](#rollback-procedures)
8. [Monitoring and Logging](#monitoring-and-logging)
9. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Software Requirements

**Server Requirements:**
- Windows Server 2019 or later / Linux (Ubuntu 20.04+)
- .NET 8.0 Runtime
- SQL Server 2019 or later
- IIS 10.0+ (Windows) or Nginx/Apache (Linux)
- SSL Certificate for HTTPS

**Development Tools (for deployment):**
- .NET 8.0 SDK
- SQL Server Management Studio (SSMS)
- Git

### Hardware Requirements

**Minimum (Development/Staging):**
- CPU: 2 cores
- RAM: 4 GB
- Disk: 20 GB SSD

**Recommended (Production):**
- CPU: 4+ cores
- RAM: 8+ GB
- Disk: 50+ GB SSD
- Network: 1 Gbps

---

## Environment Setup

### 1. Development Environment

```bash
# Clone repository
git clone https://github.com/yourcompany/pos-web.git
cd pos-web

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Run migrations
dotnet ef database update --context WebPosMembershipDbContext --project Pos.Web.Infrastructure --startup-project Pos.Web.API
```

### 2. Staging Environment

Same as development, but with staging configuration:

```bash
# Set environment
$env:ASPNETCORE_ENVIRONMENT="Staging"

# Use staging connection strings
# Update appsettings.Staging.json
```

### 3. Production Environment

```bash
# Set environment
$env:ASPNETCORE_ENVIRONMENT="Production"

# Use production connection strings from environment variables
$env:ConnectionStrings__WebPosMembership="Server=prod-sql;Database=WebPosMembership;..."
$env:ConnectionStrings__PosDatabase="Server=prod-sql;Database=POS;..."
```

---

## Database Setup

### Step 1: Create Database

```sql
-- Connect to SQL Server
-- Run as sa or database administrator

-- Create database
CREATE DATABASE WebPosMembership;
GO

-- Verify database
USE WebPosMembership;
GO

SELECT DB_NAME() AS CurrentDatabase;
```

### Step 2: Configure Database User

```sql
-- Create login for application
CREATE LOGIN PosWebApp WITH PASSWORD = 'StrongPassword123!';
GO

-- Create user in WebPosMembership database
USE WebPosMembership;
GO

CREATE USER PosWebApp FOR LOGIN PosWebApp;
GO

-- Grant permissions
ALTER ROLE db_datareader ADD MEMBER PosWebApp;
ALTER ROLE db_datawriter ADD MEMBER PosWebApp;
ALTER ROLE db_ddladmin ADD MEMBER PosWebApp; -- For migrations
GO

-- Grant execute permissions for stored procedures
GRANT EXECUTE TO PosWebApp;
GO
```

### Step 3: Configure Connection Strings

**Development (appsettings.Development.json):**
```json
{
  "ConnectionStrings": {
    "WebPosMembership": "Server=127.0.0.1;Database=WebPosMembership;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30",
    "PosDatabase": "Server=127.0.0.1;Database=POS;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  }
}
```

**Production (Environment Variables):**
```bash
# Windows
setx ConnectionStrings__WebPosMembership "Server=prod-sql.company.com;Database=WebPosMembership;User Id=PosWebApp;Password=StrongPassword123!;Encrypt=True;MultipleActiveResultSets=True;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30"

setx ConnectionStrings__PosDatabase "Server=prod-sql.company.com;Database=POS;User Id=PosWebApp;Password=StrongPassword123!;Encrypt=True;MultipleActiveResultSets=True"

# Linux
export ConnectionStrings__WebPosMembership="Server=prod-sql.company.com;Database=WebPosMembership;User Id=PosWebApp;Password=StrongPassword123!;Encrypt=True;MultipleActiveResultSets=True;Min Pool Size=5;Max Pool Size=100;Connection Timeout=30"

export ConnectionStrings__PosDatabase="Server=prod-sql.company.com;Database=POS;User Id=PosWebApp;Password=StrongPassword123!;Encrypt=True;MultipleActiveResultSets=True"
```

### Step 4: Run Migrations

```bash
# Navigate to solution directory
cd Pos.Web

# Run Entity Framework migrations
dotnet ef database update --context WebPosMembershipDbContext --project Pos.Web.Infrastructure --startup-project Pos.Web.API

# Verify tables created
# Connect to SQL Server and check tables:
# - AspNetUsers, AspNetRoles, AspNetUserRoles
# - RefreshTokens, UserSessions, AuthAuditLog, PasswordHistory
```

### Step 5: Seed System Roles

Roles are automatically seeded on application startup by `DbInitializer.cs`.

Verify roles:
```sql
USE WebPosMembership;
GO

SELECT * FROM AspNetRoles;
-- Should see: Admin, Manager, Cashier, Waiter, Kitchen
```

---

## Application Configuration

### 1. JWT Configuration

**CRITICAL: Never commit JWT secret to source control!**

**Development (User Secrets):**
```bash
# Navigate to API project
cd Pos.Web/Pos.Web.API

# Initialize user secrets
dotnet user-secrets init

# Set JWT secret (minimum 32 characters)
dotnet user-secrets set "Jwt:SecretKey" "your-super-secret-key-at-least-32-characters-long-for-256-bit-security"

# Set JWT issuer and audience
dotnet user-secrets set "Jwt:Issuer" "MyChairPOS.API"
dotnet user-secrets set "Jwt:Audience" "MyChairPOS.Client"
```

**Production (Environment Variables):**
```bash
# Windows
setx JWT_SECRET_KEY "your-super-secret-key-at-least-32-characters-long-for-256-bit-security"

# Linux
export JWT_SECRET_KEY="your-super-secret-key-at-least-32-characters-long-for-256-bit-security"
```

**Verify JWT Configuration:**
```json
{
  "Jwt": {
    "SecretKey": "from-environment-or-user-secrets",
    "Issuer": "MyChairPOS.API",
    "Audience": "MyChairPOS.Client",
    "ExpirationMinutes": 60
  }
}
```

### 2. CORS Configuration

**appsettings.json:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://pos.yourcompany.com",
      "https://staging-pos.yourcompany.com"
    ]
  }
}
```

**Development:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5055",
      "https://localhost:7055"
    ]
  }
}
```

### 3. Rate Limiting Configuration

**appsettings.json:**
```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 1000
      },
      {
        "Endpoint": "*/api/membership/auth/login",
        "Period": "1m",
        "Limit": 100
      }
    ]
  }
}
```

### 4. Logging Configuration

**appsettings.Production.json:**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/pos-api-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "WebPosMembership",
          "tableName": "ApplicationLogs",
          "autoCreateSqlTable": true
        }
      }
    ]
  }
}
```

---

## Deployment Steps

### Option 1: IIS Deployment (Windows)

#### Step 1: Publish Application

```bash
# Navigate to API project
cd Pos.Web/Pos.Web.API

# Publish for production
dotnet publish -c Release -o C:\inetpub\wwwroot\pos-api

# Navigate to Client project
cd ../Pos.Web.Client

# Publish Blazor WebAssembly
dotnet publish -c Release -o C:\inetpub\wwwroot\pos-client
```

#### Step 2: Configure IIS

```powershell
# Import IIS module
Import-Module WebAdministration

# Create application pool
New-WebAppPool -Name "PosWebAPI" -Force
Set-ItemProperty IIS:\AppPools\PosWebAPI -Name "managedRuntimeVersion" -Value ""

# Create website
New-Website -Name "PosWebAPI" `
    -PhysicalPath "C:\inetpub\wwwroot\pos-api" `
    -ApplicationPool "PosWebAPI" `
    -Port 443 `
    -Protocol https `
    -HostHeader "api.yourcompany.com"

# Bind SSL certificate
$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*yourcompany.com*"}
New-WebBinding -Name "PosWebAPI" -Protocol https -Port 443 -HostHeader "api.yourcompany.com" -SslFlags 1
$binding = Get-WebBinding -Name "PosWebAPI" -Protocol https
$binding.AddSslCertificate($cert.Thumbprint, "My")
```

#### Step 3: Configure Application Pool

- Set .NET CLR Version: No Managed Code
- Set Identity: ApplicationPoolIdentity or custom service account
- Set Start Mode: AlwaysRunning
- Set Idle Timeout: 0 (never timeout)

#### Step 4: Set Environment Variables

```powershell
# Set environment variables for application pool
$appPoolPath = "IIS:\AppPools\PosWebAPI"
Set-ItemProperty $appPoolPath -Name "environmentVariables" -Value @{
    "ASPNETCORE_ENVIRONMENT" = "Production"
    "JWT_SECRET_KEY" = "your-secret-key"
    "ConnectionStrings__WebPosMembership" = "your-connection-string"
}
```

### Option 2: Linux Deployment (Nginx)

#### Step 1: Publish Application

```bash
# Publish API
dotnet publish -c Release -o /var/www/pos-api

# Publish Client
dotnet publish -c Release -o /var/www/pos-client
```

#### Step 2: Create Systemd Service

```bash
# Create service file
sudo nano /etc/systemd/system/pos-api.service
```

**pos-api.service:**
```ini
[Unit]
Description=MyChair POS API
After=network.target

[Service]
WorkingDirectory=/var/www/pos-api
ExecStart=/usr/bin/dotnet /var/www/pos-api/Pos.Web.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=pos-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=JWT_SECRET_KEY=your-secret-key
Environment=ConnectionStrings__WebPosMembership=your-connection-string

[Install]
WantedBy=multi-user.target
```

```bash
# Enable and start service
sudo systemctl enable pos-api
sudo systemctl start pos-api
sudo systemctl status pos-api
```

#### Step 3: Configure Nginx

```bash
# Create Nginx configuration
sudo nano /etc/nginx/sites-available/pos-api
```

**pos-api:**
```nginx
server {
    listen 443 ssl http2;
    server_name api.yourcompany.com;

    ssl_certificate /etc/ssl/certs/yourcompany.com.crt;
    ssl_certificate_key /etc/ssl/private/yourcompany.com.key;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Real-IP $remote_addr;
    }
}

# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name api.yourcompany.com;
    return 301 https://$server_name$request_uri;
}
```

```bash
# Enable site
sudo ln -s /etc/nginx/sites-available/pos-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Option 3: Docker Deployment

#### Step 1: Create Dockerfile

**Pos.Web/Pos.Web.API/Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Pos.Web.API/Pos.Web.API.csproj", "Pos.Web.API/"]
COPY ["Pos.Web.Infrastructure/Pos.Web.Infrastructure.csproj", "Pos.Web.Infrastructure/"]
COPY ["Pos.Web.Shared/Pos.Web.Shared.csproj", "Pos.Web.Shared/"]
RUN dotnet restore "Pos.Web.API/Pos.Web.API.csproj"
COPY . .
WORKDIR "/src/Pos.Web.API"
RUN dotnet build "Pos.Web.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Pos.Web.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pos.Web.API.dll"]
```

#### Step 2: Build and Run

```bash
# Build image
docker build -t pos-web-api:1.0.0 -f Pos.Web/Pos.Web.API/Dockerfile .

# Run container
docker run -d \
  --name pos-api \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e JWT_SECRET_KEY=your-secret-key \
  -e ConnectionStrings__WebPosMembership="your-connection-string" \
  pos-web-api:1.0.0
```

#### Step 3: Docker Compose

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  pos-api:
    image: pos-web-api:1.0.0
    ports:
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JWT_SECRET_KEY=${JWT_SECRET_KEY}
      - ConnectionStrings__WebPosMembership=${CONNECTION_STRING}
    restart: always
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

```bash
# Start services
docker-compose up -d
```

---

## Post-Deployment Verification

### 1. Health Check

```bash
# Check API health
curl https://api.yourcompany.com/health

# Expected response:
# Healthy
```

### 2. Verify Database Connection

```bash
# Check database connectivity
curl https://api.yourcompany.com/api/membership/auth/login -X POST \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'

# Should return 401 (authentication failed) not 503 (database error)
```

### 3. Test Authentication Flow

```bash
# 1. Login (should fail with invalid credentials)
curl -X POST https://api.yourcompany.com/api/membership/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"wrong"}'

# 2. Check Swagger UI
# Navigate to: https://api.yourcompany.com/swagger
```

### 4. Verify Logging

```bash
# Check application logs
# Windows: C:\inetpub\wwwroot\pos-api\logs
# Linux: /var/www/pos-api/logs

# Check database logs
SELECT TOP 10 * FROM WebPosMembership.dbo.ApplicationLogs
ORDER BY TimeStamp DESC;
```

### 5. Verify Background Services

```sql
-- Check session cleanup is running
SELECT COUNT(*) as ActiveSessions
FROM UserSessions
WHERE EndedAt IS NULL;

-- Check audit log archival
SELECT COUNT(*) as RecentLogs
FROM AuthAuditLog
WHERE Timestamp > DATEADD(DAY, -7, GETUTCDATE());
```

---

## Rollback Procedures

### Scenario 1: Application Rollback

```bash
# Stop application
# Windows IIS:
Stop-Website -Name "PosWebAPI"

# Linux:
sudo systemctl stop pos-api

# Restore previous version
# Windows:
Copy-Item "C:\backups\pos-api-v0.9.0\*" "C:\inetpub\wwwroot\pos-api" -Recurse -Force

# Linux:
sudo cp -r /backups/pos-api-v0.9.0/* /var/www/pos-api/

# Start application
# Windows:
Start-Website -Name "PosWebAPI"

# Linux:
sudo systemctl start pos-api
```

### Scenario 2: Database Rollback

```sql
-- Restore database from backup
USE master;
GO

-- Set database to single user mode
ALTER DATABASE WebPosMembership SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Restore from backup
RESTORE DATABASE WebPosMembership
FROM DISK = 'C:\Backups\WebPosMembership_BeforeDeployment.bak'
WITH REPLACE, RECOVERY;
GO

-- Set back to multi-user mode
ALTER DATABASE WebPosMembership SET MULTI_USER;
GO
```

### Scenario 3: Configuration Rollback

```bash
# Restore previous configuration
# Windows:
Copy-Item "C:\backups\appsettings.Production.json" "C:\inetpub\wwwroot\pos-api\" -Force

# Linux:
sudo cp /backups/appsettings.Production.json /var/www/pos-api/

# Restart application
# Windows:
Restart-WebAppPool -Name "PosWebAPI"

# Linux:
sudo systemctl restart pos-api
```

---

## Monitoring and Logging

### Application Monitoring

**Key Metrics to Monitor:**
- Request rate (requests/second)
- Response time (average, p95, p99)
- Error rate (4xx, 5xx responses)
- CPU usage
- Memory usage
- Database connection pool usage

**Tools:**
- Application Insights (Azure)
- Prometheus + Grafana
- ELK Stack (Elasticsearch, Logstash, Kibana)

### Database Monitoring

```sql
-- Monitor active sessions
SELECT COUNT(*) as ActiveSessions
FROM sys.dm_exec_sessions
WHERE database_id = DB_ID('WebPosMembership');

-- Monitor blocking queries
SELECT * FROM sys.dm_exec_requests
WHERE blocking_session_id <> 0;

-- Monitor database size
EXEC sp_spaceused;
```

### Log Monitoring

```sql
-- Monitor error logs
SELECT TOP 100 *
FROM ApplicationLogs
WHERE Level = 'Error'
ORDER BY TimeStamp DESC;

-- Monitor failed login attempts
SELECT TOP 100 *
FROM AuthAuditLog
WHERE EventType = 'LoginFailed'
  AND Timestamp > DATEADD(HOUR, -1, GETUTCDATE())
ORDER BY Timestamp DESC;
```

---

## Troubleshooting

### Issue 1: Application Won't Start

**Symptoms:** Application fails to start, 502 Bad Gateway

**Diagnosis:**
```bash
# Check application logs
# Windows: Event Viewer → Application
# Linux: journalctl -u pos-api -n 100

# Check if port is in use
netstat -ano | findstr :5000  # Windows
sudo netstat -tulpn | grep :5000  # Linux
```

**Solutions:**
1. Check connection strings are correct
2. Verify JWT secret key is set
3. Check database is accessible
4. Verify .NET 8.0 runtime is installed

### Issue 2: Database Connection Failures

**Symptoms:** 503 Service Unavailable, "Database connection failed"

**Diagnosis:**
```sql
-- Test connection from server
sqlcmd -S server-name -U username -P password -d WebPosMembership -Q "SELECT 1"
```

**Solutions:**
1. Verify SQL Server is running
2. Check firewall rules allow connection
3. Verify connection string is correct
4. Check database user has permissions

### Issue 3: JWT Token Validation Failures

**Symptoms:** 401 Unauthorized on all authenticated requests

**Diagnosis:**
```bash
# Check JWT configuration
# Verify secret key length (minimum 32 characters)
# Verify issuer and audience match
```

**Solutions:**
1. Ensure JWT_SECRET_KEY environment variable is set
2. Verify secret key is at least 32 characters
3. Check issuer and audience configuration
4. Verify ClockSkew is set to TimeSpan.Zero

### Issue 4: High Memory Usage

**Symptoms:** Application consuming excessive memory

**Diagnosis:**
```bash
# Check memory usage
# Windows: Task Manager
# Linux: top or htop

# Check for memory leaks
dotnet-dump collect -p <process-id>
dotnet-dump analyze <dump-file>
```

**Solutions:**
1. Check for memory leaks in custom code
2. Verify connection pooling is configured correctly
3. Check for large objects in memory cache
4. Review audit log retention policy

---

## Security Checklist

- [ ] HTTPS enabled with valid SSL certificate
- [ ] JWT secret key is at least 32 characters
- [ ] JWT secret key stored in environment variables (not appsettings.json)
- [ ] Database connection uses encrypted connection (Encrypt=True)
- [ ] Database user has minimum required permissions
- [ ] CORS configured with explicit origin whitelist
- [ ] Rate limiting enabled
- [ ] HSTS enabled in production
- [ ] Input validation enabled (FluentValidation)
- [ ] Audit logging enabled
- [ ] Password policy enforced
- [ ] Account lockout enabled
- [ ] Backup and restore procedures tested

---

## Support

For deployment support:
- **DevOps Team:** devops@yourcompany.com
- **Documentation:** https://docs.yourcompany.com/deployment
- **Emergency Hotline:** +1-555-0100
