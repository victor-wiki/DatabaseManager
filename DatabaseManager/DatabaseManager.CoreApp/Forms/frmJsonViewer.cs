using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DatabaseManager.Forms
{
    public partial class frmJsonViewer : Form
    {
        private const string JObjectText = "{}";
        private const string JArraryText = "[]";
        private const string NodeTextSeparator = ": ";
        private Brush nodeTextDefaultForeBrush = new SolidBrush(Color.Black);
        private Brush nodeTextStringValueForeBrush = new SolidBrush(Color.Red);
        private Brush nodeNameHighlightBrush = new SolidBrush(Color.Blue);

        public frmJsonViewer()
        {
            InitializeComponent();
        }

        private void frmJsonViewer_Load(object sender, EventArgs e)
        {

        }

        private void LoadJsonTree()
        {
            this.tvViwer.Nodes.Clear();

            string json = this.txtJson.Text.Trim();

            if (json.Length == 0)
            {
                return;
            }

            try
            {
                var result = JsonConvert.DeserializeObject(json);

                TreeNode rootNode = null;

                if (result is JObject jObject)
                {
                    rootNode = this.CreateTreeNode(null, JObjectText);

                    this.AddChildren(rootNode, jObject.Children());
                }
                else if (result is JArray jArray)
                {
                    rootNode = this.CreateTreeNode(null, JArraryText);

                    this.AddChildren(rootNode, jArray.Children());
                }

                this.tvViwer.Nodes.Add(rootNode);

                rootNode.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddChildren(TreeNode parentNode, JEnumerable<JToken> children)
        {
            foreach (var child in children)
            {
                var property = child as JProperty;

                TreeNode node = null;

                if (property != null)
                {
                    string name = property.Name;
                    object value = property.Value;

                    if (value is JObject jObject)
                    {
                        node = this.CreateTreeNode(name, JObjectText);

                        this.AddChildren(node, jObject.Children());
                    }
                    else if (value is JArray jArray)
                    {
                        node = this.CreateTreeNode(name, JArraryText);

                        this.AddChildren(node, jArray.Children());
                    }
                    else if (value is JValue jValue)
                    {
                        object v = jValue.Value;

                        node = this.CreateTreeNode(name, v);
                    }
                    else
                    {
                        node = this.CreateTreeNode(null, value);
                    }
                }
                else
                {
                    if (child is JObject jObject)
                    {
                        node = this.CreateTreeNode(null, JObjectText);

                        this.AddChildren(node, jObject.Children());
                    }
                    else if (child is JArray jArray)
                    {
                        node = this.CreateTreeNode(null, JArraryText);

                        this.AddChildren(node, jArray.Children());
                    }
                    else if (child is JValue jValue)
                    {
                        object value = jValue.Value;

                        node = this.CreateTreeNode(null, value);
                    }
                    else
                    {
                        node = this.CreateTreeNode(null, child.ToString());
                    }
                }

                parentNode.Nodes.Add(node);
            }
        }

        private TreeNode CreateTreeNode(string name, object value)
        {
            TreeNode node = new TreeNode();

            string strValue = null;

            if (value == null)
            {
                strValue = "null";
            }
            else if (value.GetType() == typeof(string))
            {
                if (value != JObjectText && value != JArraryText)
                {
                    strValue = $"\"{value}\"";
                }
                else
                {
                    strValue = value.ToString();
                }
            }
            else if (value.GetType() == typeof(bool))
            {
                strValue = value.ToString().ToLower();
            }
            else
            {
                strValue = value.ToString();
            }

            if (!string.IsNullOrEmpty(name))
            {
                node.Text = $"\"{name}\"{NodeTextSeparator}{value}";
            }
            else
            {
                node.Text = value?.ToString();
            }

            node.Tag = new TreeNodeTextInfo() { Name = name, Value = strValue, UseQuotation = true };

            return node;
        }

        private void txtJson_TextChanged(object sender, EventArgs e)
        {

        }

        private void tvViwer_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = false;

            string text = e.Node.Text;
            Font font = e.Node.NodeFont ?? this.tvViwer.Font;

            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                int width = TextRenderer.MeasureText(text, font).Width + 25;

                e.Graphics.FillRectangle(Brushes.LightBlue, new Rectangle(e.Bounds.X, e.Bounds.Y, width, e.Bounds.Height));
            }
            else
            {
                e.Graphics.FillRectangle(Brushes.White, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
            }

            Brush brush = new SolidBrush(e.Node.BackColor);

            e.Graphics.FillRectangle(brush, e.Bounds);

            TreeNodeTextInfo textInfo = e.Node.Tag as TreeNodeTextInfo;

            string name = textInfo.Name;

            if (string.IsNullOrEmpty(name))
            {
                e.Graphics.DrawString(text, font, nodeTextDefaultForeBrush, e.Bounds.Left, e.Bounds.Top);
            }
            else
            {
                int nameLength = name.Length + (textInfo.UseQuotation ? 2 : 0);

                string strName = text.Substring(0, nameLength);

                e.Graphics.DrawString(strName, font, this.nodeNameHighlightBrush, e.Bounds.Left, e.Bounds.Top);

                string separator = NodeTextSeparator;

                int nameWidth = TextRenderer.MeasureText(strName, font).Width;
                int separatorWidth = TextRenderer.MeasureText(separator, font).Width;

                e.Graphics.DrawString(separator, font, this.nodeTextDefaultForeBrush, e.Bounds.Left + nameWidth, e.Bounds.Top);

                string value = textInfo.Value ?? text;

                if (value != null)
                {
                    value = value.Replace(Environment.NewLine, " ");
                }

                bool isStringValue = value?.StartsWith("\"") == true;

                e.Graphics.DrawString(value, font, (isStringValue ? this.nodeTextStringValueForeBrush : this.nodeTextDefaultForeBrush), e.Bounds.Left + nameWidth + separatorWidth, e.Bounds.Top);
            }
        }

        private void tsmiExpandAll_Click(object sender, EventArgs e)
        {
            TreeNode node = this.tvViwer.SelectedNode;

            this.tvViwer.ExpandAll();

            if (node != null)
            {
                node.EnsureVisible();
            }
        }

        private void tsmiCollapseAll_Click(object sender, EventArgs e)
        {
            this.tvViwer.CollapseAll();
        }


        private void tsmiExpandChildren_Click(object sender, EventArgs e)
        {
            TreeNode node = this.tvViwer.SelectedNode;

            if (node != null)
            {
                node.ExpandAll();
                node.EnsureVisible();
            }
        }

        private void tsmiCollapseToChildren_Click(object sender, EventArgs e)
        {
            TreeNode node = this.tvViwer.SelectedNode;

            if (node != null)
            {
                node.Collapse(false);
            }
        }

        private void tsmiOpenFile_Click(object sender, EventArgs e)
        {
            DialogResult result = this.openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                string filePath = this.openFileDialog1.FileName;

                this.txtJson.Text = File.ReadAllText(filePath);

                this.LoadJsonTree();
            }
        }

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tvViwer_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.tvViwer.SelectedNode = e.Node;

                this.contextMenuStrip1.Show(Cursor.Position);
            }
        }

        private void txtJson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.V)
                {
                    this.txtJson.Text = "";

                    if (this.txtJson.Text.Length == 0 || this.txtJson.SelectedText.Length == this.txtJson.Text.Length)
                    {
                        e.SuppressKeyPress = true;

                        this.txtJson.Clear();

                        string content = Clipboard.GetText();

                        this.txtJson.Text = content;
                    }

                    this.LoadJsonTree();
                }
            }
        }

        private void tsmiLoadJsonTree_Click(object sender, EventArgs e)
        {
            this.LoadJsonTree();
        }

        private void tvViwer_AfterExpand(object sender, TreeViewEventArgs e)
        {
            this.tvViwer.Invalidate();
        }

        private void tsmiFormat_Click(object sender, EventArgs e)
        {
            string json = this.txtJson.Text.Trim();

            if (json.Length == 0)
            {
                return;
            }

            try
            {
                var result = JsonConvert.DeserializeObject(json);

                this.txtJson.Text = JsonConvert.SerializeObject(result, Formatting.Indented);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tvViwer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                if (this.tvViwer.Nodes.Count > 0)
                {
                    this.FindNode();
                }
            }
        }

        private void FindNode(bool findChild = false)
        {
            frmFindBox findBox = new frmFindBox(true);

            DialogResult result = findBox.ShowDialog();

            if (result == DialogResult.OK)
            {
                string word = findBox.FindWord;

                bool matchCase = findBox.MatchCase;
                bool matchWholeWord = findBox.MatchWholeWord;

                if (!string.IsNullOrEmpty(word))
                {
                    TreeNode node = findChild ? this.tvViwer.SelectedNode : this.tvViwer.Nodes[0];

                    TreeNode foundNode = this.FindTreeNode(node, word, matchCase, matchWholeWord);

                    if (foundNode != null)
                    {
                        this.tvViwer.SelectedNode = foundNode;

                        foundNode.EnsureVisible();
                    }
                    else
                    {
                        MessageBox.Show("Not found.");
                    }
                }
            }
        }

        private TreeNode FindTreeNode(TreeNode node, string word, bool matchCase, bool matchWholeWord)
        {
            TreeNodeTextInfo textInfo = node.Tag as TreeNodeTextInfo;

            string name = textInfo.Name;
            string value = textInfo.Value;

            if (name != null)
            {
                var matches = this.MatchWord(name, word, matchCase, matchWholeWord);

                if (matches != null && matches.Count > 0)
                {
                    return node;
                }
            }

            if (value != null)
            {
                var matches = this.MatchWord(value, word, matchCase, matchWholeWord);

                if (matches != null && matches.Count > 0)
                {
                    return node;
                }
            }

            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode child in node.Nodes)
                {
                    var foundNode = this.FindTreeNode(child, word, matchCase, matchWholeWord);

                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
            }

            return null;
        }

        private MatchCollection MatchWord(string value, string word, bool matchCase, bool matchWholeWord)
        {
            string pattern = word;

            if (matchWholeWord)
            {
                pattern = $@"\b({word})\b";
            }

            string regex = pattern;

            RegexOptions option = RegexOptions.Multiline;

            if (!matchCase)
            {
                option = option | RegexOptions.IgnoreCase;
            }

            regex = regex.Replace("[", "\\[").Replace("]", "\\]");

            try
            {
                MatchCollection matches = Regex.Matches(value, regex, option);

                return matches;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void tsmiFind_Click(object sender, EventArgs e)
        {
            this.FindNode();
        }

        private void tsmiFindChild_Click(object sender, EventArgs e)
        {
            this.FindNode(true);
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.tsmiFindChild.Visible = this.tvViwer.SelectedNode != null;
        }
    }

    public class TreeNodeTextInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool UseQuotation { get; set; }
    }
}
