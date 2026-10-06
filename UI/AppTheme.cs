using System.Drawing;
using System.Windows.Forms;

/// <summary>Shared presentation for the desktop forms.</summary>
internal static class AppTheme
{
    private static readonly Color Background = Color.FromArgb(241, 245, 249);
    private static readonly Color Surface = Color.White;
    private static readonly Color Text = Color.FromArgb(30, 41, 59);
    private static readonly Color Border = Color.FromArgb(203, 213, 225);
    private static readonly Color Accent = Color.FromArgb(29, 78, 216);

    public static void Apply(Form form)
    {
        // Keep the OS accessibility palette when high contrast is enabled.
        if (SystemInformation.HighContrast) return;

        form.SuspendLayout();
        try
        {
            ApplyControl(form);
        }
        finally
        {
            form.ResumeLayout(false);
        }
    }

    private static void ApplyControl(Control control)
    {
        control.BackColor = Background;
        control.ForeColor = Text;

        var button = control as Button;
        if (button != null)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(219, 234, 254);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(191, 219, 254);
            button.UseVisualStyleBackColor = false;
            button.BackColor = Surface;
            button.ForeColor = Accent;
        }

        var page = control as TabPage;
        if (page != null) page.UseVisualStyleBackColor = false;

        if (control is TextBoxBase || control is ListView ||
            control is ListBox || control is ComboBox || control is NumericUpDown)
            control.BackColor = Surface;

        var grid = control as DataGridView;
        if (grid != null)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Background;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Background;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Text;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Text;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Background;
        }

        var strip = control as ToolStrip;
        if (strip != null)
        {
            strip.Renderer = new ToolStripSystemRenderer();
            foreach (ToolStripItem item in strip.Items)
            {
                item.BackColor = Background;
                item.ForeColor = Text;
            }
        }

        // Existing fixed sizes and diagnostic monospace fonts stay intact.
        foreach (Control child in control.Controls) ApplyControl(child);
    }
}
