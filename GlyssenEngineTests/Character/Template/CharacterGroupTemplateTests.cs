/// <summary>
///  This code is not compiled.
///  Experimental code. Not used at this time.
/// </summary>

using GlyssenEngine.Character;
using GlyssenEngine.Character.Template;
using NUnit.Framework;

namespace GlyssenEngineTests.Character.Template
{
	[TestFixture]
	public class CharacterGroupTemplateTests
	{
		[Test]
		public void AddCharacterToGroup()
		{
			var template = new CharacterGroupTemplate(TestProject.CreateBasicTestProject());

			// Adds new group
			template.AddCharacterToGroup("Test Character", 1);

			CharacterGroup group;
			Assert.IsTrue(template.CharacterGroups.TryGetValue(1, out group));
			Assert.IsTrue(group.CharacterIds.Contains("Test Character"));

			// Adds to existing group
			template.AddCharacterToGroup("Test Character 2", 1);

			Assert.IsTrue(group.CharacterIds.Contains("Test Character 2"));
		}
	}
}
