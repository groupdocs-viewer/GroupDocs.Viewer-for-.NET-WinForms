using GroupDocs.Viewer.Exceptions;
using GroupDocs.Viewer.Options;
using GroupsDocs.Viewer.Forms.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GroupDocs.Viewer.Forms.UI
{
    public partial class MainForm : Form
    {
        private string CurrentFilePath { get; set; }
        private Results.ViewInfo ViewInfo { get; set; }

        private GroupDocs.Viewer.Viewer Viewer { get; set; }

        private int CurrentPage { get; set; } = 1;

        const string GeneralTitle = "Viewer for .NET Windows Forms";

        public MainForm()
        {
            InitializeComponent();
            SetLicense();
            UpdateControlsVisibility();
        }

        private void UpdateControlsVisibility()
        {
            firstPageBtn.Enabled = lastPageBtn.Enabled = prevPageBtn.Enabled = nextPageBtn.Enabled = (ViewInfo != null);
            firstPageBtn.Visible = lastPageBtn.Visible = prevPageBtn.Visible = nextPageBtn.Visible = (ViewInfo != null && ViewInfo.Pages.Count > 1);

            if (ViewInfo != null)
            {
                firstPageBtn.Enabled = lastPageBtn.Enabled = (ViewInfo.Pages.Count > 0);

                if (CurrentPage <= 1)
                {
                    firstPageBtn.Enabled = false;
                    lastPageBtn.Enabled = (ViewInfo.Pages.Count > 0);
                }

                if (CurrentPage > 1)
                {
                    firstPageBtn.Enabled = true;
                    lastPageBtn.Enabled = (ViewInfo.Pages.Count == CurrentPage);
                }

                prevPageBtn.Enabled = (CurrentPage != 1);
                nextPageBtn.Enabled = (CurrentPage != ViewInfo.Pages.Count);
            }
        }

        private void SetLicense()
        {
            string fileName = "GroupDocs.Viewer.lic";

            if (File.Exists(fileName))
            {
                GroupDocs.Viewer.License license = new License();
                license.SetLicense(fileName);
                SetLabelText(licenseStatusLabel, "Licensed");
            }
        }

        private void openFileBtn_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\examples";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    CurrentFilePath = openFileDialog.FileName;
                    openFileBtn.Enabled = false;
                }
            }

            if (Viewer != null)
            {
                Viewer.Dispose();
                ViewInfo = null;
                CurrentPage = 1;
                DisplayViewInfo();
            }

            new Thread(() =>
            {
                try
                {
                    SetPagesInfoText($"Loading {Path.GetFileName(CurrentFilePath)}");

                    Viewer = new GroupDocs.Viewer.Viewer(CurrentFilePath);
                    GroupDocs.Viewer.Options.ViewInfoOptions viewInfo = GroupDocs.Viewer.Options.ViewInfoOptions.ForHtmlView();

                    try
                    {
                        ViewInfo = Viewer.GetViewInfo(viewInfo);
                    }
                    catch (PasswordRequiredException)
                    {
                        // Ask for password
                        EnterPasswordBox enterPasswordbox = new EnterPasswordBox();
                        DialogResult res = enterPasswordbox.ShowDialog();

                        if (res == DialogResult.OK)
                        {
                            Viewer.Dispose();
                            ViewInfo = null;

                            LoadOptions loadOptions = new LoadOptions();
                            loadOptions.Password = enterPasswordbox.ResultValue;

                            Viewer = new GroupDocs.Viewer.Viewer(CurrentFilePath, loadOptions);
                            ViewInfo = Viewer.GetViewInfo(viewInfo);
                        }
                        else
                        {
                            ViewInfo = null;
                            webBrowerMain.DocumentText = string.Empty;

                            throw;
                        }
                    }

                    ViewFile(Viewer);
                    openFileBtn.Enabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error occured! {ex.Message}");
                    ViewInfo = null;
                    DisplayViewInfo();
                    openFileBtn.Enabled = true;
                }


            }).Start();

        }

        private void ViewFile(Viewer viewer, int page = 1)
        {
            if (ViewInfo != null)
            {
                HtmlViewOptions htmlViewOptions = HtmlViewOptions.ForEmbeddedResources("temp.html");
                viewer.View(htmlViewOptions, page);

                string curDir = Directory.GetCurrentDirectory();
                this.webBrowerMain.Url = new Uri(String.Format("file:///{0}/temp.html", curDir));
            }

            UpdateControlsVisibility();
            DisplayViewInfo();
        }

        private void DisplayViewInfo()
        {
            if (ViewInfo != null)
            {
                SetPagesInfoText($"{CurrentPage}/{ViewInfo.Pages.Count}");
                SetFormTitle(Path.GetFileName(CurrentFilePath));
            }
            else
            {
                SetPagesInfoText(" ");
                SetFormTitle("No document loaded.");
            }
        }

        private void SetFormTitle(string text)
        {
            this.Text = GeneralTitle + " - " + text;
        }

        #region delegates


        private void SetPagesInfoText(string text)
        {
            pagesStatusLabel.Text = text;
        }

        private void SetLabelText(ToolStripLabel control, string text)
        {
            control.Text = text;

        }

        #endregion

        private void firstPageBtn_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            ViewFile(Viewer, CurrentPage);
        }

        private void prevPageBtn_Click(object sender, EventArgs e)
        {
            if (CurrentPage != 1)
            {
                CurrentPage = CurrentPage - 1;
                ViewFile(Viewer, CurrentPage);
            }
        }

        private void nextPageBtn_Click(object sender, EventArgs e)
        {
            if (CurrentPage != ViewInfo.Pages.Count)
            {
                CurrentPage = CurrentPage + 1;
                ViewFile(Viewer, CurrentPage);
            }
        }

        private void lastPageBtn_Click(object sender, EventArgs e)
        {
            CurrentPage = ViewInfo.Pages.Count;
            ViewFile(Viewer, CurrentPage);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Viewer != null)
            {
                Viewer.Dispose();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //Control control = (Control)sender;

            //// Ensure the Form remains square (Height = Width).
            //if (control.Size.Height != control.Size.Width)
            //{
            //    // Resize web browser size automatically
            //    webBrowser1.Size = new System.Drawing.Size(control.Size.Width - 50, control.Size.Height - 170);

            //    //// Position other controls on resize
            //    //firstPageBtn.Top = prevPageBtn.Top = nextPageBtn.Top = lastPageBtn.Top = openFileBtn.Top = webBrowser1.Bottom + 20;
            //    //PagesLabel.Top = PagesInfoLabel.Top = firstPageBtn.Top + 30;
            //    //licenseStatusLabel.Top = licenseStatusTextLabel.Top = PagesLabel.Top + 15;
            //}
        }
    }
}
