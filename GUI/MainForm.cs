using GUI.ANTLR;
using GUI.Lexer;
using GUI.Syntax;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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

        private void ClearResultsGrid()
        {
            dgvResults.Rows.Clear();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ConfirmSaveIfDirty())
                e.Cancel = true;
        }

        private void RenderAnalysisResult(LexerResult result)
        {
            ClearResultsGrid();
            ConfigureResultsGridForLexer();

            foreach (var item in result.Items)
            {
                int rowIndex = dgvResults.Rows.Add(
                    item.DisplayCode,
                    item.TypeName,
                    item.DisplayText,
                    item.LocationText);

                var row = dgvResults.Rows[rowIndex];
                row.Tag = item;

                if (item.IsError)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }

        private void CmdNew_Click(object sender, EventArgs e)
        {
            if (!ConfirmSaveIfDirty()) return;

            _suppressDirty = true;
            rtbEditor.Clear();
            ClearResultsGrid();
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

                ClearResultsGrid();
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
            var lexer = new LexicalAnalyzer();
            var lexResult = lexer.Analyze(rtbEditor.Text);

            var parserTokens = lexResult.Items
                .Where(x => x.Code.HasValue)
                .ToList();

            var parser = new SyntaxAnalyzer();
            var syntaxResult = parser.Parse(parserTokens);

            var lexicalErrors = lexResult.Items
                .Where(x => x.IsError)
                .Select(x => new SyntaxError
                {
                    InvalidFragment = string.IsNullOrWhiteSpace(x.Lexeme) ? "(пусто)" : x.Lexeme,
                    Line = x.Line,
                    StartColumn = x.StartColumn,
                    EndColumn = x.EndColumn,
                    AbsoluteIndex = x.AbsoluteIndex,
                    Message = string.IsNullOrWhiteSpace(x.Message) ? x.DisplayText : x.Message
                });

            var allErrors = lexicalErrors
                .Concat(syntaxResult.Errors)
                .OrderBy(x => x.AbsoluteIndex)
                .ThenBy(x => x.Line)
                .ThenBy(x => x.StartColumn)
                .ToList();

            syntaxResult.Errors.Clear();
            syntaxResult.Errors.AddRange(allErrors);

            RenderSyntaxResult(syntaxResult);
        }

        private System.Collections.Generic.List<SyntaxError> MergeErrors(
    LexerResult lexResult,
    SyntaxResult syntaxResult)
        {
            var lexicalErrors = lexResult.Items
                .Where(x => x.IsError)
                .Select(x => new SyntaxError
                {
                    InvalidFragment = string.IsNullOrWhiteSpace(x.Lexeme) ? "(пусто)" : x.Lexeme,
                    Line = x.Line,
                    StartColumn = x.StartColumn,
                    EndColumn = x.EndColumn,
                    AbsoluteIndex = x.AbsoluteIndex,
                    Message = string.IsNullOrWhiteSpace(x.Message) ? x.DisplayText : x.Message
                })
                .ToList();

            var filteredSyntaxErrors = syntaxResult.Errors
                .Where(x => !ShouldSuppressSyntaxError(x, lexicalErrors))
                .ToList();

            return lexicalErrors
                .Concat(filteredSyntaxErrors)
                .OrderBy(x => x.AbsoluteIndex)
                .ThenBy(x => x.Line)
                .ThenBy(x => x.StartColumn)
                .ToList();
        }

        private bool ShouldSuppressSyntaxError(
            SyntaxError syntaxError,
            System.Collections.Generic.List<SyntaxError> lexicalErrors)
        {
            if (lexicalErrors.Any(x => RangesOverlap(x, syntaxError)))
                return true;

            if (syntaxError.Message == "Ожидалось ключевое слово val" &&
                lexicalErrors.Any(x =>
                    x.Line == syntaxError.Line &&
                    GetAbsoluteEndIndex(x) < syntaxError.AbsoluteIndex))
            {
                return true;
            }

            if (syntaxError.Message == "Ожидалась лексема listOf" &&
                lexicalErrors.Any(x =>
                    x.Line == syntaxError.Line &&
                    GetAbsoluteEndIndex(x) < syntaxError.AbsoluteIndex &&
                    GetNextNonWhitespaceCharAfter(GetAbsoluteEndIndex(x)) == '('))
            {
                return true;
            }

            return false;
        }

        private bool RangesOverlap(SyntaxError left, SyntaxError right)
        {
            int leftStart = left.AbsoluteIndex;
            int leftEnd = GetAbsoluteEndIndex(left);

            int rightStart = right.AbsoluteIndex;
            int rightEnd = GetAbsoluteEndIndex(right);

            return leftStart <= rightEnd && rightStart <= leftEnd;
        }

        private int GetAbsoluteEndIndex(SyntaxError error)
        {
            int length = 1;

            if (!string.IsNullOrWhiteSpace(error.InvalidFragment) &&
                error.InvalidFragment != "(пусто)")
            {
                length = error.InvalidFragment.Length;
            }
            else if (error.EndColumn >= error.StartColumn)
            {
                length = error.EndColumn - error.StartColumn + 1;
            }

            return error.AbsoluteIndex + Math.Max(length, 1) - 1;
        }

        private char? GetNextNonWhitespaceCharAfter(int absoluteIndex)
        {
            string text = rtbEditor.Text;

            for (int i = absoluteIndex + 1; i < text.Length; i++)
            {
                char ch = text[i];

                if (ch == '\r' || ch == '\n')
                    return null;

                if (!char.IsWhiteSpace(ch))
                    return ch;
            }

            return null;
        }
        private void RenderSyntaxResult(SyntaxResult result)
        {
            ClearResultsGrid();
            ConfigureResultsGridForSyntax();

            if (!result.HasErrors)
            {
                dgvResults.Rows.Add(
                    "-",
                    "-",
                    "Синтаксический анализ завершён успешно. Ошибок не обнаружено.",
                    string.Empty
                );
                return;
            }

            foreach (var error in result.Errors)
            {
                int rowIndex = dgvResults.Rows.Add(
                    string.IsNullOrWhiteSpace(error.InvalidFragment) ? "(пусто)" : error.InvalidFragment,
                    error.LocationText,
                    error.Message,
                    string.Empty
                );

                var row = dgvResults.Rows[rowIndex];
                row.Tag = error;
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }

            int totalRowIndex = dgvResults.Rows.Add(
                "Общее количество ошибок",
                "-",
                result.ErrorCount.ToString(),
                string.Empty
            );

            var totalRow = dgvResults.Rows[totalRowIndex];
            totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            totalRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
            totalRow.Tag = null;
        }

        private void dgvResults_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dgvResults.Rows[e.RowIndex];

            if (row.Tag is LexerItem lexerItem)
            {
                HighlightRange(
                    lexerItem.Line,
                    lexerItem.StartColumn,
                    lexerItem.Line,
                    lexerItem.EndColumn);
                return;
            }

            if (row.Tag is SyntaxError syntaxError)
            {
                HighlightRange(
                    syntaxError.Line,
                    syntaxError.StartColumn,
                    syntaxError.Line,
                    syntaxError.EndColumn);
            }

            if (row.Tag is AntlrSyntaxError antlrError)
            {
                HighlightRange(
                    antlrError.Line,
                    antlrError.StartColumn,
                    antlrError.Line,
                    antlrError.EndColumn);
                return;
            }
        }

        private int GetCharIndexFromLineColumn(int line, int column)
        {
            if (line < 1)
                return 0;

            int firstChar = rtbEditor.GetFirstCharIndexFromLine(line - 1);
            if (firstChar < 0)
                return rtbEditor.TextLength;

            int index = firstChar + Math.Max(0, column - 1);
            return Math.Min(index, rtbEditor.TextLength);
        }

        private void HighlightRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            int startIndex = GetCharIndexFromLineColumn(startLine, startColumn);
            int endIndex = GetCharIndexFromLineColumn(endLine, endColumn);

            if (endIndex < startIndex)
                endIndex = startIndex;

            int length = Math.Max(1, endIndex - startIndex + 1);

            rtbEditor.Focus();
            rtbEditor.Select(startIndex, length);
            rtbEditor.ScrollToCaret();
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

        private void ConfigureResultsGridForLexer()
        {
            if (dgvResults.Columns.Count < 4)
                return;

            dgvResults.Columns[0].Visible = true;
            dgvResults.Columns[1].Visible = true;
            dgvResults.Columns[2].Visible = true;
            dgvResults.Columns[3].Visible = true;

            dgvResults.Columns[0].HeaderText = "Условный код";
            dgvResults.Columns[1].HeaderText = "Тип лексемы";
            dgvResults.Columns[2].HeaderText = "Лексема / Сообщение";
            dgvResults.Columns[3].HeaderText = "Местоположение";

            dgvResults.Columns[0].Width = 120;
            dgvResults.Columns[1].Width = 220;
            dgvResults.Columns[2].Width = 420;
            dgvResults.Columns[3].Width = 180;
        }

        private void ConfigureResultsGridForSyntax()
        {
            if (dgvResults.Columns.Count < 4)
                return;

            dgvResults.Columns[0].Visible = true;
            dgvResults.Columns[1].Visible = true;
            dgvResults.Columns[2].Visible = true;
            dgvResults.Columns[3].Visible = false;

            dgvResults.Columns[0].HeaderText = "Неверный фрагмент";
            dgvResults.Columns[1].HeaderText = "Местоположение";
            dgvResults.Columns[2].HeaderText = "Описание";

            dgvResults.Columns[0].Width = 220;
            dgvResults.Columns[1].Width = 220;
            dgvResults.Columns[2].Width = 520;
        }

        private void CmdRunAntlr_Click(object sender, EventArgs e)
        {
            var antlrAnalyzer = new AntlrAnalyzer();
            var antlrResult = antlrAnalyzer.Analyze(rtbEditor.Text);

            RenderAntlrSyntaxResult(antlrResult);
        }

        private void RenderAntlrSyntaxResult(AntlrSyntaxResult result)
        {
            ClearResultsGrid();
            ConfigureResultsGridForSyntax();

            if (!result.HasErrors)
            {
                dgvResults.Rows.Add(
                    "-",
                    "-",
                    "ANTLR-анализ завершён успешно. Ошибок не обнаружено.",
                    string.Empty
                );
                return;
            }

            foreach (var error in result.Errors)
            {
                int rowIndex = dgvResults.Rows.Add(
                    string.IsNullOrWhiteSpace(error.InvalidFragment) ? "(пусто)" : error.InvalidFragment,
                    error.LocationText,
                    error.Message,
                    string.Empty
                );

                var row = dgvResults.Rows[rowIndex];
                row.Tag = error;
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }

            int totalRowIndex = dgvResults.Rows.Add(
                "Общее количество ошибок",
                "-",
                result.ErrorCount.ToString(),
                string.Empty
            );

            var totalRow = dgvResults.Rows[totalRowIndex];
            totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            totalRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
            totalRow.Tag = null;
        }
    }

}

