using Glyssen.Bundle;
using Glyssen.ViewModel;
using NUnit.Framework;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	public class ProjectSettingsViewModelTests
	{
		[Test]
		public void ExampleSubsequentChapterAnnouncement_MultiChapterBookIncluded_ExampleBasedOnChapterTwoOfFirstBookWithMultipleChapters()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			Assert.AreEqual("1 JON 2", model.ExampleSubsequentChapterAnnouncement);
			Assert.IsFalse(model.ChapterAnnouncementIsStrictlyNumeric);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.AreEqual("2", model.ExampleSubsequentChapterAnnouncement);
			Assert.IsTrue(model.ChapterAnnouncementIsStrictlyNumeric);
		}

		[Test]
		public void ExampleSubsequentChapterAnnouncement_OnlySingleChapterBooksIncluded_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			Assert.IsNull(model.ExampleSubsequentChapterAnnouncement);
		}

		[Test]
		public void ExampleFirstChapterAnnouncement_MultiChapterBookIncluded_ExampleBasedOnChapterOneOfFirstBookWithMultipleChapters()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			Assert.AreEqual("1 JON 1", model.ExampleFirstChapterAnnouncement);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.AreEqual("1", model.ExampleFirstChapterAnnouncement);
		}

		[Test]
		public void ExampleFirstChapterAnnouncement_SkipOptionTrue_ReturnsEmptyString()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = true;
			Assert.IsEmpty(model.ExampleFirstChapterAnnouncement);
		}

		[Test]
		public void ExampleFirstChapterAnnouncement_OnlySingleChapterBooksIncluded_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			Assert.IsNull(model.ExampleFirstChapterAnnouncement);
		}

		[Test]
		public void ExampleSingleChapterAnnouncement_SingleChapterBookIncluded_ExampleBasedOnChapterOneOfFirstBookWithSingleChapter()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			model.SkipChapterAnnouncementForSingleChapterBooks = false;
			Assert.AreEqual("3 JON 1", model.ExampleSingleChapterAnnouncement);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.AreEqual("1", model.ExampleSingleChapterAnnouncement);
		}

		[Test]
		public void ExampleSingleChapterAnnouncement_SkipOptionTrue_ReturnsEmptyString()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForSingleChapterBooks = true;
			Assert.IsEmpty(model.ExampleSingleChapterAnnouncement);
		}

		[Test]
		public void ExampleSingleChapterAnnouncement_OnlyMultipleChapterBooksIncluded_ReturnsNull()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.SkipChapterAnnouncementForFirstChapter = false;
			model.SkipChapterAnnouncementForSingleChapterBooks = false;
			Assert.IsNull(model.ExampleSingleChapterAnnouncement);
		}

		[Test]
		public void ChapterAnnouncementIsStrictlyNumeric_ChapterNumberWithNoLabel_ReturnsTrue()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.ChapterLabel;
			Assert.IsTrue(model.ChapterAnnouncementIsStrictlyNumeric);
		}

		[Test]
		public void ChapterAnnouncementIsStrictlyNumeric_PageHeader_ReturnsFalse()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.GAL, TestProject.TestBook.IJN);
			var model = new ProjectSettingsViewModel(testProject);
			model.ChapterAnnouncementStyle = ChapterAnnouncement.PageHeader;
			Assert.IsFalse(model.ChapterAnnouncementIsStrictlyNumeric);
		}

		[Test]
		public void ChapterAnnouncementIsStrictlyNumeric_OnlySingleChapterBooksIncluded_ReturnsFalse()
		{
			var testProject = TestProject.CreateTestProject(TestProject.TestBook.IIJN, TestProject.TestBook.IIIJN);
			var model = new ProjectSettingsViewModel(testProject);
			Assert.IsFalse(model.ChapterAnnouncementIsStrictlyNumeric);
		}
	}
}
