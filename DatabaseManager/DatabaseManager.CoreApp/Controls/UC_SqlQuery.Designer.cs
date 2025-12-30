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
            lblMessage = new System.Windows.Forms.Label();
            queryEditor = new UC_QueryEditor();
            tabResult = new System.Windows.Forms.TabControl();
            tabPageData = new System.Windows.Forms.TabPage();
            btnExport = new System.Windows.Forms.Button();
            pagination = new UC_Pagination();
            loadingPanel = new UC_LoadingPanel();
            resultGridView = new UC_QueryResultGrid();
            tabPageMessage = new System.Windows.Forms.TabPage();
            resultTextBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tabResult.SuspendLayout();
            tabPageData.SuspendLayout();
            tabPageMessage.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(lblMessage);
            splitContainer1.Panel1.Controls.Add(queryEditor);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            splitContainer1.Panel2.Controls.Add(tabResult);
            splitContainer1.Size = new System.Drawing.Size(657, 611);
            splitContainer1.SplitterDistance = 373;
            splitContainer1.SplitterWidth = 3;
            splitContainer1.TabIndex = 1;
            // 
            // lblMessage
            // 
            lblMessage.BackColor = System.Drawing.SystemColors.Control;
            lblMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            lblMessage.Location = new System.Drawing.Point(0, 356);
            lblMessage.Name = "lblMessage";
            lblMessage.Size = new System.Drawing.Size(657, 17);
            lblMessage.TabIndex = 1;
            lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // queryEditor
            // 
            queryEditor.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            queryEditor.BackColor = System.Drawing.SystemColors.Control;
            queryEditor.DatabaseType = DatabaseType.Unknown;
            queryEditor.DbInterpreter = null;
            queryEditor.Location = new System.Drawing.Point(0, 0);
            queryEditor.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            queryEditor.Name = "queryEditor";
            queryEditor.Size = new System.Drawing.Size(657, 354);
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
            tabResult.Size = new System.Drawing.Size(657, 235);
            tabResult.TabIndex = 0;
            // 
            // tabPageData
            // 
            tabPageData.BackColor = System.Drawing.SystemColors.Control;
            tabPageData.Controls.Add(btnExport);
            tabPageData.Controls.Add(pagination);
            tabPageData.Controls.Add(loadingPanel);
            tabPageData.Controls.Add(resultGridView);
            tabPageData.Location = new System.Drawing.Point(4, 26);
            tabPageData.Margin = new System.Windows.Forms.Padding(0);
            tabPageData.Name = "tabPageData";
            tabPageData.Size = new System.Drawing.Size(649, 205);
            tabPageData.TabIndex = 1;
            tabPageData.Text = "Data";
            // 
            // btnExport
            // 
            btnExport.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnExport.Location = new System.Drawing.Point(623, 179);
            btnExport.Margin = new System.Windows.Forms.Padding(4);
            btnExport.Name = "btnExport";
            btnExport.Size = new System.Drawing.Size(22, 22);
            btnExport.TabIndex = 20;
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Visible = false;
            btnExport.Click += btnExport_Click;
            // 
            // pagination
            // 
            pagination.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pagination.BackColor = System.Drawing.Color.Transparent;
            pagination.Location = new System.Drawing.Point(2, 179);
            pagination.Margin = new System.Windows.Forms.Padding(4);
            pagination.Name = "pagination";
            pagination.PageCount = 0L;
            pagination.PageNumber = 1L;
            pagination.PageSize = 200;
            pagination.Size = new System.Drawing.Size(602, 26);
            pagination.TabIndex = 4;
            pagination.TotalCount = 0L;
            pagination.Visible = false;
            pagination.OnPageNumberChanged += pagination_OnPageNumberChanged;
            // 
            // loadingPanel
            // 
            loadingPanel.BackColor = System.Drawing.Color.Transparent;
            loadingPanel.BackgroundColor = System.Drawing.SystemColors.ControlDark;
            loadingPanel.CancellationTokenSource = null;
            loadingPanel.InterruptButtonVisible = true;
            loadingPanel.Location = new System.Drawing.Point(215, 40);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.Size = new System.Drawing.Size(166, 84);
            loadingPanel.TabIndex = 3;
            // 
            // resultGridView
            // 
            resultGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            resultGridView.Location = new System.Drawing.Point(2, 0);
            resultGridView.Margin = new System.Windows.Forms.Padding(0);
            resultGridView.Name = "resultGridView";
            resultGridView.Size = new System.Drawing.Size(647, 175);
            resultGridView.TabIndex = 0;
            resultGridView.SizeChanged += resultGridView_SizeChanged;
            // 
            // tabPageMessage
            // 
            tabPageMessage.BackColor = System.Drawing.SystemColors.Control;
            tabPageMessage.Controls.Add(resultTextBox);
            tabPageMessage.Location = new System.Drawing.Point(4, 26);
            tabPageMessage.Margin = new System.Windows.Forms.Padding(0);
            tabPageMessage.Name = "tabPageMessage";
            tabPageMessage.Size = new System.Drawing.Size(649, 205);
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
            resultTextBox.Size = new System.Drawing.Size(649, 205);
            resultTextBox.TabIndex = 0;
            resultTextBox.Text = "";
            // 
            // UC_SqlQuery
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Margin = new System.Windows.Forms.Padding(0);
            Name = "UC_SqlQuery";
            Size = new System.Drawing.Size(657, 611);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tabResult.ResumeLayout(false);
            tabPageData.ResumeLayout(false);
            tabPageMessage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabResult;
        private System.Windows.Forms.TabPage tabPageMessage;
        private System.Windows.Forms.TabPage tabPageData;
        private System.Windows.Forms.RichTextBox resultTextBox;
        private UC_QueryResultGrid resultGridView;
        private UC_QueryEditor queryEditor;
        private UC_LoadingPanel loadingPanel;
        private UC_Pagination pagination;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label lblMessage;
    }
}
