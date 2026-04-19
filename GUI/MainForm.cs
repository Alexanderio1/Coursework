using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace GUI
{
    public partial class MainForm : Form
    {
        private string _currentFilePath = null;
        private bool _isDirty = false;
        private bool _suppressDirty = false;

        private sealed class SearchResultItem
        {
            public string Value { get; set; }
            public int StartIndex { get; set; }
            public int Length { get; set; }
            public int Line { get; set; }
            public int Column { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            this.FormClosing += MainForm_FormClosing;
            this.Activated += MainForm_Activated;
            rtbEditor.TextChanged += rtbEditor_TextChanged;
            rtbEditor.SelectionChanged += rtbEditor_SelectionChanged;
            dgvResults.SelectionChanged += dgvResults_SelectionChanged;

            if (tsCmbSearchType.Items.Count > 0)
                tsCmbSearchType.SelectedIndex = 0;
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
            string text = rtbEditor.Text;

            dgvResults.Rows.Clear();
            tsLblMatchCount.Text = "Найдено: 0";
            ClearHighlights();

            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show(
                    "Нет данных для поиска",
                    "Поиск",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (tsCmbSearchType.SelectedIndex < 0)
            {
                MessageBox.Show(
                    "Выберите тип поиска.",
                    "Поиск",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string pattern = GetSelectedPattern();
            List<SearchResultItem> results = FindMatches(text, pattern);

            FillResultsGrid(results);

            if (results.Count == 0)
            {
                MessageBox.Show(
                    "Совпадения не найдены.",
                    "Поиск",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private string GetSelectedPattern()
        {
            switch (tsCmbSearchType.SelectedIndex)
            {
                case 0:
                    return @"</(?:p|li|h3)>";

                case 1:
                    return @"(?<![\w.,])[+-]?(?:0|[1-9]\d*)(?:[.,]\d+)?(?![\w.,])";

                case 2:
                    return @"(?<!\w)(?:(?:[0-8]?\d)°(?:[0-5]\d)'(?:[0-5]\d)""[NS]|90°00'00""[NS])(?!\w)";

                default:
                    return null;
            }
        }

        private List<SearchResultItem> FindMatches(string text, string pattern)
        {
            var results = new List<SearchResultItem>();

            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pattern))
                return results;

            MatchCollection matches = Regex.Matches(
                text,
                pattern,
                RegexOptions.Multiline | RegexOptions.CultureInvariant);

            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;

                var position = GetLineAndColumn(match.Index);

                results.Add(new SearchResultItem
                {
                    Value = match.Value,
                    StartIndex = match.Index,
                    Length = match.Length,
                    Line = position.Item1,
                    Column = position.Item2
                });
            }

            return results;
        }

        private Tuple<int, int> GetLineAndColumn(int startIndex)
        {
            int lineIndex = rtbEditor.GetLineFromCharIndex(startIndex);
            int firstCharIndex = rtbEditor.GetFirstCharIndexFromLine(lineIndex);
            int columnIndex = startIndex - firstCharIndex;

            return Tuple.Create(lineIndex + 1, columnIndex + 1);
        }

        private void FillResultsGrid(List<SearchResultItem> results)
        {
            dgvResults.Rows.Clear();

            foreach (SearchResultItem item in results)
            {
                dgvResults.Rows.Add(
                    item.Value,
                    item.Line,
                    item.Column,
                    item.Length,
                    item.StartIndex);
            }

            tsLblMatchCount.Text = $"Найдено: {results.Count}";

            if (dgvResults.Rows.Count > 0)
            {
                dgvResults.ClearSelection();
                dgvResults.CurrentCell = dgvResults.Rows[0].Cells[0];
                dgvResults.Rows[0].Selected = true;
                HighlightSelectedResult();
            }
        }

        private void ClearHighlights()
        {
            int savedStart = rtbEditor.SelectionStart;
            int savedLength = rtbEditor.SelectionLength;

            rtbEditor.SelectAll();
            rtbEditor.SelectionBackColor = rtbEditor.BackColor;

            rtbEditor.Select(savedStart, savedLength);
            rtbEditor.SelectionBackColor = rtbEditor.BackColor;
        }

        private void HighlightMatch(int startIndex, int length)
        {
            if (startIndex < 0 || length <= 0 || startIndex + length > rtbEditor.TextLength)
                return;

            ClearHighlights();

            rtbEditor.Select(startIndex, length);
            rtbEditor.SelectionBackColor = Color.Yellow;
            rtbEditor.ScrollToCaret();
            rtbEditor.Focus();
        }

        private void HighlightSelectedResult()
        {
            if (dgvResults.SelectedRows.Count == 0)
                return;

            DataGridViewRow row = dgvResults.SelectedRows[0];

            if (row.Cells[colStartIndex.Index].Value == null ||
                row.Cells[colLength.Index].Value == null)
                return;

            int startIndex = Convert.ToInt32(row.Cells[colStartIndex.Index].Value);
            int length = Convert.ToInt32(row.Cells[colLength.Index].Value);

            HighlightMatch(startIndex, length);
        }

        private void dgvResults_SelectionChanged(object sender, EventArgs e)
        {
            HighlightSelectedResult();
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

