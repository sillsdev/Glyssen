using System.Linq;
using GlyssenEngine.Quote;
using NUnit.Framework;
using SIL.WritingSystems;
using SIL.Xml;

namespace GlyssenEngineTests.Quote
{
	[TestFixture]
	class QuoteSystemTests
	{
		//[Test]
		//public void GetCorrespondingFirstLevelQuoteSystem_SameSystem()
		//{
		//	var french = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null); //Guillemets -- French
		//	Assert.That(french, Is.EqualTo(french.GetCorrespondingFirstLevelQuoteSystem()));
		//	new QuotationMark("", "", "", 1, QuotationMarkingSystemType.Normal);
		//}

		//[Test]
		//public void GetCorrespondingFirstLevelQuoteSystem_WithQuoteDash()
		//{
		//	var turkish = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), "—", null); //Tırnak işareti (with 2014 Quotation dash) -- Turkish/Vietnamese
		//	var french = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null); //Guillemets -- French
		//	var english = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null); //Quotation marks, double -- English, US/Canada
		//	Assert.That(french, Is.EqualTo(turkish.GetCorrespondingFirstLevelQuoteSystem()));
		//	Assert.That(english, Is.Not.EqualTo(turkish.GetCorrespondingFirstLevelQuoteSystem()));
		//}

		//[Test]
		//public void GetCorrespondingFirstLevelQuoteSystem_AllAdditionalFields()
		//{
		//	var madeup = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "―", "―"); //Made-up
		//	var french = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null); //Guillemets -- French
		//	var english = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null); //Quotation marks, double -- English, US/Canada
		//	Assert.That(english, Is.EqualTo(madeup.GetCorrespondingFirstLevelQuoteSystem()));
		//	Assert.That(french, Is.Not.EqualTo(madeup.GetCorrespondingFirstLevelQuoteSystem()));
		//}

		[TestCase("a")]
		[TestCase("\0")]
		public void SetReportingClauseDelimiters_StartDelimiterSetToNonPunctuationCharacter_ThrowsArgumentException(string delimiter)
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			Assert.That(() => quoteSystem.SetReportingClauseDelimiters(delimiter),
				Throws.ArgumentException.With.Message.Contains("start"));
		}

