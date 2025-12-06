using System.Windows;
using CP.Commands;

namespace CP
{
    public partial class DecryptionWindow : Window
    {
        public DecryptionWindow()
        {
            InitializeComponent();
        }

        private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            string key = txtDecryptionKey.Text.Trim();

            if (string.IsNullOrEmpty(key))
            {
                MessageBox.Show("Please enter decryption key!", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var decryptor = new DecryptorCommand();
            decryptor.Execute(key);
            this.Close();
        }
    }
}