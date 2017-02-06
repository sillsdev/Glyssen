using System.Collections.Generic;
using System.Linq;
using Glyssen.Character;
using NUnit.Framework;
using SIL.IO;

namespace GlyssenTests
{
	[TestFixture]
	class ProjectCharacterDetailDataTests
	{
		[Test]
		public void Load_Normal_AllLinesLoaded()
		{
			var data = new HashSet<CharacterDetail>();
			data.Add(new CharacterDetail { CharacterId = "Fred", Age = CharacterAge.Adult, Gender = CharacterGender.Male, MaxSpeakers = 5 });
			data.Add(new CharacterDetail { CharacterId = "Marta", Age = CharacterAge.Child, Gender = CharacterGender.Female, MaxSpeakers = 1 });
			data.Add(new CharacterDetail { CharacterId = "the whole gang of bums", Age = CharacterAge.YoungAdult, Gender = CharacterGender.Either, MaxSpeakers = -1 });
			using (TempFile tempFile = new TempFile())
			{
				ProjectCharacterDetailData.WriteToFile(data, tempFile.Path);
				var detailData = ProjectCharacterDetailData.Load(tempFile.Path);
				Assert.AreEqual(3, detailData.Count);
				foreach (var characterDetail in detailData)
				{
					Assert.AreEqual(1, data.Count(cd =>
						cd.CharacterId == characterDetail.CharacterId &&
						cd.Age == characterDetail.Age &&
						cd.Gender == characterDetail.Gender &&
						cd.MaxSpeakers == characterDetail.MaxSpeakers));
				}
			}
		}

		/// <summary>
		/// PG-903
		/// </summary>
		[Test]
		public void Load_DuplicateCharacterIds_DuplicatesEliminated()
		{
			var data = new HashSet<CharacterDetail>();
			data.Add(new CharacterDetail { CharacterId = "Fred", Age = CharacterAge.Adult, Gender = CharacterGender.Male, MaxSpeakers = 5 });
			data.Add(new CharacterDetail { CharacterId = "Fred", Age = CharacterAge.Child, Gender = CharacterGender.Either, MaxSpeakers = 1 });
			data.Add(new CharacterDetail { CharacterId = "the whole gang of bums", Age = CharacterAge.YoungAdult, Gender = CharacterGender.Either, MaxSpeakers = -1 });
			using (TempFile tempFile = new TempFile())
			{
				ProjectCharacterDetailData.WriteToFile(data, tempFile.Path);
				var detailData = ProjectCharacterDetailData.Load(tempFile.Path);
				Assert.AreEqual(2, detailData.Count);
				Assert.IsTrue(detailData.Any(cd =>
					cd.CharacterId == "Fred" &&
					cd.Age == CharacterAge.Child &&
					cd.Gender == CharacterGender.Either &&
					cd.MaxSpeakers == 1));
				Assert.IsTrue(detailData.Any(cd => cd.CharacterId == "the whole gang of bums"));
			}
		}
	}
}
