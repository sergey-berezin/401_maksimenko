using ConsoleApp1;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.IO;
using System.Threading.Channels;
using System.Diagnostics.Metrics;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using System.Reflection.Metadata;

namespace WpfAnswerBert
{
    /// <summary>
    /// Логика взаимодействия для TabData.xaml
    /// </summary>
    public partial class TabData : UserControl
    {
        public TabData(NuGetBert bertComponent, CancellationTokenSource tokenSource)
        {
            InitializeComponent();
            BertComponent = bertComponent;
            TokenSource = tokenSource;
            CancelBtn.IsEnabled = false;
        }

        private string selectedFilePath;
        private string answer;
        CancellationTokenSource TokenSource;
        NuGetBert BertComponent;
        private void LoadFileBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text documents (*.txt)|*.txt|All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                selectedFilePath = dialog.FileName;
                string content = File.ReadAllText(selectedFilePath);
                TextBlock.Text = content;
            }
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            TokenSource.Cancel();
        }
        private async void QueryBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CancelBtn.IsEnabled = true;
                string hobbit = TextBlock.Text;
                string question = QuestionBox.Text;
                if (string.IsNullOrWhiteSpace(hobbit))
                {
                    MessageBox.Show("Выберете файл");
                }
                else if (string.IsNullOrWhiteSpace(question))
                {
                    MessageBox.Show("Задайте вопрос");
                }
                else
                {
                    string sentence = $"{{\"question\": {question}, \"context\": \"@CTX\"}}".Replace("@CTX", hobbit);
                    var task = BertComponent.AnswerBert(sentence, TokenSource.Token).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            // Обработка исключения, если оно возникло
                            MessageBox.Show(t.Exception.Message);
                        }
                        else
                        {
                            // Устанавливаем текст в TextBlockAnswer
                            TextBlockAnswer.Text = t.Result;
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());

                    CancelBtn.IsEnabled = false;
                }
            }
        }

    }
}
