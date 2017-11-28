using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace FindReplaceTesting.wpfTestUtil
{
    public static class Utility
    {
        public static (Window win, AvalonTestPieces.TestHost host) SetupHost()
        {
            var host = new AvalonTestPieces.TestHost();
            var win = new Window();
            win.Content = host;

            return (win, host);
        }


        public static Task<RunResult> runWithUIThread(RunOnUIArgs args= null)
        {
            var promise = new TaskCompletionSource<RunResult>();

            var t = new Thread(() =>
            {
                try
                {
                    // followed some stuff here: http://reedcopsey.com/2011/11/28/launching-a-wpf-window-in-a-separate-thread-part-1/
                    SynchronizationContext.SetSynchronizationContext(
                                 new DispatcherSynchronizationContext(
                                     Dispatcher.CurrentDispatcher));

                    var uiObjects = SetupHost();

                    uiObjects.win.Closed += (_s, _args) =>
                    {
                        promise.SetResult(new RunResult
                        {
                            IsError = false
                        });
                        Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    };

                    uiObjects.win.Show();

                    // need #r "System.Windows.Presentation", and using System.WIndows.Threading to get extension to work
                    uiObjects.win.Dispatcher.BeginInvoke(() =>
                    {
                        // once the dispatcher is available then do this stuff
                        if (args != null && args.RunAfterWindowAvailable != null)
                        {
                            args.RunAfterWindowAvailable(uiObjects.win, uiObjects.host);
                        }
                    });

                    // Start the Dispatcher Processing
                    Dispatcher.Run();




                }
                catch(ThreadAbortException ex)
                {
                    // ignore
                }
                catch (Exception ex)
                {
                    promise.SetResult(new RunResult
                    {
                        IsError = true,
                        ex = ex
                    });
                }
            });

            t.TrySetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();

            return promise.Task;
        }
    }
}
