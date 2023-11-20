using WpfAnswerBert;

using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Input;
using System;
using NuGetBertSpace;

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
            TextBlockAnswer.Text = "Здесь будет ответ";
            BertComponent = bertComponent;
            TokenSource = tokenSource;
            CancelBtn.IsEnabled = false;
        }

        private string selectedFilePath;
        private string answer;
        CancellationTokenSource TokenSource;
        NuGetBertSpace.NuGetBert BertComponent;
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
        public async void QueryBox_KeyDown(object sender, KeyEventArgs e)
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
                    var sentence = $"{{\"question\": \"{question}\", \"context\": \"{hobbit}\"}}";
                    try
                    {
                        var task = await BertComponent.AnswerBert(sentence, TokenSource.Token);
                        
                        TextBlockAnswer.Text = task;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"QueryBox_KeyDown: An error occurred: {ex.Message}");
                    }
                    CancelBtn.IsEnabled = false;
                }
            }
        }

    }
}
