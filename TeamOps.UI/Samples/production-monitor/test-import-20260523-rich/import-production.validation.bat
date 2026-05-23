@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
set "EVENTS_DIR=%TEAMOPS_PRODUCTION_EVENTS_DIR%"
set "EVENTS_SRC=%TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR%"
set "DAT_SRC=%TEAMOPS_PRODUCTION_SOURCE_DAT_DIR%"
set "DONE_FILE=%TEAMOPS_PRODUCTION_COMPLETION_FILE%"

if not defined EVENTS_DIR (
    echo TEAMOPS_PRODUCTION_EVENTS_DIR nao foi informado.
    exit /b 1
)

if not defined EVENTS_SRC (
    echo TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR nao foi informado.
    exit /b 1
)

if not defined DAT_SRC (
    echo TEAMOPS_PRODUCTION_SOURCE_DAT_DIR nao foi informado.
    exit /b 1
)

if not exist "%EVENTS_DIR%" mkdir "%EVENTS_DIR%"
if not exist "%EVENTS_SRC%" mkdir "%EVENTS_SRC%"
if not exist "%DAT_SRC%" mkdir "%DAT_SRC%"

set COUNT=0

for %%F in ("%SCRIPT_DIR%260522_211D_E.txt" "%SCRIPT_DIR%260522_2400_E.txt" "%SCRIPT_DIR%260523_211D_E.txt" "%SCRIPT_DIR%260523_2400_E.txt") do (
    copy /Y "%%~fF" "%EVENTS_SRC%\%%~nxF" >nul
    copy /Y "%%~fF" "%EVENTS_DIR%\%%~nxF" >nul
    if not errorlevel 1 set /a COUNT+=1
)

for %%F in ("%SCRIPT_DIR%211D_plan_20260523.dat" "%SCRIPT_DIR%2400_plan_20260523.dat") do (
    copy /Y "%%~fF" "%DAT_SRC%\%%~nxF" >nul
    if not errorlevel 1 set /a COUNT+=1
)

if defined DONE_FILE (
    > "%DONE_FILE%" echo !COUNT! arquivo^(s^) ricos sincronizado^(s^) para validacao.
)

echo !COUNT! arquivo(s) ricos sincronizado(s).
exit /b 0
