using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using C3Tools.Properties;
using C3Tools.x360;
using DevComponents.DotNetBar;
using System.Drawing;
using Cursors = System.Windows.Forms.Cursors;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Path = System.IO.Path;
using Point = System.Drawing.Point;

namespace C3Tools
{
    public partial class MainForm : Office2007Form
    {
        public List<string> Files = new List<string>();
        public Form activeForm;
        public bool arguments;
        private readonly NemoTools Tools;
        private bool Dragging;
        private int mouseX;
        private int mouseY;
        private readonly List<Form> activeForms;
        private readonly string config;
        private Button CurrentButton;
        private readonly List<MyButton> FormButtons;
        private const int form_width = 730;
        private const int form_height = 434;
        private string bg_image = "default";
        private Color ActiveBackground;
        private readonly Random Randomizer;
        private bool IncreaseColor;
        private int ActiveRGB;
        public bool isLocked;
        private static Color mMenuBackground;
        private bool showMessage;
        private const int ButtonMinLeft = 2;
        private int ButtonMaxLeft;
        private const int ButtonMinTop = 2;
        private int ButtonMaxTop;
        private const int VerticalJump = 63;
        private const int HorizontalJump = 142;
        private bool MovedButton;
        private bool IsClickingButton;
        private List<int> ButtonColumns;
        private List<int> ButtonRows;
        private readonly Panel HelperLineLeft = new Panel();
        private readonly Panel HelperLineTop = new Panel();
        private readonly Panel lineLeft = new Panel();
        private readonly Panel lineRight = new Panel();
        private readonly Panel lineTop = new Panel();
        private readonly Panel lineBottom = new Panel();
 
        public MainForm()
        {
            CheckForIllegalCrossThreadCalls = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            InitializeComponent();
            Tools = new NemoTools();
            activeForms = new List<Form>();
            ButtonColumns = new List<int>();
            ButtonRows = new List<int>();
            mMenuBackground = contextMenuStrip1.BackColor;
            contextMenuStrip1.Renderer = new DarkRenderer();
            contextMenuStrip2.Renderer = new DarkRenderer();
            contextMenuStrip3.Renderer = new DarkRenderer();
            VariousFunctions.DeleteTempFiles();
            Randomizer = new Random();
            var buttons = new List<Button>
            {
                btnRBtoUSB, btnPackCreator, btnQuickPackEditor, btnQuickDTAEditor, btnCONCreator,
                btnVisualizer, btnMIDICleaner, btnSongAnalyzer, btnAudioAnalyzer, btnSaveFileImageEditor,
                btnSetlistManager, btnBatchExtractor, btnBatchRenamer, btnEventManager, btnFileIndexer,
                btnAdvancedArtConverter, btnCONConverter, btnWiiConverter, btnPS3Converter, btnPhaseShiftConverter,
                btnUpgradeBundler, btnVideoPreparer, btnRestricted1, btnRestricted2, btnSettings
            };
            FormButtons = new List<MyButton>();
            foreach (var mybutton in buttons.Select(button => new MyButton
            {
                Button = button,
                DefaultLocation = button.Location,
            }))
            {
                FormButtons.Add(mybutton);
            }
            HelperLineLeft = new Panel { Parent = this, Visible = false, Width = 1, Height = Height, Top = 0, BackColor = Color.White };
            HelperLineTop = new Panel { Parent = this, Visible = false, Width = Width, Height = 1, Left = 0, BackColor = Color.White };
            lineLeft = new Panel { Name = "lineLeft", Parent = this, Visible = false, Width = 1, Height = 100, BackColor = Color.Black };
            lineRight = new Panel { Name = "lineRight", Parent = this, Visible = false, Width = 1, Height = 100, BackColor = Color.Black };
            lineLeft.BringToFront();
            lineRight.BringToFront();
            lineTop = new Panel { Name = "lineTop", Parent = this, Visible = false, Width = 100, Height = 1, BackColor = Color.Black };
            lineBottom = new Panel { Name = "lineBottom", Parent = this, Visible = false, Width = 100, Height = 1, BackColor = Color.Black };
            lineTop.BringToFront();
            lineBottom.BringToFront();
            try
            {
                var sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\c3\\contools", false);
                sw.WriteLine(Application.StartupPath + "\\c3contools.exe");
                sw.Dispose();
            }
            catch (Exception)
            {}
            config = Application.StartupPath + "\\bin\\config\\main3.config";
            Tools.DeleteFile(Application.StartupPath + "\\bin\\c.exe");
            if (!Directory.Exists(Application.StartupPath + "\\bin\\config\\"))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\bin\\config\\");
            }
            if (!Tools.IsAuthorized()) return;
            btnRestricted1.Text = "Stems Isolator";
            btnRestricted2.Text = "Batch **cryptor";
        }
        
