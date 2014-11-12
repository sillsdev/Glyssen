using System;
using System.Windows.Forms;
using L10NSharp;

namespace ProtoScript.Dialogs
{
	public class SelectBundleDialog : IDisposable
	{
		private const string kResourceBundleExtension = ".zip";
		private const string kResourceBundleDescription = "Text Resource Bundle files";
		private readonly OpenFileDialog m_fileDialog;

		public SelectBundleDialog()
		{
			m_fileDialog = new OpenFileDialog
			{
				Title = LocalizationManager.GetString("SelectBundleDialog.Title", "Select a Text Resource Bundle file"),
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				Filter = kResourceBundleDescription + "|*" + kResourceBundleExtension
			};
		}

		public void ShowDialog()
		{
			if (m_fileDialog.ShowDialog() == DialogResult.OK)
				FileName = m_fileDialog.FileName;
		}

		public string FileName { get; private set; }

		public void Dispose()
		{
			m_fileDialog.Dispose();
		}
	}
}
