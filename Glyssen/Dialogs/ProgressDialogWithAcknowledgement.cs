//Copied and modified from SIL.Windows.Forms.Progress.ProgressDialog

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using L10NSharp;
using SIL.Progress;
using SIL.Reporting;

namespace Glyssen.Dialogs
{
	/// <summary>
	/// Provides a progress dialog which forces the user to acknowledge is complete by clicking OK
	/// </summary>
	public class ProgressDialogWithAcknowledgement : Form
	{
		public delegate void ProgressCallback(int progress);

		private Label m_statusLabel;
		private ProgressBar m_progressBar;
		private Label m_progressLabel;
		private Timer m_showWindowIfTakingLongTimeTimer;
		private Timer m_progressTimer;
		private bool m_isClosing;
		private Label m_overviewLabel;
		private DateTime m_startTime;
		private BackgroundWorker m_backgroundWorker;
		private ProgressState m_progressState;
		private TableLayoutPanel m_tableLayout;
		private bool m_workerStarted;
		private Button m_okButton;
		private IContainer components;
		private Button m_cancelButton;
		private TableLayoutPanel m_buttonPanel;
		private LinkLabel m_cancelLink;
		private bool m_appUsingWaitCursor;
		private Utilities.GlyssenColorPalette m_glyssenColorPalette;
		private bool m_replaceCancelButtonWithLink;

		/// <summary>
		/// Standard constructor
		/// </summary>
		public ProgressDialogWithAcknowledgement()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			m_statusLabel.BackColor = Color.Transparent;
			m_progressLabel.BackColor = Color.Transparent;
			m_overviewLabel.BackColor = Color.Transparent;
			m_startTime = default(DateTime);
			Text = UsageReporter.AppNameToUseInDialogs;

			m_statusLabel.Font = SystemFonts.MessageBoxFont;
			m_progressLabel.Font = SystemFonts.MessageBoxFont;
			m_overviewLabel.Font = SystemFonts.MessageBoxFont;

			m_statusLabel.Text = string.Empty;
			m_progressLabel.Text = string.Empty;
			m_overviewLabel.Text = string.Empty;

			m_cancelButton.MouseEnter += delegate
			{
				m_appUsingWaitCursor = Application.UseWaitCursor;
				m_cancelButton.Cursor = Cursor = Cursors.Arrow;
				Application.UseWaitCursor = false;
			};

			m_cancelButton.MouseLeave += delegate
			{
				Application.UseWaitCursor = m_appUsingWaitCursor;
			};
		}

		private void HandleTableLayoutSizeChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				CreateHandle();