        private void SaveConfig()
        {
            var sw = new StreamWriter(config, false);
            sw.WriteLine("Width=" + Width);
            sw.WriteLine("Height=" + Height);
            if (BackColor != Color.FromName("GradientInactiveCaption"))
            {
                sw.WriteLine("BackColor=#" + GetColorHex(BackColor));
            }
            else
            {
                sw.WriteLine("BackColor=Default");
            }
            sw.WriteLine("BackgroundImage=" + bg_image);
            var i = 0;
            foreach (var button in FormButtons)
            {
                i++;
                sw.WriteLine("Button" + i + "TextColor=#" + GetColorHex(button.Button.ForeColor));
                sw.WriteLine("Button" + i + "BackColor=#" + GetColorHex(button.Button.BackColor));
                sw.WriteLine("Button" + i + "LocX=" + button.Button.Left);
                sw.WriteLine("Button" + i + "LocY=" + button.Button.Top);
                sw.WriteLine("Button" + i + "Visible=" + button.Button.Visible);
            }
            sw.WriteLine("HideBanner=" + !picBanner.Visible);
            sw.WriteLine("TransparentBanner=" + transparentToolStrip.Checked);
            sw.WriteLine("BorderlessForm=" + borderlessForm.Checked);
            sw.WriteLine("TransparentForm=" + transparentFormTool.Checked);
            sw.WriteLine("ActiveColor=" + activeColorToolStripMenuItem.Checked);
            sw.WriteLine("WarnAboutAdminMode=" + administratorModeWarning.Checked);
            sw.Dispose();
        }

