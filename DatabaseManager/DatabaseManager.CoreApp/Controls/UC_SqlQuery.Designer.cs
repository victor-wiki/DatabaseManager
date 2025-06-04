using DatabaseInterpreter.Model;

namespace DatabaseManager.Controls
{
    partial class UC_SqlQuery
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            queryEditor = new UC_QueryEditor();
            tabResult = new System.Windows.Forms.TabControl();
            tabPageData = new System.Windows.Forms.TabPage();
            resultGridView = new UC_QueryResultGrid();
            tabPageMessage = new System.Windows.Forms.TabPage();
            resultTextBox = new System.Windows.Forms.RichTextBox();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            tsslMessage = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabResult.SuspendLayout();
            tabPageData.SuspendLayout();
            tabPageMessage.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(queryEditor);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            splitContainer1.Panel2.Controls.Add(tabResult);
            splitContainer1.Size = new System.Drawing.Size(504, 592);
            splitContainer1.SplitterDistance = 362;
            splitContainer1.SplitterWidth = 3;
            splitContainer1.TabIndex = 1;
            // 
            // queryEditor
            // 
            queryEditor.BackColor = System.Drawing.SystemColors.Control;
            queryEditor.DatabaseType = DatabaseType.Unknown;
            queryEditor.DbInterpreter = null;
            queryEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            queryEditor.Location = new System.Drawing.Point(0, 0);
            queryEditor.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            queryEditor.Name = "queryEditor";
            queryEditor.Size = new System.Drawing.Size(504, 362);
            queryEditor.TabIndex = 0;
            queryEditor.Load += queryEditor_Load;
            // 
            // tabResult
            // 
            tabResult.Controls.Add(tabPageData);
            tabResult.Controls.Add(tabPageMessage);
            tabResult.Dock = System.Windows.Forms.DockStyle.Fill;
            tabResult.Location = new System.Drawing.Point(0, 0);
            tabResult.Margin = new System.Windows.Forms.Padding(4);
            tabResult.Name = "tabResult";
            tabResult.SelectedIndex = 0;
            tabResult.Size = new System.Drawing.Size(504, 227);
            tabResult.TabIndex = 0;
            // 
            // tabPageData
            // 
            tabPageData.BackColor = System.Drawing.SystemColors.Control;
            tabPageData.Controls.Add(resultGridView);
            tabPageData.Location = new System.Drawing.Point(4, 26);
            tabPageData.Margin = new System.Windows.Forms.Padding(0);
            tabPageData.Name = "tabPageData";
            tabPageData.Size = new System.Drawing.Size(496, 197);
            tabPageData.TabIndex = 1;
            tabPageData.Text = "Data";
            // 
            // resultGridView
            // 
            resultGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            resultGridView.Location = new System.Drawing.Point(0, 0);
            resultGridView.Margin = new System.Windows.Forms.Padding(0);
            resultGridView.Name = "resultGridView";
            resultGridView.Size = new System.Drawing.Size(496, 197);
            resultGridView.TabIndex = 0;
            // 
            // tabPageMessage
            // 
            tabPageMessage.BackColor = System.Drawing.SystemColors.Control;
            tabPageMessage.Controls.Add(resultTextBox);
            tabPageMessage.Location = new System.Drawing.Point(4, 26);
            tabPageMessage.Margin = new System.Windows.Forms.Padding(0);
            tabPageMessage.Name = "tabPageMessage";
            tabPageMessage.Size = new System.Drawing.Size(496, 197);
            tabPageMessage.TabIndex = 0;
            tabPageMessage.Text = "Message";
            // 
            // resultTextBox
            // 
            resultTextBox.BackColor = System.Drawing.Color.White;
            resultTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resultTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            resultTextBox.Location = new System.Drawing.Point(0, 0);
            resultTextBox.Margin = new System.Windows.Forms.Padding(0);
            resultTextBox.Name = "resultTextBox";
            resultTextBox.ReadOnly = true;
            resultTextBox.Size = new System.Drawing.Size(496, 197);
            resultTextBox.TabIndex = 0;
            resultTextBox.Text = "";
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsslStatus, tsslMessage });
            statusStrip1.Location = new System.Drawing.Point(0, 589);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            statusStrip1.Size = new System.Drawing.Size(504, 22);
            statusStrip1.TabIndex = 2;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsslStatus
            // 
            tsslStatus.Name = "tsslStatus";
            tsslStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // tsslMessage
            // 
            tsslMessage.AutoSize = false;
            tsslMessage.BackColor = System.Drawing.Color.Transparent;
            tsslMessage.Name = "tsslMessage";
            tsslMessage.Size = new System.Drawing.Size(200, 17);
            tsslMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UC_SqlQuery
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(statusStrip1);
            Controls.Add(splitContainer1);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "UC_SqlQuery";
            Size = new System.Drawing.Size(504, 611);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabResult.ResumeLayout(false);
            tabPageData.ResumeLayout(false);
            tabPageMessage.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tsslMessage;
        private System.Windows.Forms.TabControl tabResult;
        private System.Windows.Forms.TabPage tabPageMessage;
        private System.Windows.Forms.TabPage tabPageData;
        private System.Windows.Forms.RichTextBox resultTextBox;
        private UC_QueryResultGrid resultGridView;
        private UC_QueryEditor queryEditor;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
    }
}
