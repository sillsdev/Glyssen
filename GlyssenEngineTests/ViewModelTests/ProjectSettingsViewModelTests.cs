using GlyssenEngine.Bundle;
using GlyssenEngine.ViewModels;
using NUnit.Framework;

namespace GlyssenEngineTests.ViewModelTests
{
	[TestFixture]
	public class ProjectSettingsViewModelTests
	{
		[Test]
		public void ExampleSubsequentChapterAnnouncement_MultiChapterBookIncluded_ExampleBasedOnChapterTwoOfFirstBookWithMultipleChapters()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			Assert.That(model.ExampleSubsequentChapterAnnouncement, Is.EqualTo("1 JON 2"));
			Assert.That(model.ChapterAnnouncementIsStrictlyNumeric, Is.False);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.That(model.ExampleSubsequentChapterAnnouncement, Is.EqualTo("2"));
			Assert.That(model.ChapterAnnouncementIsStrictlyNumeric, Is.True);
		}

		[Test]
		public void ExampleSubsequentChapterAnnouncement_OnlySingleChapterBooksIncluded_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			Assert.That(model.ExampleSubsequentChapterAnnouncement, Is.Null);
		}

		[Test]
		public void ExampleFirstChapterAnnouncement_MultiChapterBookIncluded_ExampleBasedOnChapterOneOfFirstBookWithMultipleChapters()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			Assert.That(model.ExampleFirstChapterAnnouncement, Is.EqualTo("1 JON 1"));
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.That(model.ExampleFirstChapterAnnouncement, Is.EqualTo("1"));
		}

		[Test]
		public void ExampleFirstChapterAnnouncement_SkipOptionTrue_ReturnsEmptyString()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = true;
			Assert.That(model.ExampleFirstChapterAnnouncement, Is.Empty);
		}

		[Test]
		public void ExampleFirstChapterAnnouncement_OnlySingleChapterBooksIncluded_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			Assert.That(model.ExampleFirstChapterAnnouncement, Is.Null);
		}

		[Test]
		public void ExampleSingleChapterAnnouncement_SingleChapterBookIncluded_ExampleBasedOnChapterOneOfFirstBookWithSingleChapter()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			model.SkipChapterAnnouncementForSingleChapterBooks = false;
			Assert.That(model.ExampleSingleChapterAnnouncement, Is.EqualTo("3 JON 1"));
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.That(model.ExampleSingleChapterAnnouncement, Is.EqualTo("1"));
		}

		[Test]
		public void ExampleSingleChapterAnnouncement_SkipOptionTrue_ReturnsEmptyString()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForSingleChapterBooks = true;
			Assert.That(model.ExampleSingleChapterAnnouncement, Is.Empty);
		}

		[Test]
		public void ExampleSingleChapterAnnouncement_OnlyMultipleChapterBooksIncluded_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			model.SkipChapterAnnouncementForSingleChapterBooks = false;
			Assert.That(model.ExampleSingleChapterAnnouncement, Is.Null);
		}

		[Test]
		public void ChapterAnnouncementIsStrictlyNumeric_ChapterNumberWithNoLabel_ReturnsTrue()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.That(model.ChapterAnnouncementIsStrictlyNumeric, Is.True);
		}

		[Test]
		public void ChapterAnnouncementIsStrictlyNumeric_PageHeader_ReturnsFalse()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.PageHeader;
			Assert.That(model.ChapterAnnouncementIsStrictlyNumeric, Is.False);
		}

		[Test]
		public void ChapterAnnouncementIsStrictlyNumeric_OnlySingleChapterBooksIncluded_ReturnsFalse()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			Assert.That(model.ChapterAnnouncementIsStrictlyNumeric, Is.False);
		}
	}
}
