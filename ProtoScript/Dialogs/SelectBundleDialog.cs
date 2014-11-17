using System;
using System.IO;
using System.Windows.Forms;
using L10NSharp;
using ProtoScript.Properties;

namespace ProtoScript.Dialogs
{
	public class SelectBundleDialog : IDisposable
	{
		private const string kResourceBundleExtension = ".zip";
		private const string kResourceBundleDescription = "Text Resource Bundle files";
		private readonly OpenFileDialog m_fileDialog;

		public SelectBundleDialog()
		{
			var defaultDir = Settings.Default.DefaultBundleDirectory;
			if (string.IsNullOrEmpty(defaultDir))
			{
				defaultDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}
			m_fileDialog = new OpenFileDialog
			{
				Title = LocalizationManager.GetString("SelectBundleDialog.Title", "Select a Text Resource Bundle file"),
				InitialDirectory = defaultDir,
				Filter = kResourceBundleDescription + "|*" + kResourceBundleExtension
			};
		}

		public DialogResult ShowDialog()
		{
			var result = m_fileDialog.ShowDialog();
			if (result == DialogResult.OK)
			{
				FileName = m_fileDialog.FileName;
				var dir = Path.GetDirectoryName(FileName);
				if (!string.IsNullOrEmpty(dir))
					Settings.Default.DefaultBundleDirectory = dir;
			}
			return result;
		}

		public string FileName { get; private set; }

		public void Dispose()
		{
			m_fileDialog.Dispose();
		}
	}
}
