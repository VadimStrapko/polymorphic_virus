using System;
using System.IO;
using System.Windows;

namespace CP.Commands
{
    public class CopyFilesCommand
    {
        public void CopyExeFilesToTarget()
        {
            try
            {
                string targetPath = @"C:\Users\admin\Desktop\files";
                Directory.CreateDirectory(targetPath);

                // ДИАГНОСТИКА - проверяем разные возможные пути
                string[] possiblePaths = {
                    @"C:\Users\admin\Desktop\CP\CP\bin\Debug",
                    @"C:\Users\admin\Desktop\CP\bin\Debug",
                    AppDomain.CurrentDomain.BaseDirectory,
                    Directory.GetCurrentDirectory(),
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                };

                string foundPath = null;
                string diagnosticInfo = "Проверяемые пути:\n\n";

                foreach (string path in possiblePaths)
                {
                    diagnosticInfo += $"{path}\n";
                    if (Directory.Exists(path))
                    {
                        string[] exeFiles = Directory.GetFiles(path, "*.exe");
                        diagnosticInfo += $"✅ Найдено {exeFiles.Length} .exe файлов\n";

                        foreach (string file in exeFiles)
                        {
                            diagnosticInfo += $"   - {Path.GetFileName(file)}\n";
                        }

                        if (exeFiles.Length > 0)
                        {
                            foundPath = path;
                            diagnosticInfo += $"🎯 ИСПОЛЬЗУЕМ ЭТОТ ПУТЬ!\n";
                        }
                    }
                    else
                    {
                        diagnosticInfo += $"❌ Папка не существует\n";
                    }
                    diagnosticInfo += "\n";
                }

                MessageBox.Show(diagnosticInfo, "Диагностика путей");

                if (foundPath != null)
                {
                    CopyFilesFromPath(foundPath, targetPath);
                }
                else
                {
                    MessageBox.Show("❌ Не найдено папок с .exe файлами!", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void CopyFilesFromPath(string sourcePath, string targetPath)
        {
            string[] filesToCopy = { "Encryptor.exe", "Decryptor.exe", "Monitor.exe" };
            int copiedCount = 0;

            foreach (string fileName in filesToCopy)
            {
                string sourceFile = Path.Combine(sourcePath, fileName);
                string destFile = Path.Combine(targetPath, fileName);

                if (File.Exists(sourceFile))
                {
                    File.Copy(sourceFile, destFile, true);
                    copiedCount++;
                }
            }

            if (copiedCount > 0)
            {
                MessageBox.Show($"✅ Скопировано {copiedCount} файлов из:\n{sourcePath}\nв:\n{targetPath}");
                System.Diagnostics.Process.Start("explorer.exe", targetPath);
            }
            else
            {
                MessageBox.Show($"❌ В папке найдены .exe файлы, но нет нужных:\n{sourcePath}");
            }
        }
    }
}