        private static string GetColorHex(Color color)
        {
            return color.A.ToString("X2") + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isLocked)
            {
                e.Cancel = true;
                MessageBox.Show("You must unlock the program first", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            SaveConfig();
            VariousFunctions.DeleteTempFiles();
            Tools.DeleteFolder(Application.StartupPath + "\\videoprep_input\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\videoprep_temp\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\wiiprep_input\\");
            Tools.DeleteFolder(Application.StartupPath + "\\visualizer\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\packex\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\input\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\extracted\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\quickpackeditor\\", true);
            Tools.DeleteFolder(Application.StartupPath + "\\temp\\",true);
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            Tools.CurrentFolder = Path.GetDirectoryName(files[0]);
            try
            {
                var counter = 0;
                foreach (var toLoad in files)
                {
                    if (counter == 10)
                    {
                        MessageBox.Show("You already opened 10 other song files.\nTo make sure I don't burn down your computer,\nI'm going to stop loading any more.\nFeel free to load 10 more when you're done\n with the 10 you have open.",
                            "Too many files!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    try
                    {
                        if (VariousFunctions.ReadFileType(toLoad) == XboxFileType.STFS)
                        {
                            var xExplorer = new CONExplorer(btnCONCreator.BackColor, btnCONCreator.ForeColor);
                            xExplorer.LoadCON(toLoad);
                            activeForm = xExplorer;
                            activeForms.Add(activeForm);
                            xExplorer.Show();
                            counter++;
                        }
                        else switch (Path.GetExtension(toLoad.ToLowerInvariant()))
                        {
                            case ".dta":
                                RunQuickPackEditor(toLoad, "");
                                break;
                            case ".mid":
                                RunMIDICleaner(toLoad);
                                break;
                            case ".mogg":
                            case ".ogg":
                            case ".wav":
                                RunAudioAnalyzer(toLoad);
                                break;
                            case ".setlist":
                                var newManager = new SetlistManager(btnSetlistManager.BackColor,btnSetlistManager.ForeColor,toLoad);
                                activeForm = newManager;
                                activeForms.Add(activeForm);
                                newManager.Show();
                                break;
                            case ".png":
                            case ".gif":
                            case ".jpeg":
                            case ".jpg":
                            case ".bmp":
                                bg_image = toLoad;
                                LoadBackground();
                                break;
                            case ".png_xbox":
                            case ".png_ps3":
                            case ".png_wii":
                            case ".dds":
                            case ".tpl":
                                var newAlbum = new AdvancedArtConverter(Path.GetDirectoryName(toLoad),btnAdvancedArtConverter.BackColor,btnAdvancedArtConverter.ForeColor);
                                activeForm = newAlbum;
                                activeForms.Add(activeForm);
                                newAlbum.Show();
                                break;
                            case ".config":
                                if (toLoad.EndsWith("main.config", StringComparison.Ordinal))
                                {
                                    LoadConfig(toLoad);
                                }
                                break;
                            case ".rba":
                                var newRBAConverter = new RBAConverter(btnCONConverter.BackColor, btnCONConverter.ForeColor, Path.GetDirectoryName(toLoad));
                                activeForm = newRBAConverter;
                                activeForms.Add(activeForm);
                                newRBAConverter.Show();
                                break;
                            default:
                                MessageBox.Show("That's not a valid file to drag and drop here ... try again","What are you doing?!!?", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was an error accessing that file\nThe error says:\n" + ex.Message, Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception iSuck)
            {
                MessageBox.Show("There was an error opening that file!\nThe most likely cause: this file is already open!\n\nIn case that's not it, here's the error:\n\n" +
                    iSuck.Message, "Well this is embarrassing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadBackground()
        {
            if (string.IsNullOrWhiteSpace(bg_image) || !File.Exists(bg_image))
            {
                BackgroundImage = bg_image == "default" ? Resources.bg3 : null;
                return;
            }
            timer1.Enabled = false;
            activeColorToolStripMenuItem.Checked = false;
            TransparencyKey = Color.Empty;
            transparentFormTool.Checked = false;
            BackgroundImage = Tools.NemoLoadImage(bg_image);
            ResizeBorderLines();
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }
        
        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var xCreate = new CONCreator(btnCONCreator.BackColor, btnCONCreator.ForeColor);
            activeForm = xCreate;
            activeForms.Add(activeForm);
            xCreate.Show();
        }

        private void btnPackager_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newPackager = new PackCreator(this, btnPackCreator.BackColor, btnPackCreator.ForeColor);
            activeForm = newPackager;
            activeForms.Add(activeForm);
            newPackager.Show();
        }

        private void btnArt_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newAlbum = new AdvancedArtConverter("", btnAdvancedArtConverter.BackColor, btnAdvancedArtConverter.ForeColor);
            activeForm = newAlbum;
            activeForms.Add(activeForm);
            newAlbum.Show();
        }

        private void btnVisualizer_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            RunVisualizer("");
        }

        private void RunVisualizer(string con_file)
        {
            //clear temp folder if it exists
            Tools.DeleteFolder(Application.StartupPath + "\\visualizer\\", true);
            var xVisualizer = new Visualizer(btnVisualizer.BackColor, btnVisualizer.ForeColor, con_file);
            activeForm = xVisualizer;
            activeForms.Add(activeForm);
            xVisualizer.Show();
        }

        private void MainForm_Click(object sender, EventArgs e)
        {
            if (activeForm == null) return;
            activeForm.WindowState = FormWindowState.Normal;
            activeForm.Focus();
        }

        private void LoadConfig(string file = "")
        {
            if (!File.Exists(config) && string.IsNullOrWhiteSpace(file)) return;
            int width;
            int height;
            bool activebgcolor;
            var sr = new StreamReader(string.IsNullOrWhiteSpace(file) ? config : file);
            try
            {
                width = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                height = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                var line = sr.ReadLine();
                if (line.Contains("#"))
                {
                    BackColor = ColorTranslator.FromHtml(Tools.GetConfigString(line));
                }
                bg_image = Tools.GetConfigString(sr.ReadLine());
                foreach (var button in FormButtons)
                {
                    button.Button.ForeColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                    button.Button.BackColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                    button.Button.FlatAppearance.MouseOverBackColor = button.Button.BackColor == Color.Transparent ? Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B) : Tools.LightenColor(button.Button.BackColor);
                    button.Button.Left = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                    button.Button.Top = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                    button.Button.Visible = sr.ReadLine().Contains("True");
                }
                picBanner.Visible = sr.ReadLine().Contains("False"); //hide banner?
                line = sr.ReadLine();
                picBanner.BackColor = line.Contains("True") ? Color.Transparent : Color.Black; //transparent banner?
                transparentToolStrip.Checked = line.Contains("True");
                borderlessForm.Checked = sr.ReadLine().Contains("True"); //borderless form?
                transparentFormTool.Checked = sr.ReadLine().Contains("True"); //transparent form?
                activebgcolor = sr.ReadLine().Contains("True");
                administratorModeWarning.Checked = sr.ReadLine().Contains("True");
            }
            catch (Exception)
            {
                sr.Dispose();
                Tools.DeleteFile(config);
                resetEverythingToolStrip.PerformClick();
                return;
            }
            sr.Dispose();
            ChangeFormBorder(borderlessForm.Checked);
            Height = height;
            Width = width;
            Left = (Screen.PrimaryScreen.WorkingArea.Width - Width) / 2;
            Top = (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2;
            if (activebgcolor)
            {
                activeColorToolStripMenuItem.PerformClick();
                return;
            }
            LoadBackground();
            if (!transparentFormTool.Checked) return;
            BackColor = Color.FromName("GradientInactiveCaption");
            TransparencyKey = BackColor;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            LoadConfig();
            if (administratorModeWarning.Checked && IsAdministratorMode())
            {
                MessageBox.Show("You are running " + Text + " in Administrator Mode!\nIn this mode, Windows disables drag/drop functionality and you will lose " +
                                " many of the features added for your convenience\nIt's recommended you do not run " + Text + " in Administrator Mode in future use", 
                                Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            if (Application.StartupPath.Contains("Program Files"))
            {
                MessageBox.Show("You are running " + Text + " from Program Files!\nThis can cause permission problems and some of the features will not work correctly or at all " +
                    "\nIt's recommended you move " + Text + " to a different folder for future use", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            updater.RunWorkerAsync();
        }
        
        private void btnExtractor_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newExtractor = new BatchExtractor(btnBatchExtractor.BackColor, btnBatchExtractor.ForeColor);
            activeForm = newExtractor;
            activeForms.Add(activeForm);
            newExtractor.Show();
        }

        private void btnVideo_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newVideo = new VideoPreparer(btnVideoPreparer.BackColor, btnVideoPreparer.ForeColor);
            activeForm = newVideo;
            activeForms.Add(activeForm);
            newVideo.Show();
        }
        
        private void btnWiiPrep_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newWiiPrep = new WiiConverter(btnWiiConverter.BackColor, btnWiiConverter.ForeColor, "");
            activeForm = newWiiPrep;
            activeForms.Add(activeForm);
            newWiiPrep.Show();
        }

        private void btnPhaseShift_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newPhaseShiftPrep = new PhaseShiftConverter(btnPhaseShiftConverter.BackColor, btnPhaseShiftConverter.ForeColor);
            activeForm = newPhaseShiftPrep;
            activeForms.Add(activeForm);
            newPhaseShiftPrep.Show();
        }

        private void MainForm_Move(object sender, EventArgs e)
        {
            if (activeForm == null) return;
            try
            {
                Dragging = true;
                activeForm.Left = Left + ((Width - activeForm.Width)/2);
                activeForm.Top = Top + ((Height - activeForm.Height)/2);
                activeForm.Focus();
            }
            catch (Exception)
            {
                activeForm = null;
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (!Dragging) return;
            MainForm_Click(null, null);
            Dragging = false;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            Cursor = Cursors.NoMove2D;
            mouseX = MousePosition.X;
            mouseY = MousePosition.Y;
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Default;
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (Cursor != Cursors.NoMove2D) return;
            if (MousePosition.X != mouseX)
            {
                if (MousePosition.X > mouseX)
                {
                    Left = Left + (MousePosition.X - mouseX);
                }
                else if (MousePosition.X < mouseX)
                {
                    Left = Left - (mouseX - MousePosition.X);
                }
                mouseX = MousePosition.X;
            }
            if (MousePosition.Y == mouseY) return;
            if (MousePosition.Y > mouseY)
            {
                Top = Top + (MousePosition.Y - mouseY);
            }
            else if (MousePosition.Y < mouseY)
            {
                Top = Top - (mouseY - MousePosition.Y);
            }
            mouseY = MousePosition.Y;
        }

        private void btnMIDI_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            RunMIDICleaner("");
        }

        private void RunMIDICleaner(string file)
        {
            var newMIDICleaner = new MIDICleaner(file, btnMIDICleaner.BackColor, btnMIDICleaner.ForeColor);
            activeForm = newMIDICleaner;
            activeForms.Add(activeForm);
            newMIDICleaner.Show();
        }

        private void btnQuickDTA_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            RunQuickDTAEditor("");
        }

        private void RunQuickDTAEditor(string file)
        {
            var newQuickDTA = new QuickDTAEditor(file);
            activeForm = newQuickDTA;
            activeForms.Add(activeForm);
            newQuickDTA.Show();
        }

        private void btnQuickPack_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            RunQuickPackEditor("", "");
        }

        private void RunQuickPackEditor(string dta, string pack)
        {
            var newQuickPack = new QuickPackEditor(this, btnQuickPackEditor.BackColor, btnQuickPackEditor.ForeColor, dta, pack);
            activeForm = newQuickPack;
            activeForms.Add(activeForm);
            newQuickPack.Show();
        }

        private void btnSetlist_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newSetlistManager = new SetlistManager(btnSetlistManager.BackColor, btnSetlistManager.ForeColor, "", this);
            activeForm = newSetlistManager;
            activeForms.Add(activeForm);
            newSetlistManager.Show();
        }

        private void btnPS3_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            if (!File.Exists(Application.StartupPath + "\\bin\\nemoedat.exe"))
            {
                MessageBox.Show("Required file 'nemoedat.exe' is missing!\nDownload it from the C3 CON Tools thread and place it in the \\bin\\ directory, then try to run " +
                                "this tool again", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var newPS3Prep = new PS3Converter(this,  btnPS3Converter.BackColor, btnPS3Converter.ForeColor);
            activeForm = newPS3Prep;
            activeForms.Add(activeForm);
            newPS3Prep.Show();
        }

        private void picBanner_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Process.Start("http://www.customscreators.com");
            }
        }

        private void defaultColor_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            activeColorToolStripMenuItem.Checked = false;
            BackColor = Color.FromName("GradientInactiveCaption");
        }

        private void customColor_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            activeColorToolStripMenuItem.Checked = false;
            BackColor = ColorPicker(BackColor, false);
        }

        private Color ColorPicker(Color initialcolor, bool isBackColor)
        {
            var alpha = isBackColor ? 200 : 255;
            colorDialog1.Color = initialcolor;
            colorDialog1.CustomColors = new[] { ColorTranslator.ToOle(initialcolor), ColorTranslator.ToOle(Color.FromArgb(alpha, 34, 169, 31)), ColorTranslator.ToOle(Color.FromArgb(alpha, 197, 34, 35)), ColorTranslator.ToOle(Color.FromArgb(alpha, 230, 215, 0)), ColorTranslator.ToOle(Color.FromArgb(alpha, 37, 89, 201)), ColorTranslator.ToOle(Color.FromArgb(alpha, 240, 104, 4)) };
            colorDialog1.SolidColorOnly = false;
            colorDialog1.AnyColor = true;
            colorDialog1.FullOpen = true;
            colorDialog1.AllowFullOpen = true;
            return colorDialog1.ShowDialog() == DialogResult.OK ? Color.FromArgb(alpha, colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B) : initialcolor;
        }

        private void customColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentButton.BackColor = ColorPicker(CurrentButton.BackColor, true);
            CurrentButton.FlatAppearance.MouseOverBackColor = Tools.LightenColor(CurrentButton.BackColor);
        }

