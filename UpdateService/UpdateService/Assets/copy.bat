@echo off

setlocal enabledelayedexpansion

goto entrypoint

:: Function to copy a file with retries
:copy_with_retries
set "src_file=%~1"
set "dst_file=%~2"
set "retries=5"
set "count=0"

:retry
if !count! lss !retries! (
    echo Copying "%src_file%" to "%dst_file%"...
    copy /Y "!src_file!" "!dst_file!" >nul 2>&1
    if errorlevel 1 (
        echo Failed to copy "%src_file%". Attempt !count! of !retries!...
        set /a count+=1
        timeout /t 1 >nul
        goto retry
    )
) else (
    echo Failed to copy "%src_file%" after !retries! attempts.
)

exit /b

:: Function for recursively copying files
:copy_files_recursively
set "src_dir=%~1"
set "dst_dir=%~2"

:: Create the target directory if it does not exist
if not exist "!dst_dir!" (
    mkdir "!dst_dir!"
)

:: Copy files
for %%F in ("%src_dir%\*") do (
    if exist "%%F" (
        if exist "%%F\" (
            echo %%F is directory
            :: If it is a directory, recursively copy its contents
            call :copy_files_recursively "%%F" "!dst_dir!\%%~nxF"
        ) else (
            echo %%F is file
            :: If it is a file, copy it with retries
            call :copy_with_retries "%%F" "!dst_dir!\%%~nxF"
        )
    )
)

exit /b

:entrypoint

:: Check arguments
if "%~2"=="" (
    echo Usage: %0 ^<source_folder^> ^<destination^>
    exit /b 1
)

set "src_folder=%~1"
set "dst_folder=%~2"

:: Start recursive copying
call :copy_files_recursively "%src_folder%" "%dst_folder%"

del /Q "%src_folder%"

start "" "%dst_folder%"\TestGenerator.exe
