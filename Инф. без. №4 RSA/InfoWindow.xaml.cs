using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RSA
{
    public partial class InfoWindow : Window
    {
        public InfoWindow()
        {
            InitializeComponent();
        }

        public InfoWindow(string title_and_label, string text) : this()
        {
            Title = title_and_label;
            Label.Content = title_and_label;
            InfoTextBox.Text = text;
            InfoTextBox.IsReadOnly = true;
        }

        private void ButtonCopyToClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(InfoTextBox.Text);
        }
    }
}
