using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using C3Tools.Properties;
using C3Tools.x360;
using DevComponents.AdvTree;
using Color = System.Drawing.Color;

namespace C3Tools
{
    public partial class CONExplorer : Form
    {
        private const string C3Text = "Brought to you by C3. For more great customs like this one, visit www.customscreators.com";
        private const string HmxText = "Please support the developers. Buy The Beatles: Rock Band and its DLC. Enjoy.";
        private Boolean FailedSign;
        private string outFolder;
        private string folder;
        private bool starting;
        private bool internalDrop;
        private bool Changes;
        private readonly NemoTools Tools;
        private readonly DTAParser Parser;
        private FileEntry activeDTA;
        private FileEntry activeMOGG;
        private FileEntry activePNG;
        private FileEntry activeMIDI;
        private FileEntry activeUPG;
        private readonly bool isRunningShortcut;
        private static Color mMenuBackground;

        public CONExplorer(Color ButtonBackColor, Color ButtonTextColor, bool runningshortcut = false)
        {
            CheckForIllegalCrossThreadCalls = true;
            InitializeComponent();
            mMenuBackground = contextMenuStrip1.BackColor;
            contextMenuStrip1.Renderer = new DarkRenderer();

            picContent.AllowDrop = true;
            picPackage.AllowDrop = true;
            btnDTA.AllowDrop = true;
            btnMIDI.AllowDrop = true;
            btnMOGG.AllowDrop = true;
            btnPNG.AllowDrop = true;

            Tools = new NemoTools();
            Parser = new DTAParser();
            isRunningShortcut = runningshortcut;

            var formButtons = new List<Button> { btnExtract,btnSave,btnVisualize };
            foreach (var button in formButtons)
            {
                button.BackColor = ButtonBackColor;
                button.ForeColor = ButtonTextColor;
                button.FlatAppearance.MouseOverBackColor = button.BackColor == Color.Transparent ? Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B) : Tools.LightenColor(button.BackColor);
            }

            toolTip1.SetToolTip(picContent, "Click here to select the Content Image (visible in here)");
            toolTip1.SetToolTip(picPackage, "Click here to select the Package Image (visible in the Xbox dashboard)");
            toolTip1.SetToolTip(radioCON, "Click to save as CON when rebuilding");
            toolTip1.SetToolTip(radioLIVE, "Click to save as LIVE when rebuilding");
            toolTip1.SetToolTip(btnExtract, "Click to extract the whole CON file");
            toolTip1.SetToolTip(btnSave, "Click to sign and rebuild CON file");
            toolTip1.SetToolTip(txtTitle, "Package Title (optional)");
            toolTip1.SetToolTip(txtDescription, "Package Description (optional)");
            toolTip1.SetToolTip(btnVisualize, "Click to open in Visualizer");
        }

        private STFSPackage xPackage { get; set; }

