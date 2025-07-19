# Fundoo Notes API - Port Conflict Solution

## Problem
The application was experiencing port binding conflicts when trying to run in both VS Code and Visual Studio, causing the error:
```
System.IO.IOException: Failed to bind to address http://127.0.0.1:5227: address already in use
```

## Solutions Implemented

### 1. Updated Launch Settings
Modified `Properties/launchSettings.json` to include separate profiles for different IDEs:

- **http profile**: Port 5227 (VS Code default)
- **https profile**: Port 5227 + HTTPS 7256
- **Visual Studio profile**: Port 5228 (Visual Studio specific)
- **IIS Express**: Uses IIS Express ports

### 2. Dynamic Port Configuration
Added environment variable support in `Program.cs`:
- Checks for `ASPNETCORE_PORT` environment variable
- Falls back to default port 5227 if not set
- Only applies in Development environment

### 3. Port Management Scripts

#### PowerShell Script (`run-with-port.ps1`)
```powershell
.\run-with-port.ps1 -Port 5227
```
Features:
- Automatically kills processes using the specified port
- Sets environment variables
- Provides colored output for better visibility
- Error handling

#### Batch Script (`run-with-port.bat`)
```cmd
run-with-port.bat 5227
```
Features:
- Cross-platform compatibility
- Simple command-line interface
- Automatic process cleanup

## How to Use

### For VS Code Users
1. Use the default `http` profile (port 5227)
2. Or run: `.\run-with-port.ps1 -Port 5227`

### For Visual Studio Users
1. Select "Visual Studio" profile in launch settings (port 5228)
2. Or run: `.\run-with-port.ps1 -Port 5228`

### For Manual Port Selection
```powershell
# Set environment variable and run
$env:ASPNETCORE_PORT = "5229"
dotnet run
```

## Port Assignments
- **5227**: VS Code / Default
- **5228**: Visual Studio
- **5229**: Alternative/Manual
- **31781**: IIS Express HTTP
- **44309**: IIS Express HTTPS

## Troubleshooting

### If Port is Still in Use
1. Run the PowerShell script: `.\run-with-port.ps1 -Port [desired_port]`
2. Or manually kill processes:
   ```powershell
   Get-NetTCPConnection -LocalPort 5227 | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
   ```

### Check What's Using a Port
```cmd
netstat -ano | findstr :5227
```

### Force Kill Process by PID
```cmd
taskkill /F /PID [process_id]
```

## Configuration Files Modified
1. `Properties/launchSettings.json` - Added Visual Studio profile
2. `Program.cs` - Added dynamic port configuration
3. `run-with-port.ps1` - PowerShell management script
4. `run-with-port.bat` - Batch management script

## Testing
After implementing these changes:
1. Application builds successfully
2. Can run on multiple ports without conflicts
3. Both VS Code and Visual Studio can run the application
4. Automatic port conflict resolution
