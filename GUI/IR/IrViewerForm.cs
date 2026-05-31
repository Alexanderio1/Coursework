using System.Drawing;
using System.Windows.Forms;

namespace GUI.IR
{
    public sealed class IrViewerForm : Form
    {
        public IrViewerForm(string content)
        {
            Text = "IR и локальные оптимизации";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1100;
            Height = 800;

            RichTextBox textBox = new RichTextBox();
            textBox.Dock = DockStyle.Fill;
            textBox.ReadOnly = true;
            textBox.WordWrap = false;
            textBox.ScrollBars = RichTextBoxScrollBars.Both;
            textBox.Font = new Font("Consolas", 11.0f);
            textBox.Text = content;

            Controls.Add(textBox);
        }
    }
}