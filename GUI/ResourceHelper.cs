using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace GUI
{
    public static class ResourceHelper
    {
        public static string TempFolder
        {
            get { return Path.Combine(Path.GetTempPath(), "KotlinListCompiler"); }
        }

        public static void ExtractResources()
        {
            Directory.CreateDirectory(TempFolder);

            ExtractFile("GUI.Html.Task.html", "Task.html");
            ExtractFile("GUI.Html.Grammar.html", "Grammar.html");
            ExtractFile("GUI.Html.Classification.html", "Classification.html");
            ExtractFile("GUI.Html.Method.html", "Method.html");
            ExtractFile("GUI.Html.Tests.html", "Tests.html");
            ExtractFile("GUI.Html.References.html", "References.html");

            ExtractFile("GUI.Html.styles.css", "styles.css");

            ExtractFile("GUI.Html.Graph.png", "Graph.png");
            ExtractFile("GUI.Html.Test1.png", "Test1.png");
            ExtractFile("GUI.Html.Test2.png", "Test2.png");
            ExtractFile("GUI.Html.Test3.png", "Test3.png");
            ExtractFile("GUI.Html.Test4.png", "Test4.png");
            ExtractFile("GUI.Html.Test5.png", "Test5.png");
            ExtractFile("GUI.Html.Test6.png", "Test6.png");
        }

        private static void ExtractFile(string resourceName, string outputFileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception("Ресурс не найден: " + resourceName);

                string outputPath = Path.Combine(TempFolder, outputFileName);

                using (FileStream file = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(file);
                }
            }
        }

        public static void OpenHtml(string fileName)
        {
            string path = Path.Combine(TempFolder, fileName);

            if (!File.Exists(path))
            {
                MessageBox.Show("Файл не найден: " + path, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось открыть ссылку.\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}