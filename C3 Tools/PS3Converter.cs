using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using C3Tools.Properties;
using C3Tools.x360;
using Microsoft.VisualBasic;
using SearchOption = System.IO.SearchOption;
using System.Drawing;

namespace C3Tools
{
    public partial class PS3Converter : Form
    {
        private static List<string> inputFiles;
        private DateTime endTime;
        private DateTime startTime;
        private readonly string PS3Folder;
        private readonly MainForm xMainForm;
        private readonly string rar;
        private string SongName;
        private string SongArtist;
        private string InternalName;
        private readonly NemoTools Tools;
        private readonly DTAParser Parser;
        private readonly string bin;
        private readonly string MergedSongsFolder;
        private readonly string AllSongsFolder;
        private readonly string ToMergeFolder;
        private readonly string MergeDTA;
        private readonly string CONFolder;
        private int NumericPrefix = 1;
        private int NumericAuthorID;
        private int NumericSongNumber = 1;
        private readonly string DataFolder;

        public PS3Converter(MainForm xParent, Color ButtonBackColor, Color ButtonTextColor)
        {
            xMainForm = xParent;
            InitializeComponent();
            Tools = new NemoTools();
            Parser = new DTAParser();
            inputFiles = new List<string>();
            PS3Folder = Application.StartupPath + "\\ps3\\";
            MergedSongsFolder = PS3Folder + "Merged Songs\\";
            AllSongsFolder = PS3Folder + "All Songs\\";
            ToMergeFolder = PS3Folder + "Songs to Merge\\";
            CONFolder = PS3Folder + "CONs\\";
            MergeDTA = MergedSongsFolder + "songs.dta";
            DataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\c3\\";
            bin = Application.StartupPath + "\\bin\\";
            LoadConfig();
            if (!Directory.Exists(MergedSongsFolder))
            {
                Directory.CreateDirectory(MergedSongsFolder);
            }
            if (!Directory.Exists(AllSongsFolder))
            {
                Directory.CreateDirectory(AllSongsFolder);
            }
            if (!Directory.Exists(ToMergeFolder))
            {
                Directory.CreateDirectory(ToMergeFolder);
            }
            if (!Directory.Exists(CONFolder))
            {
                Directory.CreateDirectory(CONFolder);
            }
            chkMerge.Enabled = File.Exists(MergeDTA);
            mergeSongsToolStrip.Enabled = chkMerge.Enabled;
            managePackDTAFile.Enabled = chkMerge.Enabled;
            var formButtons = new List<Button> { btnReset, btnRefresh,btnFolder,btnBegin };
            foreach (var button in formButtons)
            {
                button.BackColor = ButtonBackColor;
                button.ForeColor = ButtonTextColor;
                button.FlatAppearance.MouseOverBackColor = button.BackColor == Color.Transparent ? Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B) : Tools.LightenColor(button.BackColor);
            }
            rar = bin + "rar.exe";
            if (!File.Exists(rar))
            {
                MessageBox.Show("Can't find rar.exe ... I won't be able to create RAR files for your songs without it",
                                "Missing Executable", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                chkRAR.Checked = false;
                chkRAR.Enabled = false;
            }
            toolTip1.SetToolTip(btnBegin, "Click to begin process");
            toolTip1.SetToolTip(btnFolder, "Click to select the input folder");
            toolTip1.SetToolTip(btnRefresh, "Click to refresh if the contents of the folder have changed");
            toolTip1.SetToolTip(txtFolder, "This is the working directory");
            toolTip1.SetToolTip(lstLog, "This is the application log. Right click to export");
        }

