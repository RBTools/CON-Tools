using System.Drawing;
using System.Windows.Forms;

namespace C3Tools
{
    public partial class PasswordUnlocker : Form
    {
        public PasswordUnlocker(string name = "")
        {
            InitializeComponent();
            toolTip1.SetToolTip(btnOK,"C3 USE ONLY: Click to process your password");
            toolTip1.SetToolTip(txtPass, "C3 USE ONLY: Click to enter your password");
            txtPass.Text = name;
        }

        public string EnteredText
        {
            get
            {
                return (txtPass.Text);
            }
        }

        private void btnGo_Click(object sender, System.EventArgs e)
        {
            Close();
        }
        
        public void Renamer()
        {
            txtPass.PasswordChar = '\0';
            topLabel.Text = "Enter new name below\nthen click OK";
            btnOK.BackColor = Color.FromArgb(192, 255, 192);
            topLabel.ForeColor = Color.Green;
            toolTip1.SetToolTip(btnOK, "Click to change name");
            toolTip1.SetToolTip(txtPass, "Enter new name here");
        }

        public void LockManager()
        {
            topLabel.Text = "Enter password below\nthen click OK";
            btnOK.BackColor = Color.FromArgb(192, 255, 192);
            topLabel.ForeColor = Color.Green;
            toolTip1.SetToolTip(btnOK, "Click to save password");
            toolTip1.SetToolTip(txtPass, "Enter password here");
        }

        public void IDChanger()
        {
            txtPass.PasswordChar = '\0';
            topLabel.Text = "Enter new song ID below\nthen click OK";
            btnOK.BackColor = Color.FromArgb(192, 255, 192);
            topLabel.ForeColor = Color.Green;
            toolTip1.SetToolTip(btnOK, "Click to change song ID");
            toolTip1.SetToolTip(txtPass, "Enter new song ID here");
        }
    }
}
