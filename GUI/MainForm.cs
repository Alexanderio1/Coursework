using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GUI
{
    public partial class MainForm : Form
    {
        private string _currentFilePath = null;
        private bool _isDirty = false;
        private bool _suppressDirty = false;

        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += MainForm_FormClosing;
            rtbEditor.TextChanged += rtbEditor_TextChanged;
        }

        private void rtbEditor_TextChanged(object sender, EventArgs e)
        {
            if (_suppressDirty) return;

            _isDirty = true;
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath)
                ? "Untitled"
                : Path.GetFileName(_currentFilePath);

            Text = _isDirty ? $"{fileName} * - GUI" : $"{fileName} - GUI";
        }

        private bool ConfirmSaveIfDirty()
        {
            if (!_isDirty) return true;

            var result = MessageBox.Show(
                "Файл был изменён. Сохранить изменения?",
                "Сохранение",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Cancel) return false;
            if (result == DialogResult.No) return true;

            return SaveFile();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmSaveIfDirty())
                e.Cancel = true;
        }

        private void CmdNew_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveIfDirty()) return;

            _suppressDirty = true;
            rtbEditor.Clear();
            rtbOutput.Clear();
            _suppressDirty = false;

            _currentFilePath = null;
            _isDirty = false;
            UpdateTitle();
        }

        private void CmdSave_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void CmdSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileAs();
        }

        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
                return SaveFileAs();

            File.WriteAllText(_currentFilePath, rtbEditor.Text, Encoding.UTF8);
            _isDirty = false;

            rtbOutput.AppendText($"Сохранено: {_currentFilePath}{Environment.NewLine}");
            UpdateTitle();
            return true;
        }

        private bool SaveFileAs()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                sfd.Title = "Сохранить как";

                if (sfd.ShowDialog() != DialogResult.OK) return false;

                _currentFilePath = sfd.FileName;
                return SaveFile();
            }
        }

        private void CmdOpen_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveIfDirty()) return;

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                ofd.Title = "Открыть файл";

                if (ofd.ShowDialog() != DialogResult.OK) return;

                _suppressDirty = true;
                rtbEditor.Text = File.ReadAllText(ofd.FileName, Encoding.UTF8);
                _suppressDirty = false;

                _currentFilePath = ofd.FileName;
                _isDirty = false;

                rtbOutput.AppendText($"Открыт файл: {_currentFilePath}{Environment.NewLine}");
                UpdateTitle();
            }
        }

        private void CmdExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CmdUndo_Click(object sender, EventArgs e)
        {
            if (rtbEditor.CanUndo) rtbEditor.Undo();
        }

        private void CmdRedo_Click(object sender, EventArgs e)
        {
            try { rtbEditor.Redo(); } catch { }
        }

        private void CmdCut_Click(object sender, EventArgs e)
        {
            rtbEditor.Cut();
        }

        private void CmdCopy_Click(object sender, EventArgs e)
        {
            rtbEditor.Copy();
        }

        private void CmdPaste_Click(object sender, EventArgs e)
        {
            rtbEditor.Paste();
        }

        private void CmdDelete_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectedText = "";
        }

        private void CmdSelectAll_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectAll();
        }

        private void CmdHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "Реализовано: Файл/Правка/Справка.\nПункты «Текст» и «Пуск» пока заглушки.",
            "Справка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        }

        private void CmdAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
            "GUI (ЛР1)\nАвтор: Костоломов А.Е.\nГруппа: АВТ-314",
            "О программе",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        }

        private void CmdRun_Click(object sender, EventArgs e)
        {
            rtbOutput.AppendText("Пуск: анализатор будет реализован позже.\n");
        }

        private void CmdTextStub_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripItem item)
                MessageBox.Show($"Раздел «{item.Text}» будет реализован позже.", "Текст");
        }
    }

}