        private static Control MenuSource(object sender)
        {
            // Retrieve the ContextMenuStrip that called the function
            var owner = sender as ContextMenuStrip;
            // Get the control that is displaying this context menu
            return owner.SourceControl;
        }

        private void customColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentButton.ForeColor = ColorPicker(CurrentButton.ForeColor, false);
        }

        private void defaultColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CurrentButton.ForeColor = Color.White;
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CurrentButton = (Button)MenuSource(sender);
            CurrentButton.BringToFront();
            moveLeft.Enabled = CurrentButton.Left != ButtonMinLeft;
            moveRight.Enabled = CurrentButton.Left != ButtonMaxLeft;
            moveUp.Enabled = CurrentButton.Top != ButtonMinTop;
            moveDown.Enabled = CurrentButton.Top != ButtonMaxTop;
        }

        private void defaultColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var greens = new List<Button> { btnRBtoUSB, btnPackCreator, btnQuickPackEditor, btnQuickDTAEditor, btnCONCreator };
            var reds = new List<Button> { btnVisualizer, btnMIDICleaner, btnSongAnalyzer, btnAudioAnalyzer, btnSaveFileImageEditor };
            var yellows = new List<Button> { btnSetlistManager, btnBatchExtractor, btnBatchRenamer, btnEventManager, btnFileIndexer };
            var blues = new List<Button> { btnAdvancedArtConverter, btnCONConverter, btnWiiConverter, btnPS3Converter, btnPhaseShiftConverter };
            var oranges = new List<Button> { btnUpgradeBundler, btnVideoPreparer, btnRestricted1, btnRestricted2, btnSettings };

            if (greens.Contains(CurrentButton))
            {
                CurrentButton.BackColor = Color.FromArgb(200, 34, 169, 31);
            }
            else if (reds.Contains(CurrentButton))
            {
                CurrentButton.BackColor = Color.FromArgb(200, 197, 34, 35);
            }
            else if (yellows.Contains(CurrentButton))
            {
                CurrentButton.BackColor = Color.FromArgb(200, 230, 215, 0);
            }
            else if (blues.Contains(CurrentButton))
            {
                CurrentButton.BackColor = Color.FromArgb(200, 37, 89, 201);
            }
            else if (oranges.Contains(CurrentButton))
            {
                CurrentButton.BackColor = Color.FromArgb(200, 240, 104, 4);
            }
            CurrentButton.FlatAppearance.MouseOverBackColor = Tools.LightenColor(CurrentButton.BackColor);
        }
        
        private void ChangeFormBorder(bool borderless)
        {
            FormBorderStyle = borderless ? FormBorderStyle.None : FormBorderStyle.Sizable;
            borderlessForm.Checked = borderless;
            ResizeBorderLines();
        }

        private void ResizeBorderLines()
        {
            var visible = FormBorderStyle == FormBorderStyle.None && !transparentFormTool.Checked;
            var lines = new List<Panel> { lineTop, lineBottom, lineLeft, lineRight };
            const int line_size = 4;
            foreach (var line in lines)
            {
                line.Visible = visible;
                switch (line.Name)
                {
                    case "lineBottom":
                    case "lineTop":
                        line.Width = Width;
                        line.Height = line_size;
                        line.Left = 0;
                        break;
                    case "lineLeft":
                    case "lineRight":
                        line.Height = Height;
                        line.Top = 0;
                        line.Width = line_size;
                        break;
                }
            }
            lineBottom.Top = Height - lineBottom.Height;
            lineTop.Top = 0;
            lineLeft.Left = 0;
            lineRight.Left = Width - lineRight.Width;
        }

        private void resetEverythingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            activeColorToolStripMenuItem.Checked = false;
            BackColor = Color.FromName("GradientInactiveCaption");
            BackgroundImage = Resources.bg3;
            bg_image = "default";
            picBanner.Visible = true;
            picBanner.BackColor = Color.Transparent;
            transparentToolStrip.Checked = true;
            ChangeFormBorder(false);
            Width = form_width;
            Height = form_height;
            transparentFormTool.Checked = false;
            TransparencyKey = Color.Empty;
            alwaysOnTop.Checked = false;
            TopMost = false;
            foreach (var button in FormButtons)
            {
                button.Button.Location = button.DefaultLocation;
                button.Button.Visible = true;
                button.Button.ForeColor = Color.White;
                CurrentButton = button.Button;
                defaultColorToolStripMenuItem_Click(sender, e);
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BackgroundImage = null;
            bg_image = "";
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.gif;*.png",
                Title = "Select an image",
                InitialDirectory = Tools.CurrentFolder
            };
            if (ofd.ShowDialog() != DialogResult.OK) return;
            bg_image = ofd.FileName;
            LoadBackground();
        }

        private void hideBannerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picBanner.Visible = false;
            ResizeBorderLines();
        }

        private void allButtonsDefaultColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var button in FormButtons)
            {
                button.Button.ForeColor = Color.White;
            }
        }

        private void allButtonsCustomColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var color = ColorPicker(CurrentButton.ForeColor, false);
            if (color == Color.Empty) return; //user canceled
            foreach (var button in FormButtons)
            {
                button.Button.ForeColor = color;
            }
        }

        private void allButtonsCustomColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var color = ColorPicker(CurrentButton.BackColor, true);
            if (color == Color.Empty) return; //user canceled
            foreach (var button in FormButtons)
            {
                button.Button.BackColor = color;
                button.Button.FlatAppearance.MouseOverBackColor = Tools.LightenColor(button.Button.BackColor);
            }
        }

        private void allButtonsDefaultColorToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            foreach (var button in FormButtons)
            {
                CurrentButton = button.Button;
                defaultColorToolStripMenuItem_Click(sender, e);
                CurrentButton.FlatAppearance.MouseOverBackColor = Tools.LightenColor(CurrentButton.BackColor);
            }
        }

        private void thisButtonTransparencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentButton.BackColor = Color.Transparent;
            CurrentButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B);
        }

        private void allButtonsTransparencyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var button in FormButtons)
            {
                button.Button.BackColor = Color.Transparent;
                button.Button.FlatAppearance.MouseOverBackColor = Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B);
            }
        }

        private void transparentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picBanner.BackColor = transparentToolStrip.Checked ? Color.Transparent : Color.Black;
        }

        private void borderlessFormToolStrip_Click(object sender, EventArgs e)
        {
            ChangeFormBorder(borderlessForm.Checked);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && FormBorderStyle == FormBorderStyle.None)
            {
                Close();
            }
        }

        private void transparentFormTool_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            activeColorToolStripMenuItem.Checked = false;
            BackColor = Color.FromName("GradientInactiveCaption");
            BackgroundImage = null;
            bg_image = "";
            if (transparentFormTool.Checked)
            {
                TransparencyKey = BackColor;
                transparentToolStrip.Enabled = false;
                transparentToolStrip.Checked = false;
                picBanner.BackColor = Color.Black;
            }
            else
            {
                TransparencyKey = Color.Empty;
                transparentToolStrip.Enabled = true;
            }
            ResizeBorderLines();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (borderlessForm.Checked)
            {
                ResizeBorderLines();
            }
            ButtonMaxLeft = Width - 2 - btnVisualizer.Width;
            ButtonMaxTop = Height - 2 - btnVisualizer.Height;
            HelperLineLeft.Height = Height;
            HelperLineTop.Width = Width;
        }
        
        private void contextMenuStrip2_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            showBanner.Visible = !picBanner.Visible;
            checkForUpdates.Visible = !updater.IsBusy;
            changeBackgroundColor.Enabled = !transparentFormTool.Checked && BackgroundImage == null;
            clearToolStripMenuItem.Enabled = BackgroundImage != null;
        }

        private void showBanner_Click(object sender, EventArgs e)
        {
            picBanner.Visible = true;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        private void btnSaveFileImageViewer_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newViewer = new SaveFileImageEditor(btnSaveFileImageEditor.BackColor, btnSaveFileImageEditor.ForeColor);
            activeForm = newViewer;
            activeForms.Add(activeForm);
            newViewer.Show();
        }

        private void btnRBAConverter_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newRBAConverter = new RBAConverter(btnCONConverter.BackColor, btnCONConverter.ForeColor);
            activeForm = newRBAConverter;
            activeForms.Add(activeForm);
            newRBAConverter.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var colors = new List<int> {ActiveBackground.R, ActiveBackground.G, ActiveBackground.B};
            const int add = 5;
            const int max = 255;
            const int min = 0;
            if (IncreaseColor)
            {
                if (colors[ActiveRGB] < max)
                {
                    colors[ActiveRGB] += add;
                }
                else
                {
                    IncreaseColor = Randomizer.Next(0, 9999) % 2 != 0;
                    ActiveRGB = Randomizer.Next(0, colors.Count);
                    return;
                }
            }
            else
            {
                if (colors[ActiveRGB] > min)
                {
                    colors[ActiveRGB] -= add;
                }
                else
                {
                    IncreaseColor = Randomizer.Next(0, 9999) % 2 != 0;
                    ActiveRGB = Randomizer.Next(0, colors.Count);
                    return;
                }
            }
            for (var c = 0; c < colors.Count; c++)
            {
                if (colors[c] < min)
                {
                    colors[c] = min;
                }
                else if (colors[c] > max)
                {
                    colors[c] = max;
                }
            }
            ActiveBackground = Color.FromArgb(colors[0], colors[1], colors[2]);
            BackColor = ActiveBackground;
        }

        private void activeColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveBackground = transparentFormTool.Checked ? Color.White : BackColor;
            timer1.Enabled = true;
        }
        
        private void btnAnalyzer_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            RunSongAnalyzer("");
        }

        private void RunSongAnalyzer(string file)
        {
            var newAnalyzer = new SongAnalyzer(file);
            activeForm = newAnalyzer;
            activeForms.Add(activeForm);
            newAnalyzer.Show();
        }

        private void RunAudioAnalyzer(string file)
        {
            var analyzer = new AudioAnalyzer(file);
            activeForm = analyzer;
            activeForms.Add(activeForm);
            analyzer.Show();
        }

        private void btnIndexer_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newIndexer = new FileIndexer(btnFileIndexer.BackColor, btnFileIndexer.ForeColor);
            activeForm = newIndexer;
            activeForms.Add(activeForm);
            newIndexer.Show();
        }

        private void btnEvent_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newEventManager = new EventManager(this);
            activeForm = newEventManager;
            activeForms.Add(activeForm);
            newEventManager.Show();
        }

        private void btnWii_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].ToLowerInvariant().EndsWith(".bin", StringComparison.Ordinal))
            {
                Tools.CurrentFolder = Path.GetDirectoryName(files[0]);
                Tools.ProcessBINFile(files[0], Text);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }

        private void btnVisualizer_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS)
            {
                RunVisualizer(files[0]);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }

        private void btnCleaner_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].ToLowerInvariant().EndsWith(".mid", StringComparison.Ordinal) || (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS))
            {
                RunMIDICleaner(files[0]);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }

        private void btnAnalyzer_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].ToLowerInvariant().EndsWith(".mid", StringComparison.Ordinal) || (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS))
            {
                RunSongAnalyzer(files[0]);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }

        private void btnQuickPack_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files[0].ToLowerInvariant().EndsWith(".dta", StringComparison.Ordinal))
            {
                RunQuickPackEditor(files[0], "");
            }
            else if (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS)
            {
                RunQuickPackEditor("", files[0]);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }

        private void btnQuickDTA_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS)
            {
                RunQuickDTAEditor(files[0]);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }
        
        private static string GetAppVersion()
        {
            var vers = Assembly.GetExecutingAssembly().GetName().Version;
            return "v" + String.Format("{0}.{1}.{2}", vers.Major, vers.Minor, vers.Build);
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
        
        private void btnUSB_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newUSB = new USBnator();
            activeForm = newUSB;
            activeForms.Add(activeForm);
            newUSB.Show();
        }
        
        private void updater_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var path = Application.StartupPath + "\\bin\\update.txt";
            Tools.DeleteFile(path);
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile("http://www.keepitfishy.com/rb3/c3contools/update.txt", path);
                }
                catch (Exception)
                {}
            }
        }

        private void updater_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var path = Application.StartupPath + "\\bin\\update.txt";
            if (!File.Exists(path))
            {
                if (showMessage)
                {
                    MessageBox.Show("Unable to check for updates, try again later", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                return;
            }
            var thisVersion = GetAppVersion();
            var newVersion = "v";
            string appName;
            string releaseDate;
            string link;
            var changeLog = new List<string>();
            var sr = new StreamReader(path);
            try
            {
                var line = sr.ReadLine();
                if (line.ToLowerInvariant().Contains("html"))
                {
                    if (showMessage)
                    {
                        MessageBox.Show("Unable to check for updates, try again later", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    sr.Dispose();
                    return;
                }
                appName = Tools.GetConfigString(line);
                newVersion += Tools.GetConfigString(sr.ReadLine());
                releaseDate = Tools.GetConfigString(sr.ReadLine());
                link = Tools.GetConfigString(sr.ReadLine());
                sr.ReadLine();//ignore Change Log header
                while (sr.Peek() >= 0)
                {
                    changeLog.Add(sr.ReadLine());
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show("Error parsing update file:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                sr.Dispose();
                return;
            }
            sr.Dispose();
            Tools.DeleteFile(path);
            if (thisVersion.Equals(newVersion))
            {
                if (showMessage)
                {
                    MessageBox.Show("You have the latest version", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            var newInt = Convert.ToInt16(newVersion.Replace("v", "").Replace(".", "").Trim());
            var thisInt = Convert.ToInt16(thisVersion.Replace("v", "").Replace(".", "").Trim());
            if (newInt <= thisInt)
            {
                if (showMessage)
                {
                    MessageBox.Show("You have a newer version (" + thisVersion + ") than what's on the server (" + newVersion + ")\nNo update needed!", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
            var updaterForm = new Updater();
            updaterForm.SetInfo(Text, thisVersion, appName, newVersion, releaseDate, link, changeLog);
            updaterForm.ShowDialog();
        }
        
        private void alwaysOnTop_Click(object sender, EventArgs e)
        {
            TopMost = alwaysOnTop.Checked;
        }

        private void btnAudio_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var exts = new List<string> {".mogg", ".wav", ".ogg"};
            if (exts.Contains(Path.GetExtension(files[0].ToLowerInvariant())) || VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS)
            {
                RunAudioAnalyzer(files[0]);
            }
            else
            {
                MainForm_DragDrop(sender, e);
            }
        }

        private void btnAudio_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            RunAudioAnalyzer("");
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            contextMenuStrip2.Show(Cursor.Position.X, Cursor.Position.Y);
        }

        private void resetVisibility_Click(object sender, EventArgs e)
        {
            foreach (var button in FormButtons)
            {
                button.Button.Visible = true;
            }
        }

        private void resetLocations_Click(object sender, EventArgs e)
        {
            foreach (var button in FormButtons)
            {
                button.Button.Location = button.DefaultLocation;
            }
        }

        private void btnBatchRenamer_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var newRenamer = new BatchRenamer(btnBatchRenamer.BackColor, btnBatchRenamer.ForeColor);
            activeForm = newRenamer;
            activeForms.Add(activeForm);
            newRenamer.Show();
        }

        private void btnUpgradeBundler_Click(object sender, EventArgs e)
        {
            if (MovedButton) return;
            var bundler = new ProUpgradeBundler(btnUpgradeBundler.BackColor, btnUpgradeBundler.ForeColor);
            activeForm = bundler;
            activeForms.Add(activeForm);
            bundler.Show();
        }

        private void hideButton_Click(object sender, EventArgs e)
        {
            CurrentButton.Visible = false;
        }

        private void moveUp_Click(object sender, EventArgs e)
        {
            CurrentButton.Top -= VerticalJump;
        }

        private void moveDown_Click(object sender, EventArgs e)
        {
            CurrentButton.Top += VerticalJump;
        }

        private void moveLeft_Click(object sender, EventArgs e)
        {
            CurrentButton.Left -= HorizontalJump;
        }

        private void moveRight_Click(object sender, EventArgs e)
        {
            CurrentButton.Left += HorizontalJump;
        }
        
        private void Buttons_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            IsClickingButton = true;
            MovedButton = false;
            CurrentButton = (Button)sender;
            timer2.Enabled = true;
        }

        private void Buttons_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            timer2.Enabled = false;
            IsClickingButton = false;
            CurrentButton = (Button)sender;
            CurrentButton.Cursor = Cursors.Hand;
            CurrentButton = null;
            HelperLineLeft.Visible = false;
            HelperLineTop.Visible = false;
        }
        
        private void Buttons_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (CurrentButton == null) return;
            if (CurrentButton.Cursor != Cursors.NoMove2D) return;
            CurrentButton.Left = PointToClient(MousePosition).X - (CurrentButton.Width / 2);
            CurrentButton.Top = PointToClient(MousePosition).Y - (CurrentButton.Height / 2);
            MovedButton = true;
            HelperLineLeft.Left = CurrentButton.Left;
            HelperLineTop.Top = CurrentButton.Top;
            HelperLineLeft.Visible = ButtonColumns.Contains(CurrentButton.Left);
            HelperLineTop.Visible = ButtonRows.Contains(CurrentButton.Top);
            mouseX = MousePosition.X;
            mouseY = MousePosition.Y;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;
            if (!IsClickingButton)
            {
                MovedButton = false;
                return;
            }
            CurrentButton.Cursor = Cursors.NoMove2D;
            MovedButton = true;
            CurrentButton.BringToFront();
            HelperLineLeft.BringToFront();
            HelperLineTop.BringToFront();
            mouseX = MousePosition.X;
            mouseY = MousePosition.Y;
            IsClickingButton = false;
            ButtonColumns = new List<int>();
            ButtonRows = new List<int>();
            foreach (var button in FormButtons.Where(button => button.Button != CurrentButton))
            {
                if (!ButtonColumns.Contains(button.Button.Left))
                {
                    ButtonColumns.Add(button.Button.Left);
                }
                if (!ButtonRows.Contains(button.Button.Top))
                {
                    ButtonRows.Add(button.Button.Top);
                }
            }
        }

        public static bool IsAdministratorMode()
        {
            try
            {
                return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void aboutTool_Click(object sender, EventArgs e)
        {
            var version = GetAppVersion();
            var message = Tools.ReadHelpFile("about");
            MessageBox.Show(Text + " " + version + "\n" + message, "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void viewChangeLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var readme = Application.StartupPath + "\\c3contools_changelog.txt";
            if (!File.Exists(readme))
            {
                MessageBox.Show("Change log is missing - don't delete any files that come with this program!", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            Process.Start(readme);
        }

        private void checkForUpdates_Click(object sender, EventArgs e)
        {
            showMessage = true;
            updater.RunWorkerAsync();
        }

        private void enterPassword_Click(object sender, EventArgs e)
        {
            //REDACTED BY TROJANNEMO
        }

        private void c3Forums_Click(object sender, EventArgs e)
        {
            Process.Start("http://customscreators.com/index.php?/topic/9095-c3-con-tools-v398-073116/");
        }  
    }

    public class MyButton
    {
        public Button Button { get; set; }
        public Point DefaultLocation { get; set; }
    }
}