        private void Log(string message)
        {
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new MethodInvoker(() => lstLog.Items.Add(message)));
                lstLog.Invoke(new MethodInvoker(() => lstLog.SelectedIndex = lstLog.Items.Count - 1));
            }
            else
            {
                lstLog.Items.Add(message);
                lstLog.SelectedIndex = lstLog.Items.Count - 1;
            }
        }

        private void SetNodes()
        {
            node1.Nodes.Clear();
            fileList.Items.Clear();
            node1.DataKey = xPackage.RootDirectory;
            var x = xPackage.RootDirectory.GetSubFolders();
            foreach (var y in x)
            {
                node1.Nodes.Add(GetNode(y));
            }
            xReturn_NodeClick(node1, null);
        }

        private Node GetNode(ItemEntry x)
        {
            var xReturn = new Node { Text = x.Name, DataKey = x };
            xReturn.NodeClick += xReturn_NodeClick;
            return xReturn;
        }

        private void xReturn_NodeClick(object sender, EventArgs e)
        {
            if (!xPackage.ParseSuccess)
            {
                MessageBox.Show("There was an error. Try closing and reopening CON file\nSorry", Text,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            var xsender = (Node)sender;
            var x = (FolderEntry)xsender.DataKey;
            var xFiles = x.GetSubFiles();
            var xFolders = x.GetSubFolders();

            if (x == null || xFiles == null || xFolders == null)
            {
                return;
            }
            fileList.Items.Clear();

            foreach (var y in xFiles)
            {
                var z = new ListViewItem(y.Name);
                string size;
                if (y.Size < 1024) //1 KB
                {
                    size = "1 KB";
                }
                else if (y.Size < 1048576) //1MB
                {
                    size = decimal.Round((decimal)y.Size / 1024, 2) + " KB";
                }
                else
                {
                    size = decimal.Round((decimal)y.Size / 1048576, 2) + " MB";
                }
                z.SubItems.Add(size);
                z.Tag = y;
                fileList.Items.Add(z);
            }
            xsender.Nodes.Clear();

            //sort subfolders
            var xfolders = xFolders.Select(f => f.Name).ToList();
            xfolders.Sort();

            foreach (var f in xfolders)
            {
                for (var i = 0; i < xfolders.Count; i++)
                {
                    if (xFolders[i].Name != f) continue;
                    xsender.Nodes.Add(GetNode(xFolders[i]));
                }
            }
        }

        public void LoadCON(string file)
        {
            starting = true;
            if (VariousFunctions.ReadFileType(file) != XboxFileType.STFS)
            {
                MessageBox.Show("That's not a song file ... try again.", "What are you doing?!!?",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            xPackage = new STFSPackage(file);
            if (!xPackage.ParseSuccess)
            {
                Log("Error opening CON file");
                return;
            }
            
            tabControl1.Enabled = false;
            Log("CON file opened successfully");
            Log("Processing");

            //populate treeview and listview with package contents
            node1.NodeClick += xReturn_NodeClick;
            SetNodes();
            node1.SetChecked(true, eTreeAction.Expand);
            Log("Finished processing the contents");
            Log("Ready.");

            //populate main form with package info
            folder = file;
            Text = Path.GetFileName(file);
            toolTip1.SetToolTip(chkAutoExtract, "CON file will be extracted to " + file + "_extracted\\");
            txtTitle.Text = Tools.FixBadChars(xPackage.Header.Title_Display);
            txtDescription.Text = Tools.FixBadChars(xPackage.Header.Description);
            picContent.Image = xPackage.Header.ContentImage;
            picPackage.Image = xPackage.Header.PackageImage;

            switch (xPackage.Header.TitleID)
            {
                case 0x45410829:
                    cboGameID.SelectedIndex = 0;
                    break;
                case 0x45410869:
                    cboGameID.SelectedIndex = 1;
                    break;
                case 0x45410914:
                    cboGameID.SelectedIndex = 2;
                    break;
                default:
                    cboGameID.Items.Add("Unknown - " + xPackage.Header.TitleID.ToString(CultureInfo.InvariantCulture));
                    cboGameID.SelectedIndex = 3;
                    cboGameID.Enabled = false;
                    break;
            }

            if (xPackage.Header.ThisType == PackageType.MarketPlace)
            {
                radioLIVE.Checked = true;
            }
            else
            {
                radioCON.Checked = true;
            }

            checkDescription(true);
            tabControl1.Enabled = true;
            starting = false;
            ShowChanges(false);
        }

        private void extractFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedIndices.Count == 0)
            {
                return;
            }

            var savedir = Path.GetDirectoryName(xPackage.FileNameLong);
            for (var i = 0; i < fileList.SelectedItems.Count; i++)
            {
                var xent = (FileEntry)fileList.SelectedItems[i].Tag;
                if (xent == null)
                {
                    Log("Error extracting " + xent.Name);
                    Enabled = true;
                    return;
                }

                var extension = Path.GetExtension(xent.Name);
                if (extension != "")
                {
                    extension = extension.Substring(1).ToUpper() + " Files|*" + extension;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = extension,
                    InitialDirectory = savedir,
                    FileName = Path.GetFileNameWithoutExtension(xent.Name)
                };
                if (sfd.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(sfd.FileName))
                {
                    Log("Extracting cancelled by user");
                    Enabled = true;
                    return;
                }
                savedir = Path.GetDirectoryName(sfd.FileName);
                Tools.CurrentFolder = savedir;
                Enabled = false;
                Log("Extracting " + xent.Name);
                if (!xent.ExtractToFile(sfd.FileName))
                {
                    Log("Error extracting " + xent.Name);
                    Enabled = true;
                    return;
                }
                
                if (Path.GetExtension(sfd.FileName) == ".mogg")
                {
                    Tools.WriteOutData(Tools.ObfM(File.ReadAllBytes(sfd.FileName)), sfd.FileName);
                }
                Log("File " + xent.Name + " extracted successfully");
            }
            Enabled = true;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            extractFileToolStripMenuItem.Text = "Extract selected file";

            if (fileList.SelectedIndices.Count == 0)
            {
                extractFileToolStripMenuItem.Enabled = false;
                replaceFileToolStripMenuItem.Enabled = false;
                return;
            }

            extractFileToolStripMenuItem.Enabled = true;
            replaceFileToolStripMenuItem.Enabled = true;

            if (fileList.SelectedIndices.Count == 1)
            {
                replaceFileToolStripMenuItem.Visible = true;
            }
            else if (fileList.SelectedIndices.Count > 1)
            {
                extractFileToolStripMenuItem.Text = "Extract selected files";
                replaceFileToolStripMenuItem.Visible = false;
            }
        }

        private void EnableDisable(bool enabled)
        {
            txtTitle.Enabled = enabled;
            picPackage.Enabled = enabled;
            picContent.Enabled = enabled;
            txtDescription.Enabled = enabled;
            btnExtract.Enabled = enabled;
            btnSave.Enabled = enabled;
            chkAutoExtract.Enabled = enabled;
            btnVisualize.Enabled = enabled;
            cboGameID.Enabled = enabled;
            contextMenuStrip1.Enabled = enabled;
            contextMenuStrip2.Enabled = enabled;
            label4.Visible = enabled;
            radioCON.Visible = enabled;
            radioLIVE.Visible = enabled;
            folderTree.Enabled = enabled;
            fileList.Enabled = enabled;
            picWorking.Visible = !enabled;
        }

        private void replaceFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedIndices.Count == 0)
            {
                return;
            }
            var xent = (FileEntry)fileList.SelectedItems[0].Tag;
            var extension = Path.GetExtension(fileList.SelectedItems[0].Text);
            if (extension != null)
            {
                var filterFiles = extension.Substring(1).ToUpper() + " File|*." +
                                     extension.Substring(1);
                var file = VariousFunctions.GetUserFileLocale("Open a File", filterFiles, true);
                if (file == null)
                {
                    return;
                }
                Log("Replacing file " + xent.Name);
                var obf_file = Path.GetTempPath() + "m";
                if (Path.GetExtension(file) == ".mogg")
                {
                    Tools.DeleteFile(obf_file);
                    File.Copy(file, obf_file);
                    Tools.WriteOutData(Tools.DeObfM(File.ReadAllBytes(obf_file)), obf_file);
                }
                else
                {
                    obf_file = file;
                }
                if (!xent.Replace(obf_file))
                {
                    Log("Error replacing file " + xent.Name);
                    return;
                }
                Tools.CurrentFolder = Path.GetDirectoryName(file);
            }
            Log("Replaced file " + xent.Name + " successfully");
            ShowChanges(true);
        }

        private void ShowChanges(bool show)
        {
            Changes = show;
            Text = Text.Replace("*", "").Trim();
            if (show)
            {
                Text = "* " + Text + " *";
            }
        }
        
        private void AddFiles(IEnumerable<string> files)
        {
            var existing = false;
            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    return;
                }
                for (var i = 0; i < fileList.Items.Count; i++)
                {
                    if (!String.Equals(fileList.Items[i].Text, Path.GetFileName(file), StringComparison.InvariantCultureIgnoreCase)) continue;
                    MessageBox.Show("There is already a file with the name " + Path.GetFileName(file) +
                        "!\nIf you want to overwrite that file, right click on it and choose\n'Replace selected file'",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    existing = true;
                }

                if (existing) continue;
                if (Path.GetExtension(file) == ".mogg")
                {
                    Tools.WriteOutData(Tools.DeObfM(File.ReadAllBytes(file)), file);
                }
                var x = new DJsIO(file, DJFileMode.Open, true);
                if (!x.Accessed)
                {
                    return;
                }
                xPackage.MakeFile(x.FileNameShort, x, ((FolderEntry)folderTree.SelectedNode.DataKey).EntryID, AddType.NoOverWrite);
                x.Dispose();
                xReturn_NodeClick(folderTree.SelectedNode, null);
                Log("Added file " + x.FileNameShort + " successfully");
                ShowChanges(true);
            }
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (picWorking.Visible) return;
            if (internalDrop)
            {
                internalDrop = false;
                return;
            }

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Tools.CurrentFolder = Path.GetDirectoryName(files[0]);
            
            if (fileList.SelectedIndices.Count == 0)
            {
                AddFiles(files);
                return;
            }

            if (Path.GetExtension(fileList.SelectedItems[0].Text) == Path.GetExtension(files[0]))
            {
                if (MessageBox.Show("Replace '" + fileList.SelectedItems[0].Text + "' with '" + Path.GetFileName(files[0]) + "'?",
                    "Replace file?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                Log("Replacing file " + fileList.SelectedItems[0].Text);
                var xent = (FileEntry)fileList.SelectedItems[0].Tag;

                var obf_file = Path.GetTempPath() + "m";
                if (Path.GetExtension(files[0]) == ".mogg")
                {
                    Tools.DeleteFile(obf_file);
                    File.Copy(files[0], obf_file);
                    Tools.WriteOutData(Tools.DeObfM(File.ReadAllBytes(obf_file)), obf_file);
                }
                else
                {
                    obf_file = files[0];
                }
                if (!xent.Replace(obf_file))
                {
                    Log("Error replacing file " + fileList.SelectedItems[0].Text);
                    return;
                }
                Log("Replaced file " + fileList.SelectedItems[0].Text + " successfully");
                ShowChanges(true);
            }
            else
            {
                MessageBox.Show("Can't replace a '" + Path.GetExtension(fileList.SelectedItems[0].Text) + "' file with a '" + Path.GetExtension(files[0]) + "' file!", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void textBoxX2_TextChanged(object sender, EventArgs e)
        {
            xPackage.Header.Title_Display = txtTitle.Text;
            ShowChanges(true);
        }

        private void checkDescription(bool enforce = false)
        {
            if (txtDescription.Text.ToLowerInvariant() == "c3")
            {
                txtDescription.Text = C3Text;
            }
            if (txtDescription.Text.ToLowerInvariant() == "hmx")
            {
                txtDescription.Text = HmxText;
            }
            if (!enforce) return;
            if (txtDescription.Text.ToLowerInvariant().Replace(" ", "") != C3Text.ToLowerInvariant().Replace(" ", "") &&
                txtDescription.Text.ToLowerInvariant().Replace(" ", "") != HmxText.ToLowerInvariant().Replace(" ", ""))
                return;
            txtDescription.Enabled = false;
            toolTip1.SetToolTip(txtDescription, "This information can't be altered.");
        }

        private void textBoxX3_TextChanged(object sender, EventArgs e)
        {
            checkDescription();
            xPackage.Header.Description = txtDescription.Text;
            ShowChanges(true);
        }

        private void getImage(String file, PictureBox box)
        {
            if (picWorking.Visible) return;

            try
            {
                string contentImage;

                //if not passed a string path for the image
                //show dialog box to find one
                if (string.IsNullOrWhiteSpace(file))
                {
                    var openFileDialog1 = new OpenFileDialog
                    {
                        Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png",
                        Title = "Select a Package image",
                        InitialDirectory = Application.StartupPath + "\\res\\thumbs"
                    };
                    if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
                    contentImage = openFileDialog1.FileName;
                    Tools.CurrentFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                }
                else
                {
                    //if file is not blank, then use that for the image

                    if ((file.Contains(".jpg") || file.Contains(".bmp") ||
                        file.Contains(".png") || file.Contains(".jpeg")) && !file.Contains(".png_xbox") && !file.Contains(".png_wii"))
                    {
                        contentImage = file;
                    }
                    else
                    {
                        Log("That's not a valid image file");
                        return;
                    }
                }

                if (string.IsNullOrWhiteSpace(contentImage)) return;

                var thumbnail = Tools.NemoLoadImage(contentImage);
                if (thumbnail.Width == 64 && thumbnail.Height == 64)
                {
                    box.Image = thumbnail;
                    Log(box.Name.Replace("pic", "") + " Image changed successfully");
                    xPackage.Header.ContentImageBinary = picContent.Image.ImageToBytes(ImageFormat.Png);
                    xPackage.Header.PackageImageBinary = picPackage.Image.ImageToBytes(ImageFormat.Png);
                    ShowChanges(true);
                    return;
                }
                
                var newimage = Path.GetTempPath() + Path.GetFileNameWithoutExtension(contentImage) + ".png";
                Tools.ResizeImage(contentImage, 64, "png", newimage);

                if (File.Exists(newimage))
                {
                    box.Image = Tools.NemoLoadImage(newimage);
                    xPackage.Header.ContentImageBinary = picContent.Image.ImageToBytes(ImageFormat.Png);
                    xPackage.Header.PackageImageBinary = picPackage.Image.ImageToBytes(ImageFormat.Png);
                }
                Tools.DeleteFile(newimage);
                Log(box.Name.Replace("pic", "") + " Image changed successfully");
                ShowChanges(true);
            }
            catch
            {
                Log("Error loading image ... try again.");
            }
        }
        
        private void advTree1_Click(object sender, EventArgs e)
        {
            if (folderTree.SelectedNode != null)
            {
                folderTree.SelectedNode.Expand();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var success = true;
            xPackage.Header.ThisType = radioCON.Checked ? PackageType.SavedGame : PackageType.MarketPlace;
            xPackage.Header.MakeAnonymous();

            try
            {
                Log("Rebuilding CON file ... this might take a little while");
                var signature = radioCON.Checked ? new RSAParams(Application.StartupPath + "\\bin\\KV.bin") : new RSAParams(StrongSigned.LIVE);
                xPackage.RebuildPackage(signature);
                xPackage.FlushPackage(signature);
                xPackage.CloseIO();
            }
            catch
            {
                Log("Something went wrong with trying to rebuild CON file");
                success = false;
            }

            if (success)
            {
                Log("Trying to unlock CON file");
                if (Tools.UnlockCON(folder))
                {
                    Log("Unlocked CON file successfully");
                }
                else
                {
                    Log("Error unlocking CON file");
                    success = false;
                }
            }

            if (success)
            {
                //convert to CON if button checked, if not, leave as LIVE
                if (!radioCON.Checked) return;
                Log("Trying to sign CON file");
                if (Tools.SignCON(folder))
                {
                    Log("CON file signed successfully");
                }
                else
                {
                    Log("Error signing CON file");
                    Log("If you just extracted CON file, this is a known bug");
                    Log("Close this form, open the song again, and try again.");
                    FailedSign = true;
                }
            }
            else
            {
                FailedSign = true;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!FailedSign)
            {
                Log("Successfully rebuilt and resigned CON file");
                ShowChanges(false);
            }
            else
            {
                Log("Process failed ... please close this window down and try again");
            }
            picWorking.Visible = false;
            
            if (FailedSign) return;
            var xExplorer = new CONExplorer(btnSave.BackColor, btnSave.ForeColor);
            xExplorer.lstLog.Items.AddRange(lstLog.Items);
            xExplorer.LoadCON(folder);
            Close();
            xExplorer.Show();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (xPackage.ExtractPayload(outFolder, true, true))
                {
                    var moggs = Directory.GetFiles(outFolder, "*.mogg", SearchOption.AllDirectories);
                    foreach (var mogg in moggs)
                    {
                        Tools.WriteOutData(Tools.ObfM(File.ReadAllBytes(mogg)), mogg);
                    }
                    Log("CON file extracted successfully to:");
                    Log(outFolder);
                }
                else
                {
                    Log("Error extracting CON file!");
                }
            }
            catch
            {
                Log("Error extracting CON file!");
            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EnableDisable(true);
            checkDescription(true);
            Log("Ready");
        }

        private void btnVisualize_Click(object sender, EventArgs e)
        {
            if (Text.Contains("*"))
            {
                MessageBox.Show("You have unsaved changes, save first and then try again", Text, MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                return;
            }
            if (txtTitle.Text.Contains("Rock Band 3 Song Cache"))
            {
                MessageBox.Show("Can't Visualize that file", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            xPackage.CloseIO();
            var xVisualizer = new Visualizer(Color.FromArgb(230, 215, 0), Color.White, folder)
            {
                isRunningShortcut = isRunningShortcut
            };
            xVisualizer.Show();
            if (isRunningShortcut)
            {
                Hide();
            }
            else
            {
                Dispose();
            }
        }
        
        private void SongExplorer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (picWorking.Visible)
            {
                MessageBox.Show("Please wait until the current process finishes", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
                return;
            }
            if (Changes)
            {
                if (MessageBox.Show("You have unsaved changes\nIf you exit now nothing will be changed in the file\nAre you sure you want to exit?",
                        Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            xPackage.CloseIO();
            Tools.DeleteFolder(Application.StartupPath + "\\packex\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\temp\\", true);
        }

        private void buttonX8_Click_1(object sender, EventArgs e)
        {
            if (chkAutoExtract.Checked)
            {
                outFolder = folder + "_extracted\\";
            }
            else
            {
                folderBrowserDialog1.Description = "Select folder to extract CON file to";
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    outFolder = folderBrowserDialog1.SelectedPath + "\\" + Path.GetFileNameWithoutExtension(folder) + "_extracted\\";
                    Tools.CurrentFolder = outFolder;
                }
                else
                {
                    return;
                }
            }
            Log("Extracting CON file");
            EnableDisable(false);
            backgroundWorker2.RunWorkerAsync();
        }

        private void buttonX9_Click(object sender, EventArgs e)
        {
            EnableDisable(false);
            backgroundWorker1.RunWorkerAsync();
        }
        
        private void cboGameID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (starting || cboGameID.SelectedIndex == -1) return;
            switch (cboGameID.SelectedIndex)
            {
                case 0:
                    xPackage.Header.TitleID = 0x45410829;
                    picContent.Image = Resources.RB1;
                    break;
                case 1:
                    xPackage.Header.TitleID = 0x45410869;
                    picContent.Image = Resources.RB2;
                    break;
                case 2:
                    xPackage.Header.TitleID = 0x45410914;
                    picContent.Image = Resources.RB3;
                    break;
            }
            xPackage.Header.Title_Package = "Rock Band " + (cboGameID.SelectedIndex + 1);  
            Log("Game changed to " + xPackage.Header.Title_Package);
            xPackage.Header.ContentImageBinary = picContent.Image.ImageToBytes(ImageFormat.Png);
            ShowChanges(true);
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (fileList.SelectedItems.Count == 0)
            {
                return;
            }
            
            var files = new List<string>();
            var ext_folder = Application.StartupPath + "\\temp\\";
            if (!Directory.Exists(ext_folder))
            {
                Directory.CreateDirectory(ext_folder);
            }

            for (var i = 0; i < fileList.SelectedItems.Count; i++)
            {
                var xent = (FileEntry)fileList.SelectedItems[i].Tag;
                Enabled = false;
                Log("Extracting " + xent.Name);
                var ext_file = ext_folder + xent.Name;
                Tools.DeleteFile(ext_file);
                if (!xent.ExtractToFile(ext_file))
                {
                    Log("Error extracting " + xent.Name);
                    Enabled = true;
                    return;
                }
                files.Add(ext_file);
                if (Path.GetExtension(ext_file) == ".mogg")
                {
                    Tools.WriteOutData(Tools.ObfM(File.ReadAllBytes(ext_file)), ext_file);
                }
            }
            Enabled = true;

            try
            {
                internalDrop = true;
                DoDragDrop(new DataObject(DataFormats.FileDrop, files.ToArray()), DragDropEffects.Copy);
                
                foreach (var file in files)
                {
                    if (internalDrop)
                    {
                        Log("File " + Path.GetFileName(file) + " extracted successfully");
                    }
                    else
                    {
                        Log("Cancelled extracting file " + Path.GetFileName(file));
                    }
                    //try to clean any leftover temp files
                    Tools.DeleteFile(file);
                }
            }
            catch (Exception ex)
            {
                Log("There was an error extracting the selected file(s)!");
                Log("The error says:");
                Log(ex.Message);
            }

            internalDrop = false;
        }

        private void addNewFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Title = "Select file(s) to add",
                Multiselect = true,
                InitialDirectory = Path.GetDirectoryName(xPackage.FileNameLong)
            };

            if (ofd.ShowDialog() != DialogResult.Cancel)
            {
                AddFiles(ofd.FileNames);
            }
        }

        private void extractImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pic = "Package Image";
            var picturebox = picPackage;

            var x = tabControl1.PointToClient(new Point(MousePosition.X, MousePosition.Y)).X;
            var y = tabControl1.PointToClient(new Point(MousePosition.X, MousePosition.Y)).Y;

            if (y > picContent.Location.Y && y < picContent.Location.Y + picContent.Height)
            {
                if (x > picContent.Location.X & x < picContent.Location.X + picContent.Width)
                {
                    pic = "Content Image";
                    picturebox = picContent;
                }
            }
            
            var sfd = new SaveFileDialog
                {
                    FileName = Text.Replace(" ","") + "_" + pic.Replace(" ",""),
                    Filter = "PNG File (*.png)|*.png",
                    InitialDirectory = Tools.CurrentFolder,
                    Title = "Choose where to save the " + pic + " to"
                };

            sfd.ShowDialog();

            if (sfd.FileName != "")
            {
                var png = picturebox.Image;
                png.Save(sfd.FileName,ImageFormat.Png);
            }
            
            Tools.CurrentFolder = Path.GetDirectoryName(sfd.FileName);
            

            if (File.Exists(sfd.FileName))
            {
                Log(pic + " extracted successfully");
            }
            else
            {
                Log("Looks like extracting " + pic + " failed. Try again");
            }
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var pic = "";
            var x = tabControl1.PointToClient(new Point(MousePosition.X, MousePosition.Y)).X;
            var y = tabControl1.PointToClient(new Point(MousePosition.X, MousePosition.Y)).Y;

            if (y > picPackage.Location.Y && y < picPackage.Location.Y + picPackage.Height)
            {
                if (x > picPackage.Location.X && x < picPackage.Location.X + picPackage.Width)
                {
                    pic = "Package Image";
                }
            }

            if (y > picContent.Location.Y && y < picContent.Location.Y + picContent.Height)
            {
                if (x > picContent.Location.X & x < picContent.Location.X + picContent.Width)
                {
                    pic = "Content Image";
                }
            }
            
            extractImageToolStripMenuItem.Text = "Extract " + pic;
        }

        private void textBoxX2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.A)
                txtTitle.SelectAll();
        }

        private void textBoxX3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.A)
                txtDescription.SelectAll();
        }

        private void textBoxX2_DoubleClick(object sender, EventArgs e)
        {
            txtTitle.SelectAll();
        }

        private void textBoxX3_DoubleClick(object sender, EventArgs e)
        {
            txtDescription.SelectAll();
        }

        private void exportLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.ExportLog(Text, lstLog.Items);
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            ShowChanges(true);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ShowChanges(true);
        }

        private void PackageImage_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            getImage(files[0], picPackage);
        }

        private void ContentImage_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            getImage(files[0], picContent);
        }

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }

        private void ContentImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                getImage("", picContent);
            }
        }

        private void PackageImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                getImage("", picPackage);
            }
        }
        
        private void CONExplorer_Shown(object sender, EventArgs e)
        {
            var xfile = xPackage.GetFile("songs/songs.dta") ?? xPackage.GetFile("songs_upgrades/upgrades.dta");
            if (xfile == null) return;

            var isUpgrade = false;
            string internalname;
            
            if (!Parser.ExtractDTA(xPackage, false))
            {
                isUpgrade = Parser.ExtractDTA(xPackage, false, true);
                if (!isUpgrade) return;
            }
            if (!Parser.ReadDTA(Parser.DTA) || !Parser.Songs.Any())
            {
                return;
            }

            activeUPG = xPackage.GetFile("songs_upgrades/upgrades.dta");
            activeDTA = xPackage.GetFile("songs/songs.dta");
            if (!isUpgrade)
            {
                internalname = Parser.Songs[0].InternalName;
                if (!string.IsNullOrWhiteSpace(internalname) && Parser.Songs.Count == 1)
                {
                    activeMIDI = xPackage.GetFile("songs/" + internalname + "/" + internalname + ".mid");
                    activeMOGG = xPackage.GetFile("songs/" + internalname + "/" + internalname + ".mogg");
                    activePNG = xPackage.GetFile("songs/" + internalname + "/gen/" + internalname + "_keep.png_xbox");
                }
            }
            else
            {
                internalname = Parser.Songs[0].MIDIFile;
                activeMIDI = xPackage.GetFile(internalname);
            }

            btnDTA.Enabled = activeDTA != null || activeUPG != null;
            btnMIDI.Enabled = activeMIDI != null && Parser.Songs.Count < 2;
            btnMOGG.Enabled = activeMOGG != null && Parser.Songs.Count < 2;
            btnPNG.Enabled = activePNG != null && Parser.Songs.Count < 2;

            btnDTA.BackColor = btnDTA.Enabled ? Color.LightYellow : Color.LightGray;
            btnMIDI.BackColor = btnMIDI.Enabled ? Color.LightYellow : Color.LightGray;
            btnMOGG.BackColor = btnMOGG.Enabled ? Color.LightYellow : Color.LightGray;
            btnPNG.BackColor = btnPNG.Enabled ? Color.LightYellow : Color.LightGray;

            btnDTA.Text = activeDTA != null && activeUPG != null ? "DTAs" : "DTA";
            
            if (Parser.Songs.Count > 1)
            {
                lblSongID.Text = "N/A - PACK";
                btnVisualize.Visible = false;
                btnChange.Visible = false;
                return;
            }
            if (Parser.Songs[0].SongId == 0 || string.IsNullOrWhiteSpace(Parser.Songs[0].SongIdString))
            {
                lblSongID.Text = Parser.Songs[0].HasSongIDError ? "SONG ID MISSING" : "NOT PRESENT IN DTA";
                btnVisualize.Visible = Parser.Songs[0].HasSongIDError && !isUpgrade;
                btnChange.Visible = !isUpgrade;
                return;
            }
            lblSongID.Text = Parser.Songs[0].SongIdString;
            lblSongID.Cursor = Cursors.Hand;
            lblIDTop.Cursor = lblSongID.Cursor;
            toolTip1.SetToolTip(lblSongID, "Click to copy song ID to clipboard");
            toolTip1.SetToolTip(lblIDTop, "Click to copy song ID to clipboard");
            btnVisualize.Visible = btnVisualize.Visible && !isUpgrade && !txtTitle.Text.Contains("Rock Band 3 Song Cache");
            btnChange.Visible = !isUpgrade;
        }

        private void lblIDTop_MouseClick(object sender, MouseEventArgs e)
        {
            var fails = new List<string> {"SONG ID MISSING", "NOT PRESENT IN DTA", "N/A", "N/A - PACK"};
            if (e.Button != MouseButtons.Left || fails.Contains(lblSongID.Text) || string.IsNullOrWhiteSpace(lblSongID.Text)) return;
            Clipboard.SetText(lblSongID.Text);
            MessageBox.Show("Song ID was copied to your clipboard", "CON Explorer", MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void ExtractFile(FileEntry xent, FileEntry xent2 = null)
        {
            var ext_folder = Application.StartupPath + "\\temp\\";
            if (!Directory.Exists(ext_folder))
            {
                Directory.CreateDirectory(ext_folder);
            }
            
            Enabled = false;
            Log("Extracting " + xent.Name);
            var ext_file = new StringCollection {ext_folder + xent.Name};
            Tools.DeleteFile(ext_file[0]);
            if (!xent.ExtractToFile(ext_file[0]))
            {
                Log("Error extracting " + xent.Name);
                Enabled = true;
                return;
            }
            if (xent2 != null)
            {
                Log("Extracting " + xent2.Name);
                ext_file.Add(ext_folder + xent2.Name);
                Tools.DeleteFile(ext_file[1]);
                if (!xent2.ExtractToFile(ext_file[1]))
                {
                    Log("Error extracting " + xent2.Name);
                    ext_file.RemoveAt(1);
                }
            }
            
            if (Path.GetExtension(ext_file[0]) == ".mogg")
            {
                Tools.WriteOutData(Tools.ObfM(File.ReadAllBytes(ext_file[0])), ext_file[0]);
            }
            Enabled = true;

            if (File.Exists(ext_file[0]))
            {
                var moveEffect = new byte[] { 2, 0, 0, 0 };
                var dropEffect = new MemoryStream();
                dropEffect.Write(moveEffect, 0, moveEffect.Length);

                var data = new DataObject();
                data.SetFileDropList(ext_file);
                data.SetData("Preferred DropEffect", dropEffect);

                Clipboard.Clear();
                Clipboard.SetDataObject(data, true);
                
                Log("File " + Path.GetFileName(ext_file[0]) + " extracted successfully");
                if (ext_file.Count > 1)
                {
                    Log("File " + Path.GetFileName(ext_file[1]) + " extracted successfully");
                }
                Log(ext_file.Count > 1 ? "Files are in your clipboard, paste them anywhere" : "File is in your clipboard, paste it anywhere");
            }
            else
            {
                Log("Could not extract file " + Path.GetFileName(ext_file[0]));
            }
        }

        private void ReplaceFile(FileEntry xent, string file)
        {
            if (Path.GetExtension(xent.Name) == Path.GetExtension(file))
            {
                if (MessageBox.Show("Replace '" + xent.Name + "' with '" + Path.GetFileName(file) + "'?",
                    "Replace file?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                Log("Replacing file " + xent.Name);
                    
                var obf_file = Path.GetTempPath() + "m";
                if (Path.GetExtension(file) == ".mogg")
                {
                    Tools.DeleteFile(obf_file);
                    File.Copy(file, obf_file);
                    Tools.WriteOutData(Tools.DeObfM(File.ReadAllBytes(obf_file)), obf_file);
                }
                else
                {
                    obf_file = file;
                }
                if (!xent.Replace(obf_file))
                {
                    Log("Error replacing file " + xent.Name);
                    return;
                }
                Log("Replaced file " + xent.Name + " successfully");
                ShowChanges(true);
            }
            else
            {
                MessageBox.Show("Can't replace a '" + Path.GetExtension(xent.Name) + "' file with a '" + Path.GetExtension(file) + "' file", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnDTA_Click(object sender, EventArgs e)
        {
            if (activeDTA != null && activeUPG != null)
            {
                ExtractFile(activeDTA, activeUPG);
            }
            else 
            {
                if (activeDTA != null)
                {
                    ExtractFile(activeDTA);
                }
                if (activeUPG != null)
                {
                    ExtractFile(activeUPG);
                }
            }
        }

        private void btnMIDI_Click(object sender, EventArgs e)
        {
            ExtractFile(activeMIDI);
        }

        private void btnPNG_Click(object sender, EventArgs e)
        {
            ExtractFile(activePNG);
        }

        private void btnMOGG_Click(object sender, EventArgs e)
        {
            ExtractFile(activeMOGG);
        }

        private void btnDTA_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            switch (Path.GetFileName(files[0]).ToLowerInvariant())
            {
                case "songs.dta":
                    ReplaceFile(activeDTA, files[0]);
                    break;
                case "upgrades.dta":
                    ReplaceFile(activeUPG, files[0]);
                    break;
                default:
                    MessageBox.Show("Only songs.dta or upgrades.dta files can be replaced this way", Text,
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
            }
        }

        private void btnMIDI_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ReplaceFile(activeMIDI, files[0]);
        }

        private void btnPNG_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ReplaceFile(activePNG, files[0]);
        }

        private void btnMOGG_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ReplaceFile(activeMOGG, files[0]);
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            var fails = new List<string> { "SONG ID MISSING", "NOT PRESENT IN DTA", "N/A", "N/A - PACK" };
            var popup = new PasswordUnlocker(fails.Contains(lblSongID.Text) ? "" : lblSongID.Text);
            popup.IDChanger();
            popup.ShowDialog();
            var newID = popup.EnteredText;
            popup.Dispose();
            if (string.IsNullOrWhiteSpace(newID) || newID.Trim() == lblSongID.Text) return;
            var dta = Path.GetTempPath() + "dta";
            Tools.DeleteFile(dta);
            if (!activeDTA.ExtractToFile(dta))
            {
                Log("Error extracting DTA file...failed to change song ID!");
                return;
            }
            Tools.ReplaceSongID(dta, newID);
            if (!activeDTA.Replace(dta))
            {
                Tools.DeleteFile(dta);
                Log("Error replacing DTA file...failed to change song ID!");
                return;
            }
            Tools.DeleteFile(dta);
            lblSongID.Text = newID;
            Log("Changed song ID to '" + newID + "' successfully");
            ShowChanges(true);
        }

        private sealed class DarkRenderer : ToolStripProfessionalRenderer
        {
            public DarkRenderer() : base(new DarkColors()) { }
        }

        private sealed class DarkColors : ProfessionalColorTable
        {
            public override Color ImageMarginGradientBegin
            {
                get { return mMenuBackground; }
            }
            public override Color ImageMarginGradientEnd
            {
                get { return mMenuBackground; }
            }
            public override Color ImageMarginGradientMiddle
            {
                get { return mMenuBackground; }
            }
            public override Color ToolStripDropDownBackground
            {
                get { return mMenuBackground; }
            }
        }

        private void picPin_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            switch (picPin.Tag.ToString())
            {
                case "pinned":
                    picPin.Image = Resources.unpinned;
                    picPin.Tag = "unpinned";
                    break;
                case "unpinned":
                    picPin.Image = Resources.pinned;
                    picPin.Tag = "pinned";
                    break;
            }
            TopMost = picPin.Tag.ToString() == "pinned";
        }
    }
}