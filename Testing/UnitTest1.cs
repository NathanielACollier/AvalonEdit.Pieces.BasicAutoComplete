using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Threading.Tasks;
using System.Threading;
using ICSharpCode.AvalonEdit;

namespace FindReplaceTesting
{
    [TestClass]
    public class UnitTest1
    {



        [TestMethod]
        public async Task TestMethod1()
        {
            var result = await wpfTestUtil.Utility.runWithUIThread();

            Assert.IsFalse(result.IsError, $"Exception occured: {result.ex}");
        }


        [TestMethod]
        public async Task TestOverlayButton()
        {
            var result = await wpfTestUtil.Utility.runWithUIThread(new wpfTestUtil.RunOnUIArgs
            {
                RunAfterWindowAvailable = (win, host) =>
                {
                    var editor = host.GetTextEditor();
                    var adorner1 = new AvalonEdit.Pieces.GenericControlAdorner(editor.TextArea)
                    {
                        Child = new Button { Content = "Hello World!" }
                    };

                    AdornerLayer.GetAdornerLayer(editor.TextArea).Add(adorner1);
                }
            });

            Assert.IsFalse(result.IsError, $"Exception occured: {result.ex}");
        }


        [TestMethod]
        public async Task TestPiece()
        {
            var result = await wpfTestUtil.Utility.runWithUIThread(new wpfTestUtil.RunOnUIArgs
            {
                RunAfterWindowAvailable = (win, host) =>
                {
                    var editor = host.GetTextEditor();

                    //AvalonEdit.Pieces.LineNumberMarginWithCommands.Install(editor);
                }
            });

            Assert.IsFalse(result.IsError, $"Exception occured: {result.ex}");
        }
    }
}
