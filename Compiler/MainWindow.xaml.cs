using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //string example=InputTextBox.Text;
            //int line = InputTextBox.GetLineIndexFromCharacterIndex(InputTextBox.CaretIndex)+1;
            //string position=InputTextBox.CaretIndex.ToString();
            //OutputText.Text = position+"   "+InputTextBox.Text.Length+"   "+line.ToString()+"  "+InputTextBox.LineCount;

            Tokenizer tokenizer = new Tokenizer(InputTextBox.Text);
            AnalyzerOfTokens analyzer = new AnalyzerOfTokens(tokenizer);
            Generator generator = new Generator(analyzer);
            OutputText.Text = tokenizer.text;

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

    }
}
