using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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
    public partial class PickPrimesWindow : Window
    {
        public PickPrimesWindow()
        {
            InitializeComponent();
            ResizeMode = ResizeMode.NoResize;
            TextBoxFirstPrime.TextWrapping = TextWrapping.NoWrap;
            TextBoxSecondPrime.TextWrapping = TextWrapping.NoWrap;
        }

        private string previous_text_first_prime = "";
        private string previous_text_second_prime = "";

        public long FirstPrime { get; private set; }
        public long SecondPrime { get; private set; }

        private bool IsInteger(string text)
        {
            foreach (char c in text)
            {
                int code = (int)c;
                if (code < (int)'0' || code > '9')
                {
                    return false;
                }
            }
            return true;
        }

        private void EnterExample_Click(object sender, RoutedEventArgs e)
        {
            TextBoxFirstPrime.Text = "7541";
            TextBoxSecondPrime.Text = "9161";
        }

        private void EnterPrimes_Click(object sender, RoutedEventArgs e)
        {
            long first_prime, second_prime;
            try
            {
                first_prime = Convert.ToInt64(TextBoxFirstPrime.Text);
                second_prime = Convert.ToInt64(TextBoxSecondPrime.Text);
            }
            catch (System.FormatException)
            {
                MessageBox.Show("Incorrect input", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            string error = "";
            if (!Model.ArePrimesValid(first_prime, second_prime, ref error))
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            FirstPrime = first_prime;
            SecondPrime = second_prime;

            DialogResult = true;
            Close();
        }

        private void TextBoxFirstPrime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInteger(TextBoxFirstPrime.Text))
            {
                previous_text_first_prime = TextBoxFirstPrime.Text;
            }
            else
            {
                int previous_selection_start_first_prime = TextBoxFirstPrime.SelectionStart;
                int previous_selection_length_first_prime = TextBoxFirstPrime.SelectionLength;
                TextBoxFirstPrime.Text = previous_text_first_prime;
                TextBoxFirstPrime.SelectionStart = previous_selection_start_first_prime;
                TextBoxFirstPrime.SelectionLength = previous_selection_length_first_prime;
            }
        }

        private void TextBoxSecondPrime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInteger(TextBoxSecondPrime.Text))
            {
                previous_text_second_prime = TextBoxSecondPrime.Text;
            }
            else
            {
                int previous_selection_start_second_prime = TextBoxSecondPrime.SelectionStart;
                int previous_selection_length_second_prime = TextBoxSecondPrime.SelectionLength;
                TextBoxSecondPrime.Text = previous_text_second_prime;
                TextBoxSecondPrime.SelectionStart = previous_selection_start_second_prime;
                TextBoxSecondPrime.SelectionLength = previous_selection_length_second_prime;
            }
        }
    }
}
