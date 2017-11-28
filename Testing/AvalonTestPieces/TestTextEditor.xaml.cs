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

using ICSharpCode.AvalonEdit;


namespace FindReplaceTesting.AvalonTestPieces
{
    /// <summary>
    /// Interaction logic for TestTextEditor.xaml
    /// </summary>
    public partial class TestTextEditor : TextEditor
    {
        public TestTextEditor()
        {
            InitializeComponent();

        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {

        }


    }
}
