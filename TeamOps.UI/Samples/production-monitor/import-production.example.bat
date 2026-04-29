@echo off
setlocal EnableExtensions EnableDelayedExpansion

if not defined TEAMOPS_PRODUCTION_EVENTS_DIR (
    echo TEAMOPS_PRODUCTION_EVENTS_DIR nao definido.
    exit /b 1
)

if not defined TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR (
    echo TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR nao definido.
    exit /b 1
)

if not exist "%TEAMOPS_PRODUCTION_EVENTS_DIR%" mkdir "%TEAMOPS_PRODUCTION_EVENTS_DIR%"
if not exist "%TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR%" mkdir "%TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR%"

set COUNT=0
for %%F in ("%TEAMOPS_PRODUCTION_SOURCE_EVENTS_DIR%\\*.txt") do (
    copy /Y "%%~fF" "%TEAMOPS_PRODUCTION_EVENTS_DIR%\\" >nul
    set /a COUNT+=1
)

if defined TEAMOPS_PRODUCTION_COMPLETION_FILE (
    > "%TEAMOPS_PRODUCTION_COMPLETION_FILE%" echo !COUNT! arquivo^(s^) sincronizado^(s^) para importacao.
)

exit /b 0
