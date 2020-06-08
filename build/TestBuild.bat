@echo This will only work if the correct version of MSBuild is on the PATH.
@echo The easiest way to ensure this is to run from the Developer Command Prompt for VS.
@echo The following line might work from a regular command prompt if this batch file exists,
@echo but more than likely it will set the path such that the wrong version of MSBuild is first:
@echo //call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"

pushd .
REM Rather than hard-coding SemVer and MajorMinorPatch, a better long-term solution would be
REM to install GitVersionTask in Glyssen.proj and import GitVersionTask.props and GitVersionTask.targets. Take a look how it's done in FLExBridge.proj (search for gitversiontask) (https://github.com/ermshiperete/flexbridge/blob/feature/nuget/build/FLExBridge.proj)
MSbuild /target:Test /property:Configuration="Release" /property:Platform=x64 /property:ExtraExcludeCategories="SkipOnTeamCity" /property:GitVersion_SemVer="1.3.7-local" /property:GitVersion_MajorMinorPatch="1.3.7"
popd
PAUSE

#/verbosity:detailed