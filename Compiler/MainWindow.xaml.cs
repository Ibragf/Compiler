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
        Run firstPart;
        Run secondPart;
        Run ErrorSymbol;
        public MainWindow()
        {
            InitializeComponent();
            uploadFormula();
            uploadExample();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InputText.Foreground = Brushes.Black;
            string[] lines = null;
            try
            {
                OutputText.Text=String.Empty;
                OutputText.Foreground = Brushes.Black;
                string text= new TextRange(InputText.Document.ContentStart, InputText.Document.ContentEnd).Text;

                if (firstPart != null) firstPart.Foreground = Brushes.Black;
                if (secondPart != null) secondPart.Foreground = Brushes.Black;
                if (ErrorSymbol != null) ErrorSymbol.Foreground = Brushes.Black;

                Tokenizer tokenizer = new Tokenizer(text);
                tokenizer.Replacement();
                lines = tokenizer.lines;
                tokenizer.DevideIntoTokens();
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
                OutputText.Text=ex.Message+"\n"+"Строка: "+$"{ex.line}";
                string ErrorText = lines[ex.line - 1];
                string[] words = ErrorText.Split(' ');
                //words[ex.index - 1] = words[ex.index-1];
                StringBuilder builder=new StringBuilder();

                for (int i=0;i<words.Length;i++)
                {
                    builder.Append($"{words[i]}");
                }

                firstPart=new Run();
                ErrorSymbol = new Run();
                ErrorSymbol.Foreground = Brushes.Red;
                secondPart=new Run();

                StringBuilder changeText=new StringBuilder();
                for (int i=0;i<lines.Length;i++)
                {
                    if (i == ex.line - 1)
                    {
                        break;
                    }
                    changeText.Append(lines[i]+"\n");
                }
                for (int i=0;i<words.Length;i++)
                {
                    if(i==ex.index-1)
                    {
                        ErrorSymbol.Text=words[i]+" ";
                        firstPart.Text=changeText.ToString();
                        changeText.Clear();
                        for (int j=i+1;j<words.Length;j++)
                        {
                            changeText.Append(words[j]);
                        } 
                        changeText.Append('\n');
                        break;
                    }
                    changeText.Append(words[i]);
                }
                for (int i=ex.line;  i< lines.Length; i++)
                {
                    if(i==lines.Length-2)
                    {
                        changeText.Append(lines[i]);
                        break;
                    }
                    changeText.Append(lines[i]+"\n");
                }
                secondPart.Text=changeText.ToString().Replace(" ", String.Empty);

                Paragraph paragraph=new Paragraph();
                InputText.Document.Blocks.Clear();
                InputText.Document.Blocks.Add(paragraph);
                paragraph.Inlines.Add(firstPart);
                paragraph.Inlines.Add(ErrorSymbol);
                paragraph.Inlines.Add(secondPart);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        //нумерация строк в LineNumber(textbox)
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string richText = new TextRange(InputText.Document.ContentStart, InputText.Document.ContentEnd).Text;

            string text=String.Empty;
            for (int i=0; i<richText.Count(x=> x =='\n'); i++)
            {
                text+=(i+1)+"\n";
            }
            LineNumber.Text = text;

            //TextPointer line = InputText.CaretPosition;
            
            //прокрутка при достижении каретки последней линии или линии больше 27 строки
            /*int line = InputTextBox.GetLineIndexFromCharacterIndex(InputTextBox.CaretIndex) + 1;
            if (line == InputTextBox.LineCount||line>27)
            {
                InputTextBox.LineDown();
            }*/
        }

        //синхронизация прокрутки LineNumber и InputTextBox
        private void LineNumber_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if(sender == LineNumber)
            {
                var textToSync = InputText;
                textToSync.ScrollToVerticalOffset(e.VerticalOffset);
            }
            else
            {
                var textToSync = LineNumber;
                textToSync.ScrollToVerticalOffset(e.VerticalOffset);
            }
        }

        public void uploadFormula()
        {
            using (FileStream fstream = File.OpenRead("formula.txt"))
            {
                byte[] buffer = new byte[fstream.Length];
                fstream.Read(buffer,0, buffer.Length);
                formula.Text=Encoding.UTF8.GetString(buffer);
            }
        }


        public void uploadExample()
        {
            using(FileStream fs=File.OpenRead("example.txt"))
            {
                byte[] buffer=new byte[fs.Length];
                fs.Read(buffer,0,buffer.Length);

                Paragraph paragraph = new Paragraph();
                InputText.Document.Blocks.Clear();
                InputText.Document.Blocks.Add(paragraph);

                paragraph.Inlines.Add(new Run(Encoding.UTF8.GetString(buffer)));
            }
        }
    }
}
