using System;
using System.Windows;

namespace SchoolJournal.Client.Windows
{
    public partial class ChildIdInputDialog : Window
    {
        public int ChildId { get; private set; }

        public ChildIdInputDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtChildId.Text, out int id) && id > 0)
            {
                ChildId = id;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Введите корректный ID ребенка!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}