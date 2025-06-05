using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GlyssenCharacters;
using GlyssenEngine.Character;
using NUnit.Framework;
using static GlyssenCharacters.CharacterAge;

namespace GlyssenEngineTests
{
	[TestFixture]
	class ProjectCharacterDetailDataTests
	{
		[Test]
		public void Load_Normal_AllLinesLoaded()
		{
			var data = new HashSet<CharacterDetail>
			{
				new CharacterDetail { CharacterId = "Fred", Age = Adult,
					Gender = CharacterGender.Male, MaxSpeakers = 5 },
				new CharacterDetail { CharacterId = "Marta", Age = Child,
					Gender = CharacterGender.Female, MaxSpeakers = 1 },
				new CharacterDetail { CharacterId = "the whole gang of bums",
					Age = YoungAdult, Gender = CharacterGender.Either, MaxSpeakers = -1 }
			};
			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb))
			{
				ProjectCharacterDetailData.Write(data, writer);
				var detailData = ProjectCharacterDetailData.Load(new StringReader(sb.ToString()));
				Assert.That(detailData.Count, Is.EqualTo(3));
				foreach (var characterDetail in detailData)
				{
					Assert.That(data.Count(cd =>
						cd.CharacterId == characterDetail.CharacterId &&
						cd.Age == characterDetail.Age &&
						cd.Gender == characterDetail.Gender &&
						cd.MaxSpeakers == characterDetail.MaxSpeakers),
						Is.EqualTo(1));
				}
			}
		}

		/// <summary>
		/// PG-903
		/// </summary>
		[Test]
		public void Load_DuplicateCharacterIds_DuplicatesEliminated()
		{
			var data = new HashSet<CharacterDetail>
			{
				new CharacterDetail { CharacterId = "Fred", Age = Adult,
					Gender = CharacterGender.Male, MaxSpeakers = 5 },
				new CharacterDetail { CharacterId = "Fred", Age = Child,
					Gender = CharacterGender.Either, MaxSpeakers = 1 },
				new CharacterDetail { CharacterId = "the whole gang of bums",
					Age = YoungAdult, Gender = CharacterGender.Either, MaxSpeakers = -1 }
			};
			var sb = new StringBuilder();
			using (var writer = new StringWriter(sb))
			{
				ProjectCharacterDetailData.Write(data, writer);
				var detailData = ProjectCharacterDetailData.Load(new StringReader(sb.ToString()));
				Assert.That(detailData.Count, Is.EqualTo(2));
				Assert.That(detailData.Any(cd =>
					cd.CharacterId == "Fred" &&
					cd.Age == Child &&
					cd.Gender == CharacterGender.Either &&
					cd.MaxSpeakers == 1));
				Assert.That(detailData.Any(cd => cd.CharacterId == "the whole gang of bums"), Is.True);
			}
		}
	}
}
