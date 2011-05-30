@echo off
call "%~dp0build.cmd" "%~dp0CSharpScriptExecutor.sln" "C# Script Executor" "%~f0.log" "%~1"
exit
