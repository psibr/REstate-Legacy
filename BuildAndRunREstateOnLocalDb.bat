@echo off

:BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
IF "%PROCESSOR_ARCHITECTURE%" EQU "amd64" (
>nul 2>&1 "%SYSTEMROOT%\SysWOW64\cacls.exe" "%SYSTEMROOT%\SysWOW64\config\system"
) ELSE (
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
)

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params = %*:"=""
    echo UAC.ShellExecute "cmd.exe", "/c ""%~s0"" %params%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------  

start cmd.exe /k "cd %~dp0 & powershell .\build.ps1 Release & powershell .\create-localdb-database.ps1 & powershell .\deploy-database.ps1 Release & start src\REstate.Services.Auth\bin\Release\REstate.Services.Auth.exe & start src\REstate.Services.Configuration\bin\Release\REstate.Services.Configuration.exe & start src\REstate.Services.Instances\bin\Release\REstate.Services.Instances.exe & start src\REstate.Services.Chrono\bin\Release\REstate.Services.Chrono.exe & start src\REstate.Services.ChronoConsumer\bin\Release\REstate.Services.ChronoConsumer.exe"