using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Glyssen.Controls;
using Glyssen.Shared;
using Glyssen.Utilities;
using GlyssenEngine.Bundle;
using GlyssenEngine.Utilities;
using L10NSharp.TMXUtils;
using L10NSharp.UI;
using SIL;

namespace Glyssen.Dialogs
{
	public partial class CastSizePlanningDlg : Form
	{
		private readonly CastSizePlanningViewModel m_viewModel;
		private readonly bool m_loaded;
		private string m_fmtProjectSummaryPlural;
		private string m_fmtSuboptimalNarratorsMsg;

		public CastSizePlanningDlg(CastSizePlanningViewModel viewModel)
		{
			InitializeComponent();
			m_viewModel = viewModel;
			m_viewModel.MaleNarratorsValueChanged += m_viewModel_MaleNarratorsValueChanged;
			m_viewModel.FemaleNarratorsValueChanged += m_viewModel_FemaleNarratorsValueChanged;
			m_castSizePlanningOptions.SetViewModel(m_viewModel);

			HandleStringsLocalized();
			LocalizeItemDlg<TMXDocument>.StringsLocalized += HandleStringsLocalized;

			m_tableLayoutStartingOver.Visible = m_viewModel.Project.CharacterGroupListPreviouslyGenerated;
			m_maleNarrators.Maximum = m_viewModel.MaximumNarratorsValue;
			SetMinimumMaleNarrators();
			m_maleNarrators.Value = m_viewModel.MaleNarrators;
			m_femaleNarrators.Maximum = m_maleNarrators.Maximum;
			m_femaleNarrators.Value = m_viewModel.FemaleNarrators;

			NarratorOption = m_viewModel.NarratorOption;
			CastSizeOption = m_viewModel.CastSizeOption;

			// set the warning icon
			m_imgNarratorWarning.Image = SystemIcons.Error.ToBitmap();

			m_loaded = true;
		}

		private void HandleStringsLocalized()
		{
			m_fmtProjectSummaryPlural = m_lblProjectSummary.Text;
			m_fmtSuboptimalNarratorsMsg = m_lblWarningSuboptimalNarratorsForNarrationByAuthor.Text;

			var project = m_viewModel.Project;
			int includedBooksCount = project.IncludedBooks.Count;
			if (includedBooksCount == 1)
				m_lblProjectSummary.Text = string.Format(Localizer.GetString("DialogBoxes.CastSizePlanningDlg.ProjectSummary.Singular",
					"This project has 1 book with {0} distinct character roles."), project.TotalCharacterCount);
			else
				m_lblProjectSummary.Text = string.Format(m_fmtProjectSummaryPlural, includedBooksCount, project.TotalCharacterCount);
			m_lblRecordingTime.Text = string.Format(m_lblRecordingTime.Text, project.GetEstimatedRecordingTime());

			Text = string.Format(Text, m_viewModel.Project.Name);
			m_lblWhenYouClick.Text = string.Format(m_lblWhenYouClick.Text, m_btnGenerate.Text, GlyssenInfo.kProduct);
			if (m_loaded)
				ShowOrHideNarratorCountWarnings();
		}

		private void CastSizePlanningDlg_Load(object sender, EventArgs e)
		{
			// TODO: re-enable these links when the messages are implemented
			m_linkMoreInfo.Visible = false;
			m_linkAbout.Visible = false;

			MainForm.SetChildFormLocation(this);
		}

		private void m_tableLayoutPanel_Paint(object sender, PaintEventArgs e)
		{
			// draw the horizontal line
			var y = m_flowLayoutPanel2.Top - 1;
			var startPoint = new Point(0, y);
			var endPoint = new Point(m_tableLayoutPanel.Width, y);
			var foreColorPen = new Pen(glyssenColorPalette.GetColor(GlyssenColors.ForeColor), 1);
			e.Graphics.DrawLine(foreColorPen, startPoint, endPoint);
		}

