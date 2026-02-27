using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            lblTitle.Text = "GUI";
            lblDescription.Text = "Специализированный текстовый редактор\nдля будущего языкового процессора";
            lblAuthor.Text = "Автор: Костоломов А.Е.";
            lblGroup.Text = "Группа: АВТ-314";
            lblTech.Text = ".NET Framework 4.8, Windows Forms";
        }
    }
}
