using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AvalonEdit.Pieces
{
    public class AutoComplete
    {
        //TODO: private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        TextEditor editor;
        CompletionWindow completionWindow;

        DispatcherTimer filterTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(400)
        };



        public static void Install(TextEditor _editor)
        {
            var a = new AutoComplete(_editor);
        }

        public AutoComplete(TextEditor _editor)
        {
            this.editor = _editor;
            _editor.TextArea.TextEntered += TextArea_TextEntered;
            _editor.TextArea.TextEntering += TextArea_TextEntering;
            _editor.TextArea.KeyDown += TextArea_KeyDown;
            _editor.TextArea.KeyUp += TextArea_KeyUp;

            filterTimer.Tick += FilterTimer_Tick;
        }

        private void TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            var isKeyNumber = e.Key >= Key.D0 && e.Key <= Key.D9;
            var isKeyLetter = e.Key >= Key.A && e.Key <= Key.Z;

            // enter, tab, and keys like that will only fire here
            if (isKeyNumber || isKeyLetter)
            {
                filterTimer.Stop();
                filterTimer.Start();
                // don't handle because we want these keys to be typed
            }
            else
            {
                if (filterTimer.IsEnabled)
                {
                    //TODO: log.Info("Stop Filter Timer");
                    filterTimer.Stop(); // if we previously pressed a key to start the timer but now we pressed another key stop it
                }
            }
        }

        private void FilterTimer_Tick(object sender, EventArgs e)
        {
            //TODO: log.Info("Filter Timer Fired");
            filterTimer.Stop();
            invokeCodeCompleteManually();
        }

        private void TextArea_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            // original from: http://stackoverflow.com/questions/32022517/avalonedit-is-not-showing-the-data-in-completionwindow-for-keydown-event
            if (e.Key == Key.Space && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                invokeCodeCompleteManually();
                e.Handled = true; // this will stop the space from appearing
            }
        }

        private void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // original from: http://avalonedit.net/documentation/html/47c58b63-f30c-4290-a2f2-881d21227446.htm
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // original from: http://avalonedit.net/documentation/html/47c58b63-f30c-4290-a2f2-881d21227446.htm

            //var completeStarters = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.";
            var completeStarters = new[]
            {
                "."
            };

            if (completeStarters.Contains(e.Text))
            {
                startCodeComplete();
            }
        }




        private bool isValidTokenChar(char ch)
        {
            if (char.IsLetterOrDigit(ch)
                || ch == '-'
                || ch == '_'
                )
            {
                return true;
            }

            return false;
        }


        private int getLastAlphaNumericCarretPositionFromCurrentPos()
        {
            // see for idea (But no code): http://community.sharpdevelop.net/forums/p/16033/42888.aspx
            var currentCarret = this.editor.CaretOffset;

            // go backwards and find the first non letter/digit
            bool found = false;
            int lastAlphaNumericCarret = currentCarret;

            // if we are at an empty textbox, then don't look backwards because there isn't anything
            if (lastAlphaNumericCarret > 0)
            {
                do
                {
                    lastAlphaNumericCarret = lastAlphaNumericCarret - 1;
                    // is this new position not a letter or a digit?
                    // if it's not then we move forward one since we moved backwards one to get to it and get out of the loop
                    char lastChar = this.editor.Document.GetCharAt(lastAlphaNumericCarret);
                    if (!isValidTokenChar(lastChar))
                    {
                        // move forward one since we want the last character to be a letter or a digit
                        lastAlphaNumericCarret++;
                        found = true;
                    }

                    // for debugging
                    string debugText = this.editor.Document.GetText(lastAlphaNumericCarret, currentCarret - lastAlphaNumericCarret);
                    //TODO: log.Info($"Code complete backwards step:  Word So Far: {debugText}");
                } while (found == false && lastAlphaNumericCarret > 0);
            }

            return lastAlphaNumericCarret;
        }


        private void invokeCodeCompleteManually()
        {
            int codeCompleteTextTypedStart = getLastAlphaNumericCarretPositionFromCurrentPos();

            startCodeComplete(codeCompleteTextTypedStart);

        }



        private void startCodeComplete(int startOffSet = -1)
        {
            // Open code completion after the user has pressed dot:
            completionWindow = new CompletionWindow(this.editor.TextArea);

            completionWindow.Closed += delegate {
                completionWindow = null;
            };

            IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
            performCompletionThread(data, this.editor.Text, whenDoneUseDispatching: () =>
            {
                if (startOffSet >= 0)
                {
                    // see: http://community.sharpdevelop.net/forums/p/16033/42888.aspx
                    completionWindow.StartOffset = startOffSet;
                    filterCompletionWindowByOffsetFromCurrentCaret(startOffSet);
                }

                //if( data!= null && data.Any())
                if (completionWindow.CompletionList.ListBox.HasItems)
                {
                    completionWindow.Show();
                }
            });






        }

        private void filterCompletionWindowByOffsetFromCurrentCaret(int startOffSet)
        {
            try
            {
                completionWindow.CompletionList.IsFiltering = true; // set just incase
                string partialWord = this.editor.Document.GetText(startOffSet, this.editor.CaretOffset - startOffSet);
                //TODO: log.Info($"Partial Word is: [{partialWord}]");

                var dataQueryForPartialWord = from item in completionWindow.CompletionList.CompletionData
                                              where string.Equals(item.Text, partialWord)
                                              select item;

                if (dataQueryForPartialWord.Any())
                {
                    completionWindow.CompletionList.CompletionData.Remove(dataQueryForPartialWord.First());
                }

                completionWindow.CompletionList.SelectItem(partialWord);
            }
            catch (Exception ex)
            {
                //TODO: log.Error($"Exception setting partial word match from last valid completion text.  Exception: {ex}");
            }
        }

        private void performCompletionThread(IList<ICompletionData> data, string programText, Action whenDoneUseDispatching = null)
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(programText))
                    {
                        var nonAlphaNumericChars = from ch in programText.ToCharArray()
                                                   where !isValidTokenChar(ch)
                                                   select ch;

                        if (nonAlphaNumericChars.Any())
                        {
                            char[] splitters = nonAlphaNumericChars
                                                       .Distinct()
                                                       .ToArray();

                            var tokens = from str in programText.Split(splitters)
                                         where !string.IsNullOrWhiteSpace(str)
                                         select str;

                            if (tokens.Any())
                            {
                                var distinctTokens = tokens.Distinct();

                                this.editor.Dispatcher.Invoke(() =>
                                {
                                    foreach (string str in distinctTokens)
                                    {
                                        data.Add(new TextEditorCompletionData(str));
                                    }
                                });

                            }
                        }
                    }
                    /*
                    data.Add(new TextEditorCompletionData("Item1"));
                    data.Add(new TextEditorCompletionData("Item2"));
                    data.Add(new TextEditorCompletionData("Item3"));
                    */
                    if (whenDoneUseDispatching != null)
                    {
                        this.editor.Dispatcher.Invoke(() =>
                        {
                            whenDoneUseDispatching();
                        });
                    }

                }
                catch (Exception ex)
                {
                    //TODO: log.Error($"Code completion population exception.  Exception: {ex}");
                }
            });

            t.Start();
        }




    }
}
