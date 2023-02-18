using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileEncrypter;

public partial class MainWindow : Window
{
    public string FilePath { get; set; } = null!;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void btnFile_Click(object sender, RoutedEventArgs e)
    {
        FileDialog dialog = new OpenFileDialog();
        dialog.Filter = "Text Files |*.txt";

        var result =dialog.ShowDialog();

        if(result == true)
            txtFile.Text = dialog.FileName;
    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {

        //if (string.IsNullOrWhiteSpace(FilePath))
        //{
        //    MessageBox.Show("Enter File Path");
        //    return;
        //}

        //if (!File.Exists(FilePath))
        //{
        //    MessageBox.Show($"File '{FilePath}' was not found");
        //    return;
        //}

        //if (rbDecrypt.IsChecked == null && rbDecrypt.IsChecked == null)
        //{
        //    MessageBox.Show("Choose Action");
        //    return;
        //}

        //if (string.IsNullOrWhiteSpace(txtPass.Text))
        //{
        //    MessageBox.Show("Enter encryption key");
        //    return;
        //}

        StringBuilder sb = new();

        if (string.IsNullOrWhiteSpace(FilePath))
            sb.Append("Enter File Path\n");

        if (!File.Exists(FilePath))
            sb.Append($"File '{FilePath}' was not found\n");

        if (rbDecrypt.IsChecked == null && rbEncrypt.IsChecked == null)
            sb.Append("Choose Action\n");

        if (string.IsNullOrWhiteSpace(txtPass.Text))
            sb.Append("Enter encryption key");

        if(sb.Length>0)
        {
            MessageBox.Show(sb.ToString());
            return;
        }

    }
}