        private void LoadConfig()
        {
            var config = Application.StartupPath + "\\bin\\config\\ps3.config";
            var backup = DataFolder + Path.GetFileName(config);
            if (!File.Exists(config) && File.Exists(backup))
            {
                if (MessageBox.Show("It looks like you don't have a PS3 configuration file in the /bin folder, but I found a backup. Do you want me to " +
                                    "restore that?\n\n[RECOMMENDED]\nClick Yes to restore and continue from the last numeric ID you used\n\n" +
                                    "[NOT RECOMMENDED]\nClick No to ignore and start over - LIKELY TO CAUSE ID CONFLICTS!", Text, MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Copy(backup, config);
                    
                }
            }
            if (File.Exists(config))
            {
                var sr = new StreamReader(config);
                try
                {
                    var line = sr.ReadLine();
                    if (line.Contains("CurrentSongID")) //old config file, delete it
                    {
                        sr.Dispose();
                        Tools.DeleteFile(config);
                        return;
                    }
                    NumericPrefix = Convert.ToInt16(Tools.GetConfigString(line));
                    NumericAuthorID = Convert.ToInt32(Tools.GetConfigString(sr.ReadLine()));
                    NumericSongNumber = Convert.ToInt32(Tools.GetConfigString(sr.ReadLine()));
                    regionNTSC.Checked = sr.ReadLine().Contains("True");
                    regionPAL.Checked = !regionNTSC.Checked;
                    var type = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                    type1.Checked = false;
                    type2.Checked = false;
                    type3.Checked = false;
                    switch (type)
                    {
                        default:
                            type1.Checked = true;
                            break;
                        case 2:
                            type2.Checked = true;
                            break;
                        case 3:
                            type3.Checked = true;
                            break;
                    }
                    var wait = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                    wait2Seconds.Checked = false;
                    wait5Seconds.Checked = false;
                    wait10Seconds.Checked = false;
                    switch (wait)
                    {
                        default:
                            wait2Seconds.Checked = true;
                            break;
                        case 5:
                            wait5Seconds.Checked = true;
                            break;
                        case 10:
                            wait10Seconds.Checked = true;
                            break;
                    }
                }
                catch (Exception)
                {
                    regionNTSC.Checked = true;
                    type1.Checked = true;
                    wait2Seconds.Checked = true;
                }
                sr.Dispose();
            }
            else
            {
                DoFirstTimeUniqueIDPrompt();
            }
        }
        
        private void UpdateSongCounter()
        {
            NumericSongNumber++;
            if (NumericSongNumber > 99999)
            {
                MessageBox.Show("You've converted more than the maximum 99,999 songs!\nI'm going to restart the counter at 1, " +
                                "please keep in mind this may create conflicts with your oldest songs\nYou may need to use a different author ID",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                NumericSongNumber = 1;
            }
            SaveConfig();
        }

        private void SaveConfig()
        {
            var config = Application.StartupPath + "\\bin\\config\\ps3.config";
            var backup = DataFolder + Path.GetFileName(config);

            var sw = new StreamWriter(config, false);
            try
            {
                sw.WriteLine("SongIDPrefix=" + NumericPrefix);
                sw.WriteLine("AuthorID=" + NumericAuthorID);
                sw.WriteLine("CurrentSongNumber=" + NumericSongNumber);
                sw.WriteLine("UseNTSC=" + regionNTSC.Checked);
                int type;
                if (type1.Checked)
                {
                    type = 1;
                }
                else if (type2.Checked)
                {
                    type = 2;
                }
                else
                {
                    type = 3;
                }
                sw.WriteLine("EncryptionType=" + type);
                int wait;
                if (wait2Seconds.Checked)
                {
                    wait = 2;
                }
                else if (wait5Seconds.Checked)
                {
                    wait = 5;
                }
                else
                {
                    wait = 10;
                }
                sw.WriteLine("Type2Wait=" + wait);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error saving the song ID configuration file\nThis is what happened:\n\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            sw.Dispose();

            try
            {
                Tools.DeleteFile(backup);
                File.Copy(config, backup);
            }
            catch (Exception)
            {}
        }

        private void DoFirstTimeUniqueIDPrompt()
        {
            MessageBox.Show("Hey there, it looks like this is the first time you're running a version of " + Text +
                " that requires an Author ID\nYou're going to see two prompts next...\n\nThe first prompt is for your Author ID - this is your Profile ID on the C3 Forums\n\nThe second prompt is for the song number ... if you've already converted other songs before using this system, " +
                "change this number to one that won't create conflicts with prior songs, otherwise you can leave it alone", Text,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            changeAuthorID_Click(null, null);
            changeSongNumber_Click(null, null);
            SaveConfig();
        }

        private string GetNumericID()
        {
            int author;
            var prefix = 1;

            if (NumericAuthorID < 10000)
            {
                author = NumericAuthorID;
            }
            else
            {
                author = NumericAuthorID - 10000;
                prefix += 1;
            }

            var auth = "0000" + author;
            var id = prefix + auth.Substring(auth.Length - 4, 4);

            var song = ("00000" + NumericSongNumber);
            song = song.Substring(song.Length - 5, 5);

            return id + song;
        }

        private void btnFolder_Click(object sender, EventArgs e)
        {
            //if user selects new folder, assign that value
            //if user cancels or selects same folder, this forces the text_changed event to run again
            var tFolder = txtFolder.Text;

            var folderUser = new FolderBrowserDialog
                {
                    SelectedPath = txtFolder.Text, 
                    Description = "Select the folder where your CON files are",
                };
            txtFolder.Text = "";
            var result = folderUser.ShowDialog();
            txtFolder.Text = result == DialogResult.OK ? folderUser.SelectedPath : tFolder;
        }

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
        
        private void txtFolder_TextChanged(object sender, EventArgs e)
        {
            if (picWorking.Visible) return;
            inputFiles.Clear();
            if (string.IsNullOrWhiteSpace(txtFolder.Text))
            {
                btnRefresh.Visible = false;
            }
            btnRefresh.Visible = true;
            chkMerge.Enabled = File.Exists(MergeDTA);
            mergeSongsToolStrip.Enabled = chkMerge.Enabled;
            managePackDTAFile.Enabled = chkMerge.Enabled;
            if (txtFolder.Text != "")
            {
                Tools.CurrentFolder = txtFolder.Text;
                Log("");
                Log("Reading input directory ... hang on");
                try
                {
                    var inFiles = Directory.GetFiles(txtFolder.Text);
                    foreach (var file in inFiles)
                    {
                        try
                        {
                            if (VariousFunctions.ReadFileType(file) == XboxFileType.STFS)
                            {
                                inputFiles.Add(file);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (Path.GetExtension(file) != "") continue;
                            Log("There was a problem accessing file " + Path.GetFileName(file));
                            Log("The error says: " + ex.Message);
                        }
                    }
                    if (!inputFiles.Any())
                    {
                        Log("Did not find any CON files ... try a different directory");
                        Log("You can also drag and drop CON files here");
                        Log("Ready");
                        btnBegin.Visible = false;
                        btnRefresh.Visible = true;
                    }
                    else
                    {
                        Log("Found " + inputFiles.Count + " CON " + (inputFiles.Count > 1 ? "files" : "file"));
                        Log("Ready to begin");
                        btnBegin.Visible = true;
                        btnRefresh.Visible = true;
                    }
                }
                catch (Exception ex)
                {
                    Log("There was an error: " + ex.Message);
                }
            }
            else
            {
                btnBegin.Visible = false;
                btnRefresh.Visible = false;
            }
            txtFolder.Focus();
        }
        
        public bool loadDTA()
        {
            try
            {
                InternalName = Parser.Songs[0].InternalName;
                SongName = Parser.Songs[0].Name;
                SongArtist = Parser.Songs[0].Artist;
                return (SongName != "" && SongArtist != "" && InternalName != "");
            }
            catch (Exception ex)
            {
                Log("There was an error processing that songs.dta file");
                Log("The error says: " + ex.Message);
                return false;
            }
        }
        
        private bool ProcessFiles()
        {
            var counter = 0;
            var success = 0;
            InternalName = "";
            SongName = "";
            SongArtist = "";
            var internalfolder = "";
            foreach (var file in inputFiles.Where(File.Exists).TakeWhile(file => !backgroundWorker1.CancellationPending))
            {
                try
                {
                    if (VariousFunctions.ReadFileType(file) != XboxFileType.STFS) continue;
                    try
                    {
                        counter++;
                        Parser.ExtractDTA(file);
                        Parser.ReadDTA(Parser.DTA);
                        if (Parser.Songs.Count > 1)
                        {
                            Log("File " + Path.GetFileName(file) + " is a pack, try dePACKing first, skipping...");
                            continue;
                        }
                        if (!Parser.Songs.Any())
                        {
                            Log("There was an error processing the songs.dta file");
                            continue;
                        }
                        if (loadDTA())
                        {
                            Log("Loaded and processed songs.dta file for song #" + counter + " successfully");
                            Log("Song #" + counter + " is " + SongArtist + " - " + SongName);
                        }
                        else
                        {
                            return false;
                        }
                        var songextracted = PS3Folder + "temp\\" + Path.GetFileName(file).Replace(" ","") + "_extracted\\";
                        var songfolder = chkMerge.Checked && chkMerge.Enabled ? MergedSongsFolder : AllSongsFolder + Tools.CleanString(SongArtist,false) + " - " + Tools.CleanString(SongName, false) + "\\";
                        internalfolder = songfolder + CleanString(InternalName) + "\\";
                        var genfolder = internalfolder + "gen\\";
                        Tools.DeleteFolder(songextracted,true);
                        var xPackage = new STFSPackage(file);
                        if (!xPackage.ParseSuccess)
                        {
                            Log("Failed to parse '" + Path.GetFileName(file) + "'");
                            Log("Skipping this file");
                            continue;
                        }
                        xPackage.ExtractPayload(songextracted, true, false);
                        xPackage.CloseIO();
                        if (!Directory.Exists(genfolder))
                        {
                            Directory.CreateDirectory(genfolder);
                        }
                        var midi = Directory.GetFiles(songextracted, "*.mid", SearchOption.AllDirectories);
                        if (midi.Count() != 0)
                        {
                            var newmidi = internalfolder + CleanString(Path.GetFileName(midi[0]));
                            Tools.DeleteFile(newmidi);
                            if (Tools.MoveFile(midi[0], newmidi))
                            {
                                Log("Extracted MIDI file " + Path.GetFileName(newmidi) + " successfully");
                                if (type1.Checked)
                                {
                                    MakeEdat("edat1", newmidi);
                                }
                                else if (type2.Checked)
                                {
                                    MakeEdat("edat2", newmidi);
                                }
                                else
                                {
                                    MakeEdat("edat3", newmidi);
                                }
                            }
                            else
                            {
                                Log("There was a problem extracting the MIDI file");
                            }
                        }
                        else
                        {
                            Log("WARNING: Did not find a MIDI file in the extracted folder");
                        }
                        var dtaOut = Directory.GetFiles(songextracted, "*.dta", SearchOption.AllDirectories);
                        if (dtaOut.Count() != 0)
                        {
                            try
                            {
                                var newdta = songfolder + "songs.dta";
                                var sr = new StreamReader(dtaOut[0]);
                                var sw = new StreamWriter(newdta, chkMerge.Checked && chkMerge.Enabled);
                                var done = false;
                                while (sr.Peek() > 0)
                                {
                                    var line = sr.ReadLine();
                                    if (line.Trim() == "(" && !done)
                                    {
                                        sw.WriteLine(line);
                                        line = "   '" + CleanString(sr.ReadLine()) + "'"; //clean shortname
                                        done = true;
                                    }
                                    if (line.Contains("rating") && line.Contains("4"))
                                    {
                                        line = line.Replace("4", "2"); //change Not Rated to Supervision Recommended as needed
                                    }
                                    else if (line.Contains("songs/"))
                                    {
                                        //PS3 is case sensitive, files below are changed, need to reflect here
                                        //this also removes unsupported characters that create conflict
                                        line = line.Replace(Parser.Songs[0].InternalName, CleanString(Parser.Songs[0].InternalName)).ToLowerInvariant();
                                    }
                                    else if (line.Contains("song_id") && chkSongID.Checked)
                                    {
                                        if (!Parser.IsNumericID(line)) //only if not already a unique C3 numeric ID
                                        {
                                            line = ";ORIG_ID=" + Parser.GetSongID(line);
                                            sw.WriteLine(line);
                                            line = "   ('song_id' " + GetNumericID() + ")";
                                            UpdateSongCounter();
                                        }
                                    }
                                    if (line.Trim() != "")
                                    {
                                        sw.WriteLine(line);
                                    }
                                }
                                sr.Dispose();
                                sw.Dispose();
                                Log("Extracted and modified songs.dta file successfully");
                            }
                            catch (Exception ex)
                            {
                                Log("WARNING: There was an error extracting and modifying the songs.dta file");
                                Log("Error says: " + ex.Message);
                            }
                        }
                        else
                        {
                            Log("WARNING: Did not find a songs.dta file in the extracted folder");
                        }
                        var art = Directory.GetFiles(songextracted, "*.png_xbox", SearchOption.AllDirectories);
                        if (art.Count() != 0)
                        {
                            var newart = genfolder + CleanString(Path.GetFileName(art[0]));
                            Tools.DeleteFile(newart);
                            if (Tools.MoveFile(art[0], newart))
                            {
                                Log("Extracted album art file " + Path.GetFileName(newart) + " successfully");
                                Log(Tools.ConvertXboxtoPS3(newart, newart, true)
                                    ? "Converted album art from png_xbox to png_ps3 successfully"
                                    : "There was a problem converting the album art to png_ps3 format");
                            }
                            else
                            {
                                Log("There was a problem extracting the album art file");
                            }
                        }
                        else
                        {
                            Log("WARNING: Did not find album art file in the extracted folder");
                        }
                        var mogg = Directory.GetFiles(songextracted, "*.mogg", SearchOption.AllDirectories);
                        if (mogg.Count() != 0)
                        {
                            var newmogg = internalfolder + CleanString(Path.GetFileName(mogg[0]));
                            Tools.DeleteFile(newmogg);
                            if (Tools.MoveFile(mogg[0], newmogg))
                            {
                                Log("Extracted mogg file " + Path.GetFileName(newmogg) + " successfully");
                                EncryptMogg(newmogg);
                            }
                            else
                            {
                                Log("There was a problem extracting the mogg file");
                            }
                        }
                        else
                        {
                            Log("WARNING: Did not find a mogg file in the extracted folder");
                        }
                        var milo = Directory.GetFiles(songextracted, "*.milo_xbox", SearchOption.AllDirectories);
                        if (milo.Count() != 0)
                        {
                            var newmilo = genfolder + CleanString(Path.GetFileNameWithoutExtension(milo[0])) + ".milo_ps3";
                            Tools.DeleteFile(newmilo);
                            if (Tools.MoveFile(milo[0], newmilo))
                            {
                                Log("Extracted milo file " + Path.GetFileName(newmilo) + " successfully");
                            }
                            else
                            {
                                Log("There was a problem extracting the milo file");
                            }
                        }
                        else
                        {
                            Log("WARNING: Did not find a milo file in the extracted folder");
                        }
                        Tools.DeleteFolder(PS3Folder + "temp\\", true);
                        success++;
                        //in case of stragglers
                        Tools.DeleteFile(internalfolder + "c.exe");
                        if (!chkRAR.Checked || (chkMerge.Checked && chkMerge.Enabled) || backgroundWorker1.CancellationPending) continue;
                        var archive = Path.GetFileName(file);
                        archive = archive.Replace(" ", "").Replace("-", "_").Replace("\\","").Replace("'", "").Replace(",", "").Replace("_rb3con","");
                        archive = Tools.CleanString(archive, false);
                        archive = txtFolder.Text + "\\" + archive + "_ps3.rar";
                        var arg = "a -m5 -r -ep1 \"" + archive + "\" \"" + songfolder + "\"";
                        Log("Creating RAR archive");
                        //in case of stragglers
                        Tools.DeleteFile(internalfolder + "c.exe");
                        Log(Tools.CreateRAR(rar, archive, arg) ? "Created RAR archive successfully" : "RAR archive creation failed");
                    }
                    catch (Exception ex)
                    {
                        //in case of stragglers
                        Tools.DeleteFile(internalfolder + "c.exe");
                        Log("There was an error: " + ex.Message);
                        Log("Attempting to continue with the next file");
                    }
                }
                catch (Exception ex)
                {
                    //in case of stragglers
                    Tools.DeleteFile(internalfolder + "c.exe");
                    Log("There was a problem accessing that file");
                    Log("The error says: " + ex.Message);
                }
            }
            //sometimes c.exe is left behind, let's make sure to delete them
            Tools.DeleteFile(internalfolder + "c.exe");
            Log("Successfully processed " + success + " of " + counter + " files");
            return true;
        }

        private static string CleanString(string line)
        {
            var bad_chars = new List<string>
            {
                "!","@","#","$","%","^","&","*","(",")","<",">","?","//","\\","~","`",",", "'"
            };
            line = bad_chars.Aggregate(line, (current, bad) => current.Replace(bad, ""));
            return line.ToLowerInvariant().Trim();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            var tFolder = txtFolder.Text;
            txtFolder.Text = "";
            txtFolder.Text = tFolder;
        }
        
        private void MakeEdat(string format, string file)
        {
            if (!File.Exists(file)) return;
            Log("Encrypting MIDI file '" + Path.GetFileName(file) + "' to EDAT");
            var region = regionNTSC.Checked ? "regionNTSC" : "regionPAL";
            var wait = wait2Seconds.Checked ? "2000" : (wait5Seconds.Checked ? "5000" : "10000");
            var arg = format + " " + region + " " + wait + " \"" + file + "\"";
            var startInfo = new ProcessStartInfo
            {
                FileName = bin + "nemoedat.exe",
                Arguments = arg,
                UseShellExecute = false
            };
            var process = Process.Start(startInfo);
            do
            {//
            } while (!process.HasExited);
            process.Dispose();
            var edat = file + ".edat";
            if (File.Exists(edat))
            {
                Log("Encrypted MIDI file to EDAT successfully");
            }
            else
            {
                Log("Failed to encrypt MIDI file " + Path.GetFileName(file) + " ... try again");
            }
        }
        
        private void HandleDragDrop(object sender, DragEventArgs e)
        {
            if (picWorking.Visible) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (btnReset.Visible)
            {
                btnReset.PerformClick();
            }
            if (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS)
            {
                txtFolder.Text = Path.GetDirectoryName(files[0]);
                Tools.CurrentFolder = txtFolder.Text;
            }
            else switch (Path.GetExtension(files[0]).ToLowerInvariant())
            {
                case ".mogg":
                    if (MessageBox.Show("Do you want to encrypt mogg file '" + Path.GetFileName(files[0]) + "'?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        EncryptMogg(files[0]);
                    }
                    break;
                case ".mid":
                    if (MessageBox.Show("Do you want to encrypt MIDI file '" + Path.GetFileName(files[0]) + "'?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        EncryptMIDI(files[0]);
                    }
                    break;
                default:
                    MessageBox.Show("That's not a valid file to drop here", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    break;
            }
        }
        
        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var message = Tools.ReadHelpFile("ps3");
            var help = new HelpForm(Text + " - Help", message);
            help.ShowDialog();
        }

        private void exportLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Tools.ExportLog(Text, lstLog.Items);
        }
        
        private void btnReset_Click(object sender, EventArgs e)
        {
            Log("Resetting...");
            inputFiles.Clear();
            btnReset.Visible = false;
            btnBegin.Visible = true;
            btnBegin.Enabled = true;
            EnableDisable(true);
            SongName = "";
            SongArtist = "";
            InternalName = "";
            btnRefresh.PerformClick();
        }

        private void EnableDisable(bool enabled)
        {
            btnFolder.Enabled = enabled;
            btnRefresh.Enabled = enabled;
            picWorking.Visible = !enabled;
            menuStrip1.Enabled = enabled;
            chkRAR.Enabled = enabled;
            txtFolder.Enabled = enabled;
            lstLog.Cursor = enabled ? Cursors.Default : Cursors.WaitCursor;
            Cursor = lstLog.Cursor;
        }

        private void btnBegin_Click(object sender, EventArgs e)
        {
            if (btnBegin.Text == "Cancel")
            {
                backgroundWorker1.CancelAsync();
                Log("User cancelled process...stopping as soon as possible");
                btnBegin.Enabled = false;
                return;
            }
            startTime = DateTime.Now;
            EnableDisable(false);
            Tools.CurrentFolder = txtFolder.Text;
            try
            {
                var files = Directory.GetFiles(txtFolder.Text);
                if (files.Count() != 0)
                {
                    btnBegin.Text = "Cancel";
                    toolTip1.SetToolTip(btnBegin, "Click to cancel process");
                    backgroundWorker1.RunWorkerAsync();
                }
                else
                {
                    Log("No files found ... there's nothing to do");
                    EnableDisable(true);
                }
            }
            catch (Exception ex)
            {
                Log("Error retrieving files to process");
                Log("The error says:" + ex.Message);
                EnableDisable(true);
            }
        }
        
        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }
        
        private void PS3Prep_Resize(object sender, EventArgs e)
        {
            btnRefresh.Left = txtFolder.Left + txtFolder.Width - btnRefresh.Width;
            btnBegin.Left = txtFolder.Left + txtFolder.Width - btnBegin.Width;
            picWorking.Left = (Width / 2) - (picWorking.Width / 2);
        }
        
        private void PS3Prep_Shown(object sender, EventArgs e)
        {
            if (Tools.IsAuthorized())
            {
                chkRAR.Checked = true;
            }
            Log("Welcome to " + Text);
            Log("Drag and drop the CON /LIVE file(s) to be converted here");
            Log("Or click 'Change Input Folder' to select the files");
            Log("Ready to begin");
            txtFolder.Text = CONFolder;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            Log("Done!");
            endTime = DateTime.Now;
            var timeDiff = endTime - startTime;
            Log("Process took " + timeDiff.Minutes + (timeDiff.Minutes == 1 ? " minute" : " minutes") + " and " + (timeDiff.Minutes == 0 && timeDiff.Seconds == 0 ? "1 second" : timeDiff.Seconds + " seconds"));
            Log("Click 'Reset' to start again or just close me down");
            //clear up any leftover png_xbox files
            var xbox_files = Directory.GetFiles(txtFolder.Text, "*.png_xbox", SearchOption.AllDirectories);
            foreach (var xbox_file in xbox_files)
            {
                Tools.DeleteFile(xbox_file);
            }
            picWorking.Visible = false;
            btnReset.Visible = true;
            btnReset.Enabled = true;
            lstLog.Cursor = Cursors.Default;
            Cursor = lstLog.Cursor;
            toolTip1.SetToolTip(btnBegin, "Click to begin");
            btnBegin.Text = "&Begin";
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (ProcessFiles()) return;
            Log("There was an error processing the files ... stopping here");
        }

        private void chkMerge_CheckedChanged(object sender, EventArgs e)
        {
            chkRAR.Enabled = !chkMerge.Checked || !chkMerge.Enabled;
        }

        private void mergeSongsToolStrip_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will merge songs that have already been converted to PS3 format with your existing songs and combine their songs.dta files\n\nTo start, copy all the songs you want to merge to the 'Songs to Merge' folder\n\nPress OK when you're ready to start", "Merge Songs Tool", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
            {
                return;
            }
            Log("Merging songs...");
            try
            {
                var counter = 0;
                var DTAs = Directory.GetFiles(ToMergeFolder, "songs.dta", SearchOption.AllDirectories);
                if (!DTAs.Any())
                {
                    Log("No songs found in 'Songs to Merge' directory");
                    return;
                }
                var folders = DTAs.Select(Path.GetDirectoryName).ToList();
                if (!folders.Any())
                {
                    Log("No songs found in 'Songs to Merge' directory");
                    return;
                }
                foreach (var song in folders)
                {
                    var skip = false;
                    var dirs = Directory.GetDirectories(song);
                    foreach (var dir in dirs)
                    {
                        var newdir = MergedSongsFolder + dir.Replace(song, "") + "\\";
                        if (Directory.Exists(newdir))
                        {
                            Log("Song folder '" + dir.Replace(song + "\\", "") + "' already exists in 'Merged Songs' folder, skipping...");
                            skip = true;
                            continue;
                        }
                        Directory.CreateDirectory(newdir + "gen\\");
                        var files = Directory.GetFiles(dir);
                        foreach (var file in files)
                        {
                            Tools.MoveFile(file, newdir + Path.GetFileName(file));
                        }
                        files = Directory.GetFiles(dir + "\\gen\\");
                        foreach (var file in files)
                        {
                            Tools.MoveFile(file, newdir + "gen\\" + Path.GetFileName(file));
                        }
                    }
                    if (skip) continue;
                    var sr = new StreamReader(song + "\\songs.dta");
                    var sw = new StreamWriter(MergeDTA, true);
                    sw.Write(sr.ReadToEnd());
                    sr.Dispose();
                    sw.Dispose();
                    counter++;
                    Tools.DeleteFolder(song, true);
                }
                Log(counter == 0 ? "No songs merged" : "Merged " + counter + " " + (counter > 1 ? "songs" : "song") + " successfully");
                if (!chkSongID.Checked) return;
                Log("A total of " + counter + " song IDs were merged into the existing DTA file");
                Log("Beginning batch replacing of song IDs with unique numeric values");
                doBatchReplace(MergeDTA, true);
            }
            catch (Exception ex)
            {
                Log("Failed to merge songs");
                Log("Error says: " + ex.Message);
            }
        }

        private void managePackDTAFile_Click(object sender, EventArgs e)
        {
            Log("Sending songs.dta file to Quick Pack Editor...");
            var newQuickPack = new QuickPackEditor(xMainForm, Color.FromArgb(197, 34, 35), Color.White, MergeDTA);
            newQuickPack.ShowDialog();
            if (!File.Exists(MergedSongsFolder + "deleted.txt"))
            {
                Log("Quick Pack Editor closed: no songs were removed from the songs.dta file");
                return;
            }
            var deleted = new List<string>();
            var sr = new StreamReader(MergedSongsFolder + "deleted.txt");
            while (sr.Peek() > -1)
            {
                deleted.Add(Tools.GetConfigString(sr.ReadLine()));
            }
            sr.Dispose();
            Tools.DeleteFile(MergedSongsFolder + "deleted.txt");
            Log("Quick Pack Editor closed: " + deleted.Count + (deleted.Count > 1 ? " songs were" : " song was") + " removed from the songs.dta file");
            var message = "The following " + (deleted.Count > 1 ? "songs were" : "song was") + " removed from the songs.dta file:\n";
            message = deleted.Aggregate(message, (current, line) => current + "\n" + line);
            message = message + "\n\nDo you want to remove the corresponding song " + (deleted.Count > 1 ? "folders" : "folder") + "?";
            if (MessageBox.Show(message, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                Log("No song folders deleted");
                return;
            }
            foreach (var folder in from line in deleted let index = line.IndexOf("::", StringComparison.Ordinal) select line.Substring(index + 2, line.Length - index - 2).Trim())
            {
                Log("Sending folder '" + folder + "' from 'Merged Songs' directory to Recycle Bin");
                Tools.SendtoTrash(MergedSongsFolder + folder, true);
            }
            Log("Deleted " + deleted.Count + " song " + (deleted.Count > 1 ? "folders" : "folder"));
        }

        private void tutorialToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://customscreators.com/index.php?/topic/10443-how-to-playing-customs-on-ps3/");
        }

        private void PS3Converter_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!picWorking.Visible)
            {
                SaveConfig();
                return;
            }
            MessageBox.Show("Please wait until the current process finishes", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            e.Cancel = true;
        }

        private void type1defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            type1.Checked = true;
            type2.Checked = false;
            type3.Checked = false;
        }

        private void type2_Click(object sender, EventArgs e)
        {
            type1.Checked = false;
            type2.Checked = true;
            type3.Checked = false;
        }

        private void wait2Seconds_Click(object sender, EventArgs e)
        {
            wait2Seconds.Checked = true;
            wait5Seconds.Checked = false;
            wait10Seconds.Checked = false;
        }

        private void wait5Seconds_Click(object sender, EventArgs e)
        {
            wait2Seconds.Checked = false;
            wait5Seconds.Checked = true;
            wait10Seconds.Checked = false;
        }

        private void wait10Seconds_Click(object sender, EventArgs e)
        {
            wait2Seconds.Checked = false;
            wait5Seconds.Checked = false;
            wait10Seconds.Checked = true;
        }

        private void encryptReplacementMogg_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "MOGG Audio File (*.mogg)|*.mogg",
                InitialDirectory = Environment.CurrentDirectory,
                Title = "Select MOGG file to encrypt",
                Multiselect = false
            };
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            EncryptMogg(ofd.FileName);
        }

