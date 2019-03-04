call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"

pushd .
MSbuild /target:installer /property:teamcity_build_checkoutDir=..\ /property:Configuration="Release" /property:teamcity_dotnet_nunitlauncher_msbuild_task="notthere" /property:BUILD_NUMBER="*.*.0.001" /property:Minor="18"
popd
PAUSE

#/verbosity:detailed