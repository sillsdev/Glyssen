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

### Changed

- GlyssenCharacters.CharacterVerse Added parameter to the constructor to set the QuotePosition.
- GlyssenCharacters.CharacterVerseData Changed several protected members to protected internal to support use by new CharacterVerseUpdater class in DevTools.
- GlyssenCharacters.ControlCharacterVerseData Changed protected ProcessLine method to protected internal to support use by new CharacterVerseUpdater class in DevTools.
- GlyssenEngine.Quote.QuoteParser constructor now takes a QuoteSystem parameter (instead of setting via static SetQuoteSystem method)
- GlyssenEngine.Quote.QuoteParser.Parse Fixed logic mistake to properly handle successive paragraphs with dialogue dashes
- GlyssenEngine.Paratext.ParatextScrTextWrapper Made QuotationMarks property virtual and UnderlyingScrText property protected
- GlyssenEngine.Quote.QuoteParser Detects places where \pi marker is used for discourse blocks that appear to be quotes (in the absence of quotation marks)

### Added

- GlyssenCharacters.QuotePosition enum.
- Optional Quote Position column to CharacterVerse.txt data
- GlyssenEngine.Script.BookScript.GetQuotePosition method
- GlyssenEngine.Paratext.ParatextScrTextWrapper Added ReportingClauseStartDelimiter and ReportingClauseEndDelimiter properties
- GlyssenCharacters.CharacterVerse.CharacterSpeakingMode Added parameter to the constructor to set the expected quote position and added ExpectedPosition property to expose this.

### Removed
- GlyssenEngine.Quote.QuoteParser.SetQuoteSystem static method
- GlyssenCharacters.CharacterVerseData.kiMinRequiredFields (now private).

## [5.0.0] - 2022-11-11

### Changed

- Updated to the latest libraries. This includes a fix for a potential security vulnerability.
- Separated some existing functionality into a separate DLL for use in other software.

### Added

- ProjectExporter.SeparateChapterSheets and ProjectExporter.SheetNameFormat to provide support for an export option to break out chapters into separate sheets.

## [4.0.0] - 2022-08-10

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
