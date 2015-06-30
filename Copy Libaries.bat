REM For this to work, you need palaso as a sibling of this project
set palaso=libpalaso
if NOT EXIST ..\%palaso% set palaso=palaso

REM The Palaso DLL should be the strong-named one that works with ParatextShared
REM copy /Y ..\%palaso%\output\debug\palaso.dll lib\dotnet\
REM copy /Y ..\%palaso%\output\debug\palaso.pdb lib\dotnet\

copy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.DblBundle.dll  lib\dotnet
copy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.DblBundle.pdb  lib\dotnet

copy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.dll  lib\dotnet
copy /Y ..\%palaso%\output\debug\SIL.Windows.Forms.pdb  lib\dotnet

copy /Y ..\%palaso%\output\debug\SIL.Core.dll  lib\dotnet
copy /Y ..\%palaso%\output\debug\SIL.Core.pdb  lib\dotnet

copy /Y ..\%palaso%\output\debug\SIL.DblBundle.dll  lib\dotnet
copy /Y ..\%palaso%\output\debug\SIL.DblBundle.pdb  lib\dotnet

copy /Y ..\%palaso%\output\debug\L10NSharp.dll  lib\dotnet
copy /Y ..\%palaso%\output\debug\L10NSharp.pdb  lib\dotnet

pause