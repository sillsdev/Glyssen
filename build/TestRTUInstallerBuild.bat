call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\Tools\VsDevCmd.bat"

pushd .
MSbuild /target:ReferenceTextUtilityInstaller /property:teamcity_build_checkoutDir=..\ /property:Configuration="Release" /property:GitVersion_SemVer="1.1.0" /property:GitVersion_MajorMinorPatch="1.1.0"
popd
PAUSE

#/verbosity:detailed