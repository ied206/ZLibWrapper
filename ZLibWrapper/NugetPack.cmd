@echo off
SETLOCAL ENABLEEXTENSIONS

REM Adjust this statement according to your envrionment
REM SET MSBUILD_PATH="%windir%\Microsoft.NET\Framework\v4.0.30319\"
SET MSBUILD_PATH="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin"

REM %~dp0 => absolute path of directory where batch file exists
cd %~dp0
SET SOLUTION="%cd%\..\ZLibWrapper.sln"

%MSBUILD_PATH%\MSBuild.exe %SOLUTION% /p:Configuration=Release_NET40
%MSBUILD_PATH%\MSBuild.exe %SOLUTION% /p:Configuration=Release_NET45

nuget pack ZLibWrapper.nuspec

ENDLOCAL