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
using DataManagerSpace;
using Microsoft.Extensions.Options;
using static DataManagerSpace.QandA;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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
            TextBlockAnswer.Text = "Здесь будет ответ";
            BertComponent = bertComponent;
            TokenSource = tokenSource;
            CancelBtn.IsEnabled = false;
        }
        public TabData(NuGetBert bertComponent, CancellationTokenSource tokenSource, string text)
        {
            InitializeComponent();
            TextBlockAnswer.Text = "Здесь будет ответ";
            BertComponent = bertComponent;
            TokenSource = tokenSource;
            CancelBtn.IsEnabled = false;
            TextBlock.Text = text;
            DisplayAnswer();
        }
        int tabId;
        string text;
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
                      
                        using (var context = new DataManager())
                        {
                            // Проверяем наличие текста вкладки
                            bool tabTextExists = context.TabTexts.Any(t => t.Text == TextBlock.Text);

                            // Проверяем наличие запроса и ответа в базе данных
                            if (tabTextExists)
                            {   
                                //Текст уже был
                                tabId = context.TabTexts.FirstOrDefault(t => t.Text == TextBlock.Text).Id;
                                
                                var existingQandA = context.AnswerHistories
                                    .FirstOrDefault(qa => qa.QueryText == question && qa.AnswerText != null && qa.TabTextId == tabId);
                                /*int maxId = context.AnswerHistories
                                      .Where(qa => qa.TabTextId == tabId)
                                      .Select(qa => qa.Id)
                                      .DefaultIfEmpty()
                                      .Max();*/
                                if (existingQandA != null)
                                {
                                    
                                    //Если ответ есть в бд, получаем и увеличиваем id
                                    TextBlockAnswer.Text = existingQandA.AnswerText;
                                    var newQandA = new AnswerHistory { QueryText = existingQandA.QueryText, AnswerText = existingQandA.AnswerText, TabTextId = tabId };
                                    context.AnswerHistories.Remove(existingQandA);
                                    context.AnswerHistories.Add(newQandA);
                                }
                                else
                                {
                                    // Если ответа нет в базе данных, выполняем запрос
                                    var task = await BertComponent.AnswerBert(sentence, TokenSource.Token);
                                    TextBlockAnswer.Text = task;
                                    
                                    var newQandA = new AnswerHistory { QueryText = question, AnswerText = task, TabTextId = tabId};
                                    context.AnswerHistories.Add(newQandA);
                                }
                            }
                            else
                            {
                                //Текст новый
                                Random random = new Random();
                                int randomNumber = random.Next();
                                var newTabText = new TabText { Text = TextBlock.Text, Id = randomNumber };
                                context.TabTexts.Add(newTabText);

                                var task = await BertComponent.AnswerBert(sentence, TokenSource.Token);
                                TextBlockAnswer.Text = task;
                                var newQandA = new AnswerHistory { QueryText = question, AnswerText = task, TabTextId = randomNumber, Id = 0 };
                                context.AnswerHistories.Add(newQandA);
                            }
                            await context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"QueryBox_KeyDown: An error occurred: {ex.Message}");
                    }
                    CancelBtn.IsEnabled = false;
                }
            }
        }

        private void DisplayAnswer()
        {
            using (var context = new DataManager())
            {

                var lastQandA = context.AnswerHistories
                 .Where(qa => qa.TabTextId == context.TabTexts.FirstOrDefault(t => t.Text == TextBlock.Text).Id)
                 .OrderByDescending(qa => qa.Id)
                 .FirstOrDefault();

                if (lastQandA != null)
                {
                    lastQuestionTextBox.Text = $"Last Question: {lastQandA.QueryText}";
                    lastAnswerTextBox.Text = $"Last Answer: {lastQandA.AnswerText}";
                }
                else
                {
                    // Если база данных пуста или нет записей в таблице AnswerHistories
                    lastQuestionTextBox.Text = "No data in the database.";
                    lastAnswerTextBox.Text = "No data in the database.";
                }
            }
        }

    }
}
