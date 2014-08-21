.nuget\nuget.exe restore
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild RK.CalendarSync.sln /t:rebuild /p:OutDir="%~dp0\bin";BuildConfiguration="Debug"