        private void EncryptMogg(string mogg)
        {
            CryptVersion version;
            using (var fs = File.OpenRead(mogg))
            {
                using (var br = new BinaryReader(fs))
                {
                    version = (CryptVersion)br.ReadInt32();
                }
            }
            byte[] bytes = null;
            var mData = File.ReadAllBytes(mogg);
            if (Tools.IsC3Mogg(mData))
            {
                if (fixLoopingWhenConverting.Checked)
                {
                    bytes = GetPatchBytes(mogg);
                }
                Tools.DecM(mData, true, true, DecryptMode.ToFile, mogg); //remove old c3 encryption and apply new
            }
            else if (version != CryptVersion.x0A)
            {
                Log("Mogg file '" + Path.GetFileName(mogg) + "' is already encrypted");
                return;
            }
            if (fixLoopingWhenConverting.Checked && bytes != null && bytes.Count() > 0)
            {
                PatchMogg(mogg, bytes);
            }
            if (Tools.EncM(File.ReadAllBytes(mogg), mogg, true))
            {
                Log("Mogg file '" + Path.GetFileName(mogg) + "' was encrypted successfully");
            }
            else
            {
                Log("Failed to encrypt mogg file '" + Path.GetFileName(mogg) + "'");
            }
        }

        private void encryptReplacementMIDI_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "MIDI File (*.mid)|*.mid",
                InitialDirectory = Environment.CurrentDirectory,
                Title = "Select MIDI file(s) to encrypt",
                Multiselect = true
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            foreach (var file in ofd.FileNames)
            {
                EncryptMIDI(file, false);
            }
        }

