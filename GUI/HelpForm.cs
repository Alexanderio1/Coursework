using System;
using System.Drawing;
using System.Windows.Forms;

namespace GUI
{
    public partial class HelpForm : Form
    {
        private SplitContainer splitContainer;
        private TreeView treeView;
        private RichTextBox richTextBox;

        public HelpForm()
        {
            InitializeUi();
            InitializeHelpTree();
        }

        private void InitializeUi()
        {
            Text = "Справка";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(900, 550);
            MinimumSize = new Size(700, 400);

            splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.SplitterDistance = 240;
            splitContainer.BorderStyle = BorderStyle.FixedSingle;

            treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.Font = new Font("Times New Roman", 11F, FontStyle.Regular);
            treeView.AfterSelect += TreeView_AfterSelect;

            richTextBox = new RichTextBox();
            richTextBox.Dock = DockStyle.Fill;
            richTextBox.ReadOnly = true;
            richTextBox.BorderStyle = BorderStyle.None;
            richTextBox.BackColor = Color.White;
            richTextBox.Font = new Font("Times New Roman", 12F, FontStyle.Regular);

            splitContainer.Panel1.Controls.Add(treeView);
            splitContainer.Panel2.Controls.Add(richTextBox);

            Controls.Add(splitContainer);
        }

        private void InitializeHelpTree()
        {
            treeView.Nodes.Clear();

            TreeNode root = new TreeNode("Справка");
            root.Nodes.Add("Файл");
            root.Nodes.Add("Правка");
            root.Nodes.Add("Текст");
            root.Nodes.Add("Пуск");
            root.Nodes.Add("Панель инструментов");
            root.Nodes.Add("О программе");

            treeView.Nodes.Add(root);
            root.Expand();

            treeView.SelectedNode = root;
            ShowSection("Справка", "Выберите раздел в дереве слева.");
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            switch (e.Node.Text)
            {
                case "Файл":
                    ShowSection(
                        "Пункт \"Файл\"",
                        "В пункте \"Файл\" доступны команды создания документа, открытия документа, сохранения текущих изменений, сохранения документа в новый файл и выхода из программы.");
                    break;

                case "Правка":
                    ShowSection(
                        "Пункт \"Правка\"",
                        "В пункте \"Правка\" доступны команды отмены изменений, повтора последнего изменения, вырезания, копирования, вставки, удаления текстового фрагмента и выделения всего содержимого документа.");
                    break;

                case "Текст":
                    ShowSection(
                        "Пункт \"Текст\"",
                        "Пункт меню \"Текст\" содержит справочную информацию по курсовой работе: постановку задачи, грамматику, классификацию грамматики, метод анализа, тестовые примеры, список литературы и исходный код программы.");
                    break;

                case "Пуск":
                    ShowSection(
                        "Пункт \"Пуск\"",
                        "При выборе пункта \"Пуск\" выполняется запуск лексического и синтаксического анализа текста.");
                    break;

                case "Панель инструментов":
                    ShowSection(
                        "Панель инструментов",
                        "Панель инструментов содержит кнопки быстрого доступа к основным командам: создание, открытие, сохранение, редактирование текста, запуск анализа и вызов справочной информации.");
                    break;

                case "О программе":
                    ShowSection(
                        "О программе",
                        "Программа предназначена для лексического и синтаксического анализа конструкции объявления списка с инициализацией на языке Kotlin.");
                    break;

                default:
                    ShowSection("Справка", "Выберите раздел в дереве слева.");
                    break;
            }
        }

        private void ShowSection(string title, string text)
        {
            richTextBox.Clear();

            richTextBox.SelectionFont = new Font("Times New Roman", 15F, FontStyle.Bold);
            richTextBox.SelectionColor = Color.DarkBlue;
            richTextBox.AppendText(title + Environment.NewLine + Environment.NewLine);

            richTextBox.SelectionFont = new Font("Times New Roman", 12F, FontStyle.Regular);
            richTextBox.SelectionColor = Color.Black;
            richTextBox.AppendText(text);
        }
    }
}