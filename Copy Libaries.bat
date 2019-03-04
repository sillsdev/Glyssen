REM For this to work, you need palaso as a sibling of this project
set palaso=libpalaso
if NOT EXIST ..\%palaso% set palaso=palaso

xcopy /Y ..\%palaso%\output\debug\SIL.Core.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Core.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.DblBundle.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.DblBundle.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.DblBundle.Tests.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.DblBundle.Tests.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Scripture.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Scripture.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.TestUtilities.dll lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.DblBundle.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.DblBundle.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.GeckoBrowserAdapter.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.GeckoBrowserAdapter.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.Keyboarding.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.Keyboarding.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.Scripture.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.Scripture.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.WritingSystems.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.WritingSystems.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\SIL.WritingSystems.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\SIL.WritingSystems.pdb lib\dotnet

xcopy /Y ..\%palaso%\output\debug\L10NSharp.dll lib\dotnet
xcopy /Y ..\%palaso%\output\debug\L10NSharp.pdb lib\dotnet

pause