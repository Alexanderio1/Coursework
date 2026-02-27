using System;
using System.Text;
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
            this.Activated += MainForm_Activated;
            rtbEditor.TextChanged += rtbEditor_TextChanged;
            rtbEditor.SelectionChanged += rtbEditor_SelectionChanged;
            UpdateTitle();
            UpdateCommandStates();
        }

        private void rtbEditor_TextChanged(object sender, EventArgs e)
        {
            if (_suppressDirty) return;

            _isDirty = true;
            UpdateTitle();
            UpdateCommandStates();
        }

        private void rtbEditor_SelectionChanged(object sender, EventArgs e)
        {
            UpdateCommandStates();
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath)
                ? "Untitled"
                : Path.GetFileName(_currentFilePath);

            Text = _isDirty ? $"{fileName} * - Analyser" : $"{fileName} - Analyser";
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
            UpdateCommandStates();
        }

        private void CmdSave_Click(object sender, EventArgs e)
        {
            SaveFile();
            UpdateCommandStates();
        }

        private void CmdSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileAs();
            UpdateCommandStates();
        }

        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
                return SaveFileAs();

            File.WriteAllText(_currentFilePath, rtbEditor.Text, Encoding.UTF8);
            _isDirty = false;

            UpdateTitle();
            UpdateCommandStates();
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

                rtbOutput.Clear();
                UpdateTitle();
                UpdateCommandStates();
            }
        }

        private void CmdExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CmdUndo_Click(object sender, EventArgs e)
        {
            if (rtbEditor.CanUndo) 
                rtbEditor.Undo();
            UpdateCommandStates();
        }

        private void CmdRedo_Click(object sender, EventArgs e)
        {
            if (rtbEditor.CanRedo)
                rtbEditor.Redo();
            UpdateCommandStates();
        }

        private void CmdCut_Click(object sender, EventArgs e)
        {
            rtbEditor.Cut();
            UpdateCommandStates();
        }

        private void CmdCopy_Click(object sender, EventArgs e)
        {
            rtbEditor.Copy();
            UpdateCommandStates();
        }

        private void CmdPaste_Click(object sender, EventArgs e)
        {
            rtbEditor.Paste();
            UpdateCommandStates();
        }

        private void CmdDelete_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectedText = "";
            UpdateCommandStates();
        }

        private void CmdSelectAll_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectAll();
            UpdateCommandStates();
        }

        private void CmdHelp_Click(object sender, EventArgs e)
        {
            using (var form = new HelpForm())
            {
                form.ShowDialog(this);
            }
        }

        private void CmdAbout_Click(object sender, EventArgs e)
        {
            using (var form = new AboutForm())
            {
                form.ShowDialog(this);
            }
        }

        private void CmdRun_Click(object sender, EventArgs e)
        {
            rtbOutput.Clear();
            rtbOutput.AppendText("Запуск языкового процессора пока не реализован." + Environment.NewLine);
        }

        private bool ClipboardHasText()
        {
            try
            {
                return Clipboard.ContainsText();
            }
            catch
            {
                return false;
            }
        }

        private void UpdateCommandStates()
        {
            bool hasText = !string.IsNullOrEmpty(rtbEditor.Text);
            bool hasSelection = rtbEditor.SelectionLength > 0;
            bool canPaste = ClipboardHasText();

            btnSaveAs.Enabled = true;
            btnExit.Enabled = true;

            miSave.Enabled = _isDirty;
            btnSave.Enabled = _isDirty;

            miUndo.Enabled = rtbEditor.CanUndo;
            btnUndo.Enabled = rtbEditor.CanUndo;

            miRedo.Enabled = rtbEditor.CanRedo;
            btnRedo.Enabled = rtbEditor.CanRedo;

            miCut.Enabled = hasSelection;
            btnCut.Enabled = hasSelection;

            miCopy.Enabled = hasSelection;
            btnCopy.Enabled = hasSelection;

            miDelete.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;

            miPaste.Enabled = canPaste;
            btnPaste.Enabled = canPaste;

            miSelectAll.Enabled = hasText;
            btnSelectAll.Enabled = hasText;

            miRunExecute.Enabled = hasText;
            btnRun.Enabled = hasText;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            UpdateCommandStates();
        }

    }

}

