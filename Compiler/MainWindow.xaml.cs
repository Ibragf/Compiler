using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Compiler
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string formula_text;
        public MainWindow()
        {
            InitializeComponent();
            uploadFormula();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = null;
            try
            {
                OutputText.Text=String.Empty;
                OutputText.Foreground = Brushes.Black;

                Tokenizer tokenizer = new Tokenizer(InputTextBox.Text);
                lines = tokenizer.lines;
                AnalyzerOfTokens analyzer = new AnalyzerOfTokens(tokenizer);
                Generator generator = new Generator(analyzer);

                List<Token> variables = generator.GetVariables();


                string outText = String.Empty;
                if (variables.Count > 0)
                {
                    variables.Sort(new VariableComparer());
                    foreach (var variable in variables)
                    {
                        outText += variable.name + " = " + variable.tokenValue + "\n";
                    }
                }
                OutputText.Text += outText;
            }
            catch (MyException ex)
            {
                OutputText.Foreground = Brushes.Red;
                OutputText.Text=ex.Message+"\n"+"Строка: "+$"{ex.line}\n"+"Слово: "+$"{ex.index}\n";
                string ErrorText = lines[ex.line - 1];
                string[] words = ErrorText.Split(' ');
                words[ex.index - 1] = "{"+words[ex.index-1]+"}";
                StringBuilder builder=new StringBuilder();

                for (int i=0;i<words.Length;i++)
                {
                    builder.Append($"{words[i]} ");
                }

                builder.Replace("\r\n", String.Empty);
                string text= "["+ builder.ToString()+"]";
                int index=text.IndexOf("{");
                string scape = new string(' ', index+words[ex.index-1].Length/2);
                scape += "↑";
                text += "\n" + scape;
                OutputText.Text+=text;

            }
        }


        //нумерация строк в LineNumber(textbox)
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text=String.Empty;
            for (int i=0; i<InputTextBox.LineCount; i++)
            {
                text+=(i+1)+"\n";
            }
            LineNumber.Text = text;

            //прокрутка при достижении каретки последней линии или линии больше 27 строки
            int line = InputTextBox.GetLineIndexFromCharacterIndex(InputTextBox.CaretIndex) + 1;
            if (line == InputTextBox.LineCount||line>27)
            {
                InputTextBox.LineDown();
            }
        }

        //синхронизация прокрутки LineNumber и InputTextBox
        private void LineNumber_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var textToSync = (sender == LineNumber) ? InputTextBox : LineNumber;
            textToSync.ScrollToVerticalOffset(e.VerticalOffset);
        }

        public async void uploadFormula()
        {
            using (FileStream fstream = File.OpenRead("formula.txt"))
            {
                byte[] buffer = new byte[fstream.Length];
                await fstream.ReadAsync(buffer,0, buffer.Length);
                formula.Text=Encoding.UTF8.GetString(buffer);
            }
        }

    }
}
