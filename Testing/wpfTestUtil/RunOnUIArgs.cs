using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FindReplaceTesting.wpfTestUtil
{
    public class RunOnUIArgs
    {

        public Action<Window, AvalonTestPieces.TestHost> RunAfterWindowAvailable { get; set; }
    }
}
