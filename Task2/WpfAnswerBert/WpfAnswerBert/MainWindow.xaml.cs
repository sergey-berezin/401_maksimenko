using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
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
using NuGetBertSpace;
namespace WpfAnswerBert
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int tabNum = 0;
        string modelPath = "D:\\Березин1\\Task2\\WpfAnswerBert\\WpfAnswerBert\\bert-large-uncased-whole-word-masking-finetuned-squad.onnx";
        NuGetBert answerTask;
        public MainWindow()
        {
            InitializeComponent();
            CancellationTokenSource cancel = new CancellationTokenSource();
            answerTask = new NuGetBert(modelPath, cancel.Token);
            try
            {
                _ = answerTask.Download();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MainWindow: An error occurred: {ex.Message}");
            }
            CreateNewTabElem(answerTask, cancel);
        }
        private void AddNewTab(string header, object content)
        {
            var newTab = new TabItem
            {
                Header = header,
                Content = content
            };

            var closeButton = new Button { Content = "X" };
            closeButton.Click += (s, e) => CloseTab(newTab);
            var headerStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerStackPanel.Children.Add(new TextBlock { Text = header });
            headerStackPanel.Children.Add(closeButton);

            newTab.Header = headerStackPanel;

            TabNumber.Items.Add(newTab);
        }

        private void CloseTab(TabItem tab)
        {
            TabNumber.Items.Remove(tab);
        }
       
        private void CreateNewTabElem(NuGetBert answerTask, CancellationTokenSource cts)
        {
            AddNewTab($"Tab {tabNum}", new TabData(answerTask, cts));
            tabNum++;
        }
     
        private void AddTabButton_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CreateNewTabElem(answerTask, cts);
        }
    }
}
