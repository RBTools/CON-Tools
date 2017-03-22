using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using C3Tools.Properties;
using C3Tools.x360;
using DevComponents.AdvTree;
using DevComponents.DotNetBar;
using System.Drawing;

namespace C3Tools
{
    public partial class CONCreator : Office2007Form
    {
        private static string sFolder;
        private readonly CreateSTFS xsession;
        private const string C3Text = "Brought to you by C3. For more great customs like this one, visit www.customscreators.com";
        private const string HmxText = "Please support the developers. Buy The Beatles: Rock Band and its DLC. Enjoy.";
        private string xOut;
        private RSAParams signature;
        private bool dupFolder;
        private readonly NemoTools Tools;
        private string Description;
        private string Title;
        private int GameIndex;
        private readonly List<string> moggs;
 
        public CONCreator(Color ButtonBackColor, Color ButtonTextColor)
        {
            InitializeComponent();
            
            Tools = new NemoTools();
            var y = new List<PackageType>();

            picContent.AllowDrop = true;
            picPackage.AllowDrop = true;

            btnCreate.BackColor = ButtonBackColor;
            btnCreate.ForeColor = ButtonTextColor;
            btnCreate.FlatAppearance.MouseOverBackColor = btnCreate.BackColor == Color.Transparent ? Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B) : Tools.LightenColor(btnCreate.BackColor);
            
            moggs = new List<string>();
            xsession = new CreateSTFS();
            var x = (PackageType[])Enum.GetValues(typeof(PackageType));
            y.AddRange(x);
            node1.DataKey = (ushort)0xFFFF;
            node1.NodeClick += xReturn_NodeClick;
            folderTree.SelectedIndex = 0;    
            
            cboGameID.SelectedIndex = 2;
            toolTip1.SetToolTip(picContent, "Click here to select the Content Image (visible in here)");
            toolTip1.SetToolTip(picPackage, "Click here to select the Package Image (visible in the Xbox dashboard)");
            toolTip1.SetToolTip(btnCreate, "Click here to create the song package");
            toolTip1.SetToolTip(radioCON, "Click here for use with retail consoles");
            toolTip1.SetToolTip(radioLIVE, "Click here for use with modded consoles");
            toolTip1.SetToolTip(txtDisplay, "Enter a title for your pack (visible in the Xbox dashboard)");
            toolTip1.SetToolTip(txtDescription, "Enter a description for your pack (visible in here)");
            toolTip1.SetToolTip(folderTree, "Add folders here");
            toolTip1.SetToolTip(fileList, "Add files here");
            toolTip1.SetToolTip(groupBox1, "Choose the format for your pack - default is CON");
            AddFolder("songs");
        }

        private void AddFolder(string folder)
        {
            string xPath;
            if (folderTree.SelectedNode != folderTree.Nodes[0])
            {
                xPath = ((CFolderEntry)folderTree.SelectedNode.Tag).Path + "/" + folder;
            }
            else
            {
                xPath = folder;
            }

            var folderexists = xsession.GetFolder(xPath);
            if (folder != "gen" && folderexists != null)
            {
                MessageBox.Show(
                    "There is already a folder with the name '" + folder +
                    "'\nYou can't have multiple folders with the same name,\ntry deleting the existing folder first",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                dupFolder = true;
                return;
            }

            if (!xsession.AddFolder(xPath))
            {
                return;
            }
            folderTree.SelectedNode.Nodes.Add(GetFoldNode(xsession.GetFolder(xPath)));
            folderTree.SelectedNode.ExpandAll();
            folderTree.SelectedNode = folderTree.FindNodeByText(folder);
        }
        
        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog {Multiselect = true, Title = "Choose files to add"};
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            foreach (var file in ofd.FileNames)
            {
                GetFiles(file);
                Tools.CurrentFolder = Path.GetDirectoryName(file);
            }
        }

