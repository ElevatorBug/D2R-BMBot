using System;
using System.Drawing;
using System.Windows.Forms;

public partial class Form1
{
    // Compatibility entry point for existing scripts and settings code.
    public void method_1(string string_3, Color ThisColor, bool LogTime = true)
    {
        AppendLog(string_3, ThisColor, LogTime);
    }

    /// <summary>Writes one message to the desktop log and existing log sinks.</summary>
    public void AppendLog(string message, Color color, bool includeTime = true)
    {
        if (richTextBox1.InvokeRequired)
        {
            // Preserve includeTime when dispatching from the timer thread.
            Action write = delegate { AppendLog(message, color, includeTime); };
            richTextBox1.Invoke(write);
            return;
        }

        if (includeTime) message += " " + GameStruc_0.GetTimeNow();
        Console.WriteLine(message);
        if (color == Color.OrangeRed && !CharConfig.LogNotUsefulErrors) return;

        richTextBox1.SelectionColor = color;
        richTextBox1.AppendText(message + Environment.NewLine);
        overlayForm.AddLogs(message, color);

        if (color == Color.Red || color == Color.Orange ||
            color == Color.DarkOrange || color == Color.OrangeRed)
            AppendTextErrorLogs(message, color);
        if (color == Color.DarkBlue) AppendTextGameLogs(message, color);

        // Existing scheduling behavior is retained during this small refactor.
        Application.DoEvents();
    }
}
