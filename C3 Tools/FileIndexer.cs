using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using C3Tools.Properties;
using C3Tools.x360;
using Color = System.Drawing.Color;

namespace C3Tools
{
    public partial class FileIndexer : Form
    {
        private readonly NemoTools Tools;
        private readonly DTAParser Parser;
        private readonly string Index;
        private readonly string config;
        private readonly List<SongIndex> Songs;
        private List<SongIndex> FilteredSongs;
        private const int LIST_SPACER = 24;
        private const double LIST_SIZER1 = 0.505;
        private const double LIST_SIZER2 = 0.495;
        private bool DoMaximize;
        private bool DontUpdate;
        private int ActiveSortColumn;
        public SortOrder ListSorting = SortOrder.Ascending;
        private bool CancelWorkers;
        private const string TypeToSearch = "Type to search...";
        private bool SearchForIDConflicts;

        public FileIndexer(Color ButtonBackColor, Color ButtonTextColor)
        {
            InitializeComponent();
            Tools = new NemoTools();
            Parser = new DTAParser();
            Songs = new List<SongIndex>();
            FilteredSongs = new List<SongIndex>();
            config = Application.StartupPath + "\\bin\\config\\indexer.config";
            LoadConfig();
            var indexFolder = Application.StartupPath + "\\bin\\indexer\\";
            if (!Directory.Exists(indexFolder))
            {
                Directory.CreateDirectory(indexFolder);
            }
            Index = indexFolder + "index.c3";
            var formButtons = new List<Button> {btnBuild, btnClear, btnDelete, btnNew, btnClearSearch};
            foreach (var button in formButtons)
            {
                button.BackColor = ButtonBackColor;
                button.ForeColor = ButtonTextColor;
                button.FlatAppearance.MouseOverBackColor = button.BackColor == Color.Transparent ? Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B) : Tools.LightenColor(button.BackColor);
            }
        }

        private void SaveConfig()
        {
            var sw = new StreamWriter(config, false);
            sw.WriteLine("FolderCount=" + lstFolders.Items.Count);
            foreach (var item in lstFolders.Items)
            {
                sw.WriteLine(item.ToString());
            }
            sw.WriteLine("SearchSubDirs=" + chkSubDirs.Checked);
            sw.WriteLine("OpenMaximized=" + (WindowState == FormWindowState.Maximized));
            sw.Dispose();
        }

