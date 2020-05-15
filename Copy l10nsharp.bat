REM For this to work, you need l10nsharp as a sibling of this project

xcopy /Y ..\l10nsharp\output\debug\L10NSharp.dll lib\dotnet
xcopy /Y ..\l10nsharp\output\debug\L10NSharp.pdb lib\dotnet

pause