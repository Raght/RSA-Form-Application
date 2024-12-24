using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using System.Windows.Input;

namespace RSA
{
    public class UICommand : ICommand
    {
        public Action action;
        public void Execute(object parameter)
        {
            action.Invoke();
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }
        public event EventHandler CanExecuteChanged;
    }

    public class ViewModel : INotifyPropertyChanged
    {
        private bool are_keys_generated = false;
        public bool AreKeysGenerated {
            get { return are_keys_generated; }
            set
            {
                if (!value) { return; }
                are_keys_generated = true;
                OnPropertyChanged("AreKeysGenerated");
                OnPropertyChanged("LabelAreKeysGenerated");
            }
        }

        public string LabelAreKeysGenerated
        {
            get { if (are_keys_generated) return "Ключи сгенерированы"; else return "Ключи не сгенерированы"; ; }
        }

        public string LabelMaxMessageLength
        {
            get {
                if (!are_keys_generated) return "Максимальная длина сообщения: ?";
                else return "Максимальная длина сообщения: " + Model.MaxMessageLength.ToString();
            }
        }

        public string SourceMessage { get; set; }
        public string EncryptedMessage { get; set; }
        public string SourceEncryptedMessage { get; set; }
        public string DecryptedMessage { get; set; }

        private bool is_encryption_key_public = true;
        public bool IsEncryptionKeyPublicKey
        {
            get { return is_encryption_key_public; }
            set { is_encryption_key_public = value; OnPropertyChanged("IsEncryptionKeyPublicKey"); }
        }

        public bool IsEncryptionKeySecretKey
        {
            get { return !is_encryption_key_public; }
            set { is_encryption_key_public = !value; OnPropertyChanged("IsEncryptionKeySecretKey"); }
        }

        private void OnPressKeyGenerate()
        {
            string error = "";
            PickPrimesWindow window = new PickPrimesWindow();

            long p, q;
            if ((bool)window.ShowDialog())
            {
                p = window.FirstPrime;
                q = window.SecondPrime;
            }
            else
            {
                return;
            }
            
            if (Model.TryGenerateKeys(p, q, ref error))
            {
                AreKeysGenerated = true;
            }
            else
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            OnPropertyChanged("LabelMaxMessageLength");
        }

        private void Encrypt()
        {
            if (SourceMessage == null)
            {
                return;
            }

            string encrypted = "";
            string error = "";
            Model.Key key = Model.Key.PUBLIC;
            if (IsEncryptionKeySecretKey)
            {
                key = Model.Key.PRIVATE;
            }
            if (!Model.Encrypt(SourceMessage, key, ref encrypted, ref error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            EncryptedMessage = encrypted;
            OnPropertyChanged("EncryptedMessage");
        }

        private void Decrypt()
        {
            if (SourceEncryptedMessage == null)
            {
                return;
            }

            string decrypted = "";
            string error = "";
            Model.Key key = Model.Key.PRIVATE;
            if (IsEncryptionKeySecretKey)
            {
                key = Model.Key.PUBLIC;
            }
            if (!Model.Decrypt(SourceEncryptedMessage, key, ref decrypted, ref error))
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DecryptedMessage = decrypted;
            OnPropertyChanged("DecryptedMessage");
        }

        private void Transmit()
        {
            Encrypt();
            SourceEncryptedMessage = EncryptedMessage;
            OnPropertyChanged("SourceEncryptedMessage");
            Decrypt();
        }

        private void ShowInfoWindow(string label, string text)
        {
            InfoWindow info_window = new InfoWindow(label, text);
            info_window.Show();
        }

        private void CopyToClipboard(string text)
        {
            Clipboard.SetText(text);
        }

        public UICommand generate_keys_command { get; set; }
        public UICommand show_modulus_command { get; set; }
        public UICommand show_public_key_command { get; set; }
        public UICommand show_private_key_command { get; set; }
        public UICommand show_log_command { get; set; }
        public UICommand copy_source_message { get; set; }
        public UICommand copy_encrypted_message { get; set; }
        public UICommand copy_source_encrypted_message { get; set; }
        public UICommand copy_decrypted_message { get; set; }
        public UICommand pick_key_command { get; set; }
        public UICommand encrypt_message_command { get; set; }
        public UICommand decrypt_message_command { get; set; }
        public UICommand transmit_message_command { get; set; }
        

        public ViewModel()
        {
            generate_keys_command = new UICommand();
            show_modulus_command = new UICommand();
            show_public_key_command = new UICommand();
            show_private_key_command = new UICommand();
            show_log_command = new UICommand();
            pick_key_command = new UICommand();
            encrypt_message_command = new UICommand();
            decrypt_message_command = new UICommand();
            transmit_message_command = new UICommand();
            copy_source_message = new UICommand();
            copy_encrypted_message = new UICommand();
            copy_source_encrypted_message = new UICommand();
            copy_decrypted_message = new UICommand();

            generate_keys_command.action = () => { OnPressKeyGenerate(); };
            show_modulus_command.action = () => { ShowInfoWindow("Модуль", Convert.ToString(Model.Modulus)); };
            show_public_key_command.action = () => { ShowInfoWindow("Публичный ключ", Convert.ToString(Model.PublicKey)); };
            show_private_key_command.action = () => { ShowInfoWindow("Секретный ключ", Convert.ToString(Model.PrivateKey)); };
            show_log_command.action = () => { ShowInfoWindow("Параметры и расчёты", Model.Log); };
            pick_key_command.action = () => { };
            copy_source_message.action = () => { CopyToClipboard(SourceMessage); };
            copy_encrypted_message.action = () => { CopyToClipboard(EncryptedMessage); };
            copy_source_encrypted_message.action = () => { CopyToClipboard(SourceEncryptedMessage); };
            copy_decrypted_message.action = () => { CopyToClipboard(DecryptedMessage); };

            encrypt_message_command.action = () => { Encrypt(); };
            decrypt_message_command.action = () => { Decrypt(); };
            transmit_message_command.action = () => { Transmit(); };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
