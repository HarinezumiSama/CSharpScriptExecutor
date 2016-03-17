@echo off

setlocal

if "%~1" EQU "" goto HELP
if "%~2" EQU "" goto HELP
if "%~3" EQU "" goto HELP

set BuildConfig=%~4
if "%BuildConfig%" EQU "" set BuildConfig=Debug

set AppDir=%ProgramFiles%
if /i "%PROCESSOR_ARCHITECTURE%" equ "AMD64" set AppDir=%ProgramFiles(x86)%

set MSBUILD=%AppDir%\MSBuild\14.0\Bin\MSBuild.exe

title [Rebuild] %~2: %BuildConfig%
echo.
echo ********************************************************************************
echo * START: [Rebuild] %~2: %BuildConfig%

if exist "%~f3" (
  del /F /Q "%~f3"
)
echo **************************************** >>"%~f3"
echo [Rebuild] %~2: %BuildConfig% >>"%~f3"
echo Using MSBuild: "%MSBUILD%" >>"%~f3"
echo **************************************** >>"%~f3"
echo START: >>"%~f3"
date /T >>"%~f3"
time /T >>"%~f3"
echo **************************************** >>"%~f3"
echo. >>"%~f3"

@echo Rebuilding "%~nx1"... >>"%~f3"
"%MSBUILD%" /t:Rebuild "%~f1" /p:Configuration="%BuildConfig%" /p:Platform="Any CPU" /Verbosity:minimal /fl /flp:Verbosity=diagnostic;Summary;LogFile="%~f3" || goto ERROR
@echo off

echo. >>"%~f3"
echo. >>"%~f3"
echo **************************************** >>"%~f3"
echo END: >>"%~f3"
date /T >>"%~f3"
time /T >>"%~f3"
echo **************************************** >>"%~f3"

:BUILD_END
@echo off
echo.
echo * END: [Rebuild] %~2: %BuildConfig%
echo ********************************************************************************
echo.
goto :EOF

:HELP
@echo off
echo.
echo Usage:
echo   %~nx0 {SolutionFile} {Title} {Log} [Configuration] [VS_Version]
echo * [Configuration] - defaults to 'Debug'
echo.
exit /b 127
goto :EOF

:ERROR
echo.
echo *** ERROR has occurred ***
pause
exit /b 1
goto :EOF
