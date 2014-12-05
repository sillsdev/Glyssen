using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProtoScript.Dialogs
{
	public partial class AssignCharacterDialog : Form
	{
		private readonly IReadOnlyList<BookScript> m_books;
		private readonly string m_fontFamily;
		private readonly int m_fontSizeInPoints;
		private readonly BlockNavigator m_navigator;

		public AssignCharacterDialog()
		{
			InitializeComponent();
		}

		public AssignCharacterDialog(Project project) : this(project, null)
		{
		}

		public AssignCharacterDialog(Project project, Block initialBlock)
		{
			InitializeComponent();

			m_books = project.Books;
			m_fontFamily = project.FontFamily;
			m_fontSizeInPoints = project.FontSizeInPoints;

			m_navigator = new BlockNavigator(m_books);

			if (initialBlock != null)
				m_navigator.CurrentBlock = initialBlock;

			LoadBlock();
		}

		public void LoadBlock()
		{
			Block previousBlock = m_navigator.PeekPreviousBlock();
			Block nextBlock = m_navigator.PeekNextBlock();
			m_blocksDisplayBrowser.DisplayHtml(
				BuildHtml(
					previousBlock == null ? null : previousBlock.GetText(true),
					m_navigator.CurrentBlock.GetText(true),
					nextBlock == null ? null : nextBlock.GetText(true),
					BuildStyle()));
			UpdateButtons();
		}

		private string BuildHtml(string previousText, string mainText, string followingText, string style)
		{
			var sb = new StringBuilder();
			sb.Append("<html><head><meta charset=\"UTF-8\"><style>");
			sb.Append(style);
			sb.Append("</style></head><body>");
			if (!string.IsNullOrEmpty(previousText))
			{
				sb.Append(previousText);
				sb.Append("<br/><br/>");
			}
			sb.Append("<span class=\"highlight\">");
			sb.Append(mainText);
			sb.Append("</span>");
			if (!string.IsNullOrEmpty(followingText))
			{
				sb.Append("<br/><br/>");
				sb.Append(followingText);
			}
			sb.Append("</body></html>");
			return sb.ToString();
		}

		private string BuildStyle()
		{
			return "body{font-family:" + m_fontFamily + ";font-size:" + m_fontSizeInPoints + "pt}.highlight{background-color:yellow}";
		}

		private void UpdateButtons()
		{
			m_btnNext.Enabled = !m_navigator.IsLastBlock(m_navigator.CurrentBlock);
			m_btnPrevious.Enabled = !m_navigator.IsFirstBlock(m_navigator.CurrentBlock);
		}

		private void LoadNextBlock()
		{
			m_navigator.NextBlock();
			LoadBlock();
		}

		private void LoadPreviousBlock()
		{
			m_navigator.PreviousBlock();
			LoadBlock();
		}

		private void m_btnNext_Click(object sender, EventArgs e)
		{
			LoadNextBlock();
		}

		private void m_btnPrevious_Click(object sender, EventArgs e)
		{
			LoadPreviousBlock();
		}
	}
}
