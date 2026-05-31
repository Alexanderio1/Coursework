using GUI.ANTLR;
using GUI.Expressions;
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

        private ToolStripMenuItem miFormat;
        private ToolStripMenuItem miFontDialog;
        private ToolStripMenuItem miIncreaseFont;
        private ToolStripMenuItem miDecreaseFont;
        private ToolStripMenuItem miResetFont;
        private ToolStripMenuItem miWordWrap;

        private ToolStripButton btnFontDialog;
        private ToolStripButton btnIncreaseFont;
        private ToolStripButton btnDecreaseFont;
        private ToolStripComboBox cmbFontSize;

        private ToolStripMenuItem miLab6ExpressionLexer;
        private ToolStripButton btnLab6ExpressionLexer;

        private ToolStripMenuItem miLab6ExpressionSyntax;
        private ToolStripButton btnLab6ExpressionSyntax;

        private ToolStripMenuItem miLab6ExpressionQuadruples;
        private ToolStripButton btnLab6ExpressionQuadruples;

        private ToolStripMenuItem miLab6ExpressionPoliz;
        private ToolStripButton btnLab6ExpressionPoliz;

        private float _defaultEditorFontSize;

        private bool _suppressFontSizeComboChanged = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeEditorFormattingControls();
            InitializeExpressionLabControls();

            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
            this.FormClosing += MainForm_FormClosing;
            this.Activated += MainForm_Activated;
            rtbEditor.TextChanged += rtbEditor_TextChanged;
            rtbEditor.SelectionChanged += rtbEditor_SelectionChanged;
            UpdateTitle();
            UpdateCommandStates();
        }

        private void InitializeExpressionLabControls()
        {
            var lab6Menu = new ToolStripMenuItem("ЛР6");

            miLab6ExpressionLexer = new ToolStripMenuItem("Лексический анализ выражения");
            miLab6ExpressionLexer.ShortcutKeys = Keys.Control | Keys.Shift | Keys.L;
            miLab6ExpressionLexer.Click += CmdRunExpressionLexer_Click;

            miLab6ExpressionSyntax = new ToolStripMenuItem("Синтаксический анализ выражения");
            miLab6ExpressionSyntax.ShortcutKeys = Keys.Control | Keys.Shift | Keys.P;
            miLab6ExpressionSyntax.Click += CmdRunExpressionSyntax_Click;

            miLab6ExpressionQuadruples = new ToolStripMenuItem("Построить тетрады");
            miLab6ExpressionQuadruples.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Q;
            miLab6ExpressionQuadruples.Click += CmdRunExpressionQuadruples_Click;

            miLab6ExpressionPoliz = new ToolStripMenuItem("Построить ПОЛИЗ");
            miLab6ExpressionPoliz.ShortcutKeys = Keys.Control | Keys.Shift | Keys.R;
            miLab6ExpressionPoliz.Click += CmdRunExpressionPoliz_Click;

            lab6Menu.DropDownItems.Add(miLab6ExpressionLexer);
            lab6Menu.DropDownItems.Add(miLab6ExpressionSyntax);
            lab6Menu.DropDownItems.Add(miLab6ExpressionQuadruples);
            lab6Menu.DropDownItems.Add(miLab6ExpressionPoliz);

            menuMain.Items.Add(lab6Menu);

            btnLab6ExpressionLexer = new ToolStripButton("ЛР6 Лексер");
            btnLab6ExpressionLexer.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLab6ExpressionLexer.ToolTipText = "Лексический анализ арифметического выражения";
            btnLab6ExpressionLexer.Click += CmdRunExpressionLexer_Click;

            btnLab6ExpressionSyntax = new ToolStripButton("ЛР6 Парсер");
            btnLab6ExpressionSyntax.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLab6ExpressionSyntax.ToolTipText = "Синтаксический анализ арифметического выражения";
            btnLab6ExpressionSyntax.Click += CmdRunExpressionSyntax_Click;

            btnLab6ExpressionQuadruples = new ToolStripButton("ЛР6 Тетрады");
            btnLab6ExpressionQuadruples.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLab6ExpressionQuadruples.ToolTipText = "Построение тетрад для арифметического выражения";
            btnLab6ExpressionQuadruples.Click += CmdRunExpressionQuadruples_Click;

            btnLab6ExpressionPoliz = new ToolStripButton("ЛР6 ПОЛИЗ");
            btnLab6ExpressionPoliz.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnLab6ExpressionPoliz.ToolTipText = "Построение ПОЛИЗ и вычисление выражения";
            btnLab6ExpressionPoliz.Click += CmdRunExpressionPoliz_Click;

            toolMain.Items.Add(new ToolStripSeparator());
            toolMain.Items.Add(btnLab6ExpressionLexer);
            toolMain.Items.Add(btnLab6ExpressionSyntax);
            toolMain.Items.Add(btnLab6ExpressionQuadruples);
            toolMain.Items.Add(btnLab6ExpressionPoliz);
        }

        private void CmdRunExpressionPoliz_Click(object sender, EventArgs e)
        {
            var lexer = new ExpressionLexer();
            var lexicalResult = lexer.Analyze(rtbEditor.Text);

            if (lexicalResult.HasErrors)
            {
                RenderExpressionLexicalResult(lexicalResult);
                return;
            }

            var parser = new ExpressionParser();
            var parseResult = parser.Analyze(lexicalResult.Tokens);

            if (parseResult.HasErrors)
            {
                RenderExpressionSyntaxResult(parseResult);
                return;
            }

            var polizBuilder = new PolizBuilder();
            var polizResult = polizBuilder.Build(parseResult.Root);

            var evaluator = new PolizEvaluator();
            var evaluationResult = evaluator.Evaluate(polizResult.Items);

            RenderExpressionPolizResult(polizResult, evaluationResult);
        }

        private void ConfigureResultsGridForPoliz()
        {
            if (dgvResults.Columns.Count < 4)
            {
                return;
            }

            dgvResults.Columns[0].Visible = true;
            dgvResults.Columns[1].Visible = true;
            dgvResults.Columns[2].Visible = true;
            dgvResults.Columns[3].Visible = true;

            dgvResults.Columns[0].HeaderText = "№";
            dgvResults.Columns[1].HeaderText = "Элемент";
            dgvResults.Columns[2].HeaderText = "Действие";
            dgvResults.Columns[3].HeaderText = "Стек / Результат";

            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResults.Columns[0].FillWeight = 10;
            dgvResults.Columns[1].FillWeight = 20;
            dgvResults.Columns[2].FillWeight = 40;
            dgvResults.Columns[3].FillWeight = 30;
        }

        private void RenderExpressionPolizResult(
    PolizGenerationResult polizResult,
    PolizEvaluationResult evaluationResult)
        {
            ClearResultsGrid();
            ConfigureResultsGridForPoliz();

            int polizRowIndex = dgvResults.Rows.Add(
                "ПОЛИЗ",
                "-",
                "Сформированная польская инверсная запись",
                string.IsNullOrEmpty(polizResult.PolizText) ? "(пусто)" : polizResult.PolizText);

            var polizRow = dgvResults.Rows[polizRowIndex];
            polizRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            polizRow.DefaultCellStyle.ForeColor = Color.DarkBlue;

            if (!evaluationResult.CanEvaluate)
            {
                int rowIndex = dgvResults.Rows.Add(
                    "Вычисление",
                    "-",
                    "Вычисление не выполняется",
                    evaluationResult.Errors.Count > 0
                        ? evaluationResult.Errors[0]
                        : "Выражение содержит идентификаторы");

                var row = dgvResults.Rows[rowIndex];
                row.DefaultCellStyle.BackColor = Color.LightYellow;
                row.DefaultCellStyle.ForeColor = Color.DarkGoldenrod;

                return;
            }

            foreach (var step in evaluationResult.Steps)
            {
                int rowIndex = dgvResults.Rows.Add(
                    step.Number.ToString(),
                    step.Token,
                    step.Action,
                    step.StackState);

                dgvResults.Rows[rowIndex].Tag = step;
            }

            if (evaluationResult.HasErrors)
            {
                foreach (var error in evaluationResult.Errors)
                {
                    int rowIndex = dgvResults.Rows.Add(
                        "Ошибка",
                        "-",
                        "Ошибка вычисления ПОЛИЗ",
                        error);

                    var row = dgvResults.Rows[rowIndex];
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }

                return;
            }

            if (evaluationResult.Success)
            {
                int totalRowIndex = dgvResults.Rows.Add(
                    "Итог",
                    "-",
                    "Вычисление завершено успешно",
                    evaluationResult.Value.ToString());

                var totalRow = dgvResults.Rows[totalRowIndex];
                totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
                totalRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
            }
        }
        private void CmdRunExpressionQuadruples_Click(object sender, EventArgs e)
        {
            var lexer = new ExpressionLexer();
            var lexicalResult = lexer.Analyze(rtbEditor.Text);

            if (lexicalResult.HasErrors)
            {
                RenderExpressionLexicalResult(lexicalResult);
                return;
            }

            var parser = new ExpressionParser();
            var parseResult = parser.Analyze(lexicalResult.Tokens);

            if (parseResult.HasErrors)
            {
                RenderExpressionSyntaxResult(parseResult);
                return;
            }

            var generator = new QuadrupleGenerator();
            var quadrupleResult = generator.Generate(parseResult.Root);

            RenderExpressionQuadruplesResult(quadrupleResult);
        }

        private void ConfigureResultsGridForQuadruples()
        {
            if (dgvResults.Columns.Count < 4)
            {
                return;
            }

            dgvResults.Columns[0].Visible = true;
            dgvResults.Columns[1].Visible = true;
            dgvResults.Columns[2].Visible = true;
            dgvResults.Columns[3].Visible = true;

            dgvResults.Columns[0].HeaderText = "№";
            dgvResults.Columns[1].HeaderText = "Операция";
            dgvResults.Columns[2].HeaderText = "Аргументы";
            dgvResults.Columns[3].HeaderText = "Результат";

            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResults.Columns[0].FillWeight = 10;
            dgvResults.Columns[1].FillWeight = 20;
            dgvResults.Columns[2].FillWeight = 45;
            dgvResults.Columns[3].FillWeight = 25;
        }

        private void RenderExpressionQuadruplesResult(QuadrupleGenerationResult result)
        {
            ClearResultsGrid();
            ConfigureResultsGridForQuadruples();

            if (result.Quadruples.Count == 0)
            {
                int emptyRowIndex = dgvResults.Rows.Add(
                    "-",
                    "-",
                    "Операции отсутствуют",
                    "Выражение является одиночным операндом");

                var emptyRow = dgvResults.Rows[emptyRowIndex];
                emptyRow.DefaultCellStyle.BackColor = Color.AliceBlue;
                emptyRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
                return;
            }

            foreach (var quadruple in result.Quadruples)
            {
                int rowIndex = dgvResults.Rows.Add(
                    quadruple.Number.ToString(),
                    quadruple.Operator,
                    string.Format("arg1 = {0}; arg2 = {1}", quadruple.Arg1, quadruple.Arg2),
                    quadruple.Result);

                var row = dgvResults.Rows[rowIndex];
                row.Tag = quadruple;
            }

            int totalRowIndex = dgvResults.Rows.Add(
                "Итог",
                "-",
                string.Format("Количество тетрад: {0}", result.Quadruples.Count),
                string.Format("Результат выражения: {0}", result.ResultName));

            var totalRow = dgvResults.Rows[totalRowIndex];
            totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            totalRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
        }
        private void CmdRunExpressionSyntax_Click(object sender, EventArgs e)
        {
            var lexer = new ExpressionLexer();
            var lexicalResult = lexer.Analyze(rtbEditor.Text);

            if (lexicalResult.HasErrors)
            {
                RenderExpressionLexicalResult(lexicalResult);
                return;
            }

            var parser = new ExpressionParser();
            var parseResult = parser.Analyze(lexicalResult.Tokens);

            RenderExpressionSyntaxResult(parseResult);
        }

        private void RenderExpressionSyntaxResult(ExpressionParseResult result)
        {
            ClearResultsGrid();
            ConfigureResultsGridForExpressionSyntax();

            foreach (var error in result.Errors)
            {
                int rowIndex = dgvResults.Rows.Add(
                    "Ошибка",
                    "Синтаксическая ошибка",
                    string.IsNullOrEmpty(error.Fragment) ? "(пусто)" : error.Fragment,
                    string.Format("строка {0}, столбец {1}: {2}", error.Line, error.Column, error.Message)
                );

                var row = dgvResults.Rows[rowIndex];
                row.Tag = error;
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }

            int totalRowIndex = dgvResults.Rows.Add(
                "Итог",
                "-",
                result.HasErrors
                    ? "Синтаксический анализ завершён с ошибками"
                    : "Синтаксический анализ завершён успешно",
                result.HasErrors
                    ? string.Format("Количество ошибок: {0}. Тетрады и ПОЛИЗ не формируются.", result.Errors.Count)
                    : "Выражение соответствует грамматике E → T A"
            );

            var totalRow = dgvResults.Rows[totalRowIndex];
            totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            totalRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
            totalRow.Tag = null;
        }

        private void ConfigureResultsGridForExpressionSyntax()
        {
            if (dgvResults.Columns.Count < 4)
            {
                return;
            }

            dgvResults.Columns[0].Visible = true;
            dgvResults.Columns[1].Visible = true;
            dgvResults.Columns[2].Visible = true;
            dgvResults.Columns[3].Visible = true;

            dgvResults.Columns[0].HeaderText = "Код";
            dgvResults.Columns[1].HeaderText = "Тип";
            dgvResults.Columns[2].HeaderText = "Фрагмент";
            dgvResults.Columns[3].HeaderText = "Описание";

            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResults.Columns[0].FillWeight = 20;
            dgvResults.Columns[1].FillWeight = 25;
            dgvResults.Columns[2].FillWeight = 25;
            dgvResults.Columns[3].FillWeight = 30;
        }
        private void CmdRunExpressionLexer_Click(object sender, EventArgs e)
        {
            var lexer = new ExpressionLexer();
            var result = lexer.Analyze(rtbEditor.Text);

            RenderExpressionLexicalResult(result);
        }

        private void RenderExpressionLexicalResult(ExpressionLexicalResult result)
        {
            ClearResultsGrid();
            ConfigureResultsGridForExpressionLexer();

            foreach (var token in result.Tokens)
            {
                if (token.Code == ExpressionTokenCode.EndOfInput)
                {
                    continue;
                }

                int rowIndex = dgvResults.Rows.Add(
                    token.Code.ToString(),
                    "Токен",
                    string.IsNullOrEmpty(token.Text) ? "(пусто)" : token.Text,
                    string.Format("строка {0}, столбец {1}", token.Line, token.Column)
                );

                var row = dgvResults.Rows[rowIndex];
                row.Tag = token;

                if (token.Code == ExpressionTokenCode.Unknown)
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }

            foreach (var error in result.Errors)
            {
                int rowIndex = dgvResults.Rows.Add(
                    "Ошибка",
                    "Лексическая ошибка",
                    string.IsNullOrEmpty(error.Fragment) ? "(пусто)" : error.Fragment,
                    string.Format("строка {0}, столбец {1}: {2}", error.Line, error.Column, error.Message)
                );

                var row = dgvResults.Rows[rowIndex];
                row.Tag = error;
                row.DefaultCellStyle.BackColor = Color.MistyRose;
                row.DefaultCellStyle.ForeColor = Color.DarkRed;
            }

            int totalRowIndex = dgvResults.Rows.Add(
                "Итог",
                "-",
                result.HasErrors ? "Лексический анализ завершён с ошибками" : "Лексический анализ завершён успешно",
                string.Format("Количество ошибок: {0}", result.Errors.Count)
            );

            var totalRow = dgvResults.Rows[totalRowIndex];
            totalRow.DefaultCellStyle.BackColor = Color.AliceBlue;
            totalRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
            totalRow.Tag = null;
        }

        private void ConfigureResultsGridForExpressionLexer()
        {
            if (dgvResults.Columns.Count < 4)
            {
                return;
            }

            dgvResults.Columns[0].Visible = true;
            dgvResults.Columns[1].Visible = true;
            dgvResults.Columns[2].Visible = true;
            dgvResults.Columns[3].Visible = true;

            dgvResults.Columns[0].HeaderText = "Код";
            dgvResults.Columns[1].HeaderText = "Тип";
            dgvResults.Columns[2].HeaderText = "Лексема / Фрагмент";
            dgvResults.Columns[3].HeaderText = "Местоположение / Описание";

            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResults.Columns[0].FillWeight = 20;
            dgvResults.Columns[1].FillWeight = 25;
            dgvResults.Columns[2].FillWeight = 25;
            dgvResults.Columns[3].FillWeight = 30;
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

            var allErrors = MergeErrors(lexResult, syntaxResult);

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

            var contextualSyntaxErrors = syntaxResult.Errors
                .Where(IsContextualReplacementSyntaxError)
                .ToList();

            lexicalErrors = lexicalErrors
                .Where(x => !contextualSyntaxErrors.Any(s => RangesOverlap(x, s)))
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

        private bool IsContextualReplacementSyntaxError(SyntaxError error)
        {
            if (error == null)
                return false;

            if (IsInsertedSyntaxError(error))
                return false;

            return error.Message == "Ожидалась запятая между элементами списка"
                || error.Message == "Ожидался идентификатор после val"
                || error.Message == "Ожидалось ключевое слово val"
                || error.Message == "Ожидалась лексема listOf"
                || error.Message == "Ожидался оператор присваивания ="
                || error.Message == "Ожидалась закрывающая круглая скобка )"
                || error.Message == "Ожидался символ ; в конце объявления";
        }

        private bool ShouldSuppressSyntaxError(
    SyntaxError syntaxError,
    System.Collections.Generic.List<SyntaxError> lexicalErrors)
        {

            if (IsInsertedSyntaxError(syntaxError))
                return false;


            if (IsListSeparatorSyntaxError(syntaxError))
                return false;

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

        private bool IsInsertedSyntaxError(SyntaxError error)
        {
            if (error == null || string.IsNullOrWhiteSpace(error.InvalidFragment))
                return false;

            return error.InvalidFragment.StartsWith("(пропущ");
        }

        private bool IsListSeparatorSyntaxError(SyntaxError error)
        {
            if (error == null || string.IsNullOrWhiteSpace(error.Message))
                return false;

            return error.Message == "Ожидалась запятая между элементами списка"
                || error.Message == "Ожидалась запятая или закрывающая круглая скобка )";
        }

        private bool RangesOverlap(SyntaxError a, SyntaxError b)
        {
            if (a == null || b == null)
                return false;

            if (a.Line != b.Line)
                return false;

            int aStart = a.StartColumn;
            int aEnd = Math.Max(a.StartColumn, a.EndColumn);

            int bStart = b.StartColumn;
            int bEnd = Math.Max(b.StartColumn, b.EndColumn);

            return aStart <= bEnd && bStart <= aEnd;
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

            if (row.Tag is ExpressionToken expressionToken)
            {
                HighlightRange(
                    expressionToken.Line,
                    expressionToken.Column,
                    expressionToken.Line,
                    expressionToken.Column + Math.Max(expressionToken.Length, 1) - 1);

                return;
            }

            if (row.Tag is ExpressionLexicalError expressionError)
            {
                HighlightRange(
                    expressionError.Line,
                    expressionError.Column,
                    expressionError.Line,
                    expressionError.Column + Math.Max(expressionError.Length, 1) - 1);

                return;
            }

            if (row.Tag is ExpressionSyntaxError expressionSyntaxError)
            {
                HighlightRange(
                    expressionSyntaxError.Line,
                    expressionSyntaxError.Column,
                    expressionSyntaxError.Line,
                    expressionSyntaxError.Column + Math.Max(expressionSyntaxError.Length, 1) - 1);

                return;
            }

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

            if (miLab6ExpressionLexer != null)
            {
                miLab6ExpressionLexer.Enabled = hasText;
            }

            if (btnLab6ExpressionLexer != null)
            {
                btnLab6ExpressionLexer.Enabled = hasText;
            }

            if (miLab6ExpressionSyntax != null)
            {
                miLab6ExpressionSyntax.Enabled = hasText;
            }

            if (btnLab6ExpressionSyntax != null)
            {
                btnLab6ExpressionSyntax.Enabled = hasText;
            }

            if (miLab6ExpressionQuadruples != null)
            {
                miLab6ExpressionQuadruples.Enabled = hasText;
            }

            if (btnLab6ExpressionQuadruples != null)
            {
                btnLab6ExpressionQuadruples.Enabled = hasText;
            }

            if (miLab6ExpressionPoliz != null)
            {
                miLab6ExpressionPoliz.Enabled = hasText;
            }

            if (btnLab6ExpressionPoliz != null)
            {
                btnLab6ExpressionPoliz.Enabled = hasText;
            }
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

            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResults.Columns[0].FillWeight = 15;
            dgvResults.Columns[1].FillWeight = 25;
            dgvResults.Columns[2].FillWeight = 40;
            dgvResults.Columns[3].FillWeight = 20;
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

            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvResults.Columns[0].FillWeight = 35;
            dgvResults.Columns[1].FillWeight = 25;
            dgvResults.Columns[2].FillWeight = 40;
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

        private void miTask_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenHtml("Task.html");
        }

        private void miGrammar_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenHtml("Grammar.html");
        }

        private void miClassification_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenHtml("Classification.html");
        }

        private void miMethod_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenHtml("Method.html");
        }

        private void miTests_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenHtml("Tests.html");
        }

        private void miReferences_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenHtml("References.html");
        }

        private void miHelp_Click(object sender, EventArgs e)
        {
            using (var form = new HelpForm())
            {
                form.ShowDialog(this);
            }
        }

        private void miSourceCode_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenUrl("https://github.com/Alexanderio1/Coursework/tree/ANTLR");
        }

        private void miCourseWork_Click(object sender, EventArgs e)
        {
            ResourceHelper.OpenUrl("https://docs.google.com/document/d/1gpVHHhTqDziBp-66MiW5A9liITRxQU-9/edit?usp=sharing");
        }


        private void InitializeEditorFormattingControls()
        {
            _defaultEditorFontSize = rtbEditor.Font.SizeInPoints;

            miFormat = new ToolStripMenuItem("Формат");

            miFontDialog = new ToolStripMenuItem("Шрифт...");
            miFontDialog.Click += (sender, e) => ShowEditorFontDialog();

            miIncreaseFont = new ToolStripMenuItem("Увеличить шрифт");
            miIncreaseFont.ShortcutKeyDisplayString = "Ctrl++";
            miIncreaseFont.Click += (sender, e) => ChangeWorkspaceFontSize(1.0f);

            miDecreaseFont = new ToolStripMenuItem("Уменьшить шрифт");
            miDecreaseFont.ShortcutKeyDisplayString = "Ctrl+-";
            miDecreaseFont.Click += (sender, e) => ChangeWorkspaceFontSize(-1.0f);

            miResetFont = new ToolStripMenuItem("Сбросить размер шрифта");
            miResetFont.ShortcutKeyDisplayString = "Ctrl+0";
            miResetFont.Click += (sender, e) => SetWorkspaceFontSize(_defaultEditorFontSize);

            miWordWrap = new ToolStripMenuItem("Перенос строк");
            miWordWrap.CheckOnClick = true;
            miWordWrap.Checked = rtbEditor.WordWrap;
            miWordWrap.Click += (sender, e) =>
            {
                rtbEditor.WordWrap = miWordWrap.Checked;
            };

            miFormat.DropDownItems.AddRange(new ToolStripItem[]
            {
        miFontDialog,
        new ToolStripSeparator(),
        miIncreaseFont,
        miDecreaseFont,
        miResetFont,
        new ToolStripSeparator(),
        miWordWrap
            });

            int formatMenuIndex = menuMain.Items.IndexOf(текстToolStripMenuItem);
            if (formatMenuIndex < 0)
                formatMenuIndex = menuMain.Items.Count;

            menuMain.Items.Insert(formatMenuIndex, miFormat);

            btnFontDialog = new ToolStripButton("Шрифт");
            btnFontDialog.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnFontDialog.ToolTipText = "Выбрать шрифт";
            btnFontDialog.Click += (sender, e) => ShowEditorFontDialog();

            btnIncreaseFont = new ToolStripButton("A+");
            btnIncreaseFont.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnIncreaseFont.ToolTipText = "Увеличить шрифт";
            btnIncreaseFont.Click += (sender, e) => ChangeWorkspaceFontSize(1.0f);

            btnDecreaseFont = new ToolStripButton("A-");
            btnDecreaseFont.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btnDecreaseFont.ToolTipText = "Уменьшить шрифт";
            btnDecreaseFont.Click += (sender, e) => ChangeWorkspaceFontSize(-1.0f);

            cmbFontSize = new ToolStripComboBox();
            cmbFontSize.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFontSize.Width = 70;
            cmbFontSize.ToolTipText = "Размер шрифта";

            string[] sizes = { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "28", "32", "36" };
            cmbFontSize.Items.AddRange(sizes);
            cmbFontSize.SelectedItem = Math.Round(rtbEditor.Font.SizeInPoints).ToString();
            cmbFontSize.SelectedIndexChanged += cmbFontSize_SelectedIndexChanged;

            int insertIndex = toolMain.Items.IndexOf(toolStripSeparator3);
            if (insertIndex < 0)
                insertIndex = toolMain.Items.Count;
            else
                insertIndex++;

            toolMain.Items.Insert(insertIndex++, new ToolStripSeparator());
            toolMain.Items.Insert(insertIndex++, btnFontDialog);
            toolMain.Items.Insert(insertIndex++, btnDecreaseFont);
            toolMain.Items.Insert(insertIndex++, cmbFontSize);
            toolMain.Items.Insert(insertIndex++, btnIncreaseFont);

            SetResultsGridFont(rtbEditor.Font);
        }

        private void ShowEditorFontDialog()
        {
            using (var dialog = new FontDialog())
            {
                dialog.Font = rtbEditor.Font;
                dialog.ShowColor = false;
                dialog.ShowEffects = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                SetWorkspaceFont(dialog.Font);
            }
        }

        private void SetWorkspaceFont(Font font)
        {
            SetWholeEditorFont(font);
            SetResultsGridFont(font);
        }

        private void ChangeWorkspaceFontSize(float delta)
        {
            float currentSize = rtbEditor.Font.SizeInPoints;
            float newSize = currentSize + delta;

            if (newSize < 8.0f)
                newSize = 8.0f;

            if (newSize > 36.0f)
                newSize = 36.0f;

            SetWorkspaceFontSize(newSize);
        }

        private void SetWorkspaceFontSize(float size)
        {
            Font currentFont = rtbEditor.Font;

            Font newFont = new Font(
                currentFont.FontFamily,
                size,
                currentFont.Style,
                GraphicsUnit.Point
            );

            SetWorkspaceFont(newFont);
        }

        private void SetResultsGridFont(Font editorFont)
        {
            Font gridFont = new Font(
                editorFont.FontFamily,
                editorFont.SizeInPoints,
                FontStyle.Regular,
                GraphicsUnit.Point
            );

            Font headerFont = new Font(
                editorFont.FontFamily,
                editorFont.SizeInPoints,
                FontStyle.Bold,
                GraphicsUnit.Point
            );

            dgvResults.Font = gridFont;
            dgvResults.DefaultCellStyle.Font = gridFont;
            dgvResults.ColumnHeadersDefaultCellStyle.Font = headerFont;

            dgvResults.RowTemplate.Height = (int)(editorFont.SizeInPoints * 2.2f);
            dgvResults.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            dgvResults.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvResults.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;

            dgvResults.Invalidate();
        }

        private void SetWholeEditorFont(Font font)
        {
            int selectionStart = rtbEditor.SelectionStart;
            int selectionLength = rtbEditor.SelectionLength;

            rtbEditor.SelectAll();
            rtbEditor.SelectionFont = font;

            rtbEditor.Font = font;

            rtbEditor.Select(selectionStart, selectionLength);
            rtbEditor.Focus();

            UpdateFontSizeCombo();
        }

        private void UpdateFontSizeCombo()
        {
            if (cmbFontSize == null)
                return;

            string sizeText = Math.Round(rtbEditor.Font.SizeInPoints).ToString();

            _suppressFontSizeComboChanged = true;

            if (cmbFontSize.Items.Contains(sizeText))
                cmbFontSize.SelectedItem = sizeText;
            else
                cmbFontSize.Text = sizeText;

            _suppressFontSizeComboChanged = false;
        }

        private void cmbFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressFontSizeComboChanged)
                return;

            if (cmbFontSize.SelectedItem == null)
                return;

            float size;
            if (!float.TryParse(cmbFontSize.SelectedItem.ToString(), out size))
                return;

            SetWorkspaceFontSize(size);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control)
                return;

            if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
            {
                ChangeWorkspaceFontSize(1.0f);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
            {
                ChangeWorkspaceFontSize(-1.0f);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.D0 || e.KeyCode == Keys.NumPad0)
            {
                SetWorkspaceFontSize(_defaultEditorFontSize);
                e.Handled = true;
            }
        }
    }

}

