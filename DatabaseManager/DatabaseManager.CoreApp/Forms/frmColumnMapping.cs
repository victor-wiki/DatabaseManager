using DatabaseInterpreter.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DatabaseManager
{
    public partial class frmColumnMapping : Form
    {
        private const string EmptyItem = "<None>";
        public string ReferenceTableName { get; set; }
        public string TableName { get; set; }
        public List<string> ReferenceTableColumns { get; set; } = new List<string>();
        public List<string> TableColumns { get; set; } = new List<string>();

        public List<ForeignKeyColumn> Mappings { get; set; } = new List<ForeignKeyColumn>();

        public frmColumnMapping()
        {
            InitializeComponent();
        }

        private void frmColumnMapping_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.gbReferenceTable.Text = this.ReferenceTableName;
            this.gbTable.Text = this.TableName;

            this.LoadMappings();
        }

        private void LoadMappings()
        {
            if (this.Mappings != null)
            {
                foreach (ForeignKeyColumn mapping in this.Mappings)
                {
                    ComboBox refCombo = this.CreateCombobox(this.panelReferenceTable, this.ReferenceTableColumns, mapping.ReferencedColumnName);

                    this.panelReferenceTable.Controls.Add(refCombo);

                    ComboBox combo = this.CreateCombobox(this.panelTable, this.TableColumns, mapping.ColumnName);

                    this.panelTable.Controls.Add(combo);
                }
            }

            ComboBox refComboEmpty = this.CreateCombobox(this.panelReferenceTable, this.ReferenceTableColumns, null);

            this.panelReferenceTable.Controls.Add(refComboEmpty);

            ComboBox comboEmpty = this.CreateCombobox(this.panelTable, this.TableColumns, null);

            this.panelTable.Controls.Add(comboEmpty);
        }

        private ComboBox CreateCombobox(Panel panel, List<string> values, string value)
        {
            ComboBox combo = new ComboBox();
            combo.MouseClick += Combo_MouseClick;
            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            combo.Width = panelTable.Width - 5;
            combo.Tag = panel.Controls.Count + 1;
            combo.SelectedIndexChanged += Combo_SelectedIndexChanged;
            combo.KeyPress += Combo_KeyPress;

            combo.Items.AddRange(this.GetValuesWithEmptyItem(values).Except(this.GetExistingValues(panel)).ToArray());

            if (panel.Controls.Count > 0)
            {
                combo.Top = panel.Controls.Count * combo.Height + panel.Controls.Count;
            }

            if (!string.IsNullOrEmpty(value) && values.Contains(value))
            {
                combo.Text = value;
            }

            return combo;
        }

        private void Combo_MouseClick(object sender, MouseEventArgs e)
        {
            ComboBox combo = sender as ComboBox;

            if (!combo.DroppedDown)
            {
                combo.DroppedDown = true;
            }
        }

        private void Combo_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private List<string> GetExistingValues(Panel panel)
        {
            List<string> values = new List<string>();
            var comboboxes = panel.Controls.OfType<ComboBox>();

            foreach (ComboBox combo in comboboxes)
            {
                if (!string.IsNullOrEmpty(combo.Text) && combo.Text != EmptyItem)
                {
                    values.Add(combo.Text);
                }
            }

            return values;
        }

        private List<string> GetValuesWithEmptyItem(List<string> values)
        {
            var cloneValues = values.Select(item => item).ToList();
            cloneValues.Add(EmptyItem);

            return cloneValues;
        }

        private void Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;

            if (combo.Parent == null)
            {
                return;
            }

            int order = Convert.ToInt32(combo.Tag);

            if (order == combo.Parent.Controls.Count)
            {
                ComboBox refCombo = this.CreateCombobox(this.panelReferenceTable, this.ReferenceTableColumns, null);

                this.panelReferenceTable.Controls.Add(refCombo);

                ComboBox cbo = this.CreateCombobox(this.panelTable, this.TableColumns, null);

                this.panelTable.Controls.Add(cbo);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            List<ForeignKeyColumn> mappings = new List<ForeignKeyColumn>();

            for (int i = 0; i < this.panelReferenceTable.Controls.Count; i++)
            {
                ComboBox refCombo = this.panelReferenceTable.Controls[i] as ComboBox;
                ComboBox combo = this.panelTable.Controls[i] as ComboBox;

                if (!string.IsNullOrEmpty(refCombo.Text) && refCombo.Text != EmptyItem
                    && !string.IsNullOrEmpty(combo.Text) && combo.Text != EmptyItem)
                {
                    ForeignKeyColumn mapping = new ForeignKeyColumn();
                    mapping.ReferencedColumnName = refCombo.Text;
                    mapping.ColumnName = combo.Text;
                    mapping.Order = mappings.Count + 1;

                    mappings.Add(mapping);
                }
            }

            this.Mappings = mappings;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
