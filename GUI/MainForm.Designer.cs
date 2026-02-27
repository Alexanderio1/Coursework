using System;

namespace GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuMain = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.miNew = new System.Windows.Forms.ToolStripMenuItem();
            this.miOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.miSave = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.miEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.miUndo = new System.Windows.Forms.ToolStripMenuItem();
            this.miRedo = new System.Windows.Forms.ToolStripMenuItem();
            this.miCut = new System.Windows.Forms.ToolStripMenuItem();
            this.miCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.miPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.miDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.miSelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.miText = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextTask = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextGrammar = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextClass = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextMethod = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextExample = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextSources = new System.Windows.Forms.ToolStripMenuItem();
            this.miTextCode = new System.Windows.Forms.ToolStripMenuItem();
            this.miRun = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpContent = new System.Windows.Forms.ToolStripMenuItem();
            this.miAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.toolMain = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnUndo = new System.Windows.Forms.ToolStripButton();
            this.btnRedo = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnCut = new System.Windows.Forms.ToolStripButton();
            this.btnPaste = new System.Windows.Forms.ToolStripButton();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnAbout = new System.Windows.Forms.ToolStripButton();
            this.btnHelp = new System.Windows.Forms.ToolStripButton();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.rtbEditor = new System.Windows.Forms.RichTextBox();
            this.rtbOutput = new System.Windows.Forms.RichTextBox();
            this.menuMain.SuspendLayout();
            this.toolMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuMain
            // 
            this.menuMain.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile,
            this.miEdit,
            this.miText,
            this.miRun,
            this.miHelp});
            this.menuMain.Location = new System.Drawing.Point(0, 0);
            this.menuMain.Name = "menuMain";
            this.menuMain.Size = new System.Drawing.Size(1588, 42);
            this.menuMain.TabIndex = 2;
            this.menuMain.Text = "menuStrip1";
            // 
            // miFile
            // 
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miNew,
            this.miOpen,
            this.miSave,
            this.miSaveAs,
            this.miExit});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(90, 38);
            this.miFile.Text = "Файл";
            // 
            // miNew
            // 
            this.miNew.Name = "miNew";
            this.miNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.miNew.Size = new System.Drawing.Size(458, 44);
            this.miNew.Text = "Создать (Ctrl+N)";
            this.miNew.Click += new System.EventHandler(this.CmdNew_Click);
            // 
            // miOpen
            // 
            this.miOpen.Name = "miOpen";
            this.miOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.miOpen.Size = new System.Drawing.Size(631, 44);
            this.miOpen.Text = "Открыть (Ctrl+O)";
            this.miOpen.Click += new System.EventHandler(this.CmdOpen_Click);
            // 
            // miSave
            // 
            this.miSave.Name = "miSave";
            this.miSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.miSave.Size = new System.Drawing.Size(631, 44);
            this.miSave.Text = "Сохранить (Ctrl+S)";
            this.miSave.Click += new System.EventHandler(this.CmdSave_Click);
            // 
            // miSaveAs
            // 
            this.miSaveAs.Name = "miSaveAs";
            this.miSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.miSaveAs.Size = new System.Drawing.Size(631, 44);
            this.miSaveAs.Text = "Сохранить как (Ctrl+Shift+S)";
            this.miSaveAs.Click += new System.EventHandler(this.CmdSaveAs_Click);
            // 
            // miExit
            // 
            this.miExit.Name = "miExit";
            this.miExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.miExit.Size = new System.Drawing.Size(631, 44);
            this.miExit.Text = "Выход (Alt+F4)";
            this.miExit.Click += new System.EventHandler(this.CmdExit_Click);
            // 
            // miEdit
            // 
            this.miEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miUndo,
            this.miRedo,
            this.miCut,
            this.miCopy,
            this.miPaste,
            this.miDelete,
            this.miSelectAll});
            this.miEdit.Name = "miEdit";
            this.miEdit.Size = new System.Drawing.Size(114, 36);
            this.miEdit.Text = "Правка";
            // 
            // miUndo
            // 
            this.miUndo.Name = "miUndo";
            this.miUndo.Size = new System.Drawing.Size(386, 44);
            this.miUndo.Text = "Отменить (Ctrl+Z)";
            this.miUndo.Click += new System.EventHandler(this.CmdUndo_Click);
            // 
            // miRedo
            // 
            this.miRedo.Name = "miRedo";
            this.miRedo.Size = new System.Drawing.Size(386, 44);
            this.miRedo.Text = "Вернуть (Ctrl+Y)";
            this.miRedo.Click += new System.EventHandler(this.CmdRedo_Click);
            // 
            // miCut
            // 
            this.miCut.Name = "miCut";
            this.miCut.Size = new System.Drawing.Size(386, 44);
            this.miCut.Text = "Вырезать (Ctrl+X)";
            this.miCut.Click += new System.EventHandler(this.CmdCut_Click);
            // 
            // miCopy
            // 
            this.miCopy.Name = "miCopy";
            this.miCopy.Size = new System.Drawing.Size(386, 44);
            this.miCopy.Text = "Копировать (Ctrl+C)";
            this.miCopy.Click += new System.EventHandler(this.CmdCopy_Click);
            // 
            // miPaste
            // 
            this.miPaste.Name = "miPaste";
            this.miPaste.Size = new System.Drawing.Size(386, 44);
            this.miPaste.Text = "Вставить (Ctrl+V)";
            this.miPaste.Click += new System.EventHandler(this.CmdPaste_Click);
            // 
            // miDelete
            // 
            this.miDelete.Name = "miDelete";
            this.miDelete.Size = new System.Drawing.Size(386, 44);
            this.miDelete.Text = "Удалить (Del)";
            this.miDelete.Click += new System.EventHandler(this.CmdDelete_Click);
            // 
            // miSelectAll
            // 
            this.miSelectAll.Name = "miSelectAll";
            this.miSelectAll.Size = new System.Drawing.Size(386, 44);
            this.miSelectAll.Text = "Выделить все (Ctrl+A)";
            this.miSelectAll.Click += new System.EventHandler(this.CmdSelectAll_Click);
            // 
            // miText
            // 
            this.miText.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miTextTask,
            this.miTextGrammar,
            this.miTextClass,
            this.miTextMethod,
            this.miTextExample,
            this.miTextSources,
            this.miTextCode});
            this.miText.Name = "miText";
            this.miText.Size = new System.Drawing.Size(93, 36);
            this.miText.Text = "Текст";
            // 
            // miTextTask
            // 
            this.miTextTask.Name = "miTextTask";
            this.miTextTask.Size = new System.Drawing.Size(454, 44);
            this.miTextTask.Text = "Постановка задачи";
            this.miTextTask.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miTextGrammar
            // 
            this.miTextGrammar.Name = "miTextGrammar";
            this.miTextGrammar.Size = new System.Drawing.Size(454, 44);
            this.miTextGrammar.Text = "Грамматика";
            this.miTextGrammar.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miTextClass
            // 
            this.miTextClass.Name = "miTextClass";
            this.miTextClass.Size = new System.Drawing.Size(454, 44);
            this.miTextClass.Text = "Классификация грамматики";
            this.miTextClass.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miTextMethod
            // 
            this.miTextMethod.Name = "miTextMethod";
            this.miTextMethod.Size = new System.Drawing.Size(454, 44);
            this.miTextMethod.Text = "Метод анализа";
            this.miTextMethod.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miTextExample
            // 
            this.miTextExample.Name = "miTextExample";
            this.miTextExample.Size = new System.Drawing.Size(454, 44);
            this.miTextExample.Text = "Тестовый пример";
            this.miTextExample.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miTextSources
            // 
            this.miTextSources.Name = "miTextSources";
            this.miTextSources.Size = new System.Drawing.Size(454, 44);
            this.miTextSources.Text = "Список литературы";
            this.miTextSources.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miTextCode
            // 
            this.miTextCode.Name = "miTextCode";
            this.miTextCode.Size = new System.Drawing.Size(454, 44);
            this.miTextCode.Text = "Исходный код программы";
            this.miTextCode.Click += new System.EventHandler(this.CmdTextStub_Click);
            // 
            // miRun
            // 
            this.miRun.Name = "miRun";
            this.miRun.Size = new System.Drawing.Size(86, 36);
            this.miRun.Text = "Пуск";
            this.miRun.Click += new System.EventHandler(this.CmdRun_Click);
            // 
            // miHelp
            // 
            this.miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHelpContent,
            this.miAbout});
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(126, 36);
            this.miHelp.Text = "Справка";
            // 
            // miHelpContent
            // 
            this.miHelpContent.Name = "miHelpContent";
            this.miHelpContent.Size = new System.Drawing.Size(359, 44);
            this.miHelpContent.Text = "Вызов справки (F1)";
            this.miHelpContent.Click += new System.EventHandler(this.CmdHelp_Click);
            // 
            // miAbout
            // 
            this.miAbout.Name = "miAbout";
            this.miAbout.Size = new System.Drawing.Size(359, 44);
            this.miAbout.Text = "О программе";
            this.miAbout.Click += new System.EventHandler(this.CmdAbout_Click);
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(471, 175);
            // 
            // toolMain
            // 
            this.toolMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.btnSave,
            this.btnUndo,
            this.btnRedo,
            this.btnCopy,
            this.btnCut,
            this.btnPaste,
            this.btnRun,
            this.btnAbout,
            this.btnHelp});
            this.toolMain.Location = new System.Drawing.Point(0, 42);
            this.toolMain.Name = "toolMain";
            this.toolMain.Size = new System.Drawing.Size(1588, 34);
            this.toolMain.TabIndex = 11;
            this.toolMain.Text = "toolStrip1";
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = global::GUI.Properties.Resources._new;
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(46, 32);
            this.btnNew.Text = "New";
            this.btnNew.Click += new System.EventHandler(this.CmdNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = global::GUI.Properties.Resources.open;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(46, 32);
            this.btnOpen.Text = "Open";
            this.btnOpen.Click += new System.EventHandler(this.CmdOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Image = global::GUI.Properties.Resources.save;
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(46, 32);
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.CmdSave_Click);
            // 
            // btnUndo
            // 
            this.btnUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUndo.Image = global::GUI.Properties.Resources.undo;
            this.btnUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(46, 32);
            this.btnUndo.Text = "Undo";
            this.btnUndo.Click += new System.EventHandler(this.CmdUndo_Click);
            // 
            // btnRedo
            // 
            this.btnRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRedo.Image = global::GUI.Properties.Resources.redo;
            this.btnRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(46, 32);
            this.btnRedo.Text = "Redo";
            this.btnRedo.Click += new System.EventHandler(this.CmdRedo_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopy.Image = global::GUI.Properties.Resources.copy;
            this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(46, 32);
            this.btnCopy.Text = "Copy";
            this.btnCopy.Click += new System.EventHandler(this.CmdCopy_Click);
            // 
            // btnCut
            // 
            this.btnCut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCut.Image = global::GUI.Properties.Resources.cut;
            this.btnCut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCut.Name = "btnCut";
            this.btnCut.Size = new System.Drawing.Size(46, 32);
            this.btnCut.Text = "Cut";
            this.btnCut.Click += new System.EventHandler(this.CmdCut_Click);
            // 
            // btnPaste
            // 
            this.btnPaste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPaste.Image = global::GUI.Properties.Resources.paste;
            this.btnPaste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(46, 32);
            this.btnPaste.Text = "Paste";
            this.btnPaste.Click += new System.EventHandler(this.CmdPaste_Click);
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRun.Image = global::GUI.Properties.Resources.run;
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(46, 32);
            this.btnRun.Text = "Run (анализатор)";
            this.btnRun.Click += new System.EventHandler(this.CmdRun_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAbout.Image = global::GUI.Properties.Resources.about;
            this.btnAbout.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnAbout.Size = new System.Drawing.Size(46, 28);
            this.btnAbout.Text = "About";
            this.btnAbout.Click += new System.EventHandler(this.CmdAbout_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHelp.Image = global::GUI.Properties.Resources.help;
            this.btnHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(46, 32);
            this.btnHelp.Text = "Help";
            this.btnHelp.Click += new System.EventHandler(this.CmdHelp_Click);
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.Location = new System.Drawing.Point(0, 76);
            this.splitMain.Name = "splitMain";
            this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.rtbEditor);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.rtbOutput);
            this.splitMain.Size = new System.Drawing.Size(1588, 819);
            this.splitMain.SplitterDistance = 520;
            this.splitMain.SplitterWidth = 6;
            this.splitMain.TabIndex = 12;
            // 
            // rtbEditor
            // 
            this.rtbEditor.AcceptsTab = true;
            this.rtbEditor.BackColor = System.Drawing.SystemColors.Window;
            this.rtbEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbEditor.Font = new System.Drawing.Font("Consolas", 10.125F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rtbEditor.Location = new System.Drawing.Point(0, 0);
            this.rtbEditor.Name = "rtbEditor";
            this.rtbEditor.Size = new System.Drawing.Size(1588, 520);
            this.rtbEditor.TabIndex = 0;
            this.rtbEditor.Text = "";
            this.rtbEditor.WordWrap = false;
            // 
            // rtbOutput
            // 
            this.rtbOutput.BackColor = System.Drawing.SystemColors.ControlLight;
            this.rtbOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbOutput.Location = new System.Drawing.Point(0, 0);
            this.rtbOutput.Name = "rtbOutput";
            this.rtbOutput.ReadOnly = true;
            this.rtbOutput.Size = new System.Drawing.Size(1588, 293);
            this.rtbOutput.TabIndex = 0;
            this.rtbOutput.TabStop = false;
            this.rtbOutput.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1588, 895);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.toolMain);
            this.Controls.Add(this.menuMain);
            this.MainMenuStrip = this.menuMain;
            this.MinimumSize = new System.Drawing.Size(800, 500);
            this.Name = "MainForm";
            this.Text = "GUI";
            this.menuMain.ResumeLayout(false);
            this.menuMain.PerformLayout();
            this.toolMain.ResumeLayout(false);
            this.toolMain.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        #endregion
        private System.Windows.Forms.MenuStrip menuMain;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miNew;
        private System.Windows.Forms.ToolStripMenuItem miOpen;
        private System.Windows.Forms.ToolStripMenuItem miSave;
        private System.Windows.Forms.ToolStripMenuItem miSaveAs;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem miEdit;
        private System.Windows.Forms.ToolStripMenuItem miText;
        private System.Windows.Forms.ToolStripMenuItem miRun;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripMenuItem miUndo;
        private System.Windows.Forms.ToolStripMenuItem miRedo;
        private System.Windows.Forms.ToolStripMenuItem miCut;
        private System.Windows.Forms.ToolStripMenuItem miCopy;
        private System.Windows.Forms.ToolStripMenuItem miPaste;
        private System.Windows.Forms.ToolStripMenuItem miDelete;
        private System.Windows.Forms.ToolStripMenuItem miSelectAll;
        private System.Windows.Forms.ToolStripMenuItem miTextTask;
        private System.Windows.Forms.ToolStripMenuItem miTextGrammar;
        private System.Windows.Forms.ToolStripMenuItem miTextClass;
        private System.Windows.Forms.ToolStripMenuItem miTextMethod;
        private System.Windows.Forms.ToolStripMenuItem miTextExample;
        private System.Windows.Forms.ToolStripMenuItem miTextSources;
        private System.Windows.Forms.ToolStripMenuItem miTextCode;
        private System.Windows.Forms.ToolStripMenuItem miHelpContent;
        private System.Windows.Forms.ToolStripMenuItem miAbout;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.ToolStrip toolMain;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.RichTextBox rtbEditor;
        private System.Windows.Forms.RichTextBox rtbOutput;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnUndo;
        private System.Windows.Forms.ToolStripButton btnRedo;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnCut;
        private System.Windows.Forms.ToolStripButton btnPaste;
        private System.Windows.Forms.ToolStripButton btnRun;
        private System.Windows.Forms.ToolStripButton btnHelp;
        private System.Windows.Forms.ToolStripButton btnAbout;
    }
}

