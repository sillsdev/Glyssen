## Glyssen Localization

### Updating Crowdin with source string changes - UPLOAD TO CROWDIN NOT YET ENABLED

All the strings that are internationalized in the Glyssen project are uploaded to Crowdin in Glyssen.en.xlf

The L10nSharp tool ExtractXliff is run on the project to get any updates to the source strings resulting in a new Glyssen.en.xlf file.

Overcrowdin is used to upload this file to Crowdin. * NOT YET *

This process is run automatically by a GitHub action if the commit comment mentions any of 'localize, l10n, i18n, internationalize, spelling' * NOT YET *

It can also be run manually as follows:
```
dotnet tool install -g overcrowdin
set CROWDIN_GLYSSEN_KEY=TheApiKeyForTheGlyssenProject
msbuild l10n.proj /t:UpdateCrowdin
```

## The remainder of this is NOT YET IMPLEMENTED ##

### Using GlyssenEngine localizations in a different project

1. Add a Nuget dependency on SIL.GlyssenEngine.l10n to the project
2. Add a build step to copy the GlyssenEngine.%langcode%.xlf files to the correct folder in your project

### Building Nuget package with the latest translations

Overcrowdin is used to build and download the latest translation data.

The resulting file is unzipped and a Nuget package is built from the l10ns.nuspec file

This process is run whenever a tag is pushed to the libpalaso repository.

It can also be run manually as follows:
```
dotnet tool install -g overcrowdin
set CROWDIN_GLYSSEN_KEY=TheApiKeyForTheSilGlyssenProject
msbuild l10n.proj /t:PackageL10n
nuget push -ApiKey TheSilNugetApiKey SIL.GlyssenEngine.l10n.nupkg
```