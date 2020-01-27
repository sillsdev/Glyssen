using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Glyssen.Controls;
using Glyssen.Properties;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine;
using GlyssenEngine.Bundle;
using GlyssenEngine.Character;
using GlyssenEngine.Paratext;
using GlyssenEngine.Quote;
using L10NSharp;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using SIL.ObjectModel;
using SIL.Scripture;
using SIL.Windows.Forms.Extensions;
using SIL.WritingSystems;
using Analytics = DesktopAnalytics.Analytics;
using ControlExtensions = SIL.Windows.Forms.Extensions.ControlExtensions;
using BlockNavigatorViewModel = GlyssenEngine.ViewModels.BlockNavigatorViewModel<System.Drawing.Font>;

namespace Glyssen.Dialogs
{
	public partial class QuotationMarksDlg : FormWithPersistedSettings
	{
		private readonly Project m_project;
		private readonly BlockNavigatorViewModel m_navigatorViewModel;
		private readonly ProjectSettingsDlg m_parentDlg;
		private string m_xOfYFmt;
		private string m_testResultsFmt;
		private object m_versesWithMissingExpectedQuotesFilterItem;
		private object m_allQuotesFilterItem;
		private bool m_endMarkerComboIncludesSameAsStartDashTextOption;
		private bool m_formLoading;
		private bool m_allowOverride;

