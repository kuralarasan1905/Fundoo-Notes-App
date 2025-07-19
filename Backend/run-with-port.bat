@echo off
echo Fundoo Notes API Port Manager
echo ==============================

if "%1"=="" (
    echo Usage: run-with-port.bat [port]
    echo Example: run-with-port.bat 5227
    echo.
    echo Available ports:
    echo   5227 - Default VS Code port
    echo   5228 - Visual Studio port
    echo   5229 - Alternative port
    goto :end
)

set PORT=%1
echo Starting Fundoo Notes API on port %PORT%...
echo.

REM Kill any existing processes on the port
for /f "tokens=5" %%a in ('netstat -ano ^| findstr :%PORT%') do (
    echo Killing process %%a using port %PORT%
    taskkill /F /PID %%a >nul 2>&1
)

REM Set environment variable and run
set ASPNETCORE_PORT=%PORT%
dotnet run

:end
pause