			var desiredHeight = m_tableLayout.Height + Padding.Top + Padding.Bottom + (Height - ClientSize.Height);
			var scn = Screen.FromControl(this);
			Height = Math.Min(desiredHeight, scn.WorkingArea.Height - 20);
			AutoScroll = (desiredHeight > scn.WorkingArea.Height - 20);
		}

		/// <summary>
		/// Get / set the text to display in the first status panel
		/// </summary>
		public string StatusText
		{
			get
			{
				return m_statusLabel.Text;
			}
			set
			{
				m_statusLabel.Text = value;
			}
		}

		/// <summary>
		/// Description of why this dialog is even showing
		/// </summary>
		public string Overview
		{
			get
			{
				return m_overviewLabel.Text;
			}
			set
			{
				m_overviewLabel.Text = value;
			}
		}

		/// <summary>
		/// Get / set the minimum range of the progress bar
		/// </summary>
		public int ProgressRangeMinimum
		{
			get
			{
				return m_progressBar.Minimum;
			}
			set
			{
				if (m_backgroundWorker == null)
				{
					m_progressBar.Minimum = value;
				}
			}
		}

		/// <summary>
		/// Get / set the maximum range of the progress bar
		/// </summary>
		public int ProgressRangeMaximum
		{
			get
			{
				return m_progressBar.Maximum;
			}
			set
			{
				if (m_backgroundWorker != null)
				{
					return;
				}
				if (InvokeRequired)
				{
					Invoke(new ProgressCallback(SetMaximumCrossThread), value);
				}
				else
				{
					m_progressBar.Maximum = value;
				}
			}
		}

		private void SetMaximumCrossThread(int amount)
		{
			ProgressRangeMaximum = amount;
		}

		/// <summary>
		/// Get / set the current value of the progress bar
		/// </summary>
		public int Progress
		{
			get
			{
				return m_progressBar.Value;
			}
			set
			{
				/* these were causing weird, hard to debug (because of threads)
				 * failures. The debugger would report that value == max, so why fail?

				 * Debug.Assert(value <= _progressBar.Maximum);
				 */
				Debug.WriteLineIf(value >  m_progressBar.Maximum,
					"***Warning progress was " + value + " but max is " + m_progressBar.Maximum);
				Debug.Assert(value >= m_progressBar.Minimum);
				if (value > m_progressBar.Maximum)
				{
					m_progressBar.Maximum = value;//not worth crashing over in Release build
				}
				if (value < m_progressBar.Minimum)
				{
					return; //not worth crashing over in Release build
				}
				m_progressBar.Value = value;
			}
		}

		/// <summary>
		/// Get/set a boolean which determines whether the form
		/// will show a cancel option (true) or not (false)
		/// </summary>
		public bool CanCancel
		{
			get
			{
				return m_cancelButton.Enabled || m_cancelLink.Enabled;
			}
			set
			{
				if (ReplaceCancelButtonWithLink)
				{
					m_cancelLink.Enabled = value;
					m_cancelLink.Visible = value;
				}
				else
				{
					m_cancelButton.Enabled = value;
					m_cancelButton.Visible = value;
				}
			}
		}

		/// <summary>
		/// If this is set before showing, the dialog will run the worker and respond
		/// to its events
		/// </summary>
		public BackgroundWorker BackgroundWorker
		{
			get
			{
				return m_backgroundWorker;
			}
			set
			{
				m_backgroundWorker = value;
				m_progressBar.Minimum = 0;
				m_progressBar.Maximum = 100;
			}
		}

		public ProgressState ProgressStateResult
		{
			get
			{
				return m_progressState;
			}
		}

		/// <summary>
		/// Gets or sets the manner in which progress should be indicated on the progress bar.
		/// </summary>
		public ProgressBarStyle BarStyle { get { return m_progressBar.Style; } set { m_progressBar.Style = value; } }

		/// <summary>
		/// Optional; one will be created (of some class or subclass) if you don't set it.
		/// E.g. dlg.ProgressState = new BackgroundWorkerState(dlg.BackgroundWorker);
		/// Also, you can use the getter to gain access to the progressstate, in order to add arguments
		/// which the worker method can get at.
		/// </summary>
		public ProgressState ProgressState
		{
			get
			{
				if(m_progressState ==null)
				{
					if(m_backgroundWorker == null)
					{
						throw new ArgumentException("You must set BackgroundWorker before accessing this property.");
					}
					ProgressState  = new BackgroundWorkerState(m_backgroundWorker);
				}
				return m_progressState;
			}

			set
			{
				if (m_progressState!=null)
				{
					CancelRequested -= m_progressState.CancelRequested;
				}
				m_progressState = value;
				CancelRequested += m_progressState.CancelRequested;
				m_progressState.TotalNumberOfStepsChanged += OnTotalNumberOfStepsChanged;
			}
		}

		public string ProgressLabelTextWhenComplete { get; set; }

		public string OkButtonText { get; set; }

		public bool ReplaceCancelButtonWithLink
		{
			get { return m_replaceCancelButtonWithLink; }
			set
			{
				m_replaceCancelButtonWithLink = value;

				m_cancelLink.Enabled = CanCancel && value;
				m_cancelLink.Visible = CanCancel && value;
				m_cancelButton.Enabled = CanCancel && !value;
				m_cancelButton.Visible = CanCancel && !value;
			}
		}

		protected virtual void OnBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if(e.Cancelled || (ProgressStateResult != null && ProgressStateResult.Cancel))
			{
				DialogResult = DialogResult.Cancel;
			}
			else if (ProgressStateResult != null && (ProgressStateResult.State == ProgressState.StateValue.StoppedWithError
													 || ProgressStateResult.ExceptionThatWasEncountered != null))
			{
				//this dialog really can't know whether this was an unexpected exception or not
				//so don't do this:  Reporting.ErrorReporter.ReportException(ProgressStateResult.ExceptionThatWasEncountered, this, false);
				DialogResult = DialogResult.Abort;//not really matching semantics
			   // _progressState.State = ProgressState.StateValue.StoppedWithError;
			}
			else
			{
				DialogResult = DialogResult.None;
				m_progressBar.Maximum = 1;
				m_progressBar.Value = 1;
				m_progressBar.Style = ProgressBarStyle.Blocks;

				m_progressLabel.Text = ProgressLabelTextWhenComplete;

				AcceptButton = m_okButton;

				m_okButton.Text = OkButtonText ?? LocalizationManager.GetString("Common.OK", "OK");
				m_okButton.DialogResult = DialogResult.OK;
				m_okButton.Enabled = true;
				m_okButton.Visible = true;
			}
		}

		void OnBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			ProgressState state = e.UserState as ProgressState;
			if (state != null)
			{
				StatusText = state.StatusLabel;
			}

			if (state == null
				|| state is BackgroundWorkerState)
			{
				Progress = e.ProgressPercentage;
			}
			else
			{
				ProgressRangeMaximum = state.TotalNumberOfSteps;
				Progress = state.NumberOfStepsCompleted;
			}
		}

		/// <summary>
		/// Raised when the cancel button is clicked
		/// </summary>
		public event EventHandler CancelRequested;

		/// <summary>
		/// Raises the cancelled event
		/// </summary>
		/// <param name="e">Event data</param>
		protected virtual void OnCancelled( EventArgs e )
		{
			EventHandler cancelled = CancelRequested;
			if( cancelled != null )
			{
				cancelled( this, e );
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (m_showWindowIfTakingLongTimeTimer != null)
				{
					m_showWindowIfTakingLongTimeTimer.Stop();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.m_statusLabel = new System.Windows.Forms.Label();
			this.m_progressBar = new System.Windows.Forms.ProgressBar();
			this.m_progressLabel = new System.Windows.Forms.Label();
			this.m_showWindowIfTakingLongTimeTimer = new System.Windows.Forms.Timer(this.components);
			this.m_progressTimer = new System.Windows.Forms.Timer(this.components);
			this.m_overviewLabel = new System.Windows.Forms.Label();
			this.m_tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this.m_buttonPanel = new System.Windows.Forms.TableLayoutPanel();
			this.m_okButton = new System.Windows.Forms.Button();
			this.m_cancelButton = new System.Windows.Forms.Button();
			this.m_cancelLink = new System.Windows.Forms.LinkLabel();
			this.m_glyssenColorPalette = new Glyssen.Utilities.GlyssenColorPalette();
			this.m_tableLayout.SuspendLayout();
			this.m_buttonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_glyssenColorPalette)).BeginInit();
			this.SuspendLayout();
			// 
			// m_statusLabel
			// 
			this.m_statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_statusLabel.AutoSize = true;
			this.m_statusLabel.BackColor = System.Drawing.SystemColors.Control;
			this.m_glyssenColorPalette.SetBackColor(this.m_statusLabel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayout.SetColumnSpan(this.m_statusLabel, 2);
			this.m_statusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_statusLabel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_glyssenColorPalette.SetForeColor(this.m_statusLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_statusLabel.Location = new System.Drawing.Point(0, 35);
			this.m_statusLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 5);
			this.m_statusLabel.Name = "m_statusLabel";
			this.m_statusLabel.Size = new System.Drawing.Size(444, 15);
			this.m_statusLabel.TabIndex = 12;
			this.m_statusLabel.Text = "#";
			this.m_statusLabel.UseMnemonic = false;
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_statusLabel, true);
			// 
			// m_progressBar
			// 
			this.m_progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_glyssenColorPalette.SetBackColor(this.m_progressBar, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayout.SetColumnSpan(this.m_progressBar, 2);
			this.m_glyssenColorPalette.SetForeColor(this.m_progressBar, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_progressBar.Location = new System.Drawing.Point(0, 55);
			this.m_progressBar.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
			this.m_progressBar.Name = "m_progressBar";
			this.m_progressBar.Size = new System.Drawing.Size(444, 18);
			this.m_progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.m_progressBar.TabIndex = 11;
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_progressBar, false);
			this.m_progressBar.Value = 1;
			// 
			// m_progressLabel
			// 
			this.m_progressLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_progressLabel.AutoEllipsis = true;
			this.m_progressLabel.AutoSize = true;
			this.m_glyssenColorPalette.SetBackColor(this.m_progressLabel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_progressLabel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_glyssenColorPalette.SetForeColor(this.m_progressLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_progressLabel.Location = new System.Drawing.Point(0, 90);
			this.m_progressLabel.Margin = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.m_progressLabel.Name = "m_progressLabel";
			this.m_progressLabel.Size = new System.Drawing.Size(314, 13);
			this.m_progressLabel.TabIndex = 9;
			this.m_progressLabel.Text = "#";
			this.m_progressLabel.UseMnemonic = false;
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_progressLabel, true);
			// 
			// m_showWindowIfTakingLongTimeTimer
			// 
			this.m_showWindowIfTakingLongTimeTimer.Interval = 2000;
			this.m_showWindowIfTakingLongTimeTimer.Tick += new System.EventHandler(this.OnTakingLongTimeTimerClick);
			// 
			// m_progressTimer
			// 
			this.m_progressTimer.Enabled = true;
			this.m_progressTimer.Interval = 1000;
			this.m_progressTimer.Tick += new System.EventHandler(this.progressTimer_Tick);
			// 
			// m_overviewLabel
			// 
			this.m_overviewLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.m_overviewLabel.AutoSize = true;
			this.m_glyssenColorPalette.SetBackColor(this.m_overviewLabel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayout.SetColumnSpan(this.m_overviewLabel, 2);
			this.m_overviewLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.m_overviewLabel.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_glyssenColorPalette.SetForeColor(this.m_overviewLabel, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_overviewLabel.Location = new System.Drawing.Point(0, 0);
			this.m_overviewLabel.Margin = new System.Windows.Forms.Padding(0, 0, 0, 20);
			this.m_overviewLabel.Name = "m_overviewLabel";
			this.m_overviewLabel.Size = new System.Drawing.Size(444, 15);
			this.m_overviewLabel.TabIndex = 8;
			this.m_overviewLabel.Text = "#";
			this.m_overviewLabel.UseMnemonic = false;
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_overviewLabel, true);
			// 
			// m_tableLayout
			// 
			this.m_tableLayout.AutoSize = true;
			this.m_tableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_tableLayout.BackColor = System.Drawing.Color.Transparent;
			this.m_glyssenColorPalette.SetBackColor(this.m_tableLayout, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_tableLayout.ColumnCount = 2;
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.m_tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_tableLayout.Controls.Add(this.m_buttonPanel, 1, 3);
			this.m_tableLayout.Controls.Add(this.m_overviewLabel, 0, 0);
			this.m_tableLayout.Controls.Add(this.m_progressLabel, 0, 3);
			this.m_tableLayout.Controls.Add(this.m_progressBar, 0, 2);
			this.m_tableLayout.Controls.Add(this.m_statusLabel, 0, 1);
			this.m_tableLayout.Controls.Add(this.m_cancelLink, 0, 4);
			this.m_tableLayout.Dock = System.Windows.Forms.DockStyle.Top;
			this.m_glyssenColorPalette.SetForeColor(this.m_tableLayout, Glyssen.Utilities.GlyssenColors.Default);
			this.m_tableLayout.Location = new System.Drawing.Point(12, 12);
			this.m_tableLayout.Name = "m_tableLayout";
			this.m_tableLayout.RowCount = 5;
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_tableLayout.Size = new System.Drawing.Size(444, 121);
			this.m_tableLayout.TabIndex = 13;
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_tableLayout, false);
			this.m_tableLayout.SizeChanged += new System.EventHandler(this.HandleTableLayoutSizeChanged);
			// 
			// m_buttonPanel
			// 
			this.m_buttonPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.m_buttonPanel.AutoSize = true;
			this.m_buttonPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.m_glyssenColorPalette.SetBackColor(this.m_buttonPanel, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_buttonPanel.ColumnCount = 2;
			this.m_buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_buttonPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.m_buttonPanel.Controls.Add(this.m_okButton, 0, 0);
			this.m_buttonPanel.Controls.Add(this.m_cancelButton, 1, 0);
			this.m_glyssenColorPalette.SetForeColor(this.m_buttonPanel, Glyssen.Utilities.GlyssenColors.Default);
			this.m_buttonPanel.Location = new System.Drawing.Point(317, 95);
			this.m_buttonPanel.Name = "m_buttonPanel";
			this.m_buttonPanel.RowCount = 1;
			this.m_tableLayout.SetRowSpan(this.m_buttonPanel, 2);
			this.m_buttonPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.m_buttonPanel.Size = new System.Drawing.Size(124, 23);
			this.m_buttonPanel.TabIndex = 14;
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_buttonPanel, false);
			// 
			// m_okButton
			// 
			this.m_okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_okButton.AutoSize = true;
			this.m_glyssenColorPalette.SetBackColor(this.m_okButton, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_okButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_glyssenColorPalette.SetForeColor(this.m_okButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_okButton.Location = new System.Drawing.Point(8, 0);
			this.m_okButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.m_okButton.Name = "m_okButton";
			this.m_okButton.Size = new System.Drawing.Size(50, 23);
			this.m_okButton.TabIndex = 13;
			this.m_okButton.Text = "&OK";
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_okButton, false);
			this.m_okButton.Visible = false;
			// 
			// m_cancelButton
			// 
			this.m_cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.m_cancelButton.AutoSize = true;
			this.m_glyssenColorPalette.SetBackColor(this.m_cancelButton, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.m_glyssenColorPalette.SetFlatAppearanceBorderColor(this.m_cancelButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_glyssenColorPalette.SetForeColor(this.m_cancelButton, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_cancelButton.Location = new System.Drawing.Point(66, 0);
			this.m_cancelButton.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.m_cancelButton.Name = "m_cancelButton";
			this.m_cancelButton.Size = new System.Drawing.Size(58, 23);
			this.m_cancelButton.TabIndex = 10;
			this.m_cancelButton.Text = "&Cancel";
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_cancelButton, false);
			this.m_cancelButton.Click += new System.EventHandler(this.OnCancelButton_Click);
			// 
			// m_cancelLink
			// 
			this.m_cancelLink.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_glyssenColorPalette.SetActiveLinkColor(this.m_cancelLink, Glyssen.Utilities.GlyssenColors.ActiveLinkColor);
			this.m_cancelLink.AutoSize = true;
			this.m_glyssenColorPalette.SetBackColor(this.m_cancelLink, Glyssen.Utilities.GlyssenColors.BackColor);
			this.m_cancelLink.BackColor = System.Drawing.SystemColors.Control;
			this.m_cancelLink.DisabledLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(133)))), ((int)(((byte)(133)))), ((int)(((byte)(133)))));
			this.m_glyssenColorPalette.SetDisabledLinkColor(this.m_cancelLink, Glyssen.Utilities.GlyssenColors.DisabledLinkColor);
			this.m_cancelLink.ForeColor = System.Drawing.SystemColors.WindowText;
			this.m_glyssenColorPalette.SetForeColor(this.m_cancelLink, Glyssen.Utilities.GlyssenColors.ForeColor);
			this.m_cancelLink.LinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_glyssenColorPalette.SetLinkColor(this.m_cancelLink, Glyssen.Utilities.GlyssenColors.LinkColor);
			this.m_cancelLink.Location = new System.Drawing.Point(25, 103);
			this.m_cancelLink.Margin = new System.Windows.Forms.Padding(25, 0, 3, 0);
			this.m_cancelLink.Name = "m_cancelLink";
			this.m_cancelLink.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
			this.m_cancelLink.Size = new System.Drawing.Size(209, 18);
			this.m_cancelLink.TabIndex = 15;
			this.m_cancelLink.TabStop = true;
			this.m_cancelLink.Text = "No! Let me group character roles manually.";
			this.m_glyssenColorPalette.SetUsePaletteColors(this.m_cancelLink, true);
			this.m_cancelLink.Visible = false;
			this.m_cancelLink.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
			this.m_glyssenColorPalette.SetVisitedLinkColor(this.m_cancelLink, Glyssen.Utilities.GlyssenColors.VisitedLinkColor);
			this.m_cancelLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnCancelLink_LinkClicked);
			// 
			// ProgressDialogWithAcknowledgement
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoSize = true;
			this.m_glyssenColorPalette.SetBackColor(this, Glyssen.Utilities.GlyssenColors.BackColor);
			this.ClientSize = new System.Drawing.Size(468, 139);
			this.ControlBox = false;
			this.Controls.Add(this.m_tableLayout);
			this.m_glyssenColorPalette.SetForeColor(this, Glyssen.Utilities.GlyssenColors.Default);
			this.ForeColor = System.Drawing.SystemColors.WindowText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressDialogWithAcknowledgement";
			this.Padding = new System.Windows.Forms.Padding(12);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Palaso";
			this.m_glyssenColorPalette.SetUsePaletteColors(this, true);
			this.Load += new System.EventHandler(this.ProgressDialog_Load);
			this.Shown += new System.EventHandler(this.ProgressDialog_Shown);
			this.m_tableLayout.ResumeLayout(false);
			this.m_tableLayout.PerformLayout();
			this.m_buttonPanel.ResumeLayout(false);
			this.m_buttonPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.m_glyssenColorPalette)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion


		private void OnTakingLongTimeTimerClick(object sender, EventArgs e)
		{
			// Show the window now the timer has elapsed, and stop the timer
			m_showWindowIfTakingLongTimeTimer.Stop();
			if (!Visible)
			{
				Show();
			}
		}

		private void OnCancelButton_Click(object sender, EventArgs e)
		{
			m_showWindowIfTakingLongTimeTimer.Stop();
			if(m_isClosing)
				return;

			//Debug.WriteLine("Dialog:OnCancelButton_Click");

			// Prevent further cancellation
			m_cancelButton.Enabled = false;
			m_progressTimer.Stop();
			m_progressLabel.Text =  LocalizationManager.GetString("DialogBoxes.ProgressDialogWithAcknowledgement.Canceling", "Canceling...");
			// Tell people we're canceling
			OnCancelled( e );
			if (m_backgroundWorker != null && m_backgroundWorker.WorkerSupportsCancellation)
			{
				m_backgroundWorker.CancelAsync();
			}
		}

		private void progressTimer_Tick(object sender, EventArgs e)
		{
			int range = m_progressBar.Maximum - m_progressBar.Minimum;
			if( range <= 0 )
			{
				return;
			}
			if( m_progressBar.Value <= 0 )
			{
				return;
			}
			if (m_startTime != default(DateTime))
			{
				TimeSpan elapsed = DateTime.Now - m_startTime;
				double estimatedSeconds = (elapsed.TotalSeconds * range) / m_progressBar.Value;
				TimeSpan estimatedToGo = new TimeSpan(0, 0, 0, (int)(estimatedSeconds - elapsed.TotalSeconds), 0);
				//_progressLabel.Text = String.Format(
				//    System.Globalization.CultureInfo.CurrentUICulture,
				//    "Elapsed: {0} Remaining: {1}",
				//    GetStringFor(elapsed),
				//    GetStringFor(estimatedToGo));
				m_progressLabel.Text = String.Format(
					CultureInfo.CurrentUICulture,
					"{0}",
					//GetStringFor(elapsed),
					GetStringFor(estimatedToGo));
			}
		}

		private static string GetStringFor( TimeSpan span )
		{
			if( span.TotalDays > 1 )
			{
				return string.Format(CultureInfo.CurrentUICulture, "{0} day {1} hour", span.Days, span.Hours);
			}
			else if( span.TotalHours > 1 )
			{
				return string.Format(CultureInfo.CurrentUICulture, "{0} hour {1} minutes", span.Hours, span.Minutes);
			}
			else if( span.TotalMinutes > 1 )
			{
				return string.Format(CultureInfo.CurrentUICulture, "{0} minutes {1} seconds", span.Minutes, span.Seconds);
			}
			return string.Format( CultureInfo.CurrentUICulture, "{0} seconds", span.Seconds );
		}

		public void OnNumberOfStepsCompletedChanged(object sender, EventArgs e)
		{
			Progress = ((ProgressState) sender).NumberOfStepsCompleted;
			//in case there is no event pump showing us (mono-threaded)
			progressTimer_Tick(this, null);
			Refresh();
		}

		public void OnTotalNumberOfStepsChanged(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new ProgressCallback(UpdateTotal), ((ProgressState)sender).TotalNumberOfSteps);
			}
			else
			{
				UpdateTotal(((ProgressState) sender).TotalNumberOfSteps);
			}
		}

		private void UpdateTotal(int steps)
		{
			m_startTime = DateTime.Now;
			ProgressRangeMaximum = steps;
			Refresh();
		}

		public void OnStatusLabelChanged(object sender, EventArgs e)
		{
			StatusText = ((ProgressState)sender).StatusLabel;
			Refresh();
		}

		private void OnStartWorker(object sender, EventArgs e)
		{
			m_workerStarted = true;
			//Debug.WriteLine("Dialog:StartWorker");

			if (m_backgroundWorker != null)
			{
				 //BW uses percentages (unless it's using our custom ProgressState in the UserState member)
				ProgressRangeMinimum = 0;
				ProgressRangeMaximum = 100;

				//if the actual task can't take cancelling, the caller of this should set CanCancel to false;
				m_backgroundWorker.WorkerSupportsCancellation = CanCancel;

				m_backgroundWorker.ProgressChanged += OnBackgroundWorker_ProgressChanged;
				m_backgroundWorker.RunWorkerCompleted += OnBackgroundWorker_RunWorkerCompleted;
				m_backgroundWorker.RunWorkerAsync(ProgressState);
			}
		}

		//This is here, in addition to the OnShown handler, because of a weird bug where a certain,
		//completely unrelated test (which doesn't use this class at all) can cause tests using this to
		//fail because the OnShown event is never fired.
		//I don't know why the orginal code we copied this from was using onshown instead of onload,
		//but it may have something to do with its "delay show" feature (which I couldn't get to work,
		//but which would be a terrific thing to have)
		private void ProgressDialog_Load(object sender, EventArgs e)
		{
			if(!m_workerStarted)
			{
				OnStartWorker(this, null);
			}
		}

		private void ProgressDialog_Shown(object sender, EventArgs e)
		{
			if(!m_workerStarted)
			{
				OnStartWorker(this, null);
			}
		}

		private void OnCancelLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			m_cancelButton.Enabled = true;
			m_cancelButton.Visible = true;
			m_cancelButton.PerformClick();
		}
	}
}
