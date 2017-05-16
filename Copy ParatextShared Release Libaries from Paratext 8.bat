@@echo For this to work, you need Paratext 8.1 installed in the default location
@goto :exit
@echo If you need to update to the latest build first, you might find it here: \\swd-build\ParatextBuilds_BetaRelease

@pause

xcopy /Y "\Program Files (x86)\Paratext 8\ParatextData.dll"  lib\dotnet\
xcopy /Y "\Program Files (x86)\Paratext 8\ParatextData.pdb"  lib\dotnet\
xcopy /Y "\Program Files (x86)\Paratext 8\Ionic.Zip.dll"  lib\dotnet\
xcopy /Y "\Program Files (x86)\Paratext 8\PtxUtils.dll" lib\dotnet\
xcopy /Y "\Program Files (x86)\Paratext 8\PtxUtils.pdb" lib\dotnet\

:exit

@pause