        private void GetFiles(string file)
        {
            if (!File.Exists(file))
            {
                return;
            }
            
            string xPath;
            if (folderTree.SelectedNode != folderTree.Nodes[0])
            {
                xPath = ((CFolderEntry) folderTree.SelectedNode.Tag).Path + "/" + Path.GetFileName(file);
            }
            else
            {
                xPath = Path.GetFileName(file);
            }

            var fileexists = xsession.GetFile(xPath);
            if (fileexists == null)
            {
                if (Path.GetExtension(file) == ".mogg")
                {
                    Tools.WriteOutData(Tools.DeObfM(File.ReadAllBytes(file)), file);
                    moggs.Add(file);
                }

                if (xsession.AddFile(file, xPath))
                {
                    GetSelFiles((CFolderEntry) folderTree.SelectedNode.Tag);
                    btnCreate.Enabled = true;
                    return;
                }

                var ent = xsession.GetFile(xPath);
                var xitem = new ListViewItem(ent.Name) {Tag = ent};
                fileList.Items.Add(xitem);
                GetSelFiles((CFolderEntry) folderTree.SelectedNode.Tag);
            }
            else
            {
                MessageBox.Show("File with name '" + Path.GetFileName(file) +
                    "' already exists\nTry deleting the existing file first", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void GetSelFiles(CFolderEntry folder)
        {
            folderTree.Enabled = fileList.Enabled = false;
            var x = folder.GetFiles();
            fileList.Items.Clear();
            fileList.Items.AddRange(x.Select(y => new ListViewItem(y.Name) {Tag = y}).ToArray());
            folderTree.Enabled = fileList.Enabled = true;
        }

        private Node GetFoldNode(CItemEntry x)
        {
            var xReturn = new Node {Text = x.Name, Tag = x, ContextMenu = contextMenuStrip3};
            xReturn.NodeClick += xReturn_NodeClick;
            return xReturn;
        }

        private void xReturn_NodeClick(object sender, EventArgs e)
        {
            var x = (Node) sender;
            if (folderTree.Nodes[0] != x)
            {
                GetSelFiles((CFolderEntry) x.Tag);
            }
            else
            {
                GetSelFiles(xsession.RootPath);
            }
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sUsed = new StringCollection();
            foreach (Node node in folderTree.SelectedNode.Nodes)
            {
                sUsed.Add(node.Text);
            }
            var newName = "";
            while (string.IsNullOrWhiteSpace(newName) || sUsed.Contains(newName))
            {
                var popup = new PasswordUnlocker();
                popup.Renamer(); //change settings for renaming
                popup.ShowDialog();
                newName = popup.EnteredText;
                popup.Dispose();
                if (string.IsNullOrWhiteSpace(newName)) return;
                if (sUsed.Contains(newName))
                {
                    MessageBox.Show("That folder name is already used, try a different name", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            AddFolder(newName);
            GetSelFiles((CFolderEntry)(folderTree.FindNodeByText(Path.GetFileName(newName))).Tag);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            addFileToolStripMenuItem.Enabled = folderTree.SelectedNode != null;
            deleteToolStripMenuItem.Enabled = fileList.SelectedIndices.Count > 0;
        }

        private void deleteFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this folder and its contents?", "Warning!",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes) return;
            var x = (CFolderEntry) folderTree.SelectedNode.Tag;
            var folderexists = xsession.GetFolder(x.Path);

            if (folderexists == null) return;
            if (!xsession.DeleteFolder(x.Path))
            {
                MessageBox.Show("didn't work");
            }
            folderTree.SelectedNode.Remove();
            folderTree.SelectedIndex = 0;

            GetSelFiles(x);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (fileList.SelectedIndices.Count == 0)
            {
                return;
            }

            for (var i = 0; i < fileList.SelectedItems.Count;i++ )
            {
                var x = (CFileEntry)fileList.SelectedItems[i].Tag;
                xsession.DeleteFile(x.Path);
                fileList.Items.Remove(fileList.SelectedItems[0]);
                deleteToolStripMenuItem_Click(sender,e);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(sFolder))
            {
                sFolder = Directory.GetCurrentDirectory();
            }
            var filename = txtDisplay.Text.Replace(" ", "").Replace(",", "").Replace("!", "").Replace("?", "").Replace("+", "").Replace(">", "")
                .Replace("<", "").Replace("\\", "").Replace("/", ""); 
            var fileOutput = new SaveFileDialog { FileName = filename, InitialDirectory = sFolder };
            if (fileOutput.ShowDialog() == DialogResult.OK)
            {
                xOut = fileOutput.FileName;
            }
            if (xOut == null) return;
            Tools.CurrentFolder = Path.GetDirectoryName(xOut);
            Description = txtDescription.Text;
            Title = txtDisplay.Text;
            GameIndex = cboGameID.SelectedIndex;
            backgroundWorker1.RunWorkerAsync();
        }
        
        private void GetImage(String file, PictureBox box)
        {
            if (picWorking.Visible) return;
            try
            {
                string contentImage = null;
                //if not passed a string path for the image
                //show dialog box to find one
                if (string.IsNullOrWhiteSpace(file))
                {
                    var openFileDialog1 = new OpenFileDialog
                        {
                            Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png",
                            Title = "Select an image",
                            InitialDirectory = Application.StartupPath + "\\res\\thumbs"
                        };
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        contentImage = openFileDialog1.FileName;
                        Tools.CurrentFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                    }
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
                        return;
                    }
                }
                if (string.IsNullOrWhiteSpace(contentImage)) return;
                var thumbnail = Tools.NemoLoadImage(contentImage);
                if (thumbnail.Width == 64 && thumbnail.Height == 64)
                {
                    box.Image = thumbnail;
                    return;
                }
                var newimage = Path.GetTempPath() + Path.GetFileNameWithoutExtension(contentImage) + ".png";
                Tools.ResizeImage(contentImage, 64, "png", newimage);
                if (File.Exists(newimage))
                {
                    box.Image = Tools.NemoLoadImage(newimage);
                }
                Tools.DeleteFile(newimage);
            }
            catch (Exception iSuck)
            {
                MessageBox.Show("Error: " + iSuck.Message);
            }
        }
        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            xsession.HeaderData.ContentImageBinary = picContent.Image.ImageToBytes(ImageFormat.Png);
            xsession.HeaderData.PackageImageBinary = picPackage.Image.ImageToBytes(ImageFormat.Png);
            switch (GameIndex)
            {
                case 0:
                    xsession.HeaderData.TitleID = 0x45410829;
                    break;
                case 1:
                    xsession.HeaderData.TitleID = 0x45410869;
                    break;
                case 2:
                    xsession.HeaderData.TitleID = 0x45410914;
                    break;
            }
            xsession.HeaderData.Publisher = "";
            xsession.HeaderData.Title_Package = "Rock Band " + (GameIndex + 1);
            xsession.HeaderData.SetLanguage(Languages.English);
            xsession.HeaderData.Title_Display = Title;
            xsession.HeaderData.Description = Description;
            xsession.STFSType = STFSType.Type0;
            xsession.HeaderData.MakeAnonymous();
            xsession.HeaderData.ThisType = radioCON.Checked ? PackageType.SavedGame : PackageType.MarketPlace;
            signature = radioCON.Checked ? new RSAParams(Application.StartupPath + "\\bin\\KV.bin") : new RSAParams(StrongSigned.LIVE);
            var xy = new STFSPackage(xsession, signature, xOut);
            xy.CloseIO();
            //now open and unlock
            if (!Tools.UnlockCON(xOut))
            {
                MessageBox.Show("There was an error unlocking CON file\nCan't finish", Text, MessageBoxButtons.OK,MessageBoxIcon.Error);
                return;
            }
            //convert to CON if button checked, if not, leave as LIVE
            if (!radioCON.Checked) return;
            if (!Tools.SignCON(xOut))
            {
                MessageBox.Show("There was an error signing CON file\nCan't finish", Text, MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            picWorking.Visible = false;
            foreach (var mogg in moggs)
            {
                Tools.WriteOutData(Tools.ObfM(File.ReadAllBytes(mogg)), mogg);
            }
            var xExplorer = new CONExplorer(Color.FromArgb(34, 169, 31), Color.White);
            xExplorer.LoadCON(xOut);
            Close();
            xExplorer.Show();
        }
        
        private void cboGameID_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboGameID.SelectedIndex)
            {
                case 0:
                    xsession.HeaderData.TitleID = 0x45410829;
                    picContent.Image = Resources.RB1;
                    break;
                case 1:
                    xsession.HeaderData.TitleID = 0x45410869;
                    picContent.Image = Resources.RB2;
                    break;
                case 2:
                    xsession.HeaderData.TitleID = 0x45410914;
                    picContent.Image = Resources.RB3;
                    break;
            }
            xsession.HeaderData.ContentImageBinary = picContent.Image.ImageToBytes(ImageFormat.Png);
            xsession.HeaderData.PackageImageBinary = picPackage.Image.ImageToBytes(ImageFormat.Png);
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            switch (txtDescription.Text.ToLowerInvariant())
            {
                case "c3":
                    txtDescription.Text = C3Text;
                    break;
                case "the beatles: rock band":
                    txtDescription.Text = HmxText;
                    break;
            }
            txtDescription.ForeColor = txtDescription.Text == "Created with C3 CON Tools" ? Color.LightGray : Color.Black;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (picWorking.Visible) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Tools.CurrentFolder = Path.GetDirectoryName(files[0]);
            foreach (var file in files)
            {
                GetFiles(file);
            }
        }

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }

        private void advTree1_DragDrop(object sender, DragEventArgs e)
        {
            if (picWorking.Visible) return;
            var folder = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (!Directory.Exists(folder[0])) return;
            Tools.CurrentFolder = folder[0];
            var dta = Path.GetDirectoryName(folder[0]) + "\\songs.dta";
            if (File.Exists(dta))
            {
                GetFiles(dta);
            }
            AddFolder(Path.GetFileName(folder[0]));
            if (dupFolder)
            {
                dupFolder = false;
                return;
            }
            var files = Directory.GetFiles(folder[0],"*.*",SearchOption.TopDirectoryOnly);
            foreach (var file in files.Where(file => !Directory.Exists(file)))
            {
                GetFiles(file);
            }
            if (!Directory.Exists(folder[0] + "\\gen")) return;
            AddFolder("gen");
            var subfiles = Directory.GetFiles(folder[0] + "\\gen", "*.*", SearchOption.TopDirectoryOnly);
            foreach (var subfile in subfiles.Where(subfile => !Directory.Exists(subfile)))
            {
                GetFiles(subfile);
            }
            folderTree.SelectedNode = folderTree.FindNodeByText(Path.GetFileName(folder[0]));
            GetSelFiles((CFolderEntry)(folderTree.FindNodeByText(Path.GetFileName(folder[0]))).Tag);
        }

        private void txtDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.A)
                txtDisplay.SelectAll();
        }

        private void txtDisplay_DoubleClick(object sender, EventArgs e)
        {
            txtDisplay.SelectAll();
        }

        private void txtDescription_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control & e.KeyCode == Keys.A)
                txtDescription.SelectAll();
        }

        private void txtDescription_DoubleClick(object sender, EventArgs e)
        {
            txtDescription.SelectAll();
        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            GetImage(files[0], picContent);
        }

        private void pictureBox2_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            GetImage(files[0], picPackage);
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GetImage("", picPackage);
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GetImage("", picContent);
            }
        }

        private void CONCreator_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!picWorking.Visible) return;
            MessageBox.Show("Please wait until the current process finishes", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            e.Cancel = true;
        }

        private void txtDescription_MouseDown(object sender, MouseEventArgs e)
        {
            if (txtDescription.Text == "Created with C3 CON Tools")
            {
                txtDescription.Text = "";
            }
        }

        private void txtDescription_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                txtDescription.Text = "Created with C3 CON Tools";
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

    public static class BytesExt
    {
        public static string HexString(this byte[] x)
        {
            return x.Aggregate("", (current, y) => current + y.ToString("X2"));
        }
    }
}