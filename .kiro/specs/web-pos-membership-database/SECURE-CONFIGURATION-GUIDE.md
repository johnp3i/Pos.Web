# Secure Configuration Guide - JWT Secret Key

## Overview

The JWT secret key is a critical security component used to sign and validate authentication tokens. This guide explains how to configure it securely for different environments.

## Security Requirements

- **Minimum Length**: 32 characters (256 bits)
- **Complexity**: Use a cryptographically secure random string
- **Uniqueness**: Different secret for each environment (dev, staging, production)
- **Confidentiality**: Never commit secrets to source control

## Configuration Methods

### Development Environment (User Secrets)

For local development, use .NET User Secrets to store the JWT secret key:

1. **Initialize User Secrets** (if not already done):
   ```bash
   cd Pos.Web/Pos.Web.API
   dotnet user-secrets init
   ```

2. **Set JWT Secret Key**:
   ```bash
   dotnet user-secrets set "Jwt:SecretKey" "your-32-character-or-longer-secret-key-here"
   ```

3. **Generate a Secure Random Key** (PowerShell):
   ```powershell
   # Generate a 64-character random key
   -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})
   ```

4. **Verify User Secrets**:
   ```bash
   dotnet user-secrets list
   ```

### Staging/Production Environment (Environment Variables)

For staging and production, use environment variables:

1. **Set Environment Variable** (Windows):
   ```powershell
   [System.Environment]::SetEnvironmentVariable('JWT_SECRET_KEY', 'your-production-secret-key', 'Machine')
   ```

2. **Set Environment Variable** (Linux/Docker):
   ```bash
   export JWT_SECRET_KEY="your-production-secret-key"
   ```

3. **Docker Compose**:
   ```yaml
   services:
     pos-api:
       environment:
         - JWT_SECRET_KEY=${JWT_SECRET_KEY}
   ```

4. **Azure App Service**:
   - Navigate to Configuration → Application settings
   - Add new setting: `JWT_SECRET_KEY` = `your-production-secret-key`

5. **Kubernetes Secret**:
   ```yaml
   apiVersion: v1
   kind: Secret
   metadata:
     name: pos-api-secrets
   type: Opaque
   stringData:
     JWT_SECRET_KEY: your-production-secret-key
   ```

## Configuration Priority

The application reads the JWT secret key in this order:

1. **Environment Variable**: `JWT_SECRET_KEY` (highest priority)
2. **User Secrets**: `Jwt:SecretKey` (development only)
3. **appsettings.json**: `Jwt:SecretKey` (fallback, not recommended for production)

## Generating Secure Keys

### PowerShell (Windows)
```powershell
# Generate 64-character alphanumeric key
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Generate 64-character key with special characters
-join ((33..126) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

### Bash (Linux/Mac)
```bash
# Generate 64-character random key
openssl rand -base64 48

# Generate 64-character hex key
openssl rand -hex 32
```

### C# (Programmatic)
```csharp
using System.Security.Cryptography;

public static string GenerateSecureKey(int length = 64)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
    var random = RandomNumberGenerator.Create();
    var bytes = new byte[length];
    random.GetBytes(bytes);
    
    return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
}
```

## Security Best Practices

### DO:
- ✅ Use environment variables for production
- ✅ Use User Secrets for development
- ✅ Generate keys with cryptographically secure random generators
- ✅ Use different keys for each environment
- ✅ Rotate keys periodically (every 6-12 months)
- ✅ Store keys in secure key management systems (Azure Key Vault, AWS Secrets Manager)
- ✅ Restrict access to production secrets (role-based access control)
- ✅ Audit secret access and changes

### DON'T:
- ❌ Commit secrets to source control (Git, SVN, etc.)
- ❌ Share secrets via email, chat, or unencrypted channels
- ❌ Use weak or predictable keys (e.g., "MySecretKey123")
- ❌ Reuse the same key across environments
- ❌ Store secrets in appsettings.json for production
- ❌ Log or display secrets in application output
- ❌ Include secrets in error messages or stack traces

## Key Rotation

When rotating JWT secret keys:

1. **Generate New Key**: Create a new secure random key
2. **Update Configuration**: Set new key in environment variable or User Secrets
3. **Restart Application**: Restart the API to load new key
4. **Invalidate Old Tokens**: All existing tokens will become invalid (users must re-login)
5. **Communicate**: Notify users of planned maintenance window

### Zero-Downtime Key Rotation (Advanced)

For zero-downtime rotation, implement dual-key validation:

1. Configure both old and new keys
2. Sign new tokens with new key
3. Validate tokens with both keys (accept either)
4. After token expiration period (e.g., 7 days), remove old key

## Troubleshooting

### Error: "JWT SecretKey not configured"
- **Cause**: No secret key found in environment variable or configuration
- **Solution**: Set `JWT_SECRET_KEY` environment variable or configure User Secrets

### Error: "JWT SecretKey must be at least 32 characters"
- **Cause**: Secret key is too short (less than 256 bits)
- **Solution**: Generate a longer key (minimum 32 characters)

### Error: "Invalid token signature"
- **Cause**: Token was signed with different key than validation key
- **Solution**: Ensure same key is used across all API instances

### Tokens Suddenly Invalid
- **Cause**: Secret key was changed
- **Solution**: Users must re-login to get new tokens

## Compliance

### GDPR / Data Protection
- JWT secret keys are considered sensitive data
- Implement access controls and audit logging
- Document key management procedures

### PCI DSS (if processing payments)
- Store keys in secure key management system
- Rotate keys regularly
- Restrict access to authorized personnel only

### SOC 2 / ISO 27001
- Implement key lifecycle management
- Document key generation, storage, rotation, and destruction
- Conduct regular security audits

## References

- [ASP.NET Core User Secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)
- [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/)
- [OWASP Key Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Key_Management_Cheat_Sheet.html)

## Support

For questions or issues with JWT secret configuration, contact the development team or refer to the main deployment guide.