		internal QuotationMarksDlg(Project project, BlockNavigatorViewModel navigatorViewModel, bool readOnly, ProjectSettingsDlg parentDlg)
		{
			InitializeComponent();
			Cursor.Current = Cursors.WaitCursor;
			m_project = project;
			m_project.AnalysisCompleted -= HandleAnalysisCompleted;
			m_project.AnalysisCompleted += HandleAnalysisCompleted;
			m_navigatorViewModel = navigatorViewModel;
			m_parentDlg = parentDlg;

			if (Settings.Default.QuoteMarksDialogShowGridView)
				m_toolStripButtonGridView.Checked = true;

			if (m_navigatorViewModel == null)
				PreventNavigation();
			else
			{
				var books = new BookSet();
				foreach (var bookId in m_navigatorViewModel.IncludedBooks)
					books.Add(bookId);
				m_scriptureReference.VerseControl.BooksPresentSet = books;
				m_scriptureReference.VerseControl.ShowEmptyBooks = false;

				m_scriptureReference.VerseControl.AllowVerseSegments = false;
				m_scriptureReference.VerseControl.Versification = m_navigatorViewModel.Versification;
				m_scriptureReference.VerseControl.VerseRefChanged += m_scriptureReference_VerseRefChanged;

				m_blocksViewer.Initialize(m_navigatorViewModel);
				m_navigatorViewModel.CurrentBlockChanged += HandleCurrentBlockChanged;
				m_scriptureReference.VerseControl.GetLocalizedBookName = L10N.GetLocalizedBookNameFunc(m_scriptureReference.VerseControl.GetLocalizedBookName);

				m_blocksViewer.VisibleChanged += (sender, args) => this.SafeInvoke(() =>
				{
					if (m_blocksViewer.Visible)
						LoadBlock();
				}, GetType().FullName + " - anonymous delegate m_blocksViewer.VisibleChanged", ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed);
			}

			SetupQuoteMarksComboBoxes(m_project.QuoteSystem);

			if (m_project.IsLiveParatextProject && readOnly)
			{
				var wrapper = m_project.GetLiveParatextDataIfCompatible(null, false);
				m_linkOverride.Visible = m_allowOverride = wrapper == null || !wrapper.UserCanEditProject;
			}

			try
			{
				HandleStringsLocalized();
				LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;

				SetFilterControlsFromMode();

				if (m_project.ProjectState == ProjectState.NeedsQuoteSystemConfirmation && !readOnly)
					UpdateTestParse(false);

				ReadOnly = readOnly;
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		private void HandleCurrentBlockChanged(object sender, EventArgs eventArgs)
		{
			LoadBlock();
		}

		private void HandleStringsLocalized()
		{
			L10N.LocalizeComboList(m_toolStripComboBoxFilter, "DialogBoxes.QuotationMarksDlg.FilterOptions");

			m_versesWithMissingExpectedQuotesFilterItem = m_toolStripComboBoxFilter.Items[1];
			m_allQuotesFilterItem = m_toolStripComboBoxFilter.Items[2];

			if (m_navigatorViewModel?.Mode != BlocksToDisplay.MissingExpectedQuote &&
				((m_project.QuoteSystemStatus & QuoteSystemStatus.NotParseReady) > 0 || m_project.QuoteSystemStatus == QuoteSystemStatus.Guessed))
			{
				m_toolStripComboBoxFilter.Items.RemoveAt(2);
				m_toolStripComboBoxFilter.Items.RemoveAt(1);
			}

			SetPromptText();
			SetupQuoteMarksComboBoxes(CurrentQuoteSystem);
			m_xOfYFmt = m_labelXofY.Text;
			if (m_labelXofY.Visible)
				UpdateRelativeNavigationPositionDisplay();
			m_testResultsFmt = m_testResults.Text;
			if (m_project.ProjectState != ProjectState.NeedsQuoteSystemConfirmation)
				ShowTestResults(PercentageOfExpectedQuotesFound(m_project.Books), false);

			Text = string.Format(Text, m_project.Name);
		}

		private void SetPromptText()
		{
			string promptText = "";
			switch (m_project.QuoteSystemStatus)
			{
				case QuoteSystemStatus.Obtained:
					if (m_project.IsSampleProject)
						promptText = LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.CannotChangeSampleMsg", "The Quote Mark Settings cannot be modified for the Sample project.");
					else if (m_project.IsLiveParatextProject)
					{
						var doNotModifyDirectlyFmt = m_allowOverride ?
							LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.ShouldNotChangeParextProjectQuoteSystem",
								"If changes are needed, the settings in this {0} project can override the Quotation Rules in the live {1} project {2}. " +
								"However, if they are overridden, the results of any {3} check in {1} will not be meaningful. Therefore, if possible, " +
								"have someone with editing privileges in the {2} project do the first two steps of the following procedure and then use " +
								"Send/Receive in {1} to update the local copy of the project before proceeding with the final step:",
								"This version is displayed when the user does not have editing privileges for the Paratext project. " +
								"Param 0: \"Glyssen\" (product name); " +
								"Param 1: \"Paratext\" (product name); " +
								"Param 2: Paratext project short name (unique project identifier); " +
								"Param 3: Name of the Paratext \"Quotations\" check") :
							LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.CannotChangeParextProjectQuoteSystem",
								"The Quote Mark Settings cannot be modified directly for this {0} project, which is based on a live {1} project. " +
								"If you need to make changes, do the following:",
								"This version is displayed when the user has editing privileges for the Paratext project. " +
								"Param 0: \"Glyssen\" (product name); " +
								"Param 1: \"Paratext\" (product name)");

						promptText = String.Format(doNotModifyDirectlyFmt + Environment.NewLine +
							LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.HowToChangeParextProjectQuoteSystem",
								"1) Open the {2} project in {1}, and on the Checking menu, click Quotation Rules.\r\n" +
								"2) After saving the changes there, re-run the {3} check for all books included in this {0} project.\r\n" +
								"   (Note: The {4} and {5} checks should also pass in order for a book to be included in a {0} project.)\r\n" +
								"3) Return to {0} and on the {6} tab of the {7} dialog box, click {8}.",
								"These steps will be introduced by either \"Project.CannotChangeParextProjectQuoteSystem\" or \"Project.ShouldNotChangeParextProjectQuoteSystem\". " +
								"Param 0: \"Glyssen\" (product name); " +
								"Param 1: \"Paratext\" (product name); " +
								"Param 2: Paratext project short name (unique project identifier); " +
								"Param 3: Name of the Paratext \"Quotations\" check; " +
								"Param 4: Name of the Paratext \"Chapter/Verse Numbers\" check; " +
								"Param 5: Name of the Paratext \"Markers\" check; " +
								"Param 6: Name of the \"General\" tab in the Project Settings dialog box; " +
								"Param 7: Title of the \"Project Settings\" dialog box; " +
								"Param 8: Name of the \"Update\" button"),
							/* 0 */ GlyssenInfo.kProduct,
							/* 1 */ ParatextScrTextWrapper.kParatextProgramName,
							/* 2 */ m_project.ParatextProjectName,
							/* 3 */ ParatextProjectBookInfo.LocalizedCheckName(ParatextScrTextWrapper.kQuotationCheckId),
							/* 4 */ ParatextProjectBookInfo.LocalizedCheckName(ParatextScrTextWrapper.kChapterVerseCheckId),
							/* 5 */ ParatextProjectBookInfo.LocalizedCheckName(ParatextScrTextWrapper.kMarkersCheckId),
							/* 6 */ m_parentDlg.LocalizedGeneralTabName,
							/* 7 */ m_parentDlg.Text,
							/* 8 */ m_parentDlg.LocalizedUpdateButtonName);
					}
					else
						promptText = LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.BundleQuoteMarks", "Quote mark information was provided by the Text Release Bundle and should not normally be changed.");
					break;
				case QuoteSystemStatus.Guessed:
					promptText = string.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.CarefullyReviewQuoteMarks", "Carefully review the quote mark settings. Update them if necessary so {0} can correctly break the text into speaking parts.", "{0} is the product name"), GlyssenInfo.kProduct);
					break;
				case QuoteSystemStatus.Reviewed:
				case QuoteSystemStatus.UserSet:
					promptText = string.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.ChangeQuoteMarks", "If necessary, change the quote mark settings so {0} can correctly break the text into speaking parts.", "{0} is the product name"), GlyssenInfo.kProduct);
					break;
			}
			m_lblPrompt.Text = promptText;
		}

