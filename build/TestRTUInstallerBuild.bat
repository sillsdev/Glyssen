call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"

pushd .
MSbuild /target:ReferenceTextUtilityInstaller /property:teamcity_build_checkoutDir=..\ /property:Configuration="Release" /property:teamcity_dotnet_nunitlauncher_msbuild_task="notthere" /property:BUILD_NUMBER="*.*.3.001" /property:Major="1" /property:Minor="0"
popd
PAUSE

#/verbosity:detailed