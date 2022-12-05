namespace DatabaseManager.Forms
{
    partial class frmDbObjectDependency
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDbObjectDependency));
            this.rbDependOnThis = new System.Windows.Forms.RadioButton();
            this.rbThisDependOn = new System.Windows.Forms.RadioButton();
            this.tvDependencies = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtObjectType = new System.Windows.Forms.TextBox();
            this.txtObjectName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // rbDependOnThis
            // 
            this.rbDependOnThis.AutoSize = true;
            this.rbDependOnThis.Checked = true;
            this.rbDependOnThis.Location = new System.Drawing.Point(12, 12);
            this.rbDependOnThis.Name = "rbDependOnThis";
            this.rbDependOnThis.Size = new System.Drawing.Size(217, 21);
            this.rbDependOnThis.TabIndex = 0;
            this.rbDependOnThis.TabStop = true;
            this.rbDependOnThis.Text = "Objects that depend on $Name$";
            this.rbDependOnThis.UseVisualStyleBackColor = true;
            this.rbDependOnThis.CheckedChanged += new System.EventHandler(this.rbDependOnThis_CheckedChanged);
            // 
            // rbThisDependOn
            // 
            this.rbThisDependOn.AutoSize = true;
            this.rbThisDependOn.Location = new System.Drawing.Point(12, 39);
            this.rbThisDependOn.Name = "rbThisDependOn";
            this.rbThisDependOn.Size = new System.Drawing.Size(237, 21);
            this.rbThisDependOn.TabIndex = 1;
            this.rbThisDependOn.Text = "Objects on which  $Name$ depends";
            this.rbThisDependOn.UseVisualStyleBackColor = true;
            // 
            // tvDependencies
            // 
            this.tvDependencies.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvDependencies.HideSelection = false;
            this.tvDependencies.ImageIndex = 0;
            this.tvDependencies.ImageList = this.imageList1;
            this.tvDependencies.Location = new System.Drawing.Point(12, 66);
            this.tvDependencies.Name = "tvDependencies";
            this.tvDependencies.SelectedImageIndex = 0;
            this.tvDependencies.Size = new System.Drawing.Size(714, 310);
            this.tvDependencies.TabIndex = 2;
            this.tvDependencies.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvDependencies_BeforeExpand);
            this.tvDependencies.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvDependencies_AfterSelect);
            this.tvDependencies.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvDependencies_KeyDown);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "tree_Fake.png");
            this.imageList1.Images.SetKeyName(1, "tree_Database.png");
            this.imageList1.Images.SetKeyName(2, "tree_Folder.png");
            this.imageList1.Images.SetKeyName(3, "tree_TableForeignKey.png");
            this.imageList1.Images.SetKeyName(4, "tree_Procedure.png");
            this.imageList1.Images.SetKeyName(5, "tree_View.png");
            this.imageList1.Images.SetKeyName(6, "tree_TableIndex.png");
            this.imageList1.Images.SetKeyName(7, "tree_TablePrimaryKey.png");
            this.imageList1.Images.SetKeyName(8, "tree_Table.png");
            this.imageList1.Images.SetKeyName(9, "tree_TableConstraint.png");
            this.imageList1.Images.SetKeyName(10, "tree_TableTrigger.png");
            this.imageList1.Images.SetKeyName(11, "Loading.gif");
            this.imageList1.Images.SetKeyName(12, "tree_Function.png");
            this.imageList1.Images.SetKeyName(13, "tree_TableColumn.png");
            this.imageList1.Images.SetKeyName(14, "tree_UserDefinedType.png");
            this.imageList1.Images.SetKeyName(15, "tree_Sequence.png");
            this.imageList1.Images.SetKeyName(16, "tree_Function_Trigger.png");
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClose.Location = new System.Drawing.Point(352, 450);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 32);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 385);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Object Type:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 413);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Object Name:";
            // 
            // txtObjectType
            // 
            this.txtObjectType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtObjectType.Location = new System.Drawing.Point(103, 382);
            this.txtObjectType.Name = "txtObjectType";
            this.txtObjectType.ReadOnly = true;
            this.txtObjectType.Size = new System.Drawing.Size(623, 23);
            this.txtObjectType.TabIndex = 6;
            // 
            // txtObjectName
            // 
            this.txtObjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtObjectName.Location = new System.Drawing.Point(103, 411);
            this.txtObjectName.Name = "txtObjectName";
            this.txtObjectName.ReadOnly = true;
            this.txtObjectName.Size = new System.Drawing.Size(623, 23);
            this.txtObjectName.TabIndex = 7;
            // 
            // frmDbObjectDependency
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 494);
            this.Controls.Add(this.txtObjectName);
            this.Controls.Add(this.txtObjectType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tvDependencies);
            this.Controls.Add(this.rbThisDependOn);
            this.Controls.Add(this.rbDependOnThis);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmDbObjectDependency";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Object Dependencies";
            this.Load += new System.EventHandler(this.frmDbObjectDependency_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rbDependOnThis;
        private System.Windows.Forms.RadioButton rbThisDependOn;
        private System.Windows.Forms.TreeView tvDependencies;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtObjectType;
        private System.Windows.Forms.TextBox txtObjectName;
    }
}