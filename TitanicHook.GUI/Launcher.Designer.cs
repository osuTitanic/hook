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
            this.testBuild = new System.Windows.Forms.CheckBox();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.extraCheckboxes = new System.Windows.Forms.CheckedListBox();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
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
            this.changelogBrowser.Size = new System.Drawing.Size(485, 284);
            this.changelogBrowser.TabIndex = 4;
            this.changelogBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // updateProgress
            // 
            this.updateProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.updateProgress.Location = new System.Drawing.Point(0, 0);
            this.updateProgress.MarqueeAnimationSpeed = 1;
            this.updateProgress.Name = "updateProgress";
            this.updateProgress.Size = new System.Drawing.Size(494, 34);
            this.updateProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.updateProgress.TabIndex = 1;
            // 
            // buttonStart
            // 
            this.buttonStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonStart.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonStart.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonStart.Location = new System.Drawing.Point(0, 0);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(494, 34);
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
            this.panel1.Location = new System.Drawing.Point(0, 365);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(494, 47);
            this.panel1.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 13);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(494, 34);
            this.panel3.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.buttonStart);
            this.panel4.Controls.Add(this.updateProgress);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(494, 34);
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
            this.statusText.Size = new System.Drawing.Size(494, 13);
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
            this.panel2.Size = new System.Drawing.Size(494, 365);
            this.panel2.TabIndex = 5;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.autoStart);
            this.panel5.Controls.Add(this.testBuild);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 316);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(494, 49);
            this.panel5.TabIndex = 10;
            // 
            // autoStart
            // 
            this.autoStart.Location = new System.Drawing.Point(12, 14);
            this.autoStart.Name = "autoStart";
            this.autoStart.Size = new System.Drawing.Size(243, 19);
            this.autoStart.TabIndex = 7;
            this.autoStart.Text = "Automatically start osu! after updating";
            this.autoStart.UseVisualStyleBackColor = true;
            // 
            // testBuild
            // 
            this.testBuild.Checked = true;
            this.testBuild.CheckState = System.Windows.Forms.CheckState.Checked;
            this.testBuild.Location = new System.Drawing.Point(328, 14);
            this.testBuild.Name = "testBuild";
            this.testBuild.Size = new System.Drawing.Size(154, 19);
            this.testBuild.TabIndex = 9;
            this.testBuild.Text = "Use fallback";
            this.testBuild.UseVisualStyleBackColor = true;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(494, 310);
            this.tabControl.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.changelogBrowser);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(486, 284);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Changelog";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.textBox2);
            this.tabPage2.Controls.Add(this.extraCheckboxes);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(486, 284);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Extras";
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
            this.textBox2.Size = new System.Drawing.Size(480, 21);
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
            this.extraCheckboxes.Size = new System.Drawing.Size(480, 254);
            this.extraCheckboxes.TabIndex = 5;
            // 
            // Launcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.CancelButton = this.buttonStart;
            this.ClientSize = new System.Drawing.Size(494, 412);
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
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
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
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.CheckBox testBuild;
    }
}
