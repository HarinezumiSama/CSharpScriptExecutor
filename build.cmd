@echo off

if "%~1" EQU "" goto HELP
if "%~2" EQU "" goto HELP
if "%~3" EQU "" goto HELP

set BuildConfig=%~4
if "%BuildConfig%" EQU "" set BuildConfig=Debug

set DevEnvVer=%~5
if "%DevEnvVer%" EQU "" set DevEnvVer=10.0

@rem TODO: if ... PROCESSOR_ARCHITEW6432=AMD64 -> `Program Files (x86)` ELSE `Program Files`

set DEVENV="C:\Program Files (x86)\Microsoft Visual Studio %DevEnvVer%\Common7\IDE\devenv.exe"

title [Rebuild] %~2: %BuildConfig%
echo.
echo ********************************************************************************
echo * START: [Rebuild] %~2: %BuildConfig%

if exist "%~f3" (
  del /F /Q "%~f3"
)
echo **************************************** >>"%~f3"
echo [Rebuild] %~2: %BuildConfig% >>"%~f3"
echo Using VS %DevEnvVer%: %DEVENV% >>"%~f3"
echo **************************************** >>"%~f3"
echo START: >>"%~f3"
date /T >>"%~f3"
time /T >>"%~f3"
echo **************************************** >>"%~f3"
echo. >>"%~f3"

echo * Cleaning "%~nx1"... >>"%~f3"
@echo on
%DEVENV% "%~f1" /Clean "%BuildConfig%"  /Out "%~f3"
@if errorlevel 1 goto BUILD_END
@echo. >>"%~f3"
@echo Rebuilding "%~nx1"... >>"%~f3"
%DEVENV% "%~f1" /Rebuild "%BuildConfig%"  /Out "%~f3"
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
goto END

:HELP
@echo off
echo.
echo Usage:
echo   %~nx0 {SolutionFile} {Title} {Log} [Configuration] [VS_Version]
echo * [Configuration] - defaults to 'Debug'
echo * [VS_Version] - defaults to '10.0'
echo.
goto END

:END
@echo off
set DevEnvVer=
set DEVENV=
set BuildConfig=
exit