		private void SetupQuoteMarksComboBoxes(QuoteSystem currentSystem)
		{
			foreach (var control in m_pnlLevels.Controls)
			{
				var cb = control as ComboBox;
				if (cb != null)
				{
					cb.Items.Clear();
					cb.Items.AddRange(QuoteUtils.AllDefaultSymbols());
				}
			}

			foreach (var level in m_project.QuoteSystem.NormalLevels)
			{
				if (level.Level == 1)
					m_chkPairedQuotations.Checked = !string.IsNullOrEmpty(level.Open);

				if (m_chkPairedQuotations.Checked)
				{
					switch (level.Level)
					{
						case 1:
							m_cbLevel1Begin.Text = BlankBecomesNone(level.Open);
							m_cbLevel1Continue.Text = BlankBecomesNone(level.Continue);
							m_cbLevel1End.Text = BlankBecomesNone(level.Close);
							break;
						case 2:
							m_cbLevel2Begin.Text = BlankBecomesNone(level.Open);
							m_cbLevel2Continue.Text = BlankBecomesNone(level.Continue);
							m_cbLevel2End.Text = BlankBecomesNone(level.Close);
							break;
						case 3:
							m_cbLevel3Begin.Text = BlankBecomesNone(level.Open);
							m_cbLevel3Continue.Text = BlankBecomesNone(level.Continue);
							m_cbLevel3End.Text = BlankBecomesNone(level.Close);
							break;
					}
				}

				EnablePairedQuotes(m_chkPairedQuotations.Checked);
			}

			var quotationDashMarker = currentSystem.QuotationDashMarker;
			m_chkDialogueQuotations.Checked = !String.IsNullOrEmpty(quotationDashMarker);
			m_cboQuotationDash.Items.Clear();
			m_cboQuotationDash.Items.Add(string.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuotationDash", "Quotation dash ({0})"), "U+2015"));
			m_cboQuotationDash.Items.Add(string.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.EmDash", "Em-dash ({0})"), "U+2014"));
			m_cboQuotationDash.Items.Add(string.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.Colon", "Colon ( {0} )"), ":"));
			switch (quotationDashMarker)
			{
				case "\u2015": m_cboQuotationDash.SelectedIndex = 0; break;
				case "\u2014": m_cboQuotationDash.SelectedIndex = 1; break;
				case ":": m_cboQuotationDash.SelectedIndex = 2; break;
				default: m_cboQuotationDash.Text = quotationDashMarker; break;
			}

			m_cboEndQuotationDash.Items.Clear();
			m_cboEndQuotationDash.Items.Add(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.EndQuotationDashWithParagraphOnly", "End of paragraph (only)"));
			if (QuotationDashSelected)
			{
				m_cboEndQuotationDash.Items.Add(SameAsStartDashText);
				m_endMarkerComboIncludesSameAsStartDashTextOption = true;
			}
#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
			m_cboEndQuotationDash.Items.Add(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.SentenceEndingPunctuation", "Sentence-ending punctuation"));
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES

			var quotationDashEndMarker = currentSystem.QuotationDashEndMarker;
			if (string.IsNullOrEmpty(quotationDashEndMarker))
				m_cboEndQuotationDash.SelectedIndex = 0;
			else if (m_endMarkerComboIncludesSameAsStartDashTextOption && quotationDashEndMarker == quotationDashMarker)
				m_cboEndQuotationDash.SelectedIndex = 1;
#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
			else if (quotationDashEndMarker == QuoteUtils.kSentenceEndingPunctuation)
				m_cboEndQuotationDash.SelectedIndex = m_endMarkerComboIncludesSameAsStartDashTextOption ? 2 : 1;
#endif //HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
			else
				m_cboEndQuotationDash.Text = quotationDashEndMarker;
		}

		private string BlankBecomesNone(string text)
		{
			return string.IsNullOrEmpty(text) ? QuoteUtils.None : text;
		}

		private string NoneBecomesBlank(string text)
		{
			return text == QuoteUtils.None ? null : text;
		}

		private bool ReadOnly
		{
			set
			{
				m_pnlLevels.Enabled = !value;
				m_pnlDialogueQuotes.Enabled = !value;
				m_chkPairedQuotations.Enabled = !value;
				m_btnOk.Enabled = !value;
				m_btnTest.Visible = !value;
				m_testResults.Visible = !value;
			}
		}

		private string SameAsStartDashText
		{
			get
			{
				string quotationDashMarker;
				switch (m_cboQuotationDash.SelectedIndex)
				{
					case 0: quotationDashMarker = "U+2015"; break;
					case 1: quotationDashMarker = "U+2014"; break;
					default:
						throw new InvalidOperationException("SameAsStartDashText is not valid in this state!");
				}

				return string.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.EndQuotationDashWithStartDash",
					"Same as start quotation dash ({0})"), quotationDashMarker);
			}
		}

		private bool ValidateQuoteSystem(QuoteSystem quoteSystem, out string validationMessage)
		{
			if (!m_chkPairedQuotations.Checked && !m_chkDialogueQuotations.Checked)
			{
				validationMessage = LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.SelectMarksOrDashes", "You must select quotation marks, quotation dashes, or both.");
				return false;
			}

			if (m_chkPairedQuotations.Checked)
			{
				var level1 = quoteSystem.FirstLevel;
				if (level1 == null || string.IsNullOrEmpty(level1.Open) || string.IsNullOrEmpty(level1.Close))
				{
					validationMessage = LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.Level1OpenCloseRequired", "Level 1 Open and Close are required.");
					return false;
				}
			}

			if (m_chkDialogueQuotations.Checked)
			{
				if (string.IsNullOrEmpty(quoteSystem.QuotationDashMarker))
				{
					validationMessage = LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuotationDashRequired", "Quotation dash is required.");
					return false;
				}
			}

			validationMessage = null;
			return true;
		}

		private void m_btnOk_Click(object sender, EventArgs e)
		{
			DisableForm(true);

			QuoteSystem currentQuoteSystem = CurrentQuoteSystem;

			if (currentQuoteSystem == m_project.QuoteSystem)
			{
				if ((m_project.QuoteSystemStatus & QuoteSystemStatus.NotParseReady) > 0)
				{
					m_project.QuoteSystemStatus = QuoteSystemStatus.Reviewed;
					// Kicks off the quote parse (which we haven't run yet)
					m_project.QuoteSystem = currentQuoteSystem;
				}
				else
					HandleAnalysisCompleted(this, null);
				return;
			}

			string validationMessage;
			if (!ValidateQuoteSystem(currentQuoteSystem, out validationMessage))
			{
				MessageBox.Show(validationMessage,
					LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuoteSystemInvalid",
					"Quote System Invalid"));
				DisableForm(false);
				return;
			}
			List<string> msgParts = new List<string>();
			if (m_project.IsLiveParatextProject)
			{
				msgParts.Add(String.Format(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuoteSystemChange.GetDataFromParatext",
					"This requires {0} to retrieve the current text from {1} project {2}, so any textual changes made there " +
					"will now be incorporated into this {0} project.",
					"Param 0: \"Glyssen\" (product name); " +
					"Param 1: \"Paratext\" (product name); " +
					"Param 2: Paratext project short name (unique project identifier)"),
					GlyssenInfo.kProduct,
					ParatextScrTextWrapper.kParatextProgramName,
					m_project.ParatextProjectName));
			}
			if (m_project.Books.SelectMany(b => b.Blocks).Any(bl => bl.UserConfirmed) && m_project.IsQuoteSystemReadyForParse && m_project.QuoteSystem != null)
			{
				msgParts.Add(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuoteSystemChange.WorkWillBePreserved",
					"An attempt will be made to preserve the work you have already completed, but some character assignments might be lost."));
			}
			if (msgParts.Any())
			{
				msgParts.Insert(0, LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuoteSystemChange.Part1",
					"Changing the quote system will require the text to be broken up into speaking parts again."));
				msgParts.Add(LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.QuoteSystemChange.BackupWillBeCreated",
					"A backup of your project will be created before this occurs."));
				string title = LocalizationManager.GetString("DialogBoxes.QuotationMarksDlg.ConfirmQuoteSystemChange",
					"Confirm Quote System Change");
				if (MessageBox.Show(String.Join(Environment.NewLine, msgParts), title, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
				{
					SetupQuoteMarksComboBoxes(m_project.QuoteSystem);
					DisableForm(false);
					return;
				}
			}

			if (m_project.QuoteSystemStatus == QuoteSystemStatus.Obtained)
				Analytics.Track("ObtainedQuoteSystemChanged", new Dictionary<string, string>
				{
					{ "old", m_project.QuoteSystem != null ? m_project.QuoteSystem.ToString() : String.Empty },
					{ "new", currentQuoteSystem.ToString() }
				});
			else if (m_project.QuoteSystemStatus != QuoteSystemStatus.UserSet)
				Analytics.Track("GuessedQuoteSystemChanged", new Dictionary<string, string>
				{
					{ "old", m_project.QuoteSystem != null ? m_project.QuoteSystem.ToString() : String.Empty },
					{ "new", currentQuoteSystem.ToString() }
				});

			// Want to set the status even if already UserSet because that triggers setting QuoteSystemDate
			m_project.QuoteSystemStatus = QuoteSystemStatus.UserSet;

			m_project.QuoteSystem = currentQuoteSystem;
			// After setting this, the user could get a subsequent dialog box giving them the chance to review the settings,
			// but since we've already saved their changes, they can't really "Cancel" those saved changes anymore.
			m_btnCancel.DialogResult = DialogResult.OK;
		}