		private void m_layoutNarrators_Paint(object sender, PaintEventArgs e)
		{
			// draw the vertical line
			var x = m_layoutMaleFemale.Left - m_layoutMaleFemale.Margin.Left/2;
			const int verticalOffset = 6;
			var startPoint = new Point(x, verticalOffset);
			var endPoint = new Point(x, m_layoutNarrators.Height - verticalOffset);
			var foreColorPen = new Pen(glyssenColorPalette.GetColor(GlyssenColors.ForeColor), 1);
			e.Graphics.DrawLine(foreColorPen, startPoint, endPoint);
		}

		private void m_linkMoreInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			// TODO: impliment this
			MessageBox.Show(this, @"TODO: Put some appropriate text here.", ProductName, MessageBoxButtons.OK);
		}

		private void m_linkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var msg = new StringBuilder();
			msg.AppendParagraph(Localizer.GetString("DialogBoxes.CastSizePlanningDlg.IsAbout.Line1",
				"Since there are usually a large number of brief character roles in a recording script, most voice actors will speak multiple roles."));
			msg.AppendParagraph(Localizer.GetString("DialogBoxes.CastSizePlanningDlg.IsAbout.Line2",
				"So how many voice actors or narrators should you have? That’s discussed in the <a href=\"#\" target=\"_blank\">Guide for the Recording Project Coordinator</a>."));
			msg.AppendParagraph(Localizer.GetString("DialogBoxes.CastSizePlanningDlg.IsAbout.Line3",
				"In the <b>Cast Size Planning</b> dialog, Glyssen provides recommendations for the size of your cast and for how to distribute the narration work. You can adjust these as needed."));
			msg.AppendParagraph(Localizer.GetString("DialogBoxes.CastSizePlanningDlg.IsAbout.Line4",
				"As a result of this planning, Glyssen will generate a list of groups called ‘Roles for Voice Actors’. This list will be useful to you for recruiting and/or assigning voice actors."));
			msg.AppendParagraph(Localizer.GetString("DialogBoxes.CastSizePlanningDlg.IsAbout.Line5",
				"Not sure how to plan? Try some selections and see how the groups work out. You are free to come back and change things."));

			var html = msg.ToString();

