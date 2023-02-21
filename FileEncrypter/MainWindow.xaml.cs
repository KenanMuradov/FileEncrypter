using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
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
    private CancellationTokenSource? _cts;
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

        var result = dialog.ShowDialog();

        if (result == true)
            txtFile.Text = dialog.FileName;
    }

    private void btnStart_Click(object sender, RoutedEventArgs e)
    {
        StringBuilder sb = new();

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            MessageBox.Show("Enter File Path");
            return;
        }

        if (!File.Exists(FilePath))
            sb.Append($"File '{FilePath}' was not found\n");

        if (rbDecrypt.IsChecked == false && rbEncrypt.IsChecked == false)
            sb.Append("Choose Action\n");

        if (string.IsNullOrWhiteSpace(txtPass.Password))
            sb.Append("Enter encryption key");

        if (txtPass.Password.Length!=16)
            sb.Append("Password must contain 16 characters");

        if (sb.Length > 0)
        {
            MessageBox.Show(sb.ToString());
            return;
        }

        Progressbar.Value = 0;

        _cts = new CancellationTokenSource();

        if (rbEncrypt.IsChecked == true)
            EncryptAndWrite(_cts.Token);

        if(rbDecrypt.IsChecked == true)
            DecryptAndWrite(_cts.Token);
    }

    //MyNameIsKepaMaxs

    private void EncryptAndWrite(CancellationToken token)
    {
        var text = File.ReadAllText(FilePath);

        var key = Encoding.UTF8.GetBytes(txtPass.Password);

        var bytesToWrite = EncryptStringToBytes(text, key, key);

        btnStart.IsEnabled = false;
        btnCancel.IsEnabled = true;

        ThreadPool.QueueUserWorkItem(o =>
        {
            using var fs = new FileStream(FilePath, FileMode.Truncate);

            for (int i = 0; i < bytesToWrite.Length; i++)
            {
                if (i % 32 == 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        fs.Dispose();
                        Dispatcher.Invoke(() => File.WriteAllText(FilePath, text));
                        Dispatcher.Invoke(() => Progressbar.Value = 0);
                        Dispatcher.Invoke(() => btnStart.IsEnabled = true);
                        return;
                    }

                    Thread.Sleep(500);
                    if (i != 0)
                        Dispatcher.Invoke(() => Progressbar.Value = 100 * i / bytesToWrite.Length);
                }
                fs.WriteByte(bytesToWrite[i]);
            }

            fs.Seek(0, SeekOrigin.Begin);

            Dispatcher.Invoke(() => btnStart.IsEnabled = true);
            Dispatcher.Invoke(() => btnCancel.IsEnabled = false);
            Dispatcher.Invoke(() => Progressbar.Value = 100);
        });
    }

    private void DecryptAndWrite(CancellationToken token)
    {
        var bytes = File.ReadAllBytes(FilePath);

        var key = Encoding.UTF8.GetBytes(txtPass.Password);

        var text = DecryptStringFromBytes(bytes, key, key);
        var bytesToWrite= Encoding.UTF8.GetBytes(text);

        btnStart.IsEnabled = false;
        btnCancel.IsEnabled = true;

        ThreadPool.QueueUserWorkItem(o =>
        {
            using var fs = new FileStream(FilePath, FileMode.Truncate);

            for (int i = 0; i < bytesToWrite.Length; i++)
            {
                if (i % 32 == 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        fs.Dispose();
                        Dispatcher.Invoke(() => File.WriteAllBytes(FilePath, bytes));
                        Dispatcher.Invoke(() => Progressbar.Value = 0);
                        Dispatcher.Invoke(() => btnStart.IsEnabled = true);
                        return;
                    }

                    Thread.Sleep(500);
                    if (i != 0)
                        Dispatcher.Invoke(() => Progressbar.Value = 100 * i / bytesToWrite.Length);
                }
                fs.WriteByte(bytesToWrite[i]);
            }

            fs.Seek(0, SeekOrigin.Begin);

            Dispatcher.Invoke(() => btnStart.IsEnabled = true);
            Dispatcher.Invoke(() => btnCancel.IsEnabled = false);
            Dispatcher.Invoke(() => Progressbar.Value = 100);
        });
    }

    private static byte[] EncryptStringToBytes(string original, byte[] key, byte[] IV)
    {
        byte[] encrypted;
        using (var encryption = Aes.Create())
        {
            encryption.Key = key;
            encryption.IV = IV;


            ICryptoTransform encryptor = encryption.CreateEncryptor(encryption.Key, encryption.IV);

            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

            using (var swEncrypt = new StreamWriter(csEncrypt))
                swEncrypt.Write(original);

            encrypted = msEncrypt.ToArray();
        }

        return encrypted;
    }

    private static string DecryptStringFromBytes(byte[] encrypted, byte[] key, byte[] IV)
    {
        string plaintext = string.Empty;

        using (var encryption = Aes.Create())
        {
            encryption.Key = key;
            encryption.IV = IV;

            ICryptoTransform decryptor = encryption.CreateDecryptor(encryption.Key, encryption.IV);

            using MemoryStream msDecrypt = new MemoryStream(encrypted);
            using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                plaintext = srDecrypt.ReadToEnd();
            }
        }

        return plaintext;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        btnCancel.IsEnabled = false;

    }
}
