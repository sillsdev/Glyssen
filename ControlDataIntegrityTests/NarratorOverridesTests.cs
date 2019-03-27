using System;
using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;
using NUnit.Framework;
using SIL.Scripture;

namespace ControlDataIntegrityTests
{
	class NarratorOverridesTests
	{
		[Test]
		public void DataIntegrity_LoadsCorrectly()
		{
			Assert.IsNotNull(NarratorOverrides.GetNarratorOverridesForBook("PSA"));
		}

		[Test]
		public void DataIntegrity_AllCharactersExistInCharacterDetail()
		{
			foreach (var overrideDetail in NarratorOverrides.NarratorOverridesByBookId.Values.SelectMany(o => o))
			{
				Assert.True(CharacterDetailData.Singleton.GetDictionary().Keys.Contains(overrideDetail.Character),
					$"Character {overrideDetail.Character} in NarratorOverides.xml was not found in CharacterDetail");
			}
		}

		[Test]
		public void DataIntegrity_StartsPrecedeEnds()
		{
			foreach (var bookId in NarratorOverrides.NarratorOverridesByBookId.Keys)
			{
				foreach (var overrideDetail in NarratorOverrides.GetNarratorOverridesForBook(bookId))
				{
					Assert.True(overrideDetail.StartChapter <= overrideDetail.EndChapter,
						$"In NarratorOverides.xml, book {bookId}: end chapter {overrideDetail.EndChapter} precedes start chapter {overrideDetail.StartChapter}.");
					if (overrideDetail.StartChapter == overrideDetail.EndChapter)
					{
						Assert.True(overrideDetail.StartVerse <= overrideDetail.EndVerse,
							$"In NarratorOverides.xml, book {bookId}, chapter {overrideDetail.StartChapter}: end verse {overrideDetail.EndVerse} precedes start verse {overrideDetail.StartVerse}.");
					}
				}
			}
		}

		[Test]
		public void DataIntegrity_StartChapterSet()
		{
			foreach (var bookId in NarratorOverrides.NarratorOverridesByBookId.Keys)
			{
				foreach (var overrideDetail in NarratorOverrides.GetNarratorOverridesForBook(bookId))
				{
					Assert.IsTrue(overrideDetail.StartChapter >= 1, $"In NarratorOverides.xml, book {bookId}, StartChapter not set for override {overrideDetail}.");
				}
			}
		}

		/// <summary>
		/// Currently, 2 is the biggest we need. Can't imagine ever needing anything bigger than 3. (Following test woudn't work if the start block was > 9.)
		/// </summary>
		[Test]
		public void DataIntegrity_StartBlockLessThanFour()
		{
			foreach (var bookId in NarratorOverrides.NarratorOverridesByBookId.Keys)
			{
				foreach (var overrideDetail in NarratorOverrides.GetNarratorOverridesForBook(bookId))
				{
					Assert.IsTrue(overrideDetail.StartBlock < 4, $"In NarratorOverides.xml, book {bookId}, StartBlock set to invalid value for override {overrideDetail}.");
				}
			}
		}

		[Test]
		public void DataIntegrity_NoOverlappingRanges()
		{
			var overridesByBook = new Dictionary<string, SortedDictionary<int, NarratorOverrides.NarratorOverrideDetail>>();
			foreach (var bookId in NarratorOverrides.NarratorOverridesByBookId.Keys)
			{
				var bookNum = BCVRef.BookToNumber(bookId);
				var dictionary = new SortedDictionary<int, NarratorOverrides.NarratorOverrideDetail>();
				overridesByBook.Add(bookId, dictionary);
				foreach (var overrideDetail in NarratorOverrides.GetNarratorOverridesForBook(bookId))
				{
					var newKey = new BCVRef(bookNum, overrideDetail.StartChapter, overrideDetail.StartVerse).BBCCCVVV * 10 + overrideDetail.StartBlock;
					if (dictionary.ContainsKey(newKey))
					Assert.Fail($"In NarratorOverides.xml, book {bookId}: two overrides start at the same place:\n{dictionary[newKey]}\nAND\n{overrideDetail}");

					dictionary.Add(newKey, overrideDetail);
				}
			}

			foreach (var kvp in overridesByBook)
			{
				var dictionary = kvp.Value;
				NarratorOverrides.NarratorOverrideDetail prev = null;
				foreach (var overrideDetail in dictionary.Values)
				{
					if (prev != null)
					{
						Assert.True(overrideDetail.EndChapter > prev.EndChapter || (overrideDetail.EndChapter == prev.EndChapter &&
							overrideDetail.EndVerse > prev.EndVerse),
							$"In NarratorOverides.xml, book {kvp.Key}: override {overrideDetail} overlaps {prev}.");
					}
					prev = overrideDetail;
				}
			}
		}
	}
}