        private void LoadConfig()
        {
            if (!File.Exists(config)) return;
            var sr = new StreamReader(config);
            var line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                sr.Dispose();
                return;
            }
            var count = Convert.ToInt16(Regex.Match(line, @"\d+").Value);
            for (var i = 0; i < count; i++)
            {
                var folder = Tools.GetConfigString(sr.ReadLine());
                if (Directory.Exists(folder))
                {
                    lstFolders.Items.Add(folder);
                }
            }
            chkSubDirs.Checked = sr.ReadLine().Contains("True");
            DoMaximize = sr.ReadLine().Contains("True");
            sr.Dispose();
            CheckFolderCount();
        }

        private void CheckFolderCount()
        {
            var enabled = lstFolders.Items.Count > 0;
            btnClear.Enabled = enabled;
            btnBuild.Enabled = enabled;
            chkSubDirs.Enabled = enabled;
        }

        private void LoadIndex()
        {
            if (!File.Exists(Index)) return;
            Songs.Clear();
            var sr = new StreamReader(Index); 
            var line = sr.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                sr.Dispose();
                return;
            }
            if (line.Contains("NewFormat"))
            {
                var count = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                for (var i = 0; i < count; i++)
                {
                    var newSong = new SongIndex
                    {
                        Name = Tools.GetConfigString(sr.ReadLine()),
                        SongID = Tools.GetConfigString(sr.ReadLine()),
                        Location = Tools.GetConfigString(sr.ReadLine())
                    };
                    if (!File.Exists(newSong.Location)) continue;
                    Songs.Add(newSong);
                }
            }
            else
            {
                var count = Convert.ToInt16(Tools.GetConfigString(line));
                for (var i = 0; i < count; i++)
                {
                    var name = Tools.GetConfigString(sr.ReadLine());
                    var path = Tools.GetConfigString(sr.ReadLine());
                    if (!File.Exists(path)) continue;
                    Songs.Add(new SongIndex());
                    Songs[Songs.Count - 1].Name = name;
                    Songs[Songs.Count - 1].Location = path;
                }
            }
            sr.Dispose();
            DisplayIndexedFiles();
            SortSongs();
            EnableSearch(Songs.Any());
        }

        private void EnableSearch(bool enabled)
        {
            txtSearch.Enabled = enabled;
            radioPackages.Enabled = enabled;
            radioSongs.Enabled = enabled;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            lstFolders.Items.Clear();
            CheckFolderCount();
        }

        private void lstFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnDelete.Enabled = lstFolders.SelectedIndex > -1;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstFolders.SelectedIndex == -1) return;
            lstFolders.Items.RemoveAt(lstFolders.SelectedIndex);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FileIndexer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!picWorking.Visible)
            {
                SaveConfig();
                return;
            }
            MessageBox.Show("Please wait until the current process finishes", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            e.Cancel = true;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            var openfolder = new FolderBrowserDialog {Description = "Select folder to add", ShowNewFolderButton = true};

            if (openfolder.ShowDialog() != DialogResult.OK) return;

            if (Directory.Exists(openfolder.SelectedPath) && !lstFolders.Items.Contains(openfolder.SelectedPath))
            {
                lstFolders.Items.Add(openfolder.SelectedPath);
            }
            CheckFolderCount();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var message = Tools.ReadHelpFile("fi");
            var help = new HelpForm(Text + " - Help", message);
            help.ShowDialog();
        }

        private void btnBuild_EnabledChanged(object sender, EventArgs e)
        {
            chkSubDirs.Enabled = btnBuild.Enabled;
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (btnBuild.Text == "Cancel")
            {
                CancelWorkers = true;
                btnBuild.Enabled = false;
                return;
            }
            if (MessageBox.Show("This might take a while depending on how many folders you have selected and how many files are in those folders\nAre you sure you want to do this now?",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            txtSearch.Text = TypeToSearch;
            btnBuild.Text = "Cancel";
            lstSongs.Items.Clear();
            EnableDisable(false);
            indexingWorker.RunWorkerAsync();
        }

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }

        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            if (picWorking.Visible) return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var folder = Path.GetDirectoryName(files[0]);

            if (string.IsNullOrWhiteSpace(folder)) return;
            if (!Directory.Exists(folder) || lstFolders.Items.Contains(folder)) return;
            lstFolders.Items.Add(folder);
            CheckFolderCount();
        }

        private void EnableDisable(bool enabled)
        {
            menuStrip1.Enabled = enabled;
            contextMenuStrip1.Enabled = enabled;
            btnClear.Enabled = enabled;
            btnDelete.Enabled = enabled;
            btnNew.Enabled = enabled;
            chkSubDirs.Enabled = enabled;
            txtSearch.Enabled = enabled;
            radioSongs.Enabled = enabled;
            radioPackages.Enabled = enabled;

            Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
            lstFolders.Cursor = Cursor;
            lstSongs.Cursor = Cursor;

            picWorking.Visible = !enabled;
            
            if (enabled) return;
            btnClearSearch.Enabled = false;
            lblWorking.Visible = true;
            lblWorking.Text = "Working...";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            CancelWorkers = false;
            DisplayIndexedFiles();
            SortSongs();
            EnableDisable(true);
            btnBuild.Text = "Build Index";
            btnBuild.Enabled = true;
            CheckFolderCount();
            EnableSearch(Songs.Any());
        }

        private void DisplayIndexedFiles(bool doFindIDs = false, string filter = "", bool filterpackage = false)
        {
            var workingSongs = doFindIDs ? FilteredSongs : Songs;
            if (!workingSongs.Any()) return;
            var packages = new List<string>();
            lstSongs.Enabled = true;
            lstSongs.Items.Clear();
            lstSongs.ListViewItemSorter = null;
            lstSongs.BeginUpdate();
            foreach (var song in workingSongs)
            {
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    if ((!filterpackage && !song.Name.ToLowerInvariant().Contains(filter)) ||
                         (filterpackage && !song.Location.ToLowerInvariant().Contains(filter))) continue;
                }
                var entry = new ListViewItem(song.Name);
                entry.SubItems.Add(song.SongID);
                entry.SubItems.Add(song.Location);
                lstSongs.Items.Add(entry);
                if (!packages.Contains(song.Location))
                {
                    packages.Add(song.Location);
                }
            }
            lstSongs.EndUpdate();
            btnBuild.Text = "Rebuild Index";
            lblWorking.Visible = true;
            CountResults(lstSongs.Items.Count, packages.Count);
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Songs.Clear();
            foreach (var file in lstFolders.Items.Cast<object>().Select(item => item.ToString()).Where(Directory.Exists).Select(
                folder => Directory.GetFiles(folder, "*.*", chkSubDirs.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)).SelectMany(
                    files => files.Where(file => string.IsNullOrWhiteSpace(Path.GetExtension(file)))).TakeWhile(file => !indexingWorker.CancellationPending))
            {
                if (CancelWorkers) return;
                try
                {
                    if (VariousFunctions.ReadFileType(file) != XboxFileType.STFS) continue;
                    if (!Parser.ExtractDTA(file)) continue;
                    if (!Parser.ReadDTA(Parser.DTA) || !Parser.Songs.Any())
                    {
                        continue;
                    }
                    foreach (var newEntry in Parser.Songs.Select(song => new SongIndex
                    {
                        Name = song.Artist + " - " + song.Name,
                        Location = file,
                        SongID = song.SongIdString
                    }))
                    {
                        Songs.Add(newEntry);
                    }
                }
                catch (Exception)
                {}
            }
            SaveIndex();
        }

        private void SaveIndex()
        {
            if (!Songs.Any()) return;
            Tools.DeleteFile(Index);
            var sw = new StreamWriter(Index, false);
            sw.WriteLine("NewFormat=True");
            sw.WriteLine("IndexedCount=" + Songs.Count);
            foreach (var song in Songs)
            {
                sw.WriteLine("SongName=" + song.Name);
                sw.WriteLine("SongID=" + song.SongID);
                sw.WriteLine("FileLocation=" + song.Location);
            }
            sw.Dispose();
        }
        
        private void openFolderThatContainsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var file = lstSongs.SelectedItems[0].SubItems[2].Text;
            Process.Start("explorer.exe", "/select," + file);
        }
        
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            txtSearch.ForeColor = txtSearch.Text == TypeToSearch ? Color.LightGray : Color.Black;
            if (txtSearch.Text == TypeToSearch || txtSearch.Text == "[FILTER: ID]" || DontUpdate || txtSearch.Text == "[FILTER: FILE]")
            {
                DontUpdate = false;
                btnClearSearch.Enabled = false;
                return;
            }
            btnClearSearch.Enabled = true;
            DisplayIndexedFiles(false, txtSearch.Text.ToLowerInvariant(), radioPackages.Checked);
            SortSongs();
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = TypeToSearch;
            EnableSearch(true);
            DisplayIndexedFiles();
            SortSongs();
        }

        private void CountResults(int count, int packs)
        {
            if (count == 0)
            {
                lblWorking.Text = "No matches found...";
            }
            else
            {
                lblWorking.Text = "Indexed " + count + " " + (count == 1 ? "song" : "songs") + (packs == 1 ? " in 1 file" : " across " + packs + " files");
            }
        }

        private void clearIndexedFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.DeleteFile(Index);
            Songs.Clear();
            lstSongs.Items.Clear();
            EnableSearch(false);

            btnBuild.Text = "Build Index";
            txtSearch.Text = TypeToSearch;
        }
        
        private void FileIndexer_Resize(object sender, EventArgs e)
        {
            lstSongs.Columns[0].Width = (int)((lstSongs.Width - LIST_SPACER) * LIST_SIZER1);
            lstSongs.Columns[1].Width = (int)((lstSongs.Width - LIST_SPACER) * LIST_SIZER2);
        }

        private void FileIndexer_Shown(object sender, EventArgs e)
        {
            if (DoMaximize)
            {
                WindowState = FormWindowState.Maximized;
            }
            Application.DoEvents();
            LoadIndex();
            txtSearch.SelectionStart = 0;
            txtSearch.SelectionLength = 0;
            picWorking.Visible = false;
        }

        private void onlyShowOtherSongs_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "[FILTER: FILE]";
            EnableSearch(false);
            var file = lstSongs.SelectedItems[0].SubItems[2].Text.ToLowerInvariant();
            DisplayIndexedFiles(false, file, true);
            SortSongs();
            btnClearSearch.Enabled = true;
        }
        
        private void txtSearch_MouseClick(object sender, MouseEventArgs e)
        {
            if (txtSearch.Text != TypeToSearch) return;
            DontUpdate = true;
            txtSearch.Text = "";
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text.Trim() != "") return;
            txtSearch.Text = TypeToSearch;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            contextMenuStrip1.Enabled = lstSongs.Items.Count > 0;
        }

        private void exportDisplayedSongs_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Title = Text,
                InitialDirectory = Tools.CurrentFolder,
                OverwritePrompt = true,
                AddExtension = true,
                Filter = "CSV Files (*.csv)|*csv"
            };

            if (sfd.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(sfd.FileName)) return;
            Tools.CurrentFolder = Environment.CurrentDirectory;
            var filename = sfd.FileName;
            if (string.IsNullOrWhiteSpace(Path.GetExtension(filename)))
            {
                filename = filename + ".csv";
            }

            var sw = new StreamWriter(filename, false);
            try
            {
                sw.WriteLine("\"Song Name\",\"Song ID\",\"File Location\"");

                for (var i = 0; i < lstSongs.Items.Count; i++)
                {
                    sw.WriteLine("\"" + lstSongs.Items[i].SubItems[0].Text + "\",\"" + lstSongs.Items[i].SubItems[1].Text + "\",\"" + lstSongs.Items[i].SubItems[2].Text + "\"");
                }
                sw.Dispose();

                MessageBox.Show("Exported to CSV successfully", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                sw.Dispose();
                MessageBox.Show("Exporting to CSV failed\nThe error says: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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

        private void SendToCONExplorer_Click(object sender, EventArgs e)
        {
            var handler = new CONExplorer(Color.FromArgb(34, 169, 31), Color.White);
            handler.LoadCON(lstSongs.SelectedItems[0].SubItems[2].Text);
            handler.Show();
        }

        private void SendToVisualizer_Click(object sender, EventArgs e)
        {
            var handler = new Visualizer(Color.FromArgb(230, 215, 0), Color.White, lstSongs.SelectedItems[0].SubItems[2].Text);
            handler.Show();
        }

        private void SendToMIDICleaner_Click(object sender, EventArgs e)
        {
            var handler = new MIDICleaner(lstSongs.SelectedItems[0].SubItems[2].Text, Color.FromArgb(230, 215, 0), Color.White);
            handler.Show();
        }

        private void SendToSongAnalyzer_Click(object sender, EventArgs e)
        {
            var handler = new SongAnalyzer(lstSongs.SelectedItems[0].SubItems[2].Text);
            handler.Show();
        }

        private void SendToAudioAnalyzer_Click(object sender, EventArgs e)
        {
            var handler = new AudioAnalyzer(lstSongs.SelectedItems[0].SubItems[2].Text);
            handler.Show();
        }
        
        private void SendToQuickPackEditor_Click(object sender, EventArgs e)
        {
            var handler = new QuickPackEditor(null, Color.FromArgb(34, 169, 31), Color.White, "", lstSongs.SelectedItems[0].SubItems[2].Text);
            handler.ShowDialog();
        }

        private void findSongsWithoutWipeproofIDs_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This might take a while depending on how many songs you have indexed\nAre you sure you want to do this now?",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                if (txtSearch.Text.Trim() == "[FILTER: WIPE_PROOF_ID]")
                {
                    txtSearch.Text = TypeToSearch;
                }
                return;
            }
            btnBuild.Text = "Cancel";
            txtSearch.Text = "[FILTER: WIPE_PROOF_ID]";
            lstSongs.Items.Clear();
            EnableDisable(false);
            SearchForIDConflicts = false;
            filteringWorker.RunWorkerAsync();
        }

        private void filteringWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            FilteredSongs = new List<SongIndex>();
            if (SearchForIDConflicts)
            {
                var IDs = new List<SongIndex>();
                foreach (var song in Songs)
                {
                    if (CancelWorkers) return;
                    foreach (var ID in IDs)
                    {
                        if (CancelWorkers) return;
                        if (!song.SongID.Equals(ID.SongID) || string.IsNullOrEmpty(song.SongID) || 
                            string.IsNullOrEmpty(ID.SongID) || song.SongID.Equals("0") || ID.SongID.Equals("0")) continue;
                        if (!FilteredSongs.Contains(song))
                        {
                            FilteredSongs.Add(song);
                        }
                        if (!FilteredSongs.Contains(ID))
                        {
                            FilteredSongs.Add(ID);
                        }
                    }
                    IDs.Add(song);
                }
            }
            else
            {
                foreach (var song in Songs.Where(song => !Parser.IsNumericID(song.SongID)))
                {
                    if (CancelWorkers) return;
                    FilteredSongs.Add(song);
                }
            }
            
        }

        private void filteringWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (CancelWorkers)
            {
                txtSearch.Text = TypeToSearch;
                DisplayIndexedFiles();
            }
            else
            {
                DisplayIndexedFiles(true);
            }
            var message = "Found " + FilteredSongs.Count + " song" + (FilteredSongs.Count > 1 ? "s" : "") + (SearchForIDConflicts ? 
                " with possible ID conflicts\nSongs are sorted by song ID and should be grouped together in the list" : " without a wipe-proof ID");
            MessageBox.Show(message, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnClearSearch.Enabled = true;
            ActiveSortColumn = 1;//sort by ID
            SortSongs();
            CancelWorkers = false;
            EnableDisable(true);
            btnBuild.Text = "Build Index";
            btnBuild.Enabled = true;
            CheckFolderCount();
            EnableSearch(txtSearch.Text == TypeToSearch);
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            switch (txtSearch.Text.Trim())
            {
                case "[FILTER: WIPE_PROOF_ID]":
                    findSongsWithoutWipeproofIDs.PerformClick();
                    break;
                case "[FILTER: FILE]":
                    onlyShowOtherSongs_Click(sender, e);
                    break;
                case "[FILTER: ID_CONFLICT]":
                    findSongsWithIDConflicts.PerformClick();
                    break;
            }
            btnClearSearch.Enabled = true;
        }

        private void lstSongs_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != ActiveSortColumn)
            {
                ListSorting = SortOrder.Ascending;
            }
            else
            {
                ListSorting = ListSorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            }
            ActiveSortColumn = e.Column;
            SortSongs();
        }

        private void SortSongs()
        {
            if (lstSongs.Items.Count < 2) return;
            lstSongs.BeginUpdate();
            lstSongs.ListViewItemSorter = new ListViewItemComparer(lstSongs, ActiveSortColumn, ListSorting);
            lstSongs.Sort();
            lstSongs.EndUpdate();
        }

        private void findSongsWithIDConflicts_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This might take a while depending on how many songs you have indexed\nAre you sure you want to do this now?",
                    Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                if (txtSearch.Text.Trim() == "[FILTER: ID_CONFLICT]")
                {
                    txtSearch.Text = TypeToSearch;
                }
                return;
            }
            btnBuild.Text = "Cancel";
            txtSearch.Text = "[FILTER: ID_CONFLICT]";
            lstSongs.Items.Clear();
            EnableDisable(false);
            SearchForIDConflicts = true;
            filteringWorker.RunWorkerAsync();
        }
    }

    public class SongIndex
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string SongID { get; set; }
    }
}
