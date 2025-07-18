# Scroll down for Release Notes

# Getting Started
Glyssen™ assists with the process of preparing an audio recording script for
dramatizing Scripture. It is intended for producing publishable-quality recordings.
To start a new project in  Glyssen, you will need either a Digital Bible Library "text
release bundle" with at least one book of Scripture that has been approved for
publishing or a "live" Paratext project on your computer. Glyssen does not read plain
Standard Format files. Once you have created a project and made a few basic decisions
about project settings and what books to record, there are four main phases to the
recording project:

1. Identify speaking parts and align vernacular script blocks with the reference text. Ideally, this task should be done by someone who knows both the vernacular language and the reference text language(s). If the vernacular text uses consistent quotation mark punctuation, Glyssen can usually do most of this work for you. In a relatively small number of places, it will require you to confirm its guesses or make manual decisions. This process can take anywhere from a few hours to a few days.
2. Make cast selections. This involves deciding how many voice actors you will use, identifying those people, and having Glyssen help divide up the work among them.
3. Field recording. Glyssen is not a recording tool. If you are not working with a ministry partner to do this phase (see below), one option is to use HearThis version 2.0 or greater. You can also use recording software such as Audacity or Adobe Audition to record all the clips for each voice actor. Be sure to set up an environment that minimizes ambient noise and use high-quality recording equipment to get the best possible results.
4. Post-production. This is key to getting a great-sounding final product. Glyssen doesn't really help with this part of the process, other than to include annotations in the scripts it produces that can help the post-production team.

It is strongly recommended that field teams work with an experienced ministry partner
to ensure a successful recording project. In most cases, [Faith Comes By Hearing](https://www.faithcomesbyhearing.com/) is able
to assist teams with all phases of a project. (They can even do step 1 for you, but you
might find it beneficial to do it yourself or at least get involved in that.) If you are
working with [IMS](https://www.internationalmediaservices.org/), FCBH, or another partner, you will want to discuss the overall project
plan with them before starting to use Glyssen. That will help you avoid unnecessary work.

If you have a question, suggestion, or encouragement for us or our backers, please visit our [web forum](https://community.scripture.software.sil.org/c/glyssen). If you have a bug report, you can get that into our "to do" list directly by emailing <glyssen-support_lsdev@sil.org>.

# What to Back Up
Glyssen stores all its project files in the Program Data folder, under the
SIL-FCBH\Glyssen directory. On a default installation of Windows, that would be
here: <a href="file:///C:/ProgramData/FCBH-SIL/Glyssen">
C:\ProgramData\FCBH-SIL\Glyssen</a>. To support multiple
recording projects, versions, and languages, there will be three levels of subfolders
under that folder. Back up the whole thing. If you are using Glyssen to generate empty
clip files for the recording phase, the software lets you choose where to create those.
Back those up also. If your project is based on a text release bundle that is in the
[Digital Bible Library](https://thedigitalbiblelibrary.org/) and you have access to that, you don't need to back it up, but
for the sake of convenience, you may want to. You might also want to back up any glyssenshare
files (usually found in <a href="file:///C:/ProgramData/FCBH-SIL/Glyssen/share">
C:\ProgramData\FCBH-SIL\Glyssen\share</a>) or exported Excel or glyssenscript files.
(If you have saved these in My Documents, they will likely already be part of your normal
back-up plan.)

-----------------------------

# Release Notes

## _DATE_ Glyssen _VERSION_
- Fixed a potential security vulnerability (low risk) related to opening files that claim to be glyssenshare files but were actually created to contain malicious content.
- Upgraded to the latest ParatextData DLL and some other thrird-party dependencies. This is not known to fix any specific bugs in Glyssen, but it might.
- Restored lost Analytics reporting functionality.

## January 24 2023 Glyssen 6.0.0
- Improvements to the data files that help Glyssen determine who is speaking where.
- Detects places where \pi marker is used for discourse blocks that appear to be quotes (in the absence of quotation marks).
- Fixed logic mistake to properly handle successive paragraphs with dialogue dashes.

## November 11 2022 Glyssen 5.0.0
Added an option when exporting to Excel to break out chapters into separate sheets.

## August 10 2022 Glyssen 4.0.0
Enhanced USX parser to correctly process ["milestone"](https://ubsicap.github.io/usfm/milestones/index.html) markers.

## April 11 2022 Glyssen 3.1.4
When a reference text row is selected, Paratext is notified so it can sync to that location in the text.

## January 20 2022 Glyssen 3.1.3
Added logic to ensure that block numbers in actor- and book-specific Excel exports match the numbers of those same blocks in the master file for the project.

## January 8 2020 Glyssen 2.0.0
Enhanced parser to identify "poetry" paragraphs as Scripture quotes when no other character speaks in the verses.

## September 23 2020 Glyssen 1.6.0
Changed English and Russian reference texts to use chevrons instead of curly quotes for improved visibility when scripting.

## August 17 2020 Glyssen 1.5.0
- Improved reference text logic when matching to reporting clauses ("he said" blocks) to prevent duplication.
- English and Russian reference texts updated to match v. 101 of the FCBH New Testament Director's Guide.

## June 16 2020 Glyssen 1.3.13
Switched localization to use XLIFF format instead of TMX. More (albeit minimal) UI languages available. Translations can be done via crowdin.com.

## May 27 2020 Glyssen 1.3.11
When a block is identified as "he said", Glyssen looks for other identical blocks and aligns them automatically.

## December 2019 Glyssen 1.3.0
As of this version, Glyssen is now a 64-bit application and will not run on 32-bit versions of Windows.

## October 14 2019 Glyssen 1.2.0
Complete Old Testament reference text available in English.

## April 2019 Glyssen 1.1.10
Improved support for right-to-left text in Excel output.

## November 2018 Glyssen 1.1
The major new feature in this version of Glyssen is support for direct access to data from live Paratext 8 projects.

## February 2018
Added reference texts for much of the Old Testament

## January 2018
Glyssen can now export a script for recording using [HearThis](https://software.sil.org/hearthis/).

## March 2017
Added context menus useful when identifying speaking parts and aligning vernacular blocks to the reference text. (This also gives a way to add project-specific characters or deliveries without having to leave "rainbow mode".)
