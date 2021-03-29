using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GroupsDocs.Viewer.Forms.UI
{
    public partial class EnterPasswordBox : Form
    {
        public EnterPasswordBox()
        {
            InitializeComponent();
        }

        public string ResultValue { get; set; }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text != null && string.IsNullOrEmpty(txtPassword.Text.Trim()))
            {
                MessageBox.Show("Please enter password!");
            }
            else
            {
                DialogResult = DialogResult.OK;
                ResultValue = txtPassword.Text;
                this.Close();
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void EnterPasswordBox_Shown(object sender, EventArgs e)
        {
            ResultValue = string.Empty;
        }
    }
}
