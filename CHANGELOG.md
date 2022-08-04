# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/)
and this project adheres to [Semantic Versioning](http://semver.org/).

<!-- Available types of changes:
### Added
### Changed
### Fixed
### Deprecated
### Removed
### Security
-->

## [Unreleased]

## [4.0.0] - 2022-08-05

### Changed

- Updated to latest SIL libraries, which included several interface changes:
- IBlockAccessor.GetIndicesOfFirstBlockAtReference, first parameter changed from VerseRef to IScrVerseRef
- BlockNavigatorViewModel.TryLoadBlock, parameter changed from VerseRef to IScrVerseRef
- BlockNavigatorViewModel.TrySelectRefInCurrentBlockMatchup, parameter changed from VerseRef to IScrVerseRef

### Added

- AssignCharacterViewModel.GetVerseRefForRow
- BlockNavigatorViewModel.IsReferenceOutsideCurrentScope

## [3.1.0] - 2021-07-27

### Changed

- Major update to the English Old Testament reference text

## [1.4.0] - 2020-07-08

### Added

- GlyssenEngine nuget package now contains the reference text files and usfm.sty.

## [1.3.13] - 2020-06-15

### Changed

- Switch localization to use XLIFF format instead of TMX. More (albeit minimal) UI languages available. Translations can be done via crowdin.com.

## [1.3.11] - 2020-05-27

### Added

- When a block is aligned to "He said" in the reference text, Glyssen automatically scans other blocks to align any others with the same text.

## [1.3.6] - 2020-03-04

### Changed

- Create nuget package
- Strong-name assembly
