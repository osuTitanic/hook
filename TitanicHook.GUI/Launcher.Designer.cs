using System.Drawing;
using System.Windows.Forms;

namespace TitanicHook.GUI
{
    partial class Launcher
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.changelogBrowser = new System.Windows.Forms.WebBrowser();
            this.updateProgress = new System.Windows.Forms.ProgressBar();
            this.buttonStart = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.statusText = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.autoStart = new System.Windows.Forms.CheckBox();
            this.fallbackToggle = new System.Windows.Forms.CheckBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSelect = new System.Windows.Forms.TabPage();
            this.releaseSplit = new System.Windows.Forms.SplitContainer();
            this.releases = new System.Windows.Forms.TreeView();
            this.details = new System.Windows.Forms.Panel();
            this.content = new System.Windows.Forms.Panel();
            this.preview = new System.Windows.Forms.Panel();
            this.previewText = new System.Windows.Forms.Label();
            this.previewImage = new System.Windows.Forms.PictureBox();
            this.versionLabel = new System.Windows.Forms.Label();
            this.releasedLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.tabChangelog = new System.Windows.Forms.TabPage();
            this.tabExtra = new System.Windows.Forms.TabPage();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.extraCheckboxes = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabSelect.SuspendLayout();
            this.releaseSplit.Panel1.SuspendLayout();
            this.releaseSplit.Panel2.SuspendLayout();
            this.releaseSplit.SuspendLayout();
            this.details.SuspendLayout();
            this.content.SuspendLayout();
            this.preview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).BeginInit();
            this.tabChangelog.SuspendLayout();
            this.tabExtra.SuspendLayout();
            this.SuspendLayout();
            // 
            // changelogBrowser
            // 
            this.changelogBrowser.AllowWebBrowserDrop = false;
            this.changelogBrowser.IsWebBrowserContextMenuEnabled = false;
            this.changelogBrowser.Location = new System.Drawing.Point(0, 0);
            this.changelogBrowser.Margin = new System.Windows.Forms.Padding(0);
            this.changelogBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.changelogBrowser.Name = "changelogBrowser";
            this.changelogBrowser.ScriptErrorsSuppressed = true;
            this.changelogBrowser.Size = new System.Drawing.Size(588, 398);
            this.changelogBrowser.TabIndex = 4;
            this.changelogBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // updateProgress
            // 
            this.updateProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.updateProgress.Location = new System.Drawing.Point(0, 0);
            this.updateProgress.MarqueeAnimationSpeed = 1;
            this.updateProgress.Name = "updateProgress";
            this.updateProgress.Size = new System.Drawing.Size(599, 34);
            this.updateProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.updateProgress.TabIndex = 1;
            // 
            // buttonStart
            // 
            this.buttonStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStart.DialogResult = System.Windows.Forms.DialogResult.None;
            this.buttonStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStart.Location = new System.Drawing.Point(0, 0);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(599, 34);
            this.buttonStart.TabIndex = 2;
            this.buttonStart.Text = "start osu!";
            this.buttonStart.UseVisualStyleBackColor = false;
            this.buttonStart.Visible = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.statusText);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 471);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(599, 47);
            this.panel1.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 13);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(599, 34);
            this.panel3.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.buttonStart);
            this.panel4.Controls.Add(this.updateProgress);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(599, 34);
            this.panel4.TabIndex = 4;
            // 
            // statusText
            // 
            this.statusText.BackColor = System.Drawing.SystemColors.Control;
            this.statusText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.statusText.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.statusText.Dock = System.Windows.Forms.DockStyle.Top;
            this.statusText.Enabled = false;
            this.statusText.Location = new System.Drawing.Point(0, 0);
            this.statusText.Name = "statusText";
            this.statusText.ReadOnly = true;
            this.statusText.Size = new System.Drawing.Size(599, 13);
            this.statusText.TabIndex = 0;
            this.statusText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.statusText.WordWrap = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.tabControl);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(599, 471);
            this.panel2.TabIndex = 5;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.autoStart);
            this.panel5.Controls.Add(this.fallbackToggle);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 422);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(599, 49);
            this.panel5.TabIndex = 10;
            // 
            // autoStart
            // 
            this.autoStart.Location = new System.Drawing.Point(12, 16);
            this.autoStart.Name = "autoStart";
            this.autoStart.Size = new System.Drawing.Size(243, 19);
            this.autoStart.TabIndex = 7;
            this.autoStart.Text = "Automatically start osu! after updating";
            this.autoStart.UseVisualStyleBackColor = true;
            // 
            // fallbackToggle
            // 
            this.fallbackToggle.Checked = true;
            this.fallbackToggle.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fallbackToggle.Location = new System.Drawing.Point(328, 16);
            this.fallbackToggle.Name = "fallbackToggle";
            this.fallbackToggle.Size = new System.Drawing.Size(154, 19);
            this.fallbackToggle.TabIndex = 9;
            this.fallbackToggle.Text = "Use fallback";
            this.fallbackToggle.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabSelect);
            this.tabControl.Controls.Add(this.tabChangelog);
            this.tabControl.Controls.Add(this.tabExtra);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(599, 424);
            this.tabControl.TabIndex = 8;
            // 
            // tabSelect
            // 
            this.tabSelect.Controls.Add(this.releaseSplit);
            this.tabSelect.Location = new System.Drawing.Point(4, 22);
            this.tabSelect.Name = "tabSelect";
            this.tabSelect.Size = new System.Drawing.Size(591, 398);
            this.tabSelect.TabIndex = 0;
            this.tabSelect.Text = "Select";
            this.tabSelect.UseVisualStyleBackColor = true;
            // 
            // releaseSplit
            // 
            this.releaseSplit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.releaseSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.releaseSplit.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.releaseSplit.Location = new System.Drawing.Point(0, 0);
            this.releaseSplit.Name = "releaseSplit";
            // 
            // releaseSplit.Panel1
            // 
            this.releaseSplit.Panel1.Controls.Add(this.releases);
            this.releaseSplit.Panel1MinSize = 120;
            // 
            // releaseSplit.Panel2
            // 
            this.releaseSplit.Panel2.Controls.Add(this.details);
            this.releaseSplit.Size = new System.Drawing.Size(591, 398);
            this.releaseSplit.SplitterDistance = 145;
            this.releaseSplit.TabIndex = 0;
            // 
            // releases
            // 
            this.releases.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.releases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.releases.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.releases.FullRowSelect = true;
            this.releases.HideSelection = false;
            this.releases.ItemHeight = 25;
            this.releases.Location = new System.Drawing.Point(0, 0);
            this.releases.Name = "releases";
            this.releases.ShowLines = false;
            this.releases.ShowPlusMinus = false;
            this.releases.ShowRootLines = false;
            this.releases.Size = new System.Drawing.Size(143, 396);
            this.releases.TabIndex = 0;
            this.releases.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnReleaseSelected);
            // 
            // details
            // 
            this.details.AutoScroll = true;
            this.details.BackColor = System.Drawing.Color.White;
            this.details.Controls.Add(this.content);
            this.details.Dock = System.Windows.Forms.DockStyle.Fill;
            this.details.Location = new System.Drawing.Point(0, 0);
            this.details.Name = "details";
            this.details.Padding = new System.Windows.Forms.Padding(10);
            this.details.Size = new System.Drawing.Size(440, 396);
            this.details.TabIndex = 0;
            // 
            // content
            // 
            this.content.Controls.Add(this.preview);
            this.content.Controls.Add(this.versionLabel);
            this.content.Controls.Add(this.releasedLabel);
            this.content.Controls.Add(this.descriptionLabel);
            this.content.Controls.Add(this.selectButton);
            this.content.Dock = System.Windows.Forms.DockStyle.Top;
            this.content.Location = new System.Drawing.Point(10, 10);
            this.content.Name = "content";
            this.content.Size = new System.Drawing.Size(420, 378);
            this.content.TabIndex = 0;
            // 
            // preview
            // 
            this.preview.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.preview.Controls.Add(this.previewText);
            this.preview.Controls.Add(this.previewImage);
            this.preview.Location = new System.Drawing.Point(0, 0);
            this.preview.Name = "preview";
            this.preview.Size = new System.Drawing.Size(420, 230);
            this.preview.TabIndex = 0;
            // 
            // previewText
            // 
            this.previewText.BackColor = System.Drawing.Color.White;
            this.previewText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewText.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previewText.ForeColor = System.Drawing.Color.Black;
            this.previewText.Location = new System.Drawing.Point(0, 0);
            this.previewText.Name = "previewText";
            this.previewText.Size = new System.Drawing.Size(420, 230);
            this.previewText.TabIndex = 1;
            this.previewText.Text = "Loading preview ...";
            this.previewText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // previewImage
            // 
            this.previewImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewImage.Location = new System.Drawing.Point(0, 0);
            this.previewImage.Name = "previewImage";
            this.previewImage.Size = new System.Drawing.Size(420, 230);
            this.previewImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.previewImage.TabIndex = 0;
            this.previewImage.TabStop = false;
            this.previewImage.Visible = false;
            // 
            // versionLabel
            // 
            this.versionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.versionLabel.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionLabel.Location = new System.Drawing.Point(3, 231);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(210, 38);
            this.versionLabel.TabIndex = 1;
            this.versionLabel.Text = "Loading";
            this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // releasedLabel
            // 
            this.releasedLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.releasedLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.releasedLabel.Location = new System.Drawing.Point(219, 231);
            this.releasedLabel.Name = "releasedLabel";
            this.releasedLabel.Size = new System.Drawing.Size(200, 39);
            this.releasedLabel.TabIndex = 2;
            this.releasedLabel.Text = "...";
            this.releasedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLabel.Location = new System.Drawing.Point(3, 270);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(414, 73);
            this.descriptionLabel.TabIndex = 3;
            this.descriptionLabel.Text = "Loading releases ...";
            // 
            // selectButton
            // 
            this.selectButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectButton.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectButton.Location = new System.Drawing.Point(3, 346);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(414, 32);
            this.selectButton.TabIndex = 4;
            this.selectButton.Text = "select";
            this.selectButton.UseVisualStyleBackColor = true;
            // 
            // tabPage1
            // 
            this.tabChangelog.Controls.Add(this.changelogBrowser);
            this.tabChangelog.Location = new System.Drawing.Point(4, 22);
            this.tabChangelog.Name = "tabPage1";
            this.tabChangelog.Padding = new System.Windows.Forms.Padding(3);
            this.tabChangelog.Size = new System.Drawing.Size(591, 398);
            this.tabChangelog.TabIndex = 1;
            this.tabChangelog.Text = "Changelog";
            this.tabChangelog.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabExtra.BackColor = System.Drawing.SystemColors.Control;
            this.tabExtra.Controls.Add(this.textBox2);
            this.tabExtra.Controls.Add(this.extraCheckboxes);
            this.tabExtra.Location = new System.Drawing.Point(4, 22);
            this.tabExtra.Name = "tabPage2";
            this.tabExtra.Padding = new System.Windows.Forms.Padding(3);
            this.tabExtra.Size = new System.Drawing.Size(591, 398);
            this.tabExtra.TabIndex = 2;
            this.tabExtra.Text = "Extras";
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Control;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox2.Enabled = false;
            this.textBox2.Location = new System.Drawing.Point(3, 3);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(585, 21);
            this.textBox2.TabIndex = 6;
            this.textBox2.Text = "Please select any optional packages you would like to install.";
            // 
            // extraCheckboxes
            // 
            this.extraCheckboxes.CheckOnClick = true;
            this.extraCheckboxes.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.extraCheckboxes.FormattingEnabled = true;
            this.extraCheckboxes.IntegralHeight = false;
            this.extraCheckboxes.Location = new System.Drawing.Point(3, 27);
            this.extraCheckboxes.Margin = new System.Windows.Forms.Padding(0);
            this.extraCheckboxes.Name = "extraCheckboxes";
            this.extraCheckboxes.Size = new System.Drawing.Size(585, 368);
            this.extraCheckboxes.TabIndex = 5;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.ClientSize = new System.Drawing.Size(599, 518);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Launcher";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Titanic!";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabSelect.ResumeLayout(false);
            this.releaseSplit.Panel1.ResumeLayout(false);
            this.releaseSplit.Panel2.ResumeLayout(false);
            this.releaseSplit.ResumeLayout(false);
            this.details.ResumeLayout(false);
            this.content.ResumeLayout(false);
            this.preview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.previewImage)).EndInit();
            this.tabChangelog.ResumeLayout(false);
            this.tabExtra.ResumeLayout(false);
            this.tabExtra.PerformLayout();
            this.ResumeLayout(false);

        }

#endregion
		
        private System.Windows.Forms.ProgressBar updateProgress;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.WebBrowser changelogBrowser;
        private System.Windows.Forms.CheckedListBox extraCheckboxes;
        private System.Windows.Forms.TextBox statusText;
        private System.Windows.Forms.CheckBox autoStart;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSelect;
        private System.Windows.Forms.TabPage tabChangelog;
        private System.Windows.Forms.TabPage tabExtra;
        private System.Windows.Forms.SplitContainer releaseSplit;
        private System.Windows.Forms.TreeView releases;
        private System.Windows.Forms.Panel details;
        private System.Windows.Forms.Panel content;
        private System.Windows.Forms.Panel preview;
        private System.Windows.Forms.PictureBox previewImage;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label releasedLabel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox fallbackToggle;
        private Label previewText;
    }
}
