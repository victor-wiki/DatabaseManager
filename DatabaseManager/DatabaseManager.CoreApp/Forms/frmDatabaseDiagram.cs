using DatabaseInterpreter.Core;
using DatabaseInterpreter.Model;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Color = Microsoft.Msagl.Drawing.Color;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;

namespace DatabaseManager.Forms
{
    public partial class frmDatabaseDiagram : Form
    {
        private frmFindBox findBox = null;
        private DbInterpreter dbInterpreter;
        private List<Table> tables;
        private List<TableForeignKey> foreignKeys;
        private List<frmTableColumnDetails> tableColumnDetailsForms = new List<frmTableColumnDetails>();
        private List<frmTableColumnRelation> tableColumnsRelationForms = new List<frmTableColumnRelation>();

        public frmDatabaseDiagram(DbInterpreter dbInterpreter)
        {
            InitializeComponent();

            this.AutoScaleMode = AutoScaleMode.Dpi;           

            this.dbInterpreter = dbInterpreter;
        }

        private void frmDatabaseDiagram_Load(object sender, EventArgs e)
        {
            this.LoadData();
        }

        private async void LoadData()
        {
            SchemaInfoFilter filter = new SchemaInfoFilter();
            filter.DatabaseObjectType = DatabaseObjectType.Table | DatabaseObjectType.ForeignKey | DatabaseObjectType.PrimaryKey;

            var schemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(filter);
            var tables = schemaInfo.Tables;
            var foreignKeys = schemaInfo.TableForeignKeys;

            this.tables = tables;
            this.foreignKeys = foreignKeys;

            var graph = new Graph(this.dbInterpreter.ConnectionInfo.Database);

            foreach (var table in tables)
            {
                var node = graph.AddNode(table.Name);
                node.UserData = table;

                this.InitNode(node);
            }

            foreach (var foreignKey in foreignKeys)
            {
                var edge = graph.AddEdge(foreignKey.TableName, foreignKey.Name, foreignKey.ReferencedTableName);
                edge.UserData = foreignKey;

                edge.Attr.ArrowheadAtTarget = ArrowStyle.Normal;

                this.InitEdge(edge);
            }

            this.gViewer.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.MDS;

            this.gViewer.Graph = graph;
        }

        private void InitNode(Node node)
        {
            node.Attr.Color = Color.Black;
            node.Attr.Shape = Shape.Box;
            node.Attr.FillColor = Color.LightSkyBlue;
        }

        private void InitEdge(Edge edge)
        {
            edge.Attr.LineWidth = 1;
            edge.Attr.Color = Color.Blue;
            edge.LabelText = "";// foreignKey.Name;
        }

        private void gViewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            bool isEnter = e.NewObject?.DrawingObject != null;
            bool isLeave = e.OldObject?.DrawingObject != null;

