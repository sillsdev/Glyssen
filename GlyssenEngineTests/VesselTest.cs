using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Glyssen.Shared;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using NUnit.Framework;

namespace GlyssenEngineTests
{
	class VesselTest
	{
		[Test]
		public void LoadParatextFileIntoGlyssen()
		{
			//Arrange
			var bundleFilePath = @"C:\Temp\ParatextProject.zip";

			//Act
			var bundle = new GlyssenBundle(bundleFilePath);

			//Assert
			Assert.AreEqual("Ri utzilaj tzij re ri kanimajawal Jesucristo", bundle.Name);
		}

		[Test]
		public void CreateGlyssenProject()
		{
			//Arrange
			var bundleFilePath = @"C:\Temp\ParatextProject.zip";

			//Act
			var bundle = new GlyssenBundle(bundleFilePath);
			var recordingProjectName = bundle.Name + " Audio";

			if (Project.FontRepository == null)
				Project.FontRepository = new TestFontRepository();

			var project = new Project(bundle, recordingProjectName);

			//Assert
			Assert.AreEqual(27, project.AvailableBooks.Count);
			Assert.AreEqual("MAT", project.AvailableBooks[0].Code);
		}

		public class TestFontRepository : IFontRepository
		{
			public bool IsFontInstalled(string fontFamilyIdentifier)
			{
				return true;
			}

			public bool DoesTrueTypeFontFileContainFontFamily(string ttfFile, string fontFamilyIdentifier)
			{
				throw new NotImplementedException();
			}

			public void TryToInstall(string fontFamilyIdentifier, IReadOnlyCollection<string> ttfFile)
			{
				throw new NotImplementedException();
			}

			public void ReportMissingFontFamily(string fontFamilyIdentifier)
			{
				throw new NotImplementedException();
			}
		}
	}
}