        private void EncryptMIDI(string file, bool message = true)
        {
            //need to make sure file path is lowercase for PS3
            var temp = Path.GetDirectoryName(file) + "\\" + Path.GetFileNameWithoutExtension(file) +"_temp.mid";
            Tools.DeleteFile(temp);
            Tools.MoveFile(file, temp);
            var midi = Path.GetDirectoryName(file) + "\\" + Path.GetFileName(file).ToLowerInvariant().Replace(" ", "").Replace("-", "").Replace("'", "").Replace("(", "").Replace(")", "");
            Tools.DeleteFile(midi);
            Tools.MoveFile(temp, midi);
            if (type1.Checked)
            {
                MakeEdat("edat1", midi);
            }
            else if (type2.Checked)
            {
                MakeEdat("edat2", midi);
            }
            else
            {
                MakeEdat("edat3", midi);
            }
            var edat = midi + ".edat";
            if (File.Exists(edat))
            {
                if (message)
                {
                    MessageBox.Show("Encrypted MIDI to EDAT successfully\nNew file can be found at:\n" + edat, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                Log("Encrypted MIDI '" + Path.GetFileName(file) + "' to EDAT '" + Path.GetFileName(edat) + "' successfully");
            }
            else
            {
                if (message)
                {
                    MessageBox.Show("Failed to encrypt MIDI file '" + Path.GetFileName(file) + "'", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                Log("Failed to encrypt MIDI file '" + Path.GetFileName(file) + "'");
            }
        }
        
        private void batchChangeIDsToNumeric_Click(object sender, EventArgs e)
        {
            var config = Application.StartupPath + "\\bin\\config\\ps3.config";
            if (!File.Exists(config))
            {
                DoFirstTimeUniqueIDPrompt();
                return;
            }
            var ofd = new OpenFileDialog
            {
                InitialDirectory = MergedSongsFolder,
                Title = "Select DTA file to edit",
                Multiselect = false,
                Filter = "DTA Files(*.dta)|*.dta"
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            if (string.IsNullOrWhiteSpace(ofd.FileName)) return;
            doBatchReplace(ofd.FileName);
        }
        
        private void doBatchReplace(string dta, bool hide_message = false)
        {
            var counter = 0;
            try
            {
                var sr = new StreamReader(dta);
                var lines = new List<string>();
                while (sr.Peek() > 0)
                {
                    lines.Add(sr.ReadLine());
                }
                sr.Dispose();
                var sw = new StreamWriter(dta, false);
                foreach (var line in lines)
                {
                    var newline = line;
                    if (line.Contains("song_id") && !line.Contains(";ORIG_ID="))
                    {
                        if (!Parser.IsNumericID(line)) //only if not already a unique C3 numeric ID
                        {
                            newline = ";ORIG_ID=" + Parser.GetSongID(line);
                            sw.WriteLine(newline);
                            newline = "   ('song_id' " + GetNumericID() + ")";
                            UpdateSongCounter();
                            counter++;
                        }
                    }
                    if (newline.Trim() != "")
                    {
                        sw.WriteLine(newline);
                    }
                }
                sw.Dispose();
                if (!hide_message)
                {
                    MessageBox.Show("Process completed without any errors\nReplaced IDs for " + counter + (counter == 1 ? " song" : " songs") + "\n\nOnly custom songs without numeric IDs were edited", Text, MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error editing that DTA file\nThe error says: " + ex.Message, Text,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            if (counter == 0)
            {
                Log("No song IDs were replaced with unique numeric values");
            }
            else
            {
                Log("Successfully replaced " + counter + " song IDs with unique numeric values");
            }
        }

        private void changeAuthorID_Click(object sender, EventArgs e)
        {
            const string message = "Your Author ID is your Profile ID on the C3 Forums\nFind yours by clicking on 'PROFILE' on the forums\n\n" +
                                   "The Author ID allows us to prevent ID conflicts with songs created by other authors using this tool\n\n" +
                                   "DO NOT CHANGE UNLESS YOU WERE TOLD TO";
            var input = Interaction.InputBox(message, Text, NumericAuthorID.ToString(CultureInfo.InvariantCulture));
            if (string.IsNullOrWhiteSpace(input)) return;
            var val = -1;
            try
            {
                val = Convert.ToInt32(input);
            }
            catch (Exception)
            {}
            if (input.Length > 5 || val < 1 || val > 11473)
            {
                MessageBox.Show("That's not a valid Author ID", Text, MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                changeAuthorID_Click(sender, e);
                return;
            }
            NumericAuthorID = val;
        }

        private void changeSongNumber_Click(object sender, EventArgs e)
        {
            const string message = "Modifying this value manually may lead to song ID conflicts with other songs you've done\n\nONLY MODIFY IF YOU ABSOLUTELY HAVE TO";
            var input = Interaction.InputBox(message, Text, NumericSongNumber.ToString(CultureInfo.InvariantCulture));
            if (string.IsNullOrWhiteSpace(input)) return;
            var val = 0;
            try
            {
                val = Convert.ToInt32(input);
            }
            catch (Exception)
            {}
            if (input.Length > 5 || val < 1 || val > 99999)
            {
                MessageBox.Show("That's not a valid song number", Text, MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                changeSongNumber_Click(sender, e);
                return;
            }
            NumericSongNumber = val;
        }

        private void changeIDPrefix_Click(object sender, EventArgs e)
        {
            const string message = "Changing this prefix might make your songs not show up in game\nDO NOT CHANGE UNLESS YOU WERE TOLD TO";
            var input = Interaction.InputBox(message, Text, NumericPrefix.ToString(CultureInfo.InvariantCulture));
            if (string.IsNullOrWhiteSpace(input)) return;
            var val = 0;
            try
            {
                val = Convert.ToInt16(input);
            }
            catch (Exception)
            {}
            if (input.Length != 1 || val < 0 || val > 9)
            {
                MessageBox.Show("That's not a valid prefix", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                changeIDPrefix_Click(sender, e);
                return;
            }
            NumericPrefix = val;
        }

        private void regionNTSC_Click(object sender, EventArgs e)
        {
            regionNTSC.Checked = true;
            regionPAL.Checked = false;
        }

        private void regionPAL_Click(object sender, EventArgs e)
        {
            regionNTSC.Checked = false;
            regionPAL.Checked = true;
        }

        private void type3_Click(object sender, EventArgs e)
        {
            type1.Checked = false;
            type2.Checked = false;
            type3.Checked = true;
        }

        private void fixMoggThatCausesLooping_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "MOGG Audio File (*.mogg)|*.mogg",
                InitialDirectory = Environment.CurrentDirectory,
                Title = "Select PS3 MOGG file to fix",
                Multiselect = false
            };
            if (ofd.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            Environment.CurrentDirectory = Path.GetDirectoryName(ofd.FileName);
            var mData = File.ReadAllBytes(ofd.FileName);
            if (!Tools.MoggIsEncrypted(mData))
            {
                MessageBox.Show("This MOGG file is not encrypted, so there is nothing for me to fix", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!Tools.DecM(mData, true, true, DecryptMode.ToFile, ofd.FileName))
            {
                MessageBox.Show("Couldn't decrypt the MOGG file to fix it, sorry", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var last_bytes = GetPatchBytes(ofd.FileName);
            if (last_bytes == null || last_bytes.Count() == 0)
            {
                MessageBox.Show("This MOGG file should not cause the looping problem that this tool fixes, I can't do anything with it\n\nIf it does" +
                                " loop for you, notify TrojanNemo on the C3 forums as that is a different problem to fix", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Tools.EncM(File.ReadAllBytes(ofd.FileName), ofd.FileName, true);
                return;
            }
            if (!Tools.EncM(File.ReadAllBytes(ofd.FileName), ofd.FileName, true))
            {
                MessageBox.Show("Failed to correct the file", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            PatchMogg(ofd.FileName, last_bytes);
            MessageBox.Show("File should be fixed now, if it still doesn't work, notify TrojanNemo on the C3 forums", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static byte[] GetPatchBytes(string mogg)
        {
            using (var fs = File.OpenRead(mogg))
            {
                using (var br = new BinaryReader(fs))
                {
                    br.ReadInt32();
                    var offset = br.ReadInt32();
                    var remainder = (int)((fs.Length - offset) % 8);
                    if (remainder == 0) return null;
                    br.BaseStream.Seek(fs.Length - remainder, SeekOrigin.Begin);
                    var bytes = br.ReadBytes(remainder);
                    fs.Close();
                    br.Close();
                    return bytes;
                }
            }
        }

        private static void PatchMogg(string mogg, byte[] patch)
        {
            using (var fs = File.OpenWrite(mogg))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    bw.BaseStream.Seek(fs.Length - patch.Length, SeekOrigin.Begin);
                    bw.Write(patch);
                }
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