            try
            {
                if (isEnter)
                {
                    var drawingObject = e.NewObject.DrawingObject;

                    if (drawingObject is Node node)
                    {
                        this.ResetEdges();

                        this.HighlightNode(node);

                        this.gViewer.Invalidate(e.NewObject);

                        string tooltip = (node.UserData as Table)?.Comment ?? "";

                        this.gViewer.SetToolTip(toolTip1, tooltip);
                    }
                    else if (drawingObject is Edge edge)
                    {
                        this.HighlightEdge(edge);

                        this.gViewer.Invalidate(e.NewObject);

                        var fk = edge.UserData as TableForeignKey;
                        string fkName = fk?.Name;

                        string tooltip = !string.IsNullOrEmpty(fkName) ? fkName : String.Format("{0}→{1}", edge.Source, edge.Target);

                        this.gViewer.SetToolTip(toolTip1, tooltip);
                    }
                }
                else if (isLeave)
                {
                    this.gViewer.SetToolTip(toolTip1, "");

                    var drawingObject = e.OldObject.DrawingObject;

                    if (drawingObject is Node node)
                    {
                        this.InitNode(node);

                        this.gViewer.Invalidate(e.OldObject);
                    }
                    else if (drawingObject is Edge edge)
                    {
                        this.InitEdge(edge);
                        this.gViewer.Invalidate(e.OldObject);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void HighlightNode(Node node)
        {
            this.ResetNodes(node);

            node.Attr.LineWidth = 1;
            node.Attr.Color = Color.Green;
            node.Attr.FillColor = Color.WhiteSmoke;

            this.gViewer.Refresh();
        }

        private void ResetNodes(Node excludeNode = null)
        {
            var nodes = this.gViewer.Graph.Nodes;

            foreach (var node in nodes)
            {
                if (excludeNode != null && excludeNode == node)
                {
                    continue;
                }

                this.InitNode(node);
            }

            this.gViewer.Refresh();
        }

        private void HighlightEdge(Edge edge)
        {
            this.ResetEdges(edge);

            edge.Attr.Color = Color.Orange;
            edge.Attr.LineWidth = 2;

            this.gViewer.Refresh();
        }

        private void ResetEdges(Edge excludeEdge = null)
        {
            var edges = this.gViewer.Graph.Edges;

            foreach (var edge in edges)
            {
                if(excludeEdge!=null && excludeEdge == edge)
                {
                    continue;
                }

                this.InitEdge(edge);
            }

            this.gViewer.Refresh();
        }

        private async void gViewer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var gviewer = (Microsoft.Msagl.GraphViewerGdi.GViewer)sender;

            var obj = gviewer.ObjectUnderMouseCursor?.DrawingObject;

            if (obj is Node node)
            {
                this.HighlightNode(node);

                Table table = this.FindTable(node.Id);

                SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = [table.Name] };
                filter.DatabaseObjectType = DatabaseObjectType.Column | DatabaseObjectType.PrimaryKey;

                var schemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(filter);

                var columns = schemaInfo.TableColumns.OrderBy(item => item.Order);
                var primaryKey = schemaInfo.TablePrimaryKeys.FirstOrDefault();
                var foreignKeys = this.foreignKeys.Where(item => item.TableName == table.Name);

                var rect = this.gViewer.MapSourceRectangleToScreenRectangle(node.BoundingBox);

                Point location = this.CalculateFormShowPosition(rect);

                this.FindAndShowTableColumnDetailsForm(location, table, columns, primaryKey, foreignKeys);
            }
            else if (obj is Edge edge)
            {
                this.HighlightEdge(edge);

                Table sourceTable = this.FindTable(edge.Source);
                Table targetTable = this.FindTable(edge.Target);

                SchemaInfoFilter filter = new SchemaInfoFilter() { TableNames = [sourceTable.Name, targetTable.Name] };
                filter.DatabaseObjectType = DatabaseObjectType.Column | DatabaseObjectType.PrimaryKey;

                var schemaInfo = await this.dbInterpreter.GetSchemaInfoAsync(filter);

                var sourceTableColumns = schemaInfo.TableColumns.Where(item => item.TableName == sourceTable.Name).OrderBy(item => item.Order);
                var targetTableColumns = schemaInfo.TableColumns.Where(item => item.TableName == targetTable.Name).OrderBy(item => item.Order);
                var sourceTablePrimaryKey = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == sourceTable.Name).FirstOrDefault();
                var targetTablePrimaryKey = schemaInfo.TablePrimaryKeys.Where(item => item.TableName == targetTable.Name).FirstOrDefault();
                var sourceTableForeignKeys = this.foreignKeys.Where(item => item.TableName == sourceTable.Name);
                var targetTableForeignKeys = this.foreignKeys.Where(item => item.TableName == targetTable.Name);

                var rect = this.gViewer.MapSourceRectangleToScreenRectangle(edge.EdgeCurve.BoundingBox);            

                this.FindAndSowTableColumnRelationForm(rect, sourceTable, sourceTableColumns, sourceTablePrimaryKey, sourceTableForeignKeys,
                    targetTable, targetTableColumns, targetTablePrimaryKey, targetTableForeignKeys);
            }
            else
            {
                this.gViewer.PanButtonPressed = !this.gViewer.PanButtonPressed;
            }
        }

        private Point CalculateFormShowPosition(Rectangle rectangle)
        {
            Point point = new Point(rectangle.X, rectangle.Bottom + this.GetFixedPointY());

            return point;
        }

        private int GetFixedPointY()
        {
            int formTitleBarHeight = 22;
            int toolbarHeight = this.gViewer.ToolBarIsVisible ? 27 : 0;

            return formTitleBarHeight + toolbarHeight;
        }

        private Point CalculateFormShowPosition(Point point)
        {
            point.Y += this.GetFixedPointY();

            return point;
        }

        private void FindAndShowTableColumnDetailsForm(Point location, Table table, IEnumerable<TableColumn> columns, TablePrimaryKey primaryKey, IEnumerable<TableForeignKey> foreignKeys)
        {
            frmTableColumnDetails frm = this.tableColumnDetailsForms.FirstOrDefault(item => item.Text == table.Name);

            if (frm == null || frm.IsDisposed)
            {
                frm = new frmTableColumnDetails(table, columns, primaryKey, foreignKeys);

                frm.FormClosed += (sender, e) =>
                {
                    this.tableColumnDetailsForms.Remove(sender as frmTableColumnDetails);
                };

                frm.StartPosition = FormStartPosition.Manual;

                this.tableColumnDetailsForms.Add(frm);
            }
            else if (frm.WindowState == FormWindowState.Minimized)
            {
                frm.WindowState = FormWindowState.Normal;
            }

            frm.Location = location;
            frm.Show();
            frm.BringToFront();
        }

