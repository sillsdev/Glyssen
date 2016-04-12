using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Glyssen.Controls;
using Glyssen.Properties;
using Glyssen.Utilities;
using Glyssen.VoiceActor;
using L10NSharp;
using L10NSharp.UI;

namespace Glyssen.Dialogs
{
	public partial class CastSizePlanningDlg : Form
	{
		private readonly CastSizePlanningViewModel m_viewModel;
		private readonly bool m_loaded;
		private readonly string m_fmtProjectSummaryPlural;

		public CastSizePlanningDlg(CastSizePlanningViewModel viewModel)
		{
			InitializeComponent();
			Icon = Resources.glyssenIcon;
			m_viewModel = viewModel;
			m_castSizePlanningOptions.SetViewModel(m_viewModel);

			m_fmtProjectSummaryPlural = m_lblProjectSummary.Text;
			HandleStringsLocalized();
			LocalizeItemDlg.StringsLocalized += HandleStringsLocalized;

			m_tableLayoutStartingOver.Visible = m_viewModel.Project.CharacterGroupListPreviouslyGenerated;
			m_maleNarrators.Maximum = m_viewModel.MaximumNarratorsValue;
			m_femaleNarrators.Maximum = m_maleNarrators.Maximum;

			NarratorOption = m_viewModel.NarratorOption;
			CastSizeOption = m_viewModel.CastSizeOption;

			m_loaded = true;
		}

		private void HandleStringsLocalized()
		{
			var project = m_viewModel.Project;
			int includedBooksCount = project.IncludedBooks.Count;
			if (includedBooksCount == 1)
				m_lblProjectSummary.Text = string.Format(LocalizationManager.GetString("DialogBoxes.CastSizePlanning.ProjectSummary.Singular",
					"This project has 1 book with {0} distinct character roles."), project.GetKeyStrokesByCharacterId().Count);
			else
				m_lblProjectSummary.Text = string.Format(m_fmtProjectSummaryPlural, includedBooksCount,
					project.GetKeyStrokesByCharacterId().Count);
			m_lblRecordingTime.Text = string.Format(m_lblRecordingTime.Text, project.GetEstimatedRecordingTime());
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
			msg.AppendParagraph(LocalizationManager.GetString("DialogBoxes.CastSizePlanning.IsAbout.Line1",
				"Since there are usually a large number of brief character roles in a recording script, most voice actors will speak multiple roles."));
			msg.AppendParagraph(LocalizationManager.GetString("DialogBoxes.CastSizePlanning.IsAbout.Line2",
				"So how many voice actors or narrators should you have? That’s discussed in the <a href=\"#\" target=\"_blank\">Guide for the Recording Project Coordinator</a>."));
			msg.AppendParagraph(LocalizationManager.GetString("DialogBoxes.CastSizePlanning.IsAbout.Line3",
				"In the <b>Cast Size Planning</b> dialog, Glyssen provides recommendations for the size of your cast and for how to distribute the narration work. You can adjust these as needed."));
			msg.AppendParagraph(LocalizationManager.GetString("DialogBoxes.CastSizePlanning.IsAbout.Line4",
				"As a result of this planning, Glyssen will generate a list of groups called ‘Roles for Voice Actors’. This list will be useful to you for recruiting and/or assigning voice actors."));
			msg.AppendParagraph(LocalizationManager.GetString("DialogBoxes.CastSizePlanning.IsAbout.Line5",
				"Not sure how to plan? Try some selections and see how the groups work out. You are free to come back and change things."));

			var html = msg.ToString();

			using (var dlg = new HtmlMessageDlg(html))
			{
				dlg.ShowDialog(this);
			}
		}

		private void m_linkVloiceActorList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowVoiceActorList(true);
		}

