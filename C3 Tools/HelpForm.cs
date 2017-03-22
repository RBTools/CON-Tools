using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace C3Tools
{
    public partial class HelpForm : Form
    {
        private readonly string form_title;

        public HelpForm(string title, string help_message, bool maximized = false)
        {
            InitializeComponent();

            if (File.Exists(Application.StartupPath + "\\c3contools_changelog.txt"))
            {
                btnReadMe.Visible = true;
            }

            form_title = title;
            txtHelp.Text = help_message;

            if (maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnReadMe_Click(object sender, EventArgs e)
        {
            Process.Start(Application.StartupPath + "\\c3contools_changelog.txt");
        }

        private void HelpForm_Shown(object sender, EventArgs e)
        {
            Text = form_title;
        }

        private void txtHelp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                Close();
            }
            if (e.Control && e.KeyCode == Keys.A)
            {
                txtHelp.SelectAll();
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                Clipboard.SetText(txtHelp.Text);
            }
        }

        private void HelpForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
            {
                Close();
            }
        }

        private void txtHelp_DoubleClick(object sender, EventArgs e)
        {
            txtHelp.SelectAll();
        }
    }
}