			using (var dlg = new HtmlMessageDlg(html))
			{
				dlg.ShowDialog(this);
			}
		}

		private void m_linkVoiceActorList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowVoiceActorList(true);
		}

		private void ShowVoiceActorList(bool keepSelection)
		{
			var actorInfoViewModel = new VoiceActorInformationViewModel(m_viewModel.Project);
			using (var actorDlg = new VoiceActorInformationDlg(actorInfoViewModel, false, false, !keepSelection))
			{
				MainForm.LogDialogDisplay(actorDlg);
				if (actorDlg.ShowDialog(this) == DialogResult.Cancel)
					keepSelection = true; // Even though Cancel doesn't actually discard changes, it should at least reflect the user's desire not to have any changes result in a change to the cast size choice

				m_viewModel.SetVoiceActorListValues(new CastSizeRowValues(m_viewModel.Project.VoiceActorList), keepSelection);
			}
			m_castSizePlanningOptions.Refresh();
			UpdateButtonState();
			ShowOrHideNarratorCountWarnings();
		}

		private NarratorsOption NarratorOption
		{
			get
			{
				if (m_rbSingleNarrator.Checked) return NarratorsOption.SingleNarrator;
				if (m_rbAuthorNarrator.Checked) return NarratorsOption.NarrationByAuthor;
				return NarratorsOption.Custom;
			}
			set
			{
				switch (value)
				{
					case NarratorsOption.SingleNarrator:
						m_rbSingleNarrator.Checked = true;
						break;

					case NarratorsOption.NarrationByAuthor:
						m_rbAuthorNarrator.Checked = true;
						break;

					default:
						m_rbCustomNarrator.Checked = true;
						break;
				}
			}
		}

		void m_viewModel_MaleNarratorsValueChanged(object sender, int e)
		{
			m_maleNarrators.Value = e;
			UpdateButtonState();
		}

		void m_viewModel_FemaleNarratorsValueChanged(object sender, int e)
		{
			m_femaleNarrators.Value = e;
			UpdateButtonState();
		}

		private void NarratorOptionChanged(object sender, EventArgs e)
		{
			m_viewModel.NarratorOption = NarratorOption;

			if (NarratorOption == NarratorsOption.Custom)
				m_femaleNarrators.Value = 0;
			m_femaleNarrators.Enabled = NarratorOption == NarratorsOption.Custom;
			m_maleNarrators.Enabled = NarratorOption != NarratorsOption.SingleNarrator;
			SetMinimumMaleNarrators();

			ShowOrHideNarratorCountWarnings();
		}

		private void SetMinimumMaleNarrators()
		{
			m_maleNarrators.Minimum = m_viewModel.NarratorOption == NarratorsOption.NarrationByAuthor ? 1 : 0;
		}

		private void MaleNarratorsValueChanged(object sender, EventArgs e)
		{
			if (!m_loaded)
				return;

			m_viewModel.MaleNarrators = (int)m_maleNarrators.Value;
			ShowOrHideNarratorCountWarnings();
		}

		private void FemaleNarratorsValueChanged(object sender, EventArgs e)
		{
			if (!m_loaded)
				return;

			m_viewModel.FemaleNarrators = (int)m_femaleNarrators.Value;
			ShowOrHideNarratorCountWarnings();
		}

		private void ShowOrHideNarratorCountWarnings()
		{
			var cast = m_viewModel.GetCastSizeRowValues(m_viewModel.CastSizeOption);
			m_tblNarratorWarning.Visible = ((m_maleNarrators.Value > cast.Male) || (m_femaleNarrators.Value > cast.Female));
			m_lblWarningSuboptimalNarratorsForNarrationByAuthor.Visible = m_viewModel.NarratorOption == NarratorsOption.NarrationByAuthor &&
				m_maleNarrators.Value < m_viewModel.Project.DefaultNarratorCountForNarrationByAuthor;
			if (m_lblWarningSuboptimalNarratorsForNarrationByAuthor.Visible)
			{
				m_lblWarningSuboptimalNarratorsForNarrationByAuthor.Text = String.Format(m_fmtSuboptimalNarratorsMsg,
					m_viewModel.Project.DefaultNarratorCountForNarrationByAuthor);
			}
			UpdateButtonState();
		}

		private void m_btnGenerate_Click(object sender, EventArgs e)
		{
			// check for minimum number of voice actors
			var actorCount = m_viewModel.GetCastSizeRowValues(m_viewModel.CastSizeOption).Total;
			if (actorCount < m_viewModel.MinimumActorCount)
			{
				var msg = Localizer.GetString("DialogBoxes.CastSizePlanningDlg.CastTooSmallWarning",
					"Using a cast size smaller than {0} will probably introduce proximity issues. Do you want to continue and generate groups using just {1} voice actors?");

				if (MessageBox.Show(this, string.Format(msg, m_viewModel.MinimumActorCount, actorCount), ProductName, MessageBoxButtons.YesNo) == DialogResult.No)
					return;
			}

			m_viewModel.Save();
			DialogResult = DialogResult.OK;
		}

		private CastSizeOption CastSizeOption
		{
			set { m_castSizePlanningOptions.SelectedCastSizeRow = value; }
		}

		private void UpdateButtonState()
		{
			m_btnGenerate.Enabled = m_viewModel.GetCastSizeRowValues(m_viewModel.CastSizeOption).Total != 0 &&
				m_viewModel.MaleNarrators + m_viewModel.FemaleNarrators > 0 &&
				!m_tblNarratorWarning.Visible;
		}

		private void m_castSizePlanningOptions_CastSizeOptionChanged(object sender, CastSizeOptionChangedEventArgs e)
		{
			if (!m_loaded)
				return;

			m_viewModel.CastSizeOption = e.Row;
			UpdateButtonState();
			ShowOrHideNarratorCountWarnings();
		}

		private void m_castSizePlanningOptions_CastSizeCustomValueChanged(object sender, CastSizeValueChangedEventArgs e)
		{
			m_viewModel.SetCustomVoiceActorValues(e.RowValues);
			UpdateButtonState();
			ShowOrHideNarratorCountWarnings();
		}

		private void CastSizePlanningDlg_Shown(object sender, EventArgs e)
		{
			if (m_viewModel.VoiceActorCount > 1)
				ShowVoiceActorList(m_viewModel.CastSizeOption == CastSizeOption.MatchVoiceActorList);
		}
	}
}