		private void HandleAnalysisCompleted(object sender, EventArgs e)
		{
			var percentageOfExpectedQuotesFound = PercentageOfExpectedQuotesFound(m_project.Books);

			if (percentageOfExpectedQuotesFound < Settings.Default.TargetPercentageOfQuotesFound)
			{
				using (var dlg = new PercentageOfExpectedQuotesFoundTooLowDlg(Text, percentageOfExpectedQuotesFound))
				{
					MainForm.LogDialogDisplay(dlg);
					dlg.ShowDialog();
					if (dlg.UserWantsToReview)
					{
						m_navigatorViewModel.BlockNavigator = new BlockNavigator(m_project.IncludedBooks);
						ShowTestResults(percentageOfExpectedQuotesFound, true);
						DisableForm(false);
						return;
					}
				}
			}
			else if (m_project.ProjectAnalysis.PercentUnknown > Settings.Default.MaxAcceptablePercentageOfUnknownQuotes)
			{
				using (var dlg = new TooManyUnexpectedQuotesFoundDlg(Text, m_project.ProjectAnalysis.PercentUnknown))
				{
					MainForm.LogDialogDisplay(dlg);
					dlg.ShowDialog();
					if (dlg.UserWantsToReview)
					{
						if (!m_toolStripComboBoxFilter.Items.Contains(m_allQuotesFilterItem))
						{
							m_toolStripComboBoxFilter.Items.Insert(m_toolStripComboBoxFilter.Items.Count - 1, m_allQuotesFilterItem);
						}
						m_toolStripComboBoxFilter.SelectedItem = m_allQuotesFilterItem;
						DisableForm(false);
						return;
					}
				}
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private double PercentageOfExpectedQuotesFound(System.Collections.Generic.IReadOnlyList<BookScript> books)
		{
			var totalExpectedQuotesInIncludedChapters = 0;
			var totalVersesWithExpectedQuotes = 0;

			var expectedQuotes = ControlCharacterVerseData.Singleton.ExpectedQuotes;
			foreach (var book in expectedQuotes.Keys)
			{
				var bookScript = books.FirstOrDefault(b => BCVRef.BookToNumber(b.BookId) == book);
				if (bookScript == null)
					continue;
				foreach (var chapter in expectedQuotes[book])
				{
					var chapterCounted = false;
					foreach (var verseWithExpectedQuote in chapter.Value)
					{
						var referenceForExpectedQuote = new VerseRef(book, chapter.Key, verseWithExpectedQuote, ScrVers.English);
						referenceForExpectedQuote.ChangeVersification(m_project.Versification);
						var blocks =
							bookScript.GetBlocksForVerse(referenceForExpectedQuote.ChapterNum, referenceForExpectedQuote.VerseNum).ToList();
						if (!chapterCounted && blocks.Any())
						{
							totalExpectedQuotesInIncludedChapters += chapter.Value.Count;
							chapterCounted = true;
						}
						if (blocks.Any(b => b.IsQuote))
							totalVersesWithExpectedQuotes++;
					}
				}
			}

			return totalVersesWithExpectedQuotes * 100.0 / totalExpectedQuotesInIncludedChapters;
		}

		public QuoteSystem CurrentQuoteSystem
		{
			get
			{
				var levels = new BulkObservableList<QuotationMark>();

				if (m_chkPairedQuotations.Checked)
				{
					levels.Add(new QuotationMark(NoneBecomesBlank(m_cbLevel1Begin.Text), NoneBecomesBlank(m_cbLevel1End.Text), NoneBecomesBlank(m_cbLevel1Continue.Text), 1, QuotationMarkingSystemType.Normal));
					string level2Open = NoneBecomesBlank(m_cbLevel2Begin.Text);
					if (!string.IsNullOrEmpty(level2Open))
						levels.Add(new QuotationMark(level2Open, NoneBecomesBlank(m_cbLevel2End.Text), NoneBecomesBlank(m_cbLevel2Continue.Text), 2, QuotationMarkingSystemType.Normal));
					string level3Open = NoneBecomesBlank(m_cbLevel3Begin.Text);
					if (!string.IsNullOrEmpty(level3Open))
						levels.Add(new QuotationMark(level3Open, NoneBecomesBlank(m_cbLevel3End.Text), NoneBecomesBlank(m_cbLevel3Continue.Text), 3, QuotationMarkingSystemType.Normal));
				}

				if (m_chkDialogueQuotations.Checked)
				{
					string quotationDashMarker = null;
					string quotationDashEndMarker = null;
					switch (m_cboQuotationDash.SelectedIndex)
					{
						case 0: quotationDashMarker = "\u2015"; break;
						case 1: quotationDashMarker = "\u2014"; break;
						case 2: quotationDashMarker = ":"; break;
						default:
							if (!String.IsNullOrWhiteSpace(m_cboQuotationDash.Text))
								quotationDashMarker = m_cboQuotationDash.Text;
							break;
					}

					switch (m_cboEndQuotationDash.SelectedIndex)
					{
						case 0: break;
						case 1:
							if (m_endMarkerComboIncludesSameAsStartDashTextOption)
							{
								quotationDashEndMarker = quotationDashMarker;
								break;
							}
#if HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
							if (m_endMarkerComboIncludesSameAsStartDashTextOption)
							{
								quotationDashEndMarker = quotationDashMarker;
								break;
							}
							goto case 2;
						case 2: quotationDashEndMarker = QuoteUtils.kSentenceEndingPunctuation; break;
#else
							quotationDashEndMarker = quotationDashMarker; break;
#endif // HANDLE_SENTENCE_ENDING_PUNCTUATION_FOR_DIALOGUE_QUOTES
						default:
							if (!String.IsNullOrWhiteSpace(m_cboEndQuotationDash.Text))
								quotationDashEndMarker = m_cboEndQuotationDash.Text;
							break;
					}

					if (quotationDashMarker != null)
						levels.Add(new QuotationMark(quotationDashMarker, quotationDashEndMarker, null, 1, QuotationMarkingSystemType.Narrative));
				}

				return new QuoteSystem(levels);
			}
		}

		private void EnablePairedQuotes(bool enable)
		{
			m_cbLevel1Begin.Enabled = enable;
			m_cbLevel1Continue.Enabled = enable;
			m_cbLevel1End.Enabled = enable;

			m_cbLevel2Begin.Enabled = enable;
			m_cbLevel2Continue.Enabled = enable;
			m_cbLevel2End.Enabled = enable;

			m_cbLevel3Begin.Enabled = enable;
			m_cbLevel3Continue.Enabled = enable;
			m_cbLevel3End.Enabled = enable;

			m_lblLevel1.Enabled = enable;
			m_lblLevel2.Enabled = enable;
			m_lblLevel3.Enabled = enable;

			m_lblBegin.Enabled = enable;
			m_lblContinue.Enabled = enable;
			m_lblEnd.Enabled = enable;
		}

		private void ShowTestResults(double percentageOfExpected, bool changeFilter)
		{
			m_testResults.Text = string.Format(m_testResultsFmt, percentageOfExpected);

			var showWarning = (100 - percentageOfExpected) > Settings.Default.MaxAcceptablePercentageOfUnknownQuotes;
			m_testResults.ForeColor = glyssenColorPalette.GetColor(showWarning ? GlyssenColors.Warning : GlyssenColors.ForeColor);

			m_testResults.Visible = true;

			if (changeFilter)
				SelectMissingExpectedQuotesFilter();
		}

		private void SelectMissingExpectedQuotesFilter()
		{
			if (!m_toolStripComboBoxFilter.Items.Contains(m_versesWithMissingExpectedQuotesFilterItem))
			{
				m_toolStripComboBoxFilter.Items.Insert(1, m_versesWithMissingExpectedQuotesFilterItem);
			}
			m_toolStripComboBoxFilter.SelectedItem = m_versesWithMissingExpectedQuotesFilterItem;
		}

		private void DisableForm(bool disabled)
		{
			Cursor.Current = disabled ? Cursors.WaitCursor : Cursors.Default;
			Enabled = !disabled;
			Refresh();
		}

		#region Form events
		protected override void OnLoad(EventArgs e)
		{
			m_formLoading = true;

			base.OnLoad(e);
			if (Settings.Default.QuotationMarksDlgSplitterDistance > 0)
				m_splitContainer.SplitterDistance = Settings.Default.QuotationMarksDlgSplitterDistance;
		}

		private void QuotationMarksDlg_Shown(object sender, EventArgs e)
		{
			m_formLoading = false;
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			m_navigatorViewModel.LoadNextRelevantBlock();
		}

		private void m_btnPrevious_Click(object sender, EventArgs e)
		{
			m_navigatorViewModel.LoadPreviousRelevantBlock();
		}

		private void m_chkDialogueQuotations_CheckedChanged(object sender, EventArgs e)
		{
			m_cboQuotationDash.Enabled = m_chkDialogueQuotations.Checked;
			m_cboEndQuotationDash.Enabled = m_chkDialogueQuotations.Checked;
			m_lblStartDialogueQuote.Enabled = m_chkDialogueQuotations.Checked;
			m_lblEndDialogueQuote.Enabled = m_chkDialogueQuotations.Checked;
			m_chkAlternateSpeakersInFirstLevelQuotes.Enabled = m_chkDialogueQuotations.Checked;
			HandleSettingChange(sender, e);
		}

		private bool QuotationDashSelected
		{
			get { return (m_cboQuotationDash.SelectedIndex == 0 || m_cboQuotationDash.SelectedIndex == 1); }
		}

		private void m_cboQuotationDash_TextChanged(object sender, EventArgs e)
		{
			if (m_endMarkerComboIncludesSameAsStartDashTextOption && !QuotationDashSelected)
			{
				if (m_cboEndQuotationDash.SelectedIndex == 1)
					m_cboEndQuotationDash.SelectedIndex = 0;
				m_cboEndQuotationDash.Items.RemoveAt(1);
				m_endMarkerComboIncludesSameAsStartDashTextOption = false;
			}

			if (QuotationDashSelected)
			{
				if (m_endMarkerComboIncludesSameAsStartDashTextOption)
					m_cboEndQuotationDash.Items[1] = SameAsStartDashText;
				else
					m_cboEndQuotationDash.Items.Insert(1, SameAsStartDashText);
				m_endMarkerComboIncludesSameAsStartDashTextOption = true;
			}
		}

		private void HandleFilterChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				return;

			if (!m_testResults.Visible)
				UpdateTestParse(false);

			BlocksToDisplay mode;

			switch (m_toolStripComboBoxFilter.SelectedIndex)
			{
				case 0: mode = BlocksToDisplay.AllExpectedQuotes; break;
				case 4: mode = BlocksToDisplay.AllScripture; break;
				default:
					if (m_toolStripComboBoxFilter.SelectedItem == m_versesWithMissingExpectedQuotesFilterItem)
						mode = BlocksToDisplay.MissingExpectedQuote;
					else if (m_toolStripComboBoxFilter.SelectedItem == m_allQuotesFilterItem)
						mode = BlocksToDisplay.AllQuotes;
					else
						mode = BlocksToDisplay.AllScripture;
					break;
			}

			m_navigatorViewModel.SetMode(mode);

			if (m_navigatorViewModel.RelevantBlockCount > 0)
			{
				LoadBlock();
			}
			else
			{
				m_labelXofY.Visible = false;
				UpdateNavigationButtonState();
				m_blocksViewer.ShowNothingMatchesFilterMessage();
			}
		}

		private void SetFilterControlsFromMode()
		{
			if (m_navigatorViewModel == null)
				return;
			var mode = m_navigatorViewModel.Mode;
			if ((mode & BlocksToDisplay.AllExpectedQuotes) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = 0;
			else if ((mode & BlocksToDisplay.MissingExpectedQuote) != 0)
			{
				Debug.Assert(m_toolStripComboBoxFilter.Items.Count > 2);
				m_toolStripComboBoxFilter.SelectedIndex = 1;
			}
			else if ((mode & BlocksToDisplay.AllScripture) != 0)
				m_toolStripComboBoxFilter.SelectedIndex = m_toolStripComboBoxFilter.Items.Count - 1;
			else
				// ReSharper disable once NotResolvedInText
				throw new InvalidEnumArgumentException("mode", (int) mode, typeof (BlocksToDisplay));
		}

		public void LoadBlock()
		{
			UpdateDisplay();
			UpdateNavigationButtonState();
		}

		private void UpdateDisplay()
		{
			if (m_navigatorViewModel == null)
				return;
			var blockRef = m_navigatorViewModel.GetBlockVerseRef();
			int versesInBlock = m_navigatorViewModel.CurrentBlock.LastVerseNum - blockRef.VerseNum;
			var displayedRefMinusBlockStartRef = m_scriptureReference.VerseControl.VerseRef.BBBCCCVVV - blockRef.BBBCCCVVV;
			if (displayedRefMinusBlockStartRef < 0 || displayedRefMinusBlockStartRef > versesInBlock)
				m_scriptureReference.VerseControl.VerseRef = m_navigatorViewModel.GetBlockVerseRef();
			m_labelXofY.Visible = m_navigatorViewModel.IsCurrentLocationRelevant;
			Debug.Assert(m_navigatorViewModel.RelevantBlockCount >= m_navigatorViewModel.CurrentDisplayIndex);
			UpdateRelativeNavigationPositionDisplay();

			m_navigatorViewModel.GetBlockVerseRef().SendScrReference();
		}

		private void UpdateRelativeNavigationPositionDisplay()
		{
			m_labelXofY.Text = string.Format(m_xOfYFmt, m_navigatorViewModel.CurrentDisplayIndex, m_navigatorViewModel.RelevantBlockCount);
		}

		private void UpdateNavigationButtonState()
		{
			m_btnNext.Enabled = m_navigatorViewModel.CanNavigateToNextRelevantBlock;
			m_btnPrevious.Enabled = m_navigatorViewModel.CanNavigateToPreviousRelevantBlock;
		}

		private void HandleHtmlViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonGridView.Checked = !m_toolStripButtonHtmlView.Checked;

				Debug.Assert(!m_toolStripButtonGridView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Html;
				Settings.Default.QuoteMarksDialogShowGridView = false;
			}
		}

		private void HandleDataGridViewCheckChanged(object sender, EventArgs e)
		{
			if (m_toolStripButtonHtmlView.Checked == m_toolStripButtonGridView.Checked)
			{
				m_toolStripButtonHtmlView.Checked = !m_toolStripButtonGridView.Checked;

				Debug.Assert(!m_toolStripButtonHtmlView.Checked);

				m_blocksViewer.ViewType = ScriptBlocksViewType.Grid;
				Settings.Default.QuoteMarksDialogShowGridView = true;
			}
		}

		private void HandleViewTypeToolStripButtonClick(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			if (!button.Checked)
			{
				button.Checked = true;

				Analytics.Track("SwitchView", new Dictionary<string, string> { { "dialog", Name }, { "view", button.ToString() } });
		}
		}

		private void IncreaseFont(object sender, EventArgs e)
		{
			m_blocksViewer.IncreaseFont();
		}

		private void DecreaseFont(object sender, EventArgs e)
		{
			m_blocksViewer.DecreaseFont();
		}

		private void m_scriptureReference_VerseRefChanged(object sender, PropertyChangedEventArgs e)
		{
			m_navigatorViewModel.TryLoadBlock(m_scriptureReference.VerseControl.VerseRef);
		}

		private void m_chkPairedQuotations_CheckedChanged(object sender, EventArgs e)
		{
			EnablePairedQuotes(m_chkPairedQuotations.Checked);
		}

		private void m_btnTest_Click(object sender, EventArgs e)
		{
			UpdateTestParse(true);
		}

		private void UpdateTestParse(bool changeFilterToShowMissingExpectedQuotes)
		{
			try
			{
				DisableForm(true);
				var parsedBooks = m_project.TestQuoteSystem(CurrentQuoteSystem);
				if (parsedBooks.Any())
				{
					m_navigatorViewModel.BlockNavigator = new BlockNavigator(parsedBooks.Where(b => m_project.IncludedBooks.Any(ib => ib.BookId == b.BookId)).ToList());
					ShowTestResults(PercentageOfExpectedQuotesFound(parsedBooks), changeFilterToShowMissingExpectedQuotes);
				}
				else
					PreventNavigation();
			}
			finally
			{
				DisableForm(false);
			}
		}

		private void PreventNavigation()
		{
			m_blocksViewer.Hide();
			m_toolStrip.Hide();
			m_tableLayoutPanelDataBrowser.Hide();
			m_btnTest.Hide();
			m_splitContainer.Panel1Collapsed = true;
		}

		private void m_splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if (!m_formLoading)
				Settings.Default.QuotationMarksDlgSplitterDistance = e.SplitX;
		}
		#endregion

		private void HandleSettingChange(object sender, EventArgs e)
		{
			m_testResults.Visible = false;
		}

		private void m_linkOverride_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ReadOnly = false;
		}
	}
}
