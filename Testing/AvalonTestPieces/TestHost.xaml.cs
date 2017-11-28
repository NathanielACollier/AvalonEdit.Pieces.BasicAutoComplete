using ICSharpCode.AvalonEdit;
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

namespace FindReplaceTesting.AvalonTestPieces
{
    /// <summary>
    /// Interaction logic for TestHost.xaml
    /// </summary>
    public partial class TestHost : UserControl
    {
        public TestHost()
        {
            InitializeComponent();
        }


        public TextEditor GetTextEditor()
        {
            return this.FindName("textEditor") as TextEditor;
        }


    }
}