		[TestCase("a")]
		[TestCase(" ")]
		public void SetReportingClauseDelimiters_EndDelimiterSetToNonPunctuationCharacter_ThrowsArgumentException(string delimiter)
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			Assert.That(() => quoteSystem.SetReportingClauseDelimiters("―", delimiter),
				Throws.ArgumentException.With.Message.Contains("end"));
		}

		[Test]
		public void GetSpeakingAndReportingClauses_ReportingClauseDelimitersNotSet_GetsText()
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			const string text = "My favorite food ―he said― is frog soup.";
			Assert.That(quoteSystem.GetSpeakingAndReportingClauses(text).Single(), Is.EqualTo(text));
		}

		[TestCase("―")]
		[TestCase("%")]
		[TestCase("?")]
		public void GetSpeakingAndReportingClauses_ReportingClauseDelimitersSetToSameCharacter_BreaksOutReportingClause(string delimiter)
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			quoteSystem.SetReportingClauseDelimiters(delimiter);
			var results = quoteSystem.GetSpeakingAndReportingClauses(
				$"My favorite food {delimiter}he said{delimiter} is frog soup.").ToList();
			Assert.That(results.Count, Is.EqualTo(3));
			Assert.That(results[0], Is.EqualTo("My favorite food "));
			Assert.That(results[1], Is.EqualTo($"{delimiter}he said{delimiter} "));
			Assert.That(results[2], Is.EqualTo("is frog soup."));
		}

		[Test]
		public void GetSpeakingAndReportingClauses_MultipleReportingClauses_BreaksOutReportingClauses()
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			quoteSystem.SetReportingClauseDelimiters("―");
			var results = quoteSystem.GetSpeakingAndReportingClauses(
				"My favorite food ―and here he paused for effect― is frog soup― he announced.").ToList();
			Assert.That(results.Count, Is.EqualTo(4));
			Assert.That(results[0], Is.EqualTo("My favorite food "));
			Assert.That(results[1], Is.EqualTo("―and here he paused for effect― "));
			Assert.That(results[2], Is.EqualTo("is frog soup"));
			Assert.That(results[3], Is.EqualTo("― he announced."));
		}

		[TestCase("―", "%")]
		[TestCase("\u2015", "\u2014")]
		[TestCase("(", "}")]
		[TestCase("--", "--")]
		[TestCase("]", "-")]
		public void GetSpeakingAndReportingClauses_ReportingClauseDelimitersSetToDifferentCharacters_BreaksOutReportingClause(string start, string end)
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			quoteSystem.SetReportingClauseDelimiters(start, end);
			var results = quoteSystem.GetSpeakingAndReportingClauses(
				$"My favorite food {start}he said{end} is frog soup.").ToList();
			Assert.That(results.Count, Is.EqualTo(3));
			Assert.That(results[0], Is.EqualTo("My favorite food "));
			Assert.That(results[1], Is.EqualTo($"{start}he said{end} "));
			Assert.That(results[2], Is.EqualTo("is frog soup."));
		}

		[TestCase("\u2015")]
		[TestCase("%")]
		[TestCase("(")]
		public void GetSpeakingAndReportingClauses_OnlyEndDelimiterInText_GetsText(string end)
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			quoteSystem.SetReportingClauseDelimiters("\u2014", end);
			var text = $"My favorite food {end}he said{end} is frog soup.";
			Assert.That(quoteSystem.GetSpeakingAndReportingClauses(text).Single(), Is.EqualTo(text));
		}

		[Test]
		public void GetSpeakingAndReportingClauses_NoReportingClause_GetsText()
		{
			QuoteSystem quoteSystem = new QuoteSystem();
			quoteSystem.SetReportingClauseDelimiters("\u2014");
			var text = "My favorite food is frog soup.";
			Assert.That(quoteSystem.GetSpeakingAndReportingClauses(text).Single(), Is.EqualTo(text));
		}

		[TestCase("\u2014")]
		[TestCase("»")]
		[TestCase("(")]
		public void GetSpeakingAndReportingClauses_ContainsSecondLevelOpenerBeforeReportingClause_GetsText(string dialogueEnd)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal), "\u2015", dialogueEnd);
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.SetReportingClauseDelimiters("\u2014");
			var text = "»You shall say to the monkey, “Be made into soup \u2014then pause to let the words sink in\u2014 or forever be a monkey!”";
			Assert.That(quoteSystem.GetSpeakingAndReportingClauses(text).Single(), Is.EqualTo(text));
		}

		[TestCase("\u2014")]
		[TestCase("»")]
		[TestCase("}")]
		public void GetSpeakingAndReportingClauses_ContainsSecondLevelQuoteAfterReportingClause_BreaksOutReportingClause(string dialogueEnd)
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal), "\u2015", dialogueEnd);
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.SetReportingClauseDelimiters("\u2014");
			var text = "»You shall say to the monkey, —Pablo continued—, “Be made into soup or forever be a monkey!”";
			var results = quoteSystem.GetSpeakingAndReportingClauses(text).ToList();
			Assert.That(results.Count, Is.EqualTo(3));
			Assert.That(results[0], Is.EqualTo("»You shall say to the monkey, "));
			Assert.That(results[1], Is.EqualTo("—Pablo continued—, "));
			Assert.That(results[2], Is.EqualTo("“Be made into soup or forever be a monkey!”"));
		}

		[Test]
		public void Deserialize()
		{
			const string input = @"<QuoteSystem>
					<Name>Virgolette (with opening and closing 2014 Quotation dash)</Name>
					<MajorLanguage>Italian</MajorLanguage>
					<StartQuoteMarker>“</StartQuoteMarker>
					<EndQuoteMarker>”</EndQuoteMarker>
					<QuotationDashMarker>—</QuotationDashMarker>
					<QuotationDashEndMarker>—</QuotationDashEndMarker>
				</QuoteSystem>";
			var quoteSystem = XmlSerializationHelper.DeserializeFromString<QuoteSystem>(input);
			Assert.That(quoteSystem.Name, Is.EqualTo("Virgolette (with opening and closing 2014 Quotation dash)"));
			Assert.That(quoteSystem.MajorLanguage, Is.EqualTo("Italian"));
			Assert.That(quoteSystem.AllLevels.Count, Is.EqualTo(2));
			Assert.That(quoteSystem.NormalLevels.Count, Is.EqualTo(1));
			Assert.That(quoteSystem.FirstLevel.Open, Is.EqualTo("“"));
			Assert.That(quoteSystem.FirstLevel.Close, Is.EqualTo("”"));
			Assert.That(quoteSystem.FirstLevel.Continue, Is.EqualTo("“"));
			Assert.That(quoteSystem.FirstLevel.Level, Is.EqualTo(1));
			Assert.That(QuotationMarkingSystemType.Normal, Is.EqualTo(quoteSystem.FirstLevel.Type));
			Assert.That(quoteSystem.QuotationDashMarker, Is.EqualTo("—"));
			Assert.That(quoteSystem.QuotationDashEndMarker, Is.EqualTo("—"));
		}
	}
}