		private void ShowVoiceActorList(bool keepSelection)
		{
			var actorInfoViewModel = new VoiceActorInformationViewModel(m_viewModel.Project);
			using (var actorDlg = new VoiceActorInformationDlg(actorInfoViewModel, false, false, !keepSelection))
			{
				if (actorDlg.ShowDialog(this) == DialogResult.Cancel)
					keepSelection = true; // Even though Cancel doesn't actually discard changes, it should at least reflect the user's desire not to have any changes result in a change to the c

				var male = actorInfoViewModel.Actors.Count(a => a.Gender == ActorGender.Male && a.Age != ActorAge.Child && !a.IsInactive);
				var female = actorInfoViewModel.Actors.Count(a => a.Gender == ActorGender.Female && a.Age != ActorAge.Child && !a.IsInactive);
				var child = actorInfoViewModel.Actors.Count(a => a.Age == ActorAge.Child && !a.IsInactive);

				m_viewModel.SetVoiceActorListValues(male, female, child, keepSelection);
			}
			m_castSizePlanningOptions.Refresh();
			UpdateButtonState();
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

		private void UpdateNarratorCounts()
		{
			var narratorCounts = m_viewModel.GetNarratorValues(NarratorOption);
			m_maleNarrators.Value = narratorCounts.Item1;
			m_femaleNarrators.Value = narratorCounts.Item2;
		}

		private void NarratorOptionChanged(object sender, EventArgs e)
		{
			UpdateNarratorCounts();
			m_viewModel.SetNarratorValues((int)m_maleNarrators.Value, (int)m_femaleNarrators.Value);
			m_viewModel.NarratorOption = NarratorOption;

			m_maleNarrators.Enabled = NarratorOption == NarratorsOption.Custom;
			m_femaleNarrators.Enabled = m_maleNarrators.Enabled;
		}

		private void Save()
		{
			m_viewModel.Save();
		}

		private void NarratorsValueChanged(object sender, EventArgs e)
		{
			if (!m_loaded) return;

			m_viewModel.SetNarratorValues((int)m_maleNarrators.Value, (int)m_femaleNarrators.Value);
		}

		private void m_btnGenerate_Click(object sender, EventArgs e)
		{
			// check for minimum number of voice actors
			var actorCount = m_viewModel.GetCastSizeRowValues(m_viewModel.CastSizeOption).Total;
			if (actorCount < m_viewModel.MinimumActorCount)
			{
				var msg = LocalizationManager.GetString("DialogBoxes.CastSizePlanning.CastTooSmallWarning",
					"Using a cast size smaller than {0} will probably introduce proximity issues. Do you want to continue and generate groups using just {1} voice actors?");

				if (MessageBox.Show(this, string.Format(msg, m_viewModel.MinimumActorCount, actorCount), ProductName, MessageBoxButtons.YesNo) == DialogResult.No)
					return;
			}

			Save();
			DialogResult = DialogResult.OK;
		}

		private CastSizeRow CastSizeOption
		{
			set { m_castSizePlanningOptions.SelectedCastSizeRow = value; }
		}

		private void UpdateButtonState()
		{
			m_btnGenerate.Enabled = m_viewModel.GetCastSizeRowValues(m_viewModel.CastSizeOption).Total != 0;
		}

		private void m_castSizePlanningOptions_CastSizeOptionChanged(object sender, CastSizeOptionChangedEventArgs e)
		{
			if (!m_loaded)
				return;

			m_viewModel.CastSizeOption = e.Row;
			UpdateButtonState();
		}

		private void m_castSizePlanningOptions_CastSizeCustomValueChanged(object sender, CastSizeValueChangedEventArgs e)
		{
			m_viewModel.SetCustomVoiceActorValues(e.Male, e.Female, e.Child);
			UpdateButtonState();
		}

		private void CastSizePlanningDlg_Shown(object sender, EventArgs e)
		{
			if (m_viewModel.VoiceActorCount > 1)
				ShowVoiceActorList(m_viewModel.CastSizeOption == CastSizeRow.MatchVoiceActorList);
		}
	}
}
