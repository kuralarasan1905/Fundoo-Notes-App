# Fundoo Notes - Deployment Guide

This guide provides step-by-step instructions for deploying the Fundoo Notes application in a production environment.

## Prerequisites

- SQL Server instance accessible at `KURALARASAN\\SQLEXPRESS`
- SQL Server user `appuser` with password `kural1905`
- .NET 8.0 Runtime installed on the target server
- IIS (for Windows deployment) or reverse proxy setup

## Quick Start

### 1. Database Setup

#### Option A: Using SQL Server Management Studio (Recommended)
1. Open SQL Server Management Studio
2. Connect to your SQL Server: `KURALARASAN\\SQLEXPRESS`
3. Login with username: `appuser`, password: `kural1905`
4. Open the file: `Database/FundooNotesDB_CreateTables.sql`
5. Execute the script to create the database and all tables
6. Verify the database `FundooNotesDB` is created with sample data

#### Option B: Using PowerShell Script
```powershell
# Run from the project root directory
.\Scripts\setup-database.ps1
```

### 2. Application Configuration

Update `appsettings.Production.json` with your production settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=KURALARASAN\\SQLEXPRESS;Database=FundooNotesDB;User Id=appuser;Password=kural1905;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-production-super-secret-key-minimum-32-characters",
    "Issuer": "FundooNotesAPI",
    "Audience": "FundooNotesClient",
    "ExpirationInMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password",
    "SenderName": "Fundoo Notes"
  }
}
```

### 3. Build and Run

#### Development Mode
```powershell
# Using PowerShell script
.\Scripts\start-application.ps1

# Or manually
dotnet build
dotnet run
```

#### Production Mode
```powershell
# Build for production
dotnet publish -c Release -o ./publish

# Run in production
cd publish
dotnet fundoo-notes.dll --environment=Production
```

## API Endpoints

Once deployed, the API will be available at:
- **Base URL**: `https://localhost:7139` (HTTPS) or `http://localhost:5139` (HTTP)
- **Swagger Documentation**: `https://localhost:7139/swagger`
- **Health Check**: `https://localhost:7139/api/health`

### Key Endpoints

#### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login
- `POST /api/auth/verify-email` - Verify email
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password

#### Notes Management
- `GET /api/notes` - Get user notes
- `POST /api/notes` - Create note
- `PUT /api/notes/{id}` - Update note
- `PATCH /api/notes/{id}/pin` - Toggle pin status
- `PATCH /api/notes/{id}/archive` - Toggle archive status

#### Labels Management
- `GET /api/labels` - Get user labels
- `POST /api/labels` - Create label

## Testing the Deployment

### 1. Health Check
```bash
curl -X GET "https://localhost:7139/api/health"
```

Expected response:
```json
{
  "status": "Healthy",
  "timestamp": "2025-01-XX...",
  "version": "1.0.0.0",
  "environment": "Production",
  "machineName": "...",
  "message": "Fundoo Notes API is running successfully"
}
```

### 2. User Registration
```bash
curl -X POST "https://localhost:7139/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Test",
    "lastName": "User",
    "email": "test@example.com",
    "password": "TestPassword123!",
    "confirmPassword": "TestPassword123!"
  }'
```

### 3. User Login
```bash
curl -X POST "https://localhost:7139/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123!"
  }'
```

### 4. Create a Note (requires JWT token from login)
```bash
curl -X POST "https://localhost:7139/api/notes" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -d '{
    "title": "Test Note",
    "content": "This is a test note content.",
    "color": "#FFF9C4"
  }'
```

## Production Deployment Options

### Option 1: IIS Deployment (Windows)

1. **Install IIS and ASP.NET Core Hosting Bundle**
2. **Publish the application**:
   ```bash
   dotnet publish -c Release -o C:\inetpub\wwwroot\fundoo-notes
   ```
3. **Create IIS Application**:
   - Site Name: Fundoo Notes API
   - Physical Path: `C:\inetpub\wwwroot\fundoo-notes`
   - Binding: HTTPS on port 443
4. **Configure Application Pool**:
   - .NET CLR Version: No Managed Code
   - Process Model Identity: ApplicationPoolIdentity

### Option 2: Docker Deployment

1. **Create Dockerfile** (if not exists):
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/aspnet:8.0
   WORKDIR /app
   COPY publish/ .
   EXPOSE 80
   EXPOSE 443
   ENTRYPOINT ["dotnet", "fundoo-notes.dll"]
   ```

2. **Build and run**:
   ```bash
   docker build -t fundoo-notes .
   docker run -d -p 8080:80 -p 8443:443 fundoo-notes
   ```

### Option 3: Linux Deployment

1. **Install .NET 8.0 Runtime**
2. **Copy published files** to `/var/www/fundoo-notes/`
3. **Create systemd service**:
   ```ini
   [Unit]
   Description=Fundoo Notes API
   
   [Service]
   WorkingDirectory=/var/www/fundoo-notes
   ExecStart=/usr/bin/dotnet fundoo-notes.dll
   Restart=always
   RestartSec=10
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   
   [Install]
   WantedBy=multi-user.target
   ```
4. **Configure reverse proxy** (Nginx/Apache)

## Security Considerations

1. **HTTPS**: Always use HTTPS in production
2. **JWT Secret**: Use a strong, unique secret key (minimum 32 characters)
3. **Database**: Use encrypted connections and strong passwords
4. **CORS**: Configure CORS properly for your frontend domains
5. **Rate Limiting**: Implement rate limiting for API endpoints
6. **Logging**: Configure comprehensive logging for monitoring

## Monitoring and Maintenance

1. **Health Checks**: Monitor `/api/health` endpoint
2. **Logging**: Check application logs regularly
3. **Database**: Monitor database performance and backup regularly
4. **Updates**: Keep .NET runtime and dependencies updated
5. **SSL Certificates**: Monitor and renew SSL certificates

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Verify SQL Server is running and accessible
   - Check connection string and credentials
   - Ensure firewall allows connections on port 1433

2. **Application Won't Start**
   - Check .NET 8.0 runtime is installed
   - Verify all configuration files are present
   - Check application logs for detailed error messages

3. **JWT Token Issues**
   - Ensure JWT secret key is properly configured
   - Check token expiration settings
   - Verify issuer and audience settings

4. **Email Not Working**
   - Verify SMTP settings
   - Check email credentials and app passwords
   - Ensure less secure app access is enabled (for Gmail)

### Log Locations

- **Development**: Console output
- **Production**: Event logs (Windows) or systemd logs (Linux)
- **IIS**: IIS logs and Windows Event Viewer

## Support

For issues and support:
1. Check the application logs
2. Verify database connectivity
3. Test API endpoints using Swagger UI
4. Review configuration settings

The application includes comprehensive error handling and logging to help diagnose issues quickly.
