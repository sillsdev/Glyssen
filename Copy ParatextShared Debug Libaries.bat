@REM For this to work, you need Paratext as a sibling of this project

xcopy /Y ..\Paratext\ParatextData\bin\x86\Debug\ParatextData.dll lib\dotnet\
xcopy /Y ..\Paratext\ParatextData\bin\x86\Debug\ParatextData.pdb lib\dotnet\

xcopy /Y ..\Paratext\ParatextData\bin\x86\Debug\PtxUtils.dll lib\dotnet\
xcopy /Y ..\Paratext\ParatextData\bin\x86\Debug\PtxUtils.pdb lib\dotnet\

@pause