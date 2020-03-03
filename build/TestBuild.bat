@echo This will only work if the correct version of MSBuild is on the PATH.
@echo The easiest way to ensure this is to run from the Developer Command Prompt for VS.
@echo The following line might work from a regular command prompt if this batch file exists,
@echo but more than likely it will set the path such that the wrong version of MSBuild is first:
@echo //call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"

pushd .
MSbuild /target:Test /property:Configuration="Release" /property:Platform=x64 /property:ExtraExcludeCategories="SkipOnTeamCity"
popd
PAUSE

#/verbosity:detailed