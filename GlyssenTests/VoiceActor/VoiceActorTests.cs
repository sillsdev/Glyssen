using NUnit.Framework;

namespace GlyssenTests.VoiceActor
{
	class VoiceActorTests
	{
		[Test]
		public void HasMeaningfulData_HasName_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Name = "A" };
			Assert.IsTrue(actor.HasMeaningfulData());
		}

		[Test]
		public void HasMeaningfulData_HasGender_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Gender = "F" };
			Assert.IsTrue(actor.HasMeaningfulData());
		}

		[Test]
		public void HasMeaningfulData_HasAge_ReturnsTrue()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Age = "O" };
			Assert.IsTrue(actor.HasMeaningfulData());
		}

		[Test]
		public void HasMeaningfulData_HasOnlyId_ReturnsFalse()
		{
			var actor = new Glyssen.VoiceActor.VoiceActor { Id = 1 };
			Assert.IsFalse(actor.HasMeaningfulData());
		}
	}
}