        private void FindAndSowTableColumnRelationForm(Rectangle rectangle, Table sourceTable, IEnumerable<TableColumn> sourceTableColumns, TablePrimaryKey sourceTablePrimaryKey, IEnumerable<TableForeignKey> sourceTableForeignKeys,
            Table targetTable, IEnumerable<TableColumn> targetTableColumns, TablePrimaryKey targetTablePrimaryKey, IEnumerable<TableForeignKey> targetTableForeignKeys)
        {
            var foreignKey = this.foreignKeys.FirstOrDefault(item => item.TableName == sourceTable.Name && item.ReferencedTableName == targetTable.Name);

            frmTableColumnRelation frm = this.tableColumnsRelationForms.FirstOrDefault(item => item.Tag == foreignKey);

            if (frm == null || frm.IsDisposed)
            {
                frm = new frmTableColumnRelation(foreignKey);
                frm.Tag = foreignKey;

                frm.FormClosed += (sender, e) =>
                {
                    this.tableColumnsRelationForms.Remove(sender as frmTableColumnRelation);
                };

                frm.StartPosition = FormStartPosition.Manual;
                frm.LoadTableColumns(true, sourceTable, sourceTableColumns, sourceTablePrimaryKey, sourceTableForeignKeys);
                frm.LoadTableColumns(false, targetTable, targetTableColumns, targetTablePrimaryKey, targetTableForeignKeys);

                tableColumnsRelationForms.Add(frm);
            }
            else if (frm.WindowState == FormWindowState.Minimized)
            {
                frm.WindowState = FormWindowState.Normal;
            }

            var points = new Point[] 
            { 
                new Point(rectangle.Left, rectangle.Top), 
                new Point(rectangle.Left,rectangle.Bottom),
                new Point(rectangle.Left+rectangle.Width, rectangle.Top),
                new Point(rectangle.Left+rectangle.Width, rectangle.Bottom),
                new Point(rectangle.Left+rectangle.Width/2, rectangle.Top+rectangle.Height/2)
            };

            Point location = this.GetClosestDistancePointToCenter(points);

            frm.Location = this.CalculateFormShowPosition(location);
            frm.Show();
            frm.BringToFront();
        }

        private Table FindTable(string tableName)
        {
            Table table = this.tables.FirstOrDefault(item => item.Name == tableName);

            return table;
        }

        private void FindNode()
        {
            if (this.findBox == null || this.findBox.IsDisposed)
            {
                this.findBox = new frmFindBox(false);
                this.findBox.Text = "Find Node";
                this.findBox.MinimizeBox = true;
                this.findBox.OnFind += this.FindBox_OnFind;
            }
            else
            {
                this.findBox.WindowState = FormWindowState.Normal;
            }

            this.findBox.StartPosition = FormStartPosition.CenterScreen;

            this.findBox.Show();
            this.findBox.BringToFront();
            this.findBox.SelectAll();
        }

        private void FindBox_OnFind()
        {
            string word = this.findBox.FindWord;

            bool found = false;

            if (!string.IsNullOrEmpty(word))
            {
                var nodes = this.gViewer.Graph.Nodes;

                Rectangle rect = default(Rectangle);

                foreach (var node in nodes)
                {
                    if (node.Id.ToLower() == word.ToLower())
                    {
                        found = true;

                        this.HighlightNode(node);

                        rect = this.gViewer.MapSourceRectangleToScreenRectangle(node.BoundingBox);

                        break;
                    }
                }

                if (!found)
                {
                    foreach (var edge in this.gViewer.Graph.Edges)
                    {
                        if (edge.UserData is TableForeignKey fk)
                        {
                            if (fk.Name?.ToLower() == word.ToLower())
                            {
                                found = true;

                                this.HighlightEdge(edge);

                                rect = this.gViewer.MapSourceRectangleToScreenRectangle(edge.BoundingBox);
                            }
                        }
                    }
                }

                if (found)
                {
                    this.PanToPosition(rect);

                    this.findBox.Hide();
                }
            }

            if (!found)
            {
                MessageBox.Show("Not found.");
            }
        }

        private void PanToPosition(Rectangle rectangle)
        {
            var centerPosition = this.GetCenterPosition();

            this.gViewer.Pan(centerPosition.X - (rectangle.X + rectangle.Width / 2), centerPosition.Y - (rectangle.Y + rectangle.Height / 2));
        }

        private Point GetCenterPosition()
        {
            return new Point(this.gViewer.Width / 2, this.gViewer.Height / 2); ;
        }

        private Point GetClosestDistancePointToCenter(params Point[] points)
        {
            var centerPosition = this.GetCenterPosition();

            Point closestPoint = points.Aggregate((p1, p2) => this.Distance(centerPosition, p1) < this.Distance(centerPosition, p2) ? p1 : p2);

            return closestPoint;
        }

        private double Distance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
        }

        private void gViewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                this.FindNode();
            }
        }

        private void gViewer_MouseClick(object sender, MouseEventArgs e)
        {
            var gviewer = (Microsoft.Msagl.GraphViewerGdi.GViewer)sender;

            var obj = gviewer.ObjectUnderMouseCursor?.DrawingObject;

            if (obj is Node node)
            {
                this.HighlightNode(node);
            }
            else if(obj is Edge edge)
            {
                this.HighlightEdge(edge);
            }
        }
    }
}
