// ---------------------------------------------------------------------------------------------
#region // Copyright (c) 2018, SIL International.
// <copyright from='2018' to='2018' company='SIL International'>
//		Copyright (c) 2013, SIL International.   
//    
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright> 
#endregion
// 
// File: YesNoApplyToAllDlg.cs
// ---------------------------------------------------------------------------------------------
using System;
using System.Windows.Forms;
using Glyssen.Shared;

namespace Glyssen.Dialogs
{
	public partial class YesNoApplyToAllDlg : Form
	{
		public YesNoApplyToAllDlg(string message, string applyToAllText = null, string caption = null, bool applyToAllIsDefault = true, bool noIsDefault = false)
		{
			InitializeComponent();

			m_lblMessage.Text = message;
			if (applyToAllText != null)
				m_chkApplyToAll.Text = applyToAllText;
			Caption = caption;
			ApplyToAll = applyToAllIsDefault;
			if (noIsDefault)
				DefaultResult = DialogResult.No;
		}

		public DialogResult DefaultResult
		{
			get => (AcceptButton == m_btnNo) ? m_btnNo.DialogResult : m_btnYes.DialogResult;
			set
			{
				switch (value)
				{
					case DialogResult.No:
						AcceptButton = m_btnNo;
						break;
					case DialogResult.Yes:
						AcceptButton = m_btnYes;
						break;
					default:
						throw new ArgumentException("Yes and No are the only possible results.");
				}
			}
		}

		public string Caption
		{
			get => Text;
			set => Text = value ?? GlyssenInfo.kProduct;
		}

		public bool ApplyToAll
		{
			get => m_chkApplyToAll.Checked;
			set => m_chkApplyToAll.Checked = value;
		}
	}
}
