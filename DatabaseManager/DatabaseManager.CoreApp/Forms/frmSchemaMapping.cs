using DatabaseConverter.Core;
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
    public partial class frmSchemaMapping : Form
    {
        private const string EmptyItem = "<None>";

        internal List<string> SourceSchemas { get; set; } = new List<string>();
        internal List<string> TargetSchemas { get; set; } = new List<string>();
        internal List<SchemaMappingInfo> Mappings { get; set; } = new List<SchemaMappingInfo>();

        public frmSchemaMapping()
        {
            InitializeComponent();
        }

        private void frmColumnMapping_Load(object sender, EventArgs e)
        {
            this.InitControls();
        }

        private void InitControls()
        {
            this.LoadMappings();
        }

        private void LoadMappings()
        {
            if (this.Mappings != null)
            {
                foreach (SchemaMappingInfo mapping in this.Mappings)
                {
                    ComboBox sourceCombo = this.CreateCombobox(this.panelSourceSchema, this.SourceSchemas, mapping.SourceSchema);

                    this.panelSourceSchema.Controls.Add(sourceCombo);

                    ComboBox targetCombo = this.CreateCombobox(this.panelTargetSchema, this.TargetSchemas, mapping.TargetSchema);

                    this.panelTargetSchema.Controls.Add(targetCombo);
                }
            }

            this.CreatePlaceholder();
        }

        private void CreatePlaceholder()
        {
            ComboBox sourceComboEmpty = this.CreateCombobox(this.panelSourceSchema, this.SourceSchemas, null);

            this.panelSourceSchema.Controls.Add(sourceComboEmpty);

            ComboBox targetComboEmpty = this.CreateCombobox(this.panelTargetSchema, this.TargetSchemas, null);

            this.panelTargetSchema.Controls.Add(targetComboEmpty);
        }

        private ComboBox CreateCombobox(Panel panel, List<string> values, string value)
        {
            ComboBox combo = new ComboBox();
            combo.MouseClick += Combo_MouseClick;
            combo.DropDownStyle = ComboBoxStyle.DropDown;
            combo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            combo.Width = panel.Width - 5;
            combo.Tag = panel.Controls.Count + 1;
            combo.SelectedIndexChanged += Combo_SelectedIndexChanged;

            var displayValues = this.GetValuesWithEmptyItem(values).AsEnumerable();

            if (panel.Name == this.panelSourceSchema.Name)
            {
                displayValues = displayValues.Except(this.GetExistingValues(panel));
            }

            combo.Items.AddRange(displayValues.ToArray());

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
                ComboBox sourceCombo = this.CreateCombobox(this.panelSourceSchema, this.SourceSchemas, null);

                this.panelSourceSchema.Controls.Add(sourceCombo);

                ComboBox targetCombo = this.CreateCombobox(this.panelTargetSchema, this.TargetSchemas, null);

                this.panelTargetSchema.Controls.Add(targetCombo);
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            List<SchemaMappingInfo> mappings = new List<SchemaMappingInfo>();

            for (int i = 0; i < this.panelSourceSchema.Controls.Count; i++)
            {
                ComboBox sourceCombo = this.panelSourceSchema.Controls[i] as ComboBox;
                ComboBox targetCombo = this.panelTargetSchema.Controls[i] as ComboBox;

                if (sourceCombo.Text != EmptyItem && targetCombo.Text != EmptyItem && !(string.IsNullOrEmpty(sourceCombo.Text) && string.IsNullOrEmpty(targetCombo.Text)))
                {
                    SchemaMappingInfo mapping = new SchemaMappingInfo();
                    mapping.SourceSchema = sourceCombo.Text;
                    mapping.TargetSchema = targetCombo.Text;

                    mappings.Add(mapping);
                }
            }

            if (mappings.Any(item => !string.IsNullOrEmpty(item.SourceSchema) && item.SourceSchema != EmptyItem && mappings.Count(t => t.SourceSchema == item.SourceSchema) > 1))
            {
                MessageBox.Show("One Source Schema can't be mapped to more than one target schema!");
                return;
            }

            this.Mappings = mappings;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAutoMap_Click(object sender, EventArgs e)
        {
            this.ClearControls();

            foreach(var sourceSchema in this.SourceSchemas)
            {
                if(this.TargetSchemas.Any(item=> item==sourceSchema))
                {
                    ComboBox sourceCombo = this.CreateCombobox(this.panelSourceSchema, this.SourceSchemas, sourceSchema);

                    this.panelSourceSchema.Controls.Add(sourceCombo);

                    ComboBox targetCombo = this.CreateCombobox(this.panelTargetSchema, this.TargetSchemas, sourceSchema);

                    this.panelTargetSchema.Controls.Add(targetCombo);
                }               
            }

            this.CreatePlaceholder();
        }

        private void ClearControls()
        {
            this.panelSourceSchema.Controls.Clear();
            this.panelTargetSchema.Controls.Clear();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            this.ClearControls();
            this.CreatePlaceholder();
        }
    }
}
