namespace DatabaseManager.Forms
{
    partial class frmDatabaseDiagram
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmDatabaseDiagram));
            gViewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // gViewer
            // 
            gViewer.ArrowheadLength = 10D;
            gViewer.AsyncLayout = false;
            gViewer.AutoScroll = true;
            gViewer.BackwardEnabled = false;
            gViewer.BuildHitTree = true;
            gViewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.UseSettingsOfTheGraph;
            gViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            gViewer.EdgeInsertButtonVisible = false;
            gViewer.FileName = "";
            gViewer.ForwardEnabled = false;
            gViewer.Graph = null;
            gViewer.IncrementalDraggingModeAlways = false;
            gViewer.InsertingEdge = false;
            gViewer.LayoutAlgorithmSettingsButtonVisible = true;
            gViewer.LayoutEditingEnabled = true;
            gViewer.Location = new System.Drawing.Point(0, 0);
            gViewer.LooseOffsetForRouting = 0.25D;
            gViewer.MouseHitDistance = 0.05D;
            gViewer.Name = "gViewer";
            gViewer.NavigationVisible = true;
            gViewer.NeedToCalculateLayout = true;
            gViewer.OffsetForRelaxingInRouting = 0.6D;
            gViewer.PaddingForEdgeRouting = 8D;
            gViewer.PanButtonPressed = false;
            gViewer.SaveAsImageEnabled = true;
            gViewer.SaveAsMsaglEnabled = true;
            gViewer.SaveButtonVisible = true;
            gViewer.SaveGraphButtonVisible = true;
            gViewer.SaveInVectorFormatEnabled = true;
            gViewer.Size = new System.Drawing.Size(800, 409);
            gViewer.TabIndex = 0;
            gViewer.TightOffsetForRouting = 0.125D;
            gViewer.ToolBarIsVisible = true;
            gViewer.Transform = (Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation)resources.GetObject("gViewer.Transform");
            gViewer.UndoRedoButtonsVisible = true;
            gViewer.WindowZoomButtonPressed = false;
            gViewer.ZoomF = 1D;
            gViewer.ZoomWindowThreshold = 0.05D;
            gViewer.ObjectUnderMouseCursorChanged += gViewer_ObjectUnderMouseCursorChanged;
            gViewer.KeyDown += gViewer_KeyDown;
            gViewer.MouseClick += gViewer_MouseClick;
            gViewer.MouseDoubleClick += gViewer_MouseDoubleClick;
            // 
            // frmDatabaseDiagram
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 409);
            Controls.Add(gViewer);
            Name = "frmDatabaseDiagram";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Diagram";
            WindowState = System.Windows.Forms.FormWindowState.Maximized;
            Load += frmDatabaseDiagram_Load;
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Msagl.GraphViewerGdi.GViewer gViewer;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}