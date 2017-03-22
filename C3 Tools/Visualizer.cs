using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using C3Tools.Properties;
using C3Tools.x360;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.Misc;
using Control = System.Windows.Forms.Control;
using Encoder = System.Drawing.Imaging.Encoder;
using Font = System.Drawing.Font;
using Path = System.IO.Path;
using Point = System.Drawing.Point;

namespace C3Tools
{

    public partial class Visualizer : Form
    {
        private static int intVocals;
        private string AlbumArt;
        private string imgLink;
        public string SCon;
        private Boolean reset;
        private PictureBox pictureFrom;
        private int clicks;
        private readonly Color C3Dark = Color.FromArgb(54, 54, 54);
        private readonly Color C3Light = Color.FromArgb(208, 208, 208);
        private readonly Color ChartRed = Color.FromArgb(255, 0, 0);
        private readonly Color ChartGreen = Color.FromArgb(0, 255, 0);
        private Color songColor;
        private Color artistColor;
        private Color timeColor;
        private Color albumColor;
        private Color yearColor;
        private Color genreColor;
        private bool bRhythm;
        private int mouseX;
        private int mouseY;
        private string author;
        public string origAuthor;
        private Bitmap bitmap;
        private readonly string loadcon = "";
        private Point logoLocation;
        private readonly NemoTools Tools;
        public DTAParser Parser;
        private string Rating;
        private Color RatingColor;
        private bool loading;
        private Decimal song1;
        private Decimal song2;
        private Decimal artist1;
        private Decimal artist2;
        public bool UseOverlay;
        private bool isXOnly;
        public string ThemeName;
        private string ActiveFont;
        private string CustomFontName;
        private bool ProKeysEnabled = true;
        private List<string> C3_Authors;
        private readonly string config;
        private readonly List<string> FilesToDelete;
        private bool PlayDrums;
        private bool PlayBass;
        private bool PlayGuitar;
        private bool PlayKeys;
        private bool PlayVocals;
        private bool PlayCrowd;
        private bool PlayBacking;
        private double PlaybackSeconds;
        private string ImageOut;
        public bool isRunningShortcut;
        private int BassMixer;
        private int BassStream;
        private const int BassBuffer = 1000;
        private const int FadeTime = 3;
        public double VolumeLevel = 12.5;
        private string UserProfile = "";
        private float songX = 272f;
        private float songY = 33f;
        private float artistX = 272f;
        private float artistY = 75f;
        private float yearY = 112f;
        private const float yearX = 518f;
        private const float albumX = 277f;
        private const float albumY = 112f;
        private const float genreX = 278f;
        private float genreY = 130f;
        private const float TimeLeft = 538f;
        private const float TimeTop = 32f;
        private const float ratingX = 518f;
        private const float ratingY = 130f;
        private Image RESOURCE_PRO_BASS;
        private Image RESOURCE_PRO_GUITAR;
        private Image RESOURCE_PRO_KEYS;
        private Image RESOURCE_DIFF_NOPART;
        private Image RESOURCE_DIFF_0;
        private Image RESOURCE_DIFF_1;
        private Image RESOURCE_DIFF_2;
        private Image RESOURCE_DIFF_3;
        private Image RESOURCE_DIFF_4;
        private Image RESOURCE_DIFF_5;
        private Image RESOURCE_DIFF_6;
        private Image RESOURCE_HARM2;
        private Image RESOURCE_HARM3;
        private Image RESOURCE_2X;
        private Image RESOURCE_ALBUM_ART;
        private Image RESOURCE_THEME;
        private Image RESOURCE_AUTHOR_LOGO;
        private Image RESOURCE_ICON1;
        private Image RESOURCE_ICON2;
        private bool ShowC3Logo;

        public Visualizer(Color ButtonBackColor, Color ButtonTextColor, string con)
        {
            InitializeComponent();
            intVocals = 1;
            loadcon = con;
            config = Application.StartupPath + "\\bin\\config\\visualizer.config";
            Tools = new NemoTools();
            Parser = new DTAParser();
            C3_Authors = new List<string>();
            picLogo.AllowDrop = true;
            FilesToDelete = new List<string>();
            var author_logo = Application.StartupPath + "\\author.png";
            if (File.Exists(author_logo))
            {
                GetLogo(author_logo);
            }
            if (Tools.IsAuthorized())
            {
                picAdmin.Image = Resources.logo;
                picAdmin.Cursor = Cursors.Hand;
            }
            var formButtons = new List<Button>{btnLoad,btnClear,btnSave};
            foreach (var button in formButtons)
            {
                button.BackColor = ButtonBackColor;
                button.ForeColor = ButtonTextColor;
                button.FlatAppearance.MouseOverBackColor = button.BackColor == Color.Transparent ? Color.FromArgb(127, Color.AliceBlue.R, Color.AliceBlue.G, Color.AliceBlue.B) : Tools.LightenColor(button.BackColor);
            } 
            loadImages();
        }

        private void loadImages()
        {
            try
            {
                picVisualizer.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\background.png");
                RESOURCE_PRO_BASS = Tools.NemoLoadImage(Application.StartupPath + "\\res\\pBass.png");
                RESOURCE_PRO_GUITAR = Tools.NemoLoadImage(Application.StartupPath + "\\res\\pGuitar.png");
                RESOURCE_PRO_KEYS = Tools.NemoLoadImage(Application.StartupPath + "\\res\\pKeys.png");
                RESOURCE_DIFF_0 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff0.png");
                RESOURCE_DIFF_1 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff1.png");
                RESOURCE_DIFF_2 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff2.png");
                RESOURCE_DIFF_3 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff3.png");
                RESOURCE_DIFF_4 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff4.png");
                RESOURCE_DIFF_5 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff5.png");
                RESOURCE_DIFF_6 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\diff6.png");
                RESOURCE_DIFF_NOPART = Tools.NemoLoadImage(Application.StartupPath + "\\res\\nopart.png");
                RESOURCE_HARM2 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\harm2.png");
                RESOURCE_HARM3 = Tools.NemoLoadImage(Application.StartupPath + "\\res\\harm3.png");
                RESOURCE_2X = Tools.NemoLoadImage(Application.StartupPath + "\\res\\2x.png");
                picMulti.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\multi.png");
                picKaraoke.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\karaoke.png");
                picConvert1.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\convert.png");
                picCAT.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\cat.png");
                picXOnly.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\xonly.png");
                picRB3Ver.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\rb3.png");
                picRBass.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\rbass.png");
                picRKeys.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\rkeys.png");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading one or more of the resource images:\n" + ex.Message, "Visualizer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static bool isFontAvailable(string FontName)
        {
            try
            {
                var fontName = FontName;
                const float fontSize = 12;
                using (var fontTester = new Font(
                        fontName,
                        fontSize,
                        FontStyle.Regular,
                        GraphicsUnit.Pixel))
                {
                    return fontTester.Name == fontName;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void ReadMidi(string midifile)
        {
            if (isXOnly) return;
            if (Tools.DoesMidiHaveEMH(midifile, ProKeysEnabled)) return;
            if ((picIcon1.Image != null && picIcon2.Image != null) || isXOnly || Tools.MIDI_ERROR_MESSAGE.ToLowerInvariant().Contains("could not load midi file")) return;
            sendIcon(picXOnly);
            if (MessageBox.Show("Marked as expert only based on MIDI file\n\nClick OK to see more details, click Cancel to go back",
                    Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            {
                MessageBox.Show(Tools.MIDI_ERROR_MESSAGE, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            isXOnly = true;
        }

        private void AdjustImageParent(Control image, Control parent)
        {
            var pos = PointToScreen(image.Location);
            pos = parent.PointToClient(pos);
            image.Parent = parent;
            image.Location = pos;
            image.BackColor = Color.Transparent;
        }

        private void doImages()
        {
            //the following mess of code is necessary to get the transparency to show the image and not the form
            //separated here so I can just hide this whole bunch of crap

            AdjustImageParent(proGuitar, picVisualizer);
            AdjustImageParent(proBass, picVisualizer);
            AdjustImageParent(pic2x, picVisualizer);
            AdjustImageParent(proKeys, picVisualizer);
            AdjustImageParent(picHarm, picVisualizer);
            AdjustImageParent(diffGuitar, picVisualizer);
            AdjustImageParent(diffBass, picVisualizer);
            AdjustImageParent(diffDrums, picVisualizer);
            AdjustImageParent(diffVocals, picVisualizer);
            AdjustImageParent(diffKeys, picVisualizer);
            AdjustImageParent(picIcon1, picVisualizer);
            AdjustImageParent(picIcon2, picVisualizer);
            AdjustImageParent(picAlbumArt, picVisualizer);
            AdjustImageParent(picWorking, picAlbumArt);
            AdjustImageParent(picLogo, picAlbumArt);

            picSlider.Parent = picLine;
            picSlider.Location = new Point(0,0);

            picWorking.BringToFront();
            picLogo.BringToFront();
            picC3b.BringToFront();
            picC3a.BringToFront();
            logoLocation = picLogo.Location;

            loadDefaults();
        }

        private void loadDefaults()
        {
            reset = true;
            StopPlayback();
            picPlayPause.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\play.png");
            picPlayPause.Tag = "play";
            Height = 648;
            picPlayPause.Cursor = Cursors.WaitCursor;
            picStop.Cursor = Cursors.WaitCursor;
            UpdateTime();
            isXOnly = false;
            loading = false;
            txtSong.Visible = true;
            txtSong1.Visible = false;
            txtSong2.Visible = false;
            txtArtist.Visible = true;
            txtArtist1.Visible = false;
            txtArtist2.Visible = false;
            picIcon1.Image = null;
            picIcon2.Image = null;
            txtSong.Text = null;
            txtSong1.Text = null;
            txtSong2.Text = null;
            txtArtist.Text = null;
            txtArtist1.Text = null;
            txtArtist2.Text = null;
            txtTime.Text = null;
            txtAlbum.Text = null;
            txtYear.Text = null;
            txtYear2.Text = null;
            txtGenre.Text = null;
            txtSubGenre.Text = null;
            chkGenre.Checked = true;
            chkSubGenre.Checked = false;
            SplitJoinArtist.Text = "SPLIT";
            SplitJoinSong.Text = "SPLIT";
            UseOverlay = false;
            diffGuitar.Tag = -1;
            diffBass.Tag = -1;
            diffDrums.Tag = -1;
            diffKeys.Tag = -1;
            diffVocals.Tag = -1;
            proKeys.Tag = 0;
            proGuitar.Tag = 0;
            proBass.Tag = 0;
            pic2x.Tag = 0;
            intVocals = 1;
            toolTip1.SetToolTip(diffGuitar, "No Part");
            toolTip1.SetToolTip(diffBass, "No Part");
            toolTip1.SetToolTip(diffDrums, "No Part");
            toolTip1.SetToolTip(diffVocals, "No Part");
            toolTip1.SetToolTip(diffKeys, "No Part");
            RESOURCE_ALBUM_ART = null;
            if (string.IsNullOrWhiteSpace(UserProfile))
            {
                numFontName.Value = 20;
                numFontArtist.Value = 20;
                numFontTime.Value = 18;
                songColor = C3Dark;
                barSong.BackColor = songColor;
                artistColor = C3Light;
                barArtist.BackColor = artistColor;
                timeColor = C3Dark;
                barTime.BackColor = timeColor;
                albumColor = C3Light;
                barAlbum.BackColor = albumColor;
                yearColor = C3Light;
                barYear.BackColor = yearColor;
                genreColor = C3Light;
                barGenre.BackColor = genreColor;
            }
            else
            {
                loadProfile();
            }
            picVisualizer.Cursor = Cursors.Default;
            Cursor = Cursors.Default;
            songX = 272f;
            songY = 33f;
            yearY = 112f;
            artistX = 272f;
            artistY = 75f;
            genreY = 130f;
            lblTop.Text = "";
            lblBottom.Text = "";
            lblBottom.Cursor = Cursors.Default;
            origAuthor = "";
            picWorking.Visible = false;
            song1 = 0;
            song2 = 0;
            artist1 = 0;
            artist2 = 0;
            cboRating.SelectedIndex = 3;
            bRhythm = false;
            try
            {
                picPlayPause.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\play.png");
                picStop.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\stop.png");
                picLine.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\line.png");
                picSlider.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\slider.png");
                picVolume.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\speaker.png");
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error loading the playback images, make sure you haven't deleted any files", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            picC3a.Visible = false;
            picC3b.Visible = false;
            RESOURCE_ICON1 = null;
            RESOURCE_ICON2 = null;
            reset = false;
            picVisualizer.Invalidate();
        }

        public void getImage(String file)
        {
            if (!Directory.Exists(Application.StartupPath + "\\visualizer\\"))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\visualizer\\");
            }
            var ext = "";
            try
            {
                //if not passed a string path for the image
                //show dialog box to find one
                if (string.IsNullOrWhiteSpace(file))
                {
                    var openFileDialog1 = new OpenFileDialog
                        {
                            Filter = "Image Files|*.bmp;*.tif;*.dds;*.jpg;*.jpeg;*.gif;*.png;*.png_xbox;*.png_ps3;*.png_wii;*.tpl",
                            Title = "Select an album art image",
                            InitialDirectory = Tools.CurrentFolder
                        };
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        AlbumArt = openFileDialog1.FileName;
                        Tools.CurrentFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    //if file is not blank, then use that for the image
                    ext = Path.GetExtension(file.ToLowerInvariant());
                    var exts = new List<string> { ".jpg", ".bmp", ".tif", ".dds", ".gif", ".tpl", ".png", ".jpeg", ".png_xbox", ".png_ps3", ".png_wii" };
                    var isImage = exts.Contains(ext);

                    if (isImage)
                    {
                        AlbumArt = file;
                    }
                    else
                    {
                        return;
                    }
                }
                if (string.IsNullOrWhiteSpace(AlbumArt)) return;
                var newfile = Application.StartupPath + "\\visualizer\\" + Path.GetFileNameWithoutExtension(AlbumArt) + ".bmp";
                switch (ext)
                {
                    case ".dds":
                    case ".png_ps3":
                    case ".png_xbox":
                        if (Tools.ConvertRBImage(AlbumArt, newfile, "bmp"))
                        {
                            RESOURCE_ALBUM_ART = Tools.NemoLoadImage(newfile);
                        }
                        break;
                    case ".tpl":
                    case ".png_wii":
                        if (Tools.ConvertWiiImage(AlbumArt, newfile, "bmp"))
                        {
                            RESOURCE_ALBUM_ART = Tools.NemoLoadImage(newfile);
                        }
                        break;
                    default:
                        RESOURCE_ALBUM_ART = Tools.NemoLoadImage(AlbumArt);
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            picVisualizer.Invalidate();
        }
        
        private void ChangeDifficulty_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var instrument = (Control)sender;
            var currDiff = Tools.GetDiffTag(instrument);
            var popup = new DifficultySelector(Cursor.Position, currDiff);
            popup.ShowDialog();
            instrument.Tag = popup.Difficulty;
            toolTip1.SetToolTip(instrument, popup.Tier);
            popup.Dispose();
            picVisualizer.Invalidate();
        }

        private Image GetDifficultyImage(Control instrument)
        {
           try
            {
                var difficulty = Convert.ToInt16(instrument.Tag);
                switch (difficulty)
                {
                    case 1:
                        return RESOURCE_DIFF_0;
                    case 2:
                        return RESOURCE_DIFF_1;
                    case 3:
                        return RESOURCE_DIFF_2;
                    case 4:
                        return RESOURCE_DIFF_3;
                    case 5:
                        return RESOURCE_DIFF_4;
                    case 6:
                        return RESOURCE_DIFF_5;
                    case 7:
                        return RESOURCE_DIFF_6;
                    default:
                        return RESOURCE_DIFF_NOPART;
                }
            }
            catch (Exception)
            {}
            return RESOURCE_DIFF_NOPART;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            Tools.DeleteFolder(Application.StartupPath + "\\visualizer\\", true);
            SCon = "";
            loadDefaults();
            txtSong.Focus();
            picVisualizer.Invalidate();
        }

        private void HandleDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
        }

        private void Visualizer_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            Tools.CurrentFolder = Path.GetDirectoryName(files[0]);
            
            var ext = Path.GetExtension(files[0]).ToLowerInvariant();
            var exts = new List<string> { ".jpg", ".bmp", ".tif", ".dds", ".gif", ".tpl", ".png", ".jpeg", ".png_xbox", ".png_ps3", ".png_wii" };
            var isImage = exts.Contains(ext);

            if (isImage)
            {
                getImage(files[0]);
            }
            else
            {
                try
                {
                    if (VariousFunctions.ReadFileType(files[0]) == XboxFileType.STFS)
                    {
                        loadDefaults();
                        loadCON(files[0]);
                    }
                    else if (Path.GetExtension(files[0].ToLowerInvariant()) == ".dta")
                    {
                        if (!Parser.ReadDTA(File.ReadAllBytes(files[0])) || !Parser.Songs.Any())
                        {
                            MessageBox.Show("Something went wrong reading that songs.dta file", Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        if (Parser.Songs.Count > 1)
                        {
                            MessageBox.Show("It looks like this is a pack...\nWhat were you expecting me to do with this?\nTry a single song, please",
                                    Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                        loadDefaults();
                        loadDTA();
                    }
                    else
                    {
                        MessageBox.Show("That's not a valid file to drag and drop here\nTry again", Text,MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                                        MessageBoxDefaultButton.Button1, 0, Application.StartupPath + "\\README.txt");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error accessing that file\nThe error says:\n" + ex.Message, Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void loadCON(string con)
        {
            var albumart = "";
            var midi = "";
            Tools.DeleteFolder(Application.StartupPath + "\\visualizer", true);
            Directory.CreateDirectory(Application.StartupPath + "\\visualizer");
            SCon = con;
            loadDefaults();
            picWorking.Visible = true;
            if (con != "")
            {
                if (!Parser.ExtractDTA(con))
                {
                    MessageBox.Show("Something went wrong extracting the songs.dta file", Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    picWorking.Visible = false;
                    return;
                }
                if (!Parser.ReadDTA(Parser.DTA) || !Parser.Songs.Any())
                {
                    MessageBox.Show("Something went wrong reading the songs.dta file", Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    picWorking.Visible = false;
                    return;
                }
                if (Parser.Songs.Count > 1)
                {
                    MessageBox.Show("It looks like this is a pack...\nWhat were you expecting me to do with this?\nTry a single song, please",Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    picWorking.Visible = false;
                    return;
                }
                loadDTA();
                var xPackage = new STFSPackage(con);
                if (!xPackage.ParseSuccess)
                {
                    MessageBox.Show("There was an error parsing that file\nTry again", Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var songname = Parser.Songs[0].InternalName;
                //set file path for later use in saving image
                try
                {
                    var xFile = xPackage.GetFile("songs/" + songname + "/" + songname + ".mid");
                    if (xFile != null)
                    {
                        midi = Application.StartupPath + "\\visualizer\\" + songname + ".mid";
                        Tools.DeleteFile(midi);
                        if (!xFile.ExtractToFile(midi))
                        {
                            midi = "";
                        }
                    }
                    xFile = xPackage.GetFile("songs/" + songname + "/gen/" + songname + "_keep.png_xbox");
                    if (xFile != null)
                    {
                        albumart = Application.StartupPath + "\\visualizer\\" + songname + "_keep.png_xbox";
                        Tools.DeleteFile(albumart);
                        if (!xFile.ExtractToFile(albumart))
                        {
                            albumart = "";
                        }
                    }
                    var HMX_Sources = new List<string>
                    {
                        "rb1","acdc","rb2","rb3","rb1_dlc","rb2_dlc","rb3_dlc","rb4","rb4_dlc",
                        "greenday","gdrb","lego","beatles","tbrb"
                    };
                    //check if the song is a GHtoRB3 or a HMX song
                    if (xPackage.Header.Description.ToLowerInvariant().Contains("ghtorb3") ||
                        xPackage.Header.Description.ToLowerInvariant().Contains("tiny.cc/ghtorb") ||
                        xPackage.Header.Description.ToLowerInvariant().Contains("t=405473"))
                    {
                        origAuthor = "GHtoRB3";
                    }
                    else if (xPackage.Header.Description.ToLowerInvariant().Contains("rockband.com") || HMX_Sources.Contains(Parser.Songs[0].Source))
                    {
                        origAuthor = "Harmonix";
                    }
                    if (xPackage.Header.Description.Contains(xPackage.Header.Title_Display.Replace("\"", "")) &&
                        !xPackage.Header.Description.ToLowerInvariant().Contains("rockband.com") &&
                        !xPackage.Header.Description.ToLowerInvariant().Contains("ghtorb3") &&
                        !xPackage.Header.Description.ToLowerInvariant().Contains("tiny.cc/ghtorb") &&
                        !xPackage.Header.Description.ToLowerInvariant().Contains("t=405473")  &&
                        !xPackage.Header.Description.ToLowerInvariant().Contains("depacked with"))
                    {
                        origAuthor = "Rock Band Network";
                    }
                    xPackage.CloseIO();
                    doShowAuthor();
                    if (lblBottom.Text.Equals("Rock Band Network") || lblBottom.Text.Equals("Harmonix"))
                    {
                        sendIcon(picMulti);
                    }
                    //extract audio file for previewing
                    Height = 744;
                    audioProcessor.RunWorkerAsync();
                    if (albumart != "")
                    {
                        //grab the album art
                        getImage(albumart);
                    }
                    if (midi != "")
                    {
                        ReadMidi(midi);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error:\n" + ex.Message, Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                    try
                    {
                        xPackage.CloseIO();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("There was an error:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            picWorking.Visible = false;
        }

        private void doShowAuthor()
        {
            lblBottom.Cursor = Cursors.Default;
            lblTop.Text = "Author:";
            if (!string.IsNullOrWhiteSpace(origAuthor))
            {
                lblBottom.Text = origAuthor;
            }
            else if (!string.IsNullOrWhiteSpace(author))
            {
                lblBottom.Text = author;
            }
            else
            {
                lblBottom.Text = "Unknown";
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            //reset links in case this is a re-upload
            lblTop.Text = radioImgur.Checked ? "Uploading..." : "Saving...";
            lblBottom.Text = "";
            imgLink = "";
            if (RESOURCE_AUTHOR_LOGO == null)
            {
                picLogo.BorderStyle = BorderStyle.None;
            }
            Application.DoEvents();
            
            //capture image
            using (bitmap = new Bitmap(picVisualizer.Width, picVisualizer.Height))
            {
                string xOut;
                //***OLD METHOD - doesn't scale when using display/font scaling***
                //var location = PointToScreen(picVisualizer.Location);
                //var g = Graphics.FromImage(bitmap);
                //g.CopyFromScreen(location, new Point(0, 0), picVisualizer.Size, CopyPixelOperation.SourceCopy);
                
                //NEW METHOD - seems to scale well?
                picVisualizer.DrawToBitmap(bitmap, picVisualizer.ClientRectangle);

                if (radioLocal.Checked)
                {
                    //prepare to prompt user for save location and extension
                    var fileOutput = new SaveFileDialog
                        {
                            Filter = "JPG Image|*.jpg|PNG Image|*.png|Bitmap Image|*.bmp",
                            Title = "Where should I save the visual to?",
                            FileName = "visual_",
                            InitialDirectory = Application.StartupPath + "\\visualizer\\"
                        };
                    //if sCon has a value, i.e. we're working with a con
                    //prepopulate the fields for the user
                    if (!string.IsNullOrWhiteSpace(SCon))
                    {
                        fileOutput.InitialDirectory = Path.GetDirectoryName(SCon);
                        fileOutput.FileName = Path.GetFileNameWithoutExtension(SCon) + "_visual";
                    }
                    //show dialog, get final file name and extension
                    if (fileOutput.ShowDialog() == DialogResult.OK)
                    {
                        xOut = fileOutput.FileName;
                        Tools.CurrentFolder = Path.GetDirectoryName(xOut);
                    }
                    else
                    {
                        xOut = null;
                    }
                    if (xOut == null)
                    {
                        //re-enable the outlines for the Icon Slots
                        picIcon1.BorderStyle = BorderStyle.FixedSingle;
                        picIcon2.BorderStyle = BorderStyle.FixedSingle;
                        doShowAuthor();
                        return;
                    }
                }
                else
                {
                    xOut = Application.StartupPath + "\\bin\\temp.jpg";
                }
                ImageOut = xOut;
                //this is so image quality is higher than the default
                var myEncoder = Encoder.Quality;
                var myEncoderParameters = new EncoderParameters(1);
                var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                //set encoder based on user choice
                var imgFormat = Path.GetExtension(xOut);
                ImageCodecInfo myImageCodecInfo;
                switch (imgFormat)
                {
                    case "bmp":
                        myImageCodecInfo = Tools.GetEncoderInfo("image/bmp");
                        break;
                    case "png":
                        myImageCodecInfo = Tools.GetEncoderInfo("image/png");
                        break;
                    default:
                        myImageCodecInfo = Tools.GetEncoderInfo("image/jpeg");
                        break;
                }
                //save image
                bitmap.Save(xOut, myImageCodecInfo, myEncoderParameters);
                if (RESOURCE_AUTHOR_LOGO == null)
                {
                    picLogo.BorderStyle = BorderStyle.Fixed3D;
                }
                if (!radioImgur.Checked)
                {
                    if (File.Exists(ImageOut))
                    {
                        lblTop.Text = "Saved successfully";
                        lblBottom.Text = "Click to view";
                        lblBottom.Cursor = Cursors.Hand;
                        imgLink = ImageOut;
                    }
                    else
                    {
                        lblTop.Text = "Saving failed!";
                    }
                    return;
                }
                picWorking.Visible = true;
                Application.DoEvents();
                uploader.RunWorkerAsync();
            }
        }

        private void Visualizer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!picWorking.Visible)
            {
                StopPlayback();
                //let's not leave over any files by mistake
                Tools.DeleteFile(Path.GetTempPath() + "o");
                Tools.DeleteFile(Path.GetTempPath() + "m");
                foreach (var file in FilesToDelete)
                {
                    Tools.DeleteFile(file);
                }
                Tools.DeleteFolder(Application.StartupPath + "\\visualizer\\", true);
                SaveConfig();
                try
                {
                    if (audioProcessor.IsBusy)
                    {
                        audioProcessor.CancelAsync();
                    }
                }
                catch (Exception)
                {}

                if (isRunningShortcut)
                {
                    Environment.Exit(0);
                }
                return;
            }
            MessageBox.Show("Please wait until the current process finishes", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            e.Cancel = true;
        }
        
        private void txtSong_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset || loading) return;
            txtSong.Text = Tools.FixFeaturedArtist(txtSong.Text);
            if (string.IsNullOrWhiteSpace(txtSong.Text)) return;
            CheckSongName(txtSong);
            MeasureSong(txtSong);
            picVisualizer.Invalidate();
        }

        private void MeasureAlbum()
        {
            //create font variable for measuring string size
            var f = new Font(ActiveFont, 11, FontStyle.Bold);
            SizeF name = TextRenderer.MeasureText(txtAlbum.Text.Trim(), f);
            var maxSize = txtYear2.Text.Trim().Length > 0 ? 160 : 200;
            if (name.Width <= maxSize) return;
            if (string.IsNullOrWhiteSpace(txtAlbum.Text.Trim())) return;
            var maxLength = txtYear2.Text.Trim().Length > 0 ? 19 : 24;
            txtAlbum.Text = txtAlbum.Text.Trim().Substring(0, maxLength) + "...";
            txtAlbum.SelectionStart = txtAlbum.Text.Length;
            picVisualizer.Invalidate();
        }

        private void MeasureSong(Control tb)
        {
            var song = tb.Text;

            //create font variable for measuring string size
            var f = new Font(ActiveFont, 20, FontStyle.Bold);

            SizeF name = TextRenderer.MeasureText(song, f);

            //don't want it bigger than point 20
            float fSize;
            if (name.Width > 200) //preset size reserved for the song name
            {
                var factor = 205/name.Width; //size + 5, just because it works well
                fSize = (20*factor);
            }
            else
            {
                fSize = 20;
            }

            if ((Decimal) fSize < numFontName.Minimum)
            {
                fSize = (float) numFontName.Minimum;
            }
            if ((Decimal) fSize > numFontName.Maximum)
            {
                fSize = (float) numFontName.Maximum;
            }

            if (tb == txtSong1 || tb == txtSong2)
            {
                if (((Decimal) fSize > song1 && song1 > 0) || ((Decimal) fSize > song2 && song2 > 0))
                {
                    return;
                }
            }

            if (tb == txtSong1)
            {
                song1 = (Decimal) fSize;
            }
            else if (tb == txtSong2)
            {
                song2 = (Decimal) fSize;
            }
            numFontName.Value = (Decimal)fSize;
            picVisualizer.Invalidate();
            if (numFontName.Value >= 10 || !txtSong.Visible) return;
            SplitJoinSong_Click(null, null);
        }

        private void CheckSongName(TextBoxBase box)
        {
            if (loading) return;
            var message = "";
            var songName = box.Text;

            for (var i = 1; i < 14; i++)
            {
                switch (i)
                {
                    case 1:
                        message = "rhythm version";
                        break;
                    case 2:
                        message = "rhytm version";
                        break;
                    case 3:
                        message = "rythm version";
                        break;
                    case 4:
                        message = "rytm version";
                        break;
                    case 5:
                        message = "2x bass pedal";
                        break;
                    case 6:
                        message = "2x bass";
                        break;
                    case 7:
                        message = "2x pedal";
                        break;
                    case 8:
                        message = "rb3 version";
                        break;
                    case 9:
                        message = "rb3version";
                        break;
                    case 10:
                        message = "rhythm guitar version";
                        break;
                    case 11:
                        message = "rhytm guitar version";
                        break;
                    case 12:
                        message = "rythm guitar version";
                        break;
                    case 13:
                        message = "rytm guitar version";
                        break;
                }

                if (!box.Text.ToLowerInvariant().Contains(message)) continue;
                switch (i)
                {
                    case 13:
                    case 12:
                    case 11:
                    case 10:
                    case 4:
                    case 3:
                    case 2:
                    case 1:
                        if (!bRhythm)
                        {
                            sendIcon(picRKeys);
                            bRhythm = true;
                        }
                        break;
                    case 7:
                    case 6:
                    case 5:
                        pic2x.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\2x.png");
                        break;
                    case 9:
                    case 8:
                        sendIcon(picRB3Ver);
                        break;
                    default:
                        return;
                }
                songName = songName.ToLowerInvariant().Replace(message, "");
                songName = songName.ToLowerInvariant().Replace("()", "").Trim();
                songName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(songName);
                box.Text = songName;
                box.SelectionStart = box.Text.Length;
            }
            picVisualizer.Invalidate();
        }

        private void txtArtist_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset || loading) return;
            txtArtist2.Text = Tools.FixFeaturedArtist(txtArtist2.Text);
            if (txtArtist.Text != "")
            {
                MeasureArtist(txtArtist);
            }
            picVisualizer.Invalidate();
        }

        private void MeasureArtist(Control tb)
        {
            var artist = tb.Text;

            //create font variable for measuring string size
            var f = new Font(ActiveFont, 20, FontStyle.Bold);

            SizeF name = TextRenderer.MeasureText(artist, f);

            //don't want it bigger than point 20
            float fSize;
            if (name.Width > 230) //preset size reserved for the artist name
            {
                var factor = 230/name.Width; //size + 5, just because it works well
                fSize = (20*factor);
            }
            else
            {
                fSize = 20;
            }
            if ((Decimal) fSize < numFontArtist.Minimum)
            {
                fSize = (float) numFontArtist.Minimum;
            }
            if ((Decimal) fSize > numFontArtist.Maximum)
            {
                fSize = (float) numFontArtist.Maximum;
            }

            if (tb == txtArtist1 || tb == txtArtist2)
            {
                if (((Decimal) fSize > artist1 && artist1 > 0) || ((Decimal) fSize > artist2 && artist2 > 0))
                {
                    return;
                }
            }

            if (tb == txtArtist1)
            {
                artist1 = (Decimal) fSize;
            }
            else if (tb == txtArtist2)
            {
                artist2 = (Decimal) fSize;
            }
            numFontArtist.Value = (Decimal)fSize;
            picVisualizer.Invalidate();
            if (numFontArtist.Value >= 10 || !txtArtist.Visible) return;
            SplitJoinArtist_Click(null, null);
        }

        private void txtAlbum_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAlbum.Text))
            {
                genreY = albumY;
                picGenreDown.Visible = true;
                picGenreUp.Visible = true;
            }
            else
            {
                picGenreDown.Visible = false;
                picGenreUp.Visible = false;
            }
            picVisualizer.Invalidate();
            if (reset) return;
            MeasureAlbum();
        }
        
        private void txtYear_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset) return;
            MeasureAlbum();
        }
        
        public void loadDTA()
        {
            PlayDrums = Parser.Songs[0].ChannelsDrums > 0;
            PlayBass = Parser.Songs[0].ChannelsBass > 0;
            PlayGuitar = Parser.Songs[0].ChannelsGuitar > 0;
            PlayKeys = Parser.Songs[0].ChannelsKeys > 0;
            PlayVocals = Parser.Songs[0].ChannelsVocals > 0;
            PlayCrowd = Parser.Songs[0].ChannelsCrowd > 0;
            PlayBacking = Parser.Songs[0].ChannelsBacking() > 0;
            PlaybackSeconds = Parser.Songs[0].PreviewStart == 0 || picPreview.Tag.ToString() == "song" ? 0 : Parser.Songs[0].PreviewStart / 1000;
            lblSongLength.Text = Parser.GetSongDuration(Parser.Songs[0].Length.ToString(CultureInfo.InvariantCulture));
            UpdateTime();
            try
            {
                var folder = Application.StartupPath + "\\res\\play\\";
                picPlayDrums.Image = Tools.NemoLoadImage(folder + (PlayDrums ? "drums" : "nodrums") + ".png");
                picPlayDrums.Enabled = PlayDrums;
                picPlayBass.Image = Tools.NemoLoadImage(folder + (PlayBass ? "bass" : "nobass") + ".png");
                picPlayBass.Enabled = PlayBass;
                picPlayGuitar.Image = Tools.NemoLoadImage(folder + (PlayGuitar ? "guitar" : "noguitar") + ".png");
                picPlayGuitar.Enabled = PlayGuitar;
                picPlayKeys.Image = Tools.NemoLoadImage(folder + (PlayKeys ? "keys" : "nokeys") + ".png");
                picPlayKeys.Enabled = PlayKeys;
                picPlayVocals.Image = Tools.NemoLoadImage(folder + (PlayVocals ? "vocals" : "novocals") + ".png");
                picPlayVocals.Enabled = PlayVocals;
                picPlayCrowd.Image = Tools.NemoLoadImage(folder + (PlayCrowd ? "crowd" : "nocrowd") + ".png");
                picPlayCrowd.Enabled = PlayCrowd;
                picPlayBacking.Image = Tools.NemoLoadImage(folder + (PlayBacking ? "backing" : "nobacking") + ".png");
                picPlayBacking.Enabled = PlayBacking;
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error loading the playback images, make sure you haven't deleted any files", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            try
            {
                origAuthor = "";
                author = "";
                loading = true;
                txtSong.Text = string.IsNullOrWhiteSpace(Parser.Songs[0].OverrideName) ? Parser.Songs[0].Name : Parser.Songs[0].OverrideName;
                txtArtist.Text = Parser.Songs[0].Artist;
                txtAlbum.Text = Parser.Songs[0].Album;
                txtYear.Text = Parser.Songs[0].YearReleased == 0 ? "" : Parser.Songs[0].YearReleased.ToString(CultureInfo.InvariantCulture);
                txtYear2.Text = Parser.Songs[0].YearRecorded == 0 ? "" : Parser.Songs[0].YearRecorded.ToString(CultureInfo.InvariantCulture);
                var vocal_parts = Parser.Songs[0].VocalParts;
                txtTime.Text = Parser.Songs[0].Length == 0 ? "" : Parser.GetSongDuration(Parser.Songs[0].Length.ToString(CultureInfo.InvariantCulture));
                diffDrums.Tag = Parser.Songs[0].DrumsDiff;
                diffBass.Tag = Parser.Songs[0].BassDiff;
                diffGuitar.Tag = Parser.Songs[0].GuitarDiff;
                diffVocals.Tag = Parser.Songs[0].VocalsDiff;
                diffKeys.Tag = Parser.Songs[0].KeysDiff;
                if (Parser.Songs[0].ProBassDiff > 0)
                {
                    diffBass.Tag = Parser.Songs[0].ProBassDiff;
                    proBass.Tag = 1;
                }
                else
                {
                    proBass.Tag = 0;
                }
                if (Parser.Songs[0].ProGuitarDiff > 0)
                {
                    diffGuitar.Tag = Parser.Songs[0].ProGuitarDiff;
                    proGuitar.Tag = 1;
                }
                else
                {
                    proGuitar.Tag = 0;
                }
                if (Parser.Songs[0].ProKeysDiff > 0)
                {
                    diffKeys.Tag = Parser.Songs[0].ProKeysDiff;
                    proKeys.Tag = 1;
                }
                else
                {
                    proKeys.Tag = 0;
                    ProKeysEnabled = false;
                }
                txtGenre.Text = Parser.Songs[0].Genre;
                txtSubGenre.Text = Parser.Songs[0].SubGenre;
                if (txtSubGenre.Text != txtGenre.Text && txtSubGenre.Text != "Other")
                {
                    chkSubGenre.Checked = true;
                }
                try
                {
                    cboRating.SelectedIndex = Parser.Songs[0].Rating - 1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                author = Parser.Songs[0].ChartAuthor;
                if (string.IsNullOrWhiteSpace(author))
                {
                    var HMX_Sources = new List<string>
                    {
                        "rb1","acdc","rb2","rb3","rb1_dlc","rb2_dlc","rb3_dlc","rb4","rb4_dlc",
                        "greenday","gdrb","lego","beatles","tbrb"
                    };
                    if (HMX_Sources.Contains(Parser.Songs[0].Source))
                    {
                        author = "Harmonix";
                    }
                }
                if (Parser.Songs[0].DisableProKeys)
                {
                    proKeys.Image = null;
                }
                if (Parser.Songs[0].RhythmBass)
                {
                    bRhythm = true;
                    sendIcon(picRBass);
                }
                if (Parser.Songs[0].RhythmKeys && !bRhythm)
                {
                    bRhythm = true;
                    sendIcon(picRKeys);
                }
                pic2x.Tag = Parser.Songs[0].DoubleBass ? 1 : 0;
                if (Parser.Songs[0].Karaoke)
                {
                    sendIcon(picKaraoke);
                }
                if (Parser.Songs[0].Multitrack)
                {
                    sendIcon(picMulti);
                }
                if (Parser.Songs[0].Convert)
                {
                    if (picIcon1.Image == null || picIcon2.Image == null)
                    {
                        sendIcon(picConvert1);
                    }
                }
                if (Parser.Songs[0].RB3Version)
                {
                    sendIcon(picRB3Ver);
                }
                if (Parser.Songs[0].CATemh)
                {
                    if ((picIcon1.Image == null || picIcon2.Image == null) && !isXOnly)
                    {
                        sendIcon(picCAT);
                    }
                }
                if (Parser.Songs[0].ExpertOnly)
                {
                    if ((picIcon1.Image == null || picIcon2.Image == null) && !isXOnly)
                    {
                        sendIcon(picXOnly);
                    }
                    isXOnly = true;
                }
                loading = false;
                CheckSongName(txtSong);
                if (txtSong.Text != "")
                {
                    MeasureSong(txtSong);
                }
                MeasureArtist(txtArtist);
                switch (vocal_parts)
                {
                    case 2:
                        intVocals = 2;
                        break;
                    case 3:
                        intVocals = 3;
                        break;
                    default:
                        intVocals = 1;
                        break;
                }
                if (picAdmin.Image != null)
                {
                    LoadC3Authors();
                    var authors = C3_Authors;
                    if (!authors.Any()) //if no updated list is present default to these basics
                    {
                        authors = new List<string>
                        {
                            "farottone","trojannemo","pksage","espher","nyx",
                            "tate","pikmin","ghtorb3","lyoko","lowlander","drummerockband",
                            "bearzunlimited","addymilldike","four neat guys"
                        };
                    }
                    if (authors.Any(auth => lblBottom.Text.ToLowerInvariant().Contains(auth.ToLowerInvariant())))
                    {
                        ShowC3Logo = true;
                    }
                }
                if (string.IsNullOrWhiteSpace(txtAlbum.Text))
                {
                    genreY = albumY;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem Visualizing that file\nError: " + ex.Message, "Visualizer",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            picVisualizer.Invalidate();
        }

        private void LoadC3Authors()
        {
            C3_Authors = new List<string>();
            var authors = Application.StartupPath + "\\bin\\authors";

            if (!File.Exists(authors)) return;

            var sr = new StreamReader(authors);
            while (sr.Peek() >= 0)
            {
                var line = sr.ReadLine();
                if (line.Contains("//")) continue;
                C3_Authors.Add(line);
            }
            sr.Dispose();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
                {
                    Title = "Open song file",
                    InitialDirectory = Tools.CurrentFolder
                };
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            var file = openFileDialog1.FileName;

            try
            {
                var ext = Path.GetExtension(file).ToLowerInvariant();
                var exts = new List<string> { ".jpg", ".bmp", ".tif", ".dds", ".gif", ".tpl", ".png", ".jpeg", ".png_xbox", ".png_ps3", ".png_wii" };
                var isImage = exts.Contains(ext);

                if (isImage)
                {
                    getImage(file);
                }
                else if (VariousFunctions.ReadFileType(file) == XboxFileType.STFS)
                {
                    loadDefaults();
                    loadCON(file);
                    Tools.CurrentFolder = Path.GetDirectoryName(file);
                }
                else if (Path.GetExtension(file.ToLowerInvariant()) == ".dta")
                {
                    if (!Parser.ReadDTA(File.ReadAllBytes(file)) || !Parser.Songs.Any())
                    {
                        MessageBox.Show("Something went wrong reading that songs.dta file", Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    loadDefaults();
                    loadDTA();
                }
                else
                {
                    MessageBox.Show("That's not a song file ... try again.", "What are you doing?!!?",MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error accessing that file\nThe error says:\n" + ex.Message, Text,MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var message = Tools.ReadHelpFile("vi");
            var help = new HelpForm(Text + " - Help", message);
            help.ShowDialog();
        }

        private void sendImage(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            clicks++;
            if (clicks == 2)
            {
                clicks = 0;
                if (pictureFrom == sender)
                {
                    sendIcon(sender);
                    return;
                }
            }
            pictureFrom = ((PictureBox)(sender));
            var image = pictureFrom == picIcon1 ? RESOURCE_ICON1 : (pictureFrom == picIcon2 ? RESOURCE_ICON2 : ((PictureBox)(sender)).Image);
            if (image == null) return;
            DoDragDrop(image, DragDropEffects.Move);
            picVisualizer.Invalidate();
        }

        private void picIcon1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
                e.Effect = DragDropEffects.Move;
        }

        private void picIcon2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
                e.Effect = DragDropEffects.Move;
        }

        private void receiveImage(object sender, DragEventArgs e)
        {
            try
            {
                var icon = (Bitmap) e.Data.GetData(DataFormats.Bitmap);
                if (pictureFrom == picIcon1)
                {
                    RESOURCE_ICON1 = RESOURCE_ICON2;
                }
                else if (pictureFrom == picIcon2)
                {
                    RESOURCE_ICON2 = RESOURCE_ICON1;
                }
                if (sender == picIcon1)
                {
                    RESOURCE_ICON1 = icon;
                }
                else
                {
                    RESOURCE_ICON2 = icon;
                }
                clicks = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            picVisualizer.Invalidate();
        }

        private void sendIcon(object sender)
        {
            var bmp = ((PictureBox) (sender)).Image;
            if (bmp == null) return;
            if (RESOURCE_ICON1 == null)
            {
                RESOURCE_ICON1 = bmp;
            }
            else
            {
                if (RESOURCE_ICON2 != null)
                {
                    RESOURCE_ICON1 = RESOURCE_ICON2;
                }
                RESOURCE_ICON2 = bmp;
            }
            picVisualizer.Invalidate();
        }
        
        private void lblGenre_Click(object sender, EventArgs e)
        {
            chkGenre.Checked = !chkGenre.Checked;
        }

        private void lblSubGenre_Click(object sender, EventArgs e)
        {
            chkSubGenre.Checked = !chkSubGenre.Checked;
        }
        
        private void ChangeColor(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            colorDialog1.Color = ((PictureBox)(sender)).BackColor;
            colorDialog1.CustomColors = new[] { ColorTranslator.ToOle(C3Dark), ColorTranslator.ToOle(C3Light) };
            colorDialog1.SolidColorOnly = true;
            colorDialog1.ShowDialog();
            ((PictureBox)(sender)).BackColor = colorDialog1.Color;
            switch (((PictureBox)(sender)).Name)
            {
                case "barSong":
                    songColor = colorDialog1.Color;
                    break;
                case "barArtist":
                    artistColor = colorDialog1.Color;
                    break;
                case "barTime":
                    timeColor = colorDialog1.Color;
                    break;
                case "barAlbum":
                    albumColor = colorDialog1.Color;
                    break;
                case "barYear":
                    yearColor = colorDialog1.Color;
                    break;
                case "barGenre":
                    genreColor = colorDialog1.Color;
                    break;
            }
            picVisualizer.Invalidate();
        }
        
        private void songJoystick_MouseMove(object sender, MouseEventArgs e)
        {
            if (songJoystick.Cursor != Cursors.NoMove2D) return;
            if (MousePosition.X != mouseX)
            {
                if (MousePosition.X > mouseX)
                {
                    songX = songX + (MousePosition.X - mouseX);
                }
                else if (MousePosition.X < mouseX)
                {
                    songX = songX - (mouseX - MousePosition.X);
                }
                mouseX = MousePosition.X;
            }
            picVisualizer.Invalidate();
            if (MousePosition.Y == mouseY) return;
            if (MousePosition.Y > mouseY)
            {
                songY = songY + (MousePosition.Y - mouseY);
            }
            else if (MousePosition.Y < mouseY)
            {
                songY = songY - (mouseY - MousePosition.Y);
            }
            mouseY = MousePosition.Y;
            picVisualizer.Invalidate();
        }
        
        private void artistJoystick_MouseMove(object sender, MouseEventArgs e)
        {
            if (artistJoystick.Cursor != Cursors.NoMove2D) return;
            if (MousePosition.X != mouseX)
            {
                if (MousePosition.X > mouseX)
                {
                    artistX = artistX + (MousePosition.X - mouseX);
                }
                else if (MousePosition.X < mouseX)
                {
                    artistX = artistX - (mouseX - MousePosition.X);
                }
                mouseX = MousePosition.X;
            }
            picVisualizer.Invalidate();
            if (MousePosition.Y == mouseY) return;
            if (MousePosition.Y > mouseY)
            {
                artistY = artistY + (MousePosition.Y - mouseY);
            }
            else if (MousePosition.Y < mouseY)
            {
                artistY = artistY - (mouseY - MousePosition.Y);
            }
            mouseY = MousePosition.Y;
            picVisualizer.Invalidate();
        }

        private void saveProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog
                {
                    InitialDirectory = Tools.CurrentFolder,
                    Filter = "C3 Profile|*.c3",
                    Title = "Save Visualizer Profile",
                };
            sfd.ShowDialog();
            if (string.IsNullOrWhiteSpace(sfd.FileName)) return;
            UserProfile = sfd.FileName + ".c3";
            if (string.IsNullOrWhiteSpace(UserProfile)) return;
            try
            {
                if (File.Exists(UserProfile))
                {
                    if (MessageBox.Show("A profile with the name " + Path.GetFileName(sfd.FileName) + " already exists\nDo you want to overwrite it?", "File already exists!",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != DialogResult.Yes)
                    {
                        saveProfileToolStripMenuItem.PerformClick();
                        return;
                    }
                }
                var sw = new StreamWriter(UserProfile, false, Encoding.Default);
                sw.WriteLine("//Visualizer Profile");
                sw.WriteLine("//DO NOT EDIT MANUALLY");
                sw.WriteLine("songColor=#" + songColor.R.ToString("X2") + songColor.G.ToString("X2") + songColor.B.ToString("X2"));
                sw.WriteLine("artistColor=#" + artistColor.R.ToString("X2") + artistColor.G.ToString("X2") + artistColor.B.ToString("X2"));
                sw.WriteLine("timeColor=#" + timeColor.R.ToString("X2") + timeColor.G.ToString("X2") + timeColor.B.ToString("X2"));
                sw.WriteLine("albumColor=#" + albumColor.R.ToString("X2") + albumColor.G.ToString("X2") + albumColor.B.ToString("X2"));
                sw.WriteLine("yearColor=#" + yearColor.R.ToString("X2") + yearColor.G.ToString("X2") + yearColor.B.ToString("X2"));
                sw.WriteLine("genreColor=#" + genreColor.R.ToString("X2") + genreColor.G.ToString("X2") + genreColor.B.ToString("X2"));
                sw.WriteLine("songFontSize=" + numFontName.Value);
                sw.WriteLine("artistFontSize=" + numFontArtist.Value);
                sw.WriteLine("timeFontSize=" + numFontTime.Value);
                sw.WriteLine("authorLogoX=" + picLogo.Left);
                sw.WriteLine("authorLogoY=" + picLogo.Top);
                sw.Dispose();
                Tools.CurrentFolder = Path.GetDirectoryName(UserProfile);
                MessageBox.Show("Visualizer Profile saved successfully!", "Success", MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error saving your Visualizer Profile\nThe error I got was:\n\n" + ex.Message,"Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserProfile = "";
            try
            {
                var ofd = new OpenFileDialog
                    {
                        InitialDirectory = Tools.CurrentFolder,
                        Filter = "C3 Profile|*.c3",
                        Title = "Open Visualizer Profile"
                    };
                ofd.ShowDialog();
                if (string.IsNullOrWhiteSpace(ofd.FileName)) return;
                if (string.IsNullOrWhiteSpace(ofd.FileName)) return;
                UserProfile = ofd.FileName;
                loadProfile();
                Tools.CurrentFolder = Path.GetDirectoryName(UserProfile);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error loading the Visualizer Profile\nThe error I got was\n\n" + ex.Message, "Error!",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadProfile()
        {
            if (!File.Exists(UserProfile)) return;
            var sr = new StreamReader(UserProfile, Encoding.Default);
            try
            {
                sr.ReadLine();//skip header
                sr.ReadLine();//skip header
                songColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                artistColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                timeColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                albumColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                yearColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                genreColor = ColorTranslator.FromHtml(Tools.GetConfigString(sr.ReadLine()));
                numFontName.Value = Convert.ToDecimal(Tools.GetConfigString(sr.ReadLine()));
                numFontArtist.Value = Convert.ToDecimal(Tools.GetConfigString(sr.ReadLine()));
                numFontTime.Value = Convert.ToDecimal(Tools.GetConfigString(sr.ReadLine()));
                picLogo.Left = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
                picLogo.Top = Convert.ToInt16(Tools.GetConfigString(sr.ReadLine()));
            }
            catch (Exception)
            {}
            sr.Dispose();
            barSong.BackColor = songColor;
            barArtist.BackColor = artistColor;
            barTime.BackColor = timeColor;
            barYear.BackColor = yearColor;
            barAlbum.BackColor = albumColor;
            barGenre.BackColor = genreColor;
            picVisualizer.Invalidate();
        }

        private void txtSong1_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset || loading) return;
            txtSong1.Text = Tools.FixFeaturedArtist(txtSong1.Text);
            if (string.IsNullOrWhiteSpace(txtSong1.Text))
            {
                txtSong2.Text = "";
                txtSong2.Enabled = false;
            }
            else
            {
                txtSong2.Enabled = true;
            }
            if (txtSong1.Text.Length > txtSong2.Text.Length)
            {
                if (string.IsNullOrWhiteSpace(txtSong1.Text)) return;
                CheckSongName(txtSong1);
                MeasureSong(txtSong1);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtSong2.Text)) return;
                CheckSongName(txtSong2);
                MeasureSong(txtSong2);
            }
            picVisualizer.Invalidate();
        }

        private void txtSong2_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset || loading) return;
            txtSong2.Text = Tools.FixFeaturedArtist(txtSong2.Text);
            if (txtSong2.Text.Length > txtSong1.Text.Length)
            {
                if (string.IsNullOrWhiteSpace(txtSong2.Text)) return;
                CheckSongName(txtSong2);
                MeasureSong(txtSong2);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtSong1.Text)) return;
                CheckSongName(txtSong1);
                MeasureSong(txtSong1);
            }
            picVisualizer.Invalidate();
        }

        private void txtArtist1_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset || loading) return;
            txtArtist1.Text = Tools.FixFeaturedArtist(txtArtist1.Text);
            if (string.IsNullOrWhiteSpace(txtArtist1.Text))
            {
                txtArtist2.Text = "";
                txtArtist2.Enabled = false;
            }
            else
            {
                txtArtist2.Enabled = true;
            }
            if (txtArtist1.Text.Length > txtArtist2.Text.Length && !string.IsNullOrWhiteSpace(txtArtist1.Text))
            {
                MeasureArtist(txtArtist1);
            }
            else if (!string.IsNullOrWhiteSpace(txtArtist2.Text))
            {
                MeasureArtist(txtArtist2);
            }
            picVisualizer.Invalidate();
        }

        private void txtArtist2_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset || loading) return;
            txtArtist2.Text = Tools.FixFeaturedArtist(txtArtist2.Text);
            if (txtArtist2.Text.Length > txtArtist1.Text.Length && !string.IsNullOrWhiteSpace(txtArtist2.Text))
            {
                MeasureArtist(txtArtist2);
            }
            else if (!string.IsNullOrWhiteSpace(txtArtist1.Text))
            {
                MeasureArtist(txtArtist1);
            }
            picVisualizer.Invalidate();
        }

        private void SplitJoinSong_Click(object sender, EventArgs e)
        {
            if (SplitJoinSong.Text == "SPLIT")
            {
                if (string.IsNullOrWhiteSpace(txtSong.Text)) return;
                int divider;
                if (txtSong.Text.Contains("("))
                {
                    divider = txtSong.Text.IndexOf("(", StringComparison.Ordinal) - 1;
                }
                else if (txtSong.Text.Contains("feat"))
                {
                    divider = txtSong.Text.IndexOf("feat", StringComparison.Ordinal) - 1;
                }
                else if (txtSong.Text.Contains("ft."))
                {
                    divider = txtSong.Text.IndexOf("ft.", StringComparison.Ordinal) - 1;
                }
                else
                {
                    divider = txtSong.Text.Length/2;
                    divider = txtSong.Text.IndexOf(" ", divider, StringComparison.Ordinal);
                }
                if (divider <= 0)
                {
                    divider = txtSong.Text.IndexOf(" ", StringComparison.Ordinal);
                }
                if (divider <= 0)
                {
                    MessageBox.Show("What do you want me to split?\nI can't find a space in the song name!",
                                    Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                txtSong1.Visible = true;
                txtSong2.Visible = true;
                txtSong.Visible = false;
                txtSong1.Text = txtSong.Text.Substring(0, divider).Trim();
                txtSong2.Text = txtSong.Text.Substring(divider + 1, txtSong.Text.Length - divider - 1).Trim();
                txtSong.Text = "";
                txtSong2.Focus();
                txtSong2.SelectionStart = txtSong2.Text.Length;
                SplitJoinSong.Text = "JOIN";
            }
            else
            {
                txtSong1.Visible = false;
                txtSong2.Visible = false;
                txtSong.Visible = true;
                txtSong.Text = txtSong1.Text.Trim() + " " + txtSong2.Text.Trim();
                txtSong1.Text = "";
                txtSong2.Text = "";
                song1 = 0;
                song2 = 0;
                txtSong.Focus();
                txtSong.SelectionStart = txtSong.Text.Length;
                SplitJoinSong.Text = "SPLIT";
            }
            picVisualizer.Invalidate();
        }

        private void SplitJoinArtist_Click(object sender, EventArgs e)
        {
            if (SplitJoinArtist.Text == "SPLIT")
            {
                if (string.IsNullOrWhiteSpace(txtArtist.Text)) return;
                int divider;
                if (txtArtist.Text.Contains("("))
                {
                    divider = txtArtist.Text.IndexOf("(", StringComparison.Ordinal) - 1;
                }
                else if (txtArtist.Text.Contains("feat"))
                {
                    divider = txtArtist.Text.IndexOf("feat", StringComparison.Ordinal) - 1;
                }
                else if (txtArtist.Text.Contains("ft."))
                {
                    divider = txtArtist.Text.IndexOf("ft.", StringComparison.Ordinal) - 1;
                }
                else
                {
                    divider = txtArtist.Text.Length/2;
                    divider = txtArtist.Text.IndexOf(" ", divider, StringComparison.Ordinal);
                }
                if (divider <= 0)
                {
                    divider = txtArtist.Text.IndexOf(" ", StringComparison.Ordinal);
                }
                if (divider <= 0)
                {
                    MessageBox.Show("What do you want me to split?\nI can't find a space in the artist/band name!",
                                    Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                txtArtist1.Visible = true;
                txtArtist2.Visible = true;
                txtArtist.Visible = false;
                txtArtist1.Text = txtArtist.Text.Substring(0, divider).Trim();
                txtArtist2.Text = txtArtist.Text.Substring(divider + 1, txtArtist.Text.Length - divider - 1).Trim();
                txtArtist.Text = "";
                txtArtist2.Focus();
                txtArtist2.SelectionStart = txtArtist2.Text.Length;
                SplitJoinArtist.Text = "JOIN";
            }
            else
            {
                txtArtist1.Visible = false;
                txtArtist2.Visible = false;
                txtArtist.Visible = true;
                txtArtist.Text = txtArtist1.Text.Trim() + " " + txtArtist2.Text.Trim();
                txtArtist1.Text = "";
                txtArtist2.Text = "";
                artist1 = 0;
                artist2 = 0;
                txtArtist.Focus();
                txtArtist.SelectionStart = txtArtist.Text.Length;
                SplitJoinArtist.Text = "SPLIT";
            }
            picVisualizer.Invalidate();
        }

        private void SplitJoinSong_TextChanged(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(SplitJoinSong,SplitJoinSong.Text == "SPLIT"? "Click here to split the name of the song into two lines": "Click here to join the two lines into one");
        }

        private void SplitJoinArtist_TextChanged(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(SplitJoinArtist,SplitJoinArtist.Text == "SPLIT"? "Click here to split the artist / band name into two lines": "Click here to join the two lines into one");
        }
        
        private void CheckLoadFonts()
        {
            calibriToolStrip.Enabled = isFontAvailable("Calibri");
            tahomaToolStrip.Enabled = isFontAvailable("Tahoma");
            timesNewRomanToolStrip.Enabled = isFontAvailable("Times New Roman");
            
            if (File.Exists(Application.StartupPath + "\\res\\font.txt"))
            {
                var sr = new StreamReader(Application.StartupPath + "\\res\\font.txt");
                var fontName = Tools.GetConfigString(sr.ReadLine()).Replace("\"","");
                sr.Dispose();

                if (isFontAvailable(fontName))
                {
                    customFontToolStrip.Text = "Custom Font: " + fontName;
                    customFontToolStrip.Visible = true;
                    customFontToolStrip.Checked = true;
                    ActiveFont = fontName;
                    CustomFontName = fontName;
                }
                else
                {
                    MessageBox.Show("Found custom font file font.txt and it's asking for font '" + fontName +
                        "'\nbut Windows is telling me that font is not installed on your system\nPlease make sure the font name is spelled correctly and that it is actually installed on your machine",
                        Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            if (!customFontToolStrip.Checked)
            {
                myriadProToolStrip.Checked = isFontAvailable("Myriad Pro"); //use this as one default
            }
            if (myriadProToolStrip.Checked)
            {
                ActiveFont = "Myriad Pro";
            }
            else if (!customFontToolStrip.Checked)
            {
                if (calibriToolStrip.Enabled)
                {
                    calibriToolStrip.Checked = true;
                    ActiveFont = "Calibri";
                }
                else if (tahomaToolStrip.Enabled)
                {
                    tahomaToolStrip.Checked = true;
                    ActiveFont = "Tahoma";
                }
                else if (timesNewRomanToolStrip.Enabled)
                {
                    timesNewRomanToolStrip.Checked = true;
                    ActiveFont = "Times New Roman";
                }
                else
                {
                    ActiveFont = "Arial";
                }
            }
        }

        private void LoadConfig()
        {
            if (!File.Exists(config)) return;
            var preview = true;
            var loop = true;
            var autoplay = false;
            var sr = new StreamReader(config);
            try
            {
                radioImgur.Checked = sr.ReadLine().Contains("True");
                radioLocal.Checked = sr.ReadLine().Contains("True") && !radioImgur.Checked;
                preview = sr.ReadLine().Contains("True");
                loop = sr.ReadLine().Contains("True");
                autoplay = sr.ReadLine().Contains("True");
                VolumeLevel = Convert.ToDouble(Tools.GetConfigString(sr.ReadLine()));
                sr.ReadLine();//no longer used
                UserProfile = Tools.GetConfigString(sr.ReadLine());
                autoloadLastProfile.Checked = sr.ReadLine().Contains("True");
            }
            catch (Exception)
            {}
            sr.Dispose();
            if (preview)
            {
                picPreview.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\dopreview.png");
            }
            else
            {
                UpdateInfoPreview();
            }
            if (loop)
            {
                picLoop.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\loop.png");
            }
            else
            {
                UpdateInfoLoop();
            }
            if (autoplay)
            {
                picAutoPlay.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\autoplay.png");
            }
            else
            {
                UpdateInfoAutoPlay();
            }
            if (autoloadLastProfile.Checked && !string.IsNullOrWhiteSpace(UserProfile) && File.Exists(UserProfile))
            {
                loadProfile();
            }
        }

        private void SaveConfig()
        {
            var sw = new StreamWriter(config, false);
            sw.WriteLine("SaveToImgur=" + radioImgur.Checked);
            sw.WriteLine("SaveLocally=" + radioLocal.Checked);
            sw.WriteLine("PlayPreviewOnly=" + (picPreview.Tag.ToString() == "preview"));
            sw.WriteLine("LoopPlayback=" + (picLoop.Tag.ToString() == "loop"));
            sw.WriteLine("AutoPlay=" + (picAutoPlay.Tag.ToString() == "autoplay"));
            sw.WriteLine("VolumeLevel=" + VolumeLevel);
            sw.WriteLine("SpectrumID=0");
            sw.WriteLine("LastUsedProfile=" + (autoloadLastProfile.Checked ? UserProfile : ""));
            sw.WriteLine("AutoLoadProfile=" + autoloadLastProfile.Checked);
            sw.Dispose();
        }

        private void Visualizer_Shown(object sender, EventArgs e)
        {
            doImages();
            LoadConfig();
            Application.DoEvents();
            CheckLoadFonts();
            if (loadcon != "" && File.Exists(loadcon))
            {
                try
                {
                    if (VariousFunctions.ReadFileType(loadcon) == XboxFileType.STFS)
                    {
                        loadDefaults();
                        loadCON(loadcon);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There was an error accessing that file\nThe error says:\n" + ex.Message, Text,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            var themes_folder = Application.StartupPath + "\\res\\vis_themes\\";
            switch (DateTime.Now.Month)
            {
                case 1:
                    if (DateTime.Now.Day < 6 && File.Exists(themes_folder + "newyears_button.png"))
                    {
                        ThemeName = "newyears";
                    }
                    break;
                case 2:
                    if (DateTime.Now.Day > 19) return;
                    if (File.Exists(themes_folder + "love_button.png"))
                    {
                        ThemeName = "love";
                    }
                    break;
                case 3:
                    if (DateTime.Now.Day > 22) return;
                    if (File.Exists(themes_folder + "stpaddy_button.png"))
                    {
                        ThemeName = "stpaddy";
                    }
                    break;
                case 5:
                    if (DateTime.Now.Day > 22) return;
                    if (File.Exists(themes_folder + "norway_button.png"))
                    {
                        ThemeName = "norway";
                    }
                    break;
                case 6:
                    if (DateTime.Now.Day > 20 && File.Exists(themes_folder + "freedom_button.png"))
                    {
                        ThemeName = "freedom";
                    }
                    else if (File.Exists(themes_folder + "summer_button.png"))
                    {
                        ThemeName = "summer";
                    }
                    break;
                case 7:
                    if (DateTime.Now.Day > 9 && File.Exists(themes_folder + "summer_button.png"))
                    {
                        ThemeName = "summer";
                    }
                    else if (File.Exists(themes_folder + "freedom_button.png"))
                    {
                        ThemeName = "freedom";
                    }
                    break;
                case 8:
                    if (File.Exists(themes_folder + "freedom_button.png"))
                    {
                        ThemeName = "freedom";
                    }
                    break;
                case 10:
                    if (File.Exists(themes_folder + "spooky_button.png"))
                    {
                        ThemeName = "spooky";
                    }
                    break;
                case 11:
                    if (DateTime.Now.Day < 6 && File.Exists(themes_folder + "spooky_button.png"))
                    {
                        ThemeName = "spooky";
                    }
                    break;
                case 12:
                    if (DateTime.Now.Day < 25 && File.Exists(themes_folder + "xmas_button.png"))
                    {
                        ThemeName = "xmas";
                    }
                    else if (File.Exists(themes_folder + "newyears_button.png"))
                    {
                        ThemeName = "newyears";
                    }
                    break;
            }
            picVisualizer.Invalidate();
            if (string.IsNullOrWhiteSpace(ThemeName)) return;
            try
            {
                picShowTheme.Image = Tools.NemoLoadImage(themes_folder + ThemeName + "_button.png");
                picShowTheme.Visible = true;
                UseOverlay = true;
                picVisualizer.Invalidate();
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading theme button image + " + ThemeName +
                    "_button.png\nMake sure the files are named correctly and the files are in the res\\vis_themes\\ directory", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                ThemeName = "";
            }
        }

        private void GetLogo(string image_path)
        {
            if ((!image_path.Contains(".jpg") && !image_path.Contains(".bmp") && !image_path.Contains(".png") &&
                 !image_path.Contains(".jpeg")) || image_path.Contains(".png_xbox") || image_path.Contains(".png_wii"))
                return;
            picLogo.BorderStyle = BorderStyle.None;
            RESOURCE_AUTHOR_LOGO = Tools.NemoLoadImage(image_path);
            Tools.CurrentFolder = Path.GetDirectoryName(image_path);
            picVisualizer.Invalidate();
        }

        private void picLogo_MouseMove(object sender, MouseEventArgs e)
        {
            if (picLogo.Cursor != Cursors.NoMove2D) return;
            if (MousePosition.X != mouseX)
            {
                if (MousePosition.X > mouseX)
                {
                    picLogo.Left = picLogo.Left + (MousePosition.X - mouseX);
                }
                else if (MousePosition.X < mouseX)
                {
                    picLogo.Left = picLogo.Left - (mouseX - MousePosition.X);
                }
                mouseX = MousePosition.X;
            }
            if (MousePosition.Y != mouseY)
            {
                if (MousePosition.Y > mouseY)
                {
                    picLogo.Top = picLogo.Top + (MousePosition.Y - mouseY);
                }
                else if (MousePosition.Y < mouseY)
                {
                    picLogo.Top = picLogo.Top - (mouseY - MousePosition.Y);
                }
                mouseY = MousePosition.Y;
            }
            if (picLogo.Top < (-1 * picLogo.Height) + 5)
            {
                picLogo.Top = (-1 * picLogo.Height) + 5;
            }
            else if (picLogo.Top > picAlbumArt.Height - 5)
            {
                picLogo.Top = picAlbumArt.Height - 5;
            }
            if (picLogo.Left < (-1 * picLogo.Width) + 5)
            {
                picLogo.Left = (-1 * picLogo.Width) + 5;
            }
            else if (picLogo.Left > picAlbumArt.Width - 5)
            {
                picLogo.Left = picAlbumArt.Width - 5;
            }
            picVisualizer.Invalidate();
        }

        private void clearLogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RESOURCE_AUTHOR_LOGO = null;
            picLogo.BorderStyle = BorderStyle.Fixed3D;
            picLogo.Location = logoLocation;
            picVisualizer.Invalidate();
        }
        
        private void txtYear2_TextChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
            if (reset) return;
            MeasureAlbum();
        }

        private void TextBoxSelectAll(object sender, EventArgs e)
        {
            var tb = (TextBox) sender;
            tb.SelectAll();
        }
        
        private void picLogo_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[]) e.Data.GetData(DataFormats.FileDrop);
            GetLogo(files[0]);
        }

        private void cboRating_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cboRating.SelectedIndex + 1)
            {
                case 1:
                    Rating = "FF";
                    RatingColor = Color.LimeGreen;
                    break;
                case 2:
                    Rating = "SR";
                    RatingColor = Color.Yellow;
                    break;
                case 3:
                    Rating = "M";
                    RatingColor = Color.Red;
                    break;
                case 4:
                    Rating = "";
                    break;
            }
            if (cboRating.Enabled && Rating != "")
            {
                picYearDown.Visible = false;
                picYearUp.Visible = false;
            }
            else
            {
                picYearDown.Visible = true;
                picYearUp.Visible = true;
            }
            picVisualizer.Invalidate();
        }

        private void chkRating_CheckedChanged(object sender, EventArgs e)
        {
            cboRating.Enabled = chkRating.Checked;
            if (chkRating.Checked && Rating != "")
            {
                picYearDown.Visible = false;
                picYearUp.Visible = false;
            }
            else
            {
                picYearDown.Visible = true;
                picYearUp.Visible = true;
            }
            picVisualizer.Invalidate();
        }
        
        private void picImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            getImage("");
        }

        private void picLock_MouseClick(object sender, MouseEventArgs e)
        {
            var selector = new ThemeSelector(this);
            selector.ShowDialog();
        }

        public void UpdateTheme()
        {
            UseOverlay = true;
            picShowTheme.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\vis_themes\\" + ThemeName + "_button.png");
            RESOURCE_THEME = Tools.NemoLoadImage(Application.StartupPath + "\\res\\vis_themes\\" + ThemeName + "_overlay.png");
            picShowTheme.Visible = true;
            picVisualizer.Invalidate();
        }

        private void picSpecial_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            RESOURCE_THEME = Tools.NemoLoadImage(Application.StartupPath + "\\res\\vis_themes\\" + ThemeName + "_overlay.png");
            UseOverlay = !UseOverlay;
            picVisualizer.Invalidate();
        }

        private void picAdmin_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (picAdmin.Image == null)
            {
                if (!Tools.GetPassword()) return;
                if (!Tools.IsAuthorized()) return;
                picAdmin.Image = Resources.logo;
                picAdmin.Cursor = Cursors.Hand;
                picVisualizer.Invalidate();
                return;
            }
            ShowC3Logo = !ShowC3Logo;
            picVisualizer.Invalidate();
        }
        
        private void proGuitar_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            proGuitar.Tag = proGuitar.Tag.ToString() == "1" ? proGuitar.Tag = "0" : proGuitar.Tag = "1";
            picVisualizer.Invalidate();
        }

        private void proBass_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            proBass.Tag = proBass.Tag.ToString() == "1" ? proBass.Tag = "0" : proBass.Tag = "1";
            picVisualizer.Invalidate();
        }

        private void pic2x_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            pic2x.Tag = pic2x.Tag.ToString() == "1" ? pic2x.Tag = "0" : pic2x.Tag = "1";
            picVisualizer.Invalidate();
        }

        private void picHarm_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            switch (intVocals)
            {
                case 1:
                    intVocals = 2;
                    break;
                case 2:
                    intVocals = 3;
                    break;
                default:
                    intVocals = 1;
                    break;
            }
            picVisualizer.Invalidate();
        }

        private void proKeys_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            proKeys.Tag = proKeys.Tag.ToString() == "1" ? proKeys.Tag = "0" : proKeys.Tag = "1";
            picVisualizer.Invalidate();
        }
        
        private void picGenreDown_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            genreY = 130f;
            picVisualizer.Invalidate();
        }

        private void picGenreUp_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (string.IsNullOrWhiteSpace(txtAlbum.Text))
            {
                genreY = albumY;
            }
            picVisualizer.Invalidate();
        }

        private void artistJoystick_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (string.IsNullOrWhiteSpace(txtArtist.Text) && string.IsNullOrWhiteSpace(txtArtist1.Text) && string.IsNullOrWhiteSpace(txtArtist2.Text)) return;
            mouseX = MousePosition.X;
            mouseY = MousePosition.Y;
            if (artistJoystick.Cursor == Cursors.Hand)
            {
                artistJoystick.Image = null;
                artistJoystick.Cursor = Cursors.NoMove2D;
            }
            else if (artistJoystick.Cursor == Cursors.NoMove2D)
            {
                artistJoystick.Cursor = Cursors.Hand;
                artistJoystick.Image = Resources.moveall;
            }
        }

        private void songJoystick_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (string.IsNullOrWhiteSpace(txtSong.Text) && string.IsNullOrWhiteSpace(txtSong1.Text) && string.IsNullOrWhiteSpace(txtSong2.Text)) return;
            mouseX = MousePosition.X;
            mouseY = MousePosition.Y;
            if (songJoystick.Cursor == Cursors.Hand)
            {
                songJoystick.Image = null;
                songJoystick.Cursor = Cursors.NoMove2D;
            }
            else if (songJoystick.Cursor == Cursors.NoMove2D)
            {
                songJoystick.Cursor = Cursors.Hand;
                songJoystick.Image = Resources.moveall;
            }
        }

        private void picYearUp_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            yearY = 112f;
            picVisualizer.Invalidate();
        }

        private void picYearDown_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (string.IsNullOrWhiteSpace(Rating) || !cboRating.Enabled)
            {
                yearY = 130f;
            }
            picVisualizer.Invalidate();
        }

        private void picC3a_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            picC3a.Visible = false;
            picC3b.Visible = false;
            picVisualizer.Invalidate();
        }
        
        private void calibriToolStrip_Click(object sender, EventArgs e)
        {
            ActiveFont = "Calibri";
            myriadProToolStrip.Checked = false;
            calibriToolStrip.Checked = true;
            tahomaToolStrip.Checked = false;
            timesNewRomanToolStrip.Checked = false;
            customFontToolStrip.Checked = false;
            picVisualizer.Invalidate();
        }

        private void tahomaToolStrip_Click(object sender, EventArgs e)
        {
            ActiveFont = "Tahoma";
            myriadProToolStrip.Checked = false;
            calibriToolStrip.Checked = false;
            tahomaToolStrip.Checked = true;
            timesNewRomanToolStrip.Checked = false;
            customFontToolStrip.Checked = false;
            picVisualizer.Invalidate();
        }

        private void timesNewRomanToolStrip_Click(object sender, EventArgs e)
        {
            ActiveFont = "Times New Roman";
            myriadProToolStrip.Checked = false;
            calibriToolStrip.Checked = false;
            tahomaToolStrip.Checked = false;
            timesNewRomanToolStrip.Checked = true;
            customFontToolStrip.Checked = false;
            picVisualizer.Invalidate();
        }

        private void customFontToolStrip_Click(object sender, EventArgs e)
        {
            ActiveFont = CustomFontName;
            myriadProToolStrip.Checked = false;
            calibriToolStrip.Checked = false;
            tahomaToolStrip.Checked = false;
            timesNewRomanToolStrip.Checked = false;
            customFontToolStrip.Checked = true;
            picVisualizer.Invalidate();
        }

        private void myriadProToolStrip_Click(object sender, EventArgs e)
        {
            if (isFontAvailable("Myriad Pro"))
            {
                ActiveFont = "Myriad Pro";
                myriadProToolStrip.Checked = true;
                calibriToolStrip.Checked = false;
                tahomaToolStrip.Checked = false;
                timesNewRomanToolStrip.Checked = false;
                customFontToolStrip.Checked = false;
            }
            else
            {
                if (MessageBox.Show("Myriad Pro Bold is our default font, but Windows is telling me it's not installed on your system\nClick 'OK' to open the /res folder, then install 'c3.otf' and restart C3 CON Tools\nClick 'Cancel' to go back and choose a different font",
                        Text, MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    Process.Start(Application.StartupPath + "\\res\\");
                }
            }
            picVisualizer.Invalidate();
        }

        private void StopPlayback(bool Pause = false)
        {
            try
            {
                PlaybackTimer.Enabled = false;
                if (Pause)
                {
                    if (!Bass.BASS_ChannelPause(BassMixer))
                    {
                        MessageBox.Show("Error pausing playback\n" + Bass.BASS_ErrorGetCode());
                    }
                }
                else
                {
                    StopBASS();
                }
            }
            catch (Exception)
            {}
            picPlayPause.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\play.png");
            picPlayPause.Tag = "play";
        }

        private void StopBASS()
        {
            try
            {
                Bass.BASS_ChannelStop(BassMixer);
                Bass.BASS_StreamFree(BassMixer);
                Bass.BASS_Free();
            }
            catch (Exception)
            { }
            picSpect.Image = null;
        }

        private void SetPlayLocation(double time)
        {
            BassMix.BASS_Mixer_ChannelSetPosition(BassStream, Bass.BASS_ChannelSeconds2Bytes(BassStream, time));
        }

        private string GetStemsToPlay()
        {
            var stems = "";
            if (PlayDrums)
            {
                stems += "drums|";
            }
            if (PlayBass)
            {
                stems += "bass|";
            }
            if (PlayGuitar)
            {
                stems += "guitar|";
            }
            if (PlayVocals)
            {
                stems += "vocals|";
            }
            if (PlayKeys)
            {
                stems += "keys|";
            }
            if (PlayBacking)
            {
                stems += "backing|";
            }
            if (PlayCrowd)
            {
                stems += "crowd";
            }
            return stems;
        }

        private void StartPlayback()
        {
            if (Tools.PlayingSongOggData.Count() == 0)
            {
                MessageBox.Show("Couldn't play back that audio file, sorry", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                StopPlayback();
                return;
            }

            //initialize BASS.NET
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, Handle))
            {
                MessageBox.Show("Error initializing BASS.NET\n" + Bass.BASS_ErrorGetCode());
                return;
            }
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, BassBuffer);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 50);

            // create a decoder for the OGG file
            BassStream = Bass.BASS_StreamCreateFile(Tools.GetOggStreamIntPtr(), 0L, Tools.PlayingSongOggData.Length, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
            var channel_info = Bass.BASS_ChannelGetInfo(BassStream);

            // create a stereo mixer with same frequency rate as the input file
            BassMixer = BassMix.BASS_Mixer_StreamCreate(channel_info.freq, 2, BASSFlag.BASS_MIXER_END);
            BassMix.BASS_Mixer_StreamAddChannel(BassMixer, BassStream, BASSFlag.BASS_MIXER_MATRIX);

            //get and apply channel matrix
            var splitter = new MoggSplitter();
            var matrix = splitter.GetChannelMatrix(Parser.Songs[0], channel_info.chans, GetStemsToPlay());
            BassMix.BASS_Mixer_ChannelSetMatrix(BassStream, matrix);
            
            //apply volume correction to entire track
            SetPlayLocation(PlaybackSeconds);
            var track_vol = (float)Utils.DBToLevel(Convert.ToDouble(-1 * VolumeLevel), 1.0);
            if (picPreview.Tag.ToString() == "preview" && PlaybackSeconds == Parser.Songs[0].PreviewStart/1000) //enable fade-in
            {
                Bass.BASS_ChannelSetAttribute(BassMixer, BASSAttribute.BASS_ATTRIB_VOL, 0);
                Bass.BASS_ChannelSlideAttribute(BassMixer, BASSAttribute.BASS_ATTRIB_VOL, track_vol, FadeTime * 1000);
            }
            else //no fade-in
            {
                Bass.BASS_ChannelSetAttribute(BassMixer, BASSAttribute.BASS_ATTRIB_VOL, track_vol);
            }
            
            //start mix playback
            Bass.BASS_ChannelPlay(BassMixer, false);
            
            PlaybackTimer.Enabled = true;
            picPlayPause.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\pause.png");
            picPlayPause.Tag = "pause";
        }
        
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SCon) || Parser.Songs == null) return;
            var Splitter = new MoggSplitter();
            if (!Splitter.ExtractDecryptMogg(SCon, true, Tools, Parser))
            {
                Tools.PlayingSongOggData = null;
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            //analyze mogg file for length if not present in DTA
            if (Parser.Songs[0].Length == 0 && Tools.PlayingSongOggData != null && Tools.PlayingSongOggData.Count() > 0)
            {
                try
                {
                    Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, Handle);
                    var stream = Bass.BASS_StreamCreateFile(Tools.GetOggStreamIntPtr(), 0L, Tools.PlayingSongOggData.Length, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
                    var len = Bass.BASS_ChannelGetLength(stream);
                    var totaltime = Bass.BASS_ChannelBytes2Seconds(stream, len); // the total time length
                    Parser.Songs[0].Length = (int)(totaltime * 1000);
                    lblSongLength.Text = Parser.GetSongDuration(Parser.Songs[0].Length.ToString(CultureInfo.InvariantCulture));
                    txtTime.Text = lblSongLength.Text;
                    StopBASS();
                }
                catch (Exception)
                { }
            }
            if (Tools.PlayingSongOggData != null && Tools.PlayingSongOggData.Count() > 0)
            {
                picPlayPause.Cursor = Cursors.Hand;
                picStop.Cursor = Cursors.Hand;
                if (picAutoPlay.Tag.ToString() != "autoplay" || audioProcessor.CancellationPending) return;
                PlaybackSeconds = Parser.Songs[0].PreviewStart == 0 || picPreview.Tag.ToString() == "song" ? 0 : Parser.Songs[0].PreviewStart / 1000;
                StartPlayback();
            }
            else
            {
                picPlayPause.Cursor = Cursors.No;
                picStop.Cursor = Cursors.No;
            }
        }

        private void ChangePlaybackInstruments(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var pic = (PictureBox) sender;
            var enabled = true;
            var instrument = pic.Tag.ToString();
            switch (instrument)
            {
                case "drums":
                    PlayDrums = !PlayDrums;
                    enabled = PlayDrums;
                    break;
                case "bass":
                    PlayBass = !PlayBass;
                    enabled = PlayBass;
                    break;
                case "guitar":
                    PlayGuitar = !PlayGuitar;
                    enabled = PlayGuitar;
                    break;
                case "vocals":
                    PlayVocals = !PlayVocals;
                    enabled = PlayVocals;
                    break;
                case "keys":
                    PlayKeys = !PlayKeys;
                    enabled = PlayKeys;
                    break;
                case "crowd":
                    PlayCrowd = !PlayCrowd;
                    enabled = PlayCrowd;
                    break;
                case "backing":
                    PlayBacking = !PlayBacking;
                    enabled = PlayBacking;
                    break;
            }
            var image = Application.StartupPath + "\\res\\play\\" + (enabled ? "" : "no") + instrument + ".png";
            try
            {
                pic.Image = Tools.NemoLoadImage(image);
            }
            catch (Exception)
            {
                MessageBox.Show("There was an error loading the playback image:\n" + image + "\n\nMake sure you haven't deleted any files", Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            toolTip1.SetToolTip(pic, (enabled? "Disable" : "Enable") + " " + pic.Tag + " track");
            if (picPlayPause.Tag.ToString() != "pause") return;
            StopPlayback();
            StartPlayback();
        }
        
        private void picPlayPause_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (picPlayPause.Cursor == Cursors.No)
            {
                MessageBox.Show("Can't play audio for this song, most likely it is encrypted", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (picPlayPause.Cursor == Cursors.WaitCursor)
            {
                MessageBox.Show("Please wait while I extract the audio from the CON file", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!PlayDrums && !PlayBass && !PlayGuitar && !PlayKeys && !PlayVocals && !PlayCrowd && !PlayBacking)
            {
                MessageBox.Show("Enable at least one track to play", Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            try
            {
                switch (Bass.BASS_ChannelIsActive(BassMixer))
                {
                    case BASSActive.BASS_ACTIVE_PLAYING:
                        StopPlayback(true);
                        UpdateTime();
                        break;
                    case BASSActive.BASS_ACTIVE_PAUSED:
                        Bass.BASS_ChannelPlay(BassMixer, false);
                        PlaybackTimer.Enabled = true;
                        picPlayPause.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\pause.png");
                        picPlayPause.Tag = "pause";
                        toolTip1.SetToolTip(picPlayPause, "Pause");
                        break;
                    default:
                        StartPlayback();
                        break;
                }
            }
            catch (Exception)
            {
                StartPlayback();
            }
        }

        private void picStop_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (picPlayPause.Cursor == Cursors.No || picPlayPause.Cursor == Cursors.WaitCursor) return;
            StopPlayback();
            PlaybackSeconds = Parser.Songs[0].PreviewStart == 0 || picPreview.Tag.ToString() == "song" ? 0 : Parser.Songs[0].PreviewStart / 1000;
            UpdateTime();
        }
        
        private void picLoop_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            UpdateInfoLoop();
        }

        private void UpdateInfoLoop()
        {
            if (picLoop.Tag.ToString() == "loop")
            {
                picLoop.Tag = "noloop";
                picLoop.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\loop_off.png");
                toolTip1.SetToolTip(picLoop, "Enable looping of playback");
            }
            else
            {
                picLoop.Tag = "loop";
                picLoop.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\loop.png");
                toolTip1.SetToolTip(picLoop, "Disable looping of playback");
            }
        }

        private void picAutoPlay_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            UpdateInfoAutoPlay();
        }

        private void UpdateInfoAutoPlay()
        {
            if (picAutoPlay.Tag.ToString() == "autoplay")
            {
                picAutoPlay.Tag = "noautoplay";
                picAutoPlay.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\autoplay_off.png");
                toolTip1.SetToolTip(picAutoPlay, "Enable auto-play");
            }
            else
            {
                picAutoPlay.Tag = "autoplay";
                picAutoPlay.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\autoplay.png");
                toolTip1.SetToolTip(picAutoPlay, "Disable auto-play");
            }
        }

        private void picPreview_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            UpdateInfoPreview();
            try
            {
                PlaybackSeconds = Parser.Songs[0].PreviewStart == 0 || picPreview.Tag.ToString() == "song" ? 0 : Parser.Songs[0].PreviewStart / 1000;
                SetPlayLocation(PlaybackSeconds);
                UpdateTime();
            }
            catch (Exception)
            {
                PlaybackSeconds = 0;
            }
        }

        private void UpdateInfoPreview()
        {
            if (picPreview.Tag.ToString() == "preview")
            {
                picPreview.Tag = "song";
                picPreview.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\dosong.png");
                toolTip1.SetToolTip(picPreview, "Click to play in-game preview");
            }
            else
            {
                picPreview.Tag = "preview";
                picPreview.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\dopreview.png");
                toolTip1.SetToolTip(picPreview, "Click to play entire song");
            }
        }

        private void UpdateTime()
        {
            string time;
            if (PlaybackSeconds >= 3600)
            {
                var hours = (int)(PlaybackSeconds / 3600);
                var minutes = (int)(PlaybackSeconds - (hours * 3600));
                var seconds = (int)(PlaybackSeconds - (minutes * 60));
                time = hours + ":" + (minutes < 10 ? "0" : "") + minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
            }
            else if (PlaybackSeconds >= 60)
            {
                var minutes = (int)(PlaybackSeconds / 60);
                var seconds = (int)(PlaybackSeconds - (minutes * 60));
                time = minutes + ":" + (seconds < 10 ? "0" : "") + seconds;
            }
            else
            {
                time = "0:" + (PlaybackSeconds < 10 ? "0" : "") + (int)PlaybackSeconds;
            }
            if (lblTime.InvokeRequired)
            {
                lblTime.Invoke(new MethodInvoker(() => lblTime.Text = time));
            }
            else
            {
                lblTime.Text = time;
            }
            if (picSlider.Cursor == Cursors.NoMoveHoriz || reset) return;
            var percent = PlaybackSeconds / ((double)Parser.Songs[0].Length / 1000);
            if (picSlider.InvokeRequired)
            {
                picSlider.Invoke(new MethodInvoker(() => picSlider.Left = (int)((picLine.Width - picSlider.Width) * percent)));
            }
            else
            {
                picSlider.Left = (int)((picLine.Width - picSlider.Width) * percent);
            }
        }

        private void picSlider_MouseDown(object sender, MouseEventArgs e)
        {
            picSlider.Cursor = Cursors.NoMoveHoriz;
            mouseX = MousePosition.X;
        }

        private void picSlider_MouseUp(object sender, MouseEventArgs e)
        {
            picSlider.Cursor = Cursors.Hand;
            ManualTimeSelection();
        }

        private void ManualTimeSelection()
        {
            picPreview.Tag = "song";
            picPreview.Image = Tools.NemoLoadImage(Application.StartupPath + "\\res\\play\\dosong.png");
            if (Bass.BASS_ChannelIsActive(BassMixer) == BASSActive.BASS_ACTIVE_PAUSED ||
                Bass.BASS_ChannelIsActive(BassMixer) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                SetPlayLocation(PlaybackSeconds);
            }
        }

        private void picSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (picSlider.Cursor != Cursors.NoMoveHoriz) return;
            if (MousePosition.X != mouseX)
            {
                if (MousePosition.X > mouseX)
                {
                    picSlider.Left = picSlider.Left + (MousePosition.X - mouseX);
                }
                else if (MousePosition.X < mouseX)
                {
                    picSlider.Left = picSlider.Left - (mouseX - MousePosition.X);
                }
                mouseX = MousePosition.X;
            }
            if (picSlider.Left < 0)
            {
                picSlider.Left = 0;
            }
            else if (picSlider.Left > picLine.Width - picSlider.Width)
            {
                picSlider.Left = picLine.Width - picSlider.Width;
            }
            PlaybackSeconds = (int)(((double)Parser.Songs[0].Length / 1000) * ((double)picSlider.Left / (picLine.Width - picSlider.Width)));
            SetPlayLocation(PlaybackSeconds);
            UpdateTime();
        }

        private void picLine_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            PlaybackSeconds = (int)(((double)Parser.Songs[0].Length / 1000) * ((double)(e.X - (picSlider.Width / 2)) / (picLine.Width - picSlider.Width)));
            ManualTimeSelection(); 
            UpdateTime();
        }

        private void backgroundWorker2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            imgLink = Tools.UploadToImgur(ImageOut);
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            lblBottom.Cursor = Cursors.Default;
            if (!radioLocal.Checked)
            {
                Tools.DeleteFile(ImageOut);
            }
            picWorking.Visible = false;
            lblTop.Text = "";
            if (string.IsNullOrWhiteSpace(imgLink) && File.Exists(ImageOut))
            {
                lblTop.Text = "Saved successfully";
                lblBottom.Text = "Click to view";
                imgLink = ImageOut;
                lblBottom.Cursor = Cursors.Hand;
                return;
            }
            if (string.IsNullOrWhiteSpace(imgLink) && radioImgur.Checked)
            {
                lblTop.Text = "Uploading failed!";
                lblBottom.Text = "Try again later...";
                return;
            }
            lblTop.Text = "Uploaded successfully";
            lblBottom.Text = "Click to view";
            lblBottom.Cursor = Cursors.Hand;
            Clipboard.SetText(imgLink);
        }

        private void lblRating_Click(object sender, EventArgs e)
        {
            chkRating.Checked = !chkRating.Checked;
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Bass.BASS_ChannelIsActive(BassMixer) == BASSActive.BASS_ACTIVE_PLAYING)
                {
                    // the stream is still playing...
                    var pos = Bass.BASS_ChannelGetPosition(BassStream); // position in bytes
                    PlaybackSeconds = Bass.BASS_ChannelBytes2Seconds(BassStream, pos); // the elapsed time length
                    UpdateTime();
                    DrawSpectrum();
                    if (picPreview.Tag.ToString() != "preview") return;
                    //calculate how many seconds are left to play
                    var time_left = (((double)Parser.Songs[0].PreviewStart+30000) / 1000) - PlaybackSeconds;
                    if ((int)time_left == FadeTime) //start fade-out
                    {
                        Bass.BASS_ChannelSlideAttribute(BassMixer, BASSAttribute.BASS_ATTRIB_VOL, 0, FadeTime * 1000);
                    }
                    if (!(time_left <= 0)) return;
                }
                else
                {
                    PlaybackTimer.Enabled = false;
                }
                StopPlayback();
                PlaybackSeconds = Parser.Songs[0].PreviewStart == 0 || picPreview.Tag.ToString() == "song" ? 0 : Parser.Songs[0].PreviewStart / 1000;
                if (picLoop.Tag.ToString() != "loop") return;
                StartPlayback();
            }
            catch (Exception)
            { }
        }
        
        private readonly Visuals Spectrum = new Visuals(); // visuals class instance
        private void DrawSpectrum()
        {
            var width = picSpect.Width;
            var height = picSpect.Height;
            var spect = Spectrum.CreateSpectrum(BassMixer, width, height, ChartGreen, ChartRed, panelPlay.BackColor, false, false, true);
            picSpect.Image = spect;
        }

        public void UpdateVolume(double volume)
        {
            if (Bass.BASS_ChannelIsActive(BassMixer) != BASSActive.BASS_ACTIVE_PAUSED &&
                Bass.BASS_ChannelIsActive(BassMixer) != BASSActive.BASS_ACTIVE_PLAYING)
            {
                return;
            }
            var track_vol = (float)Utils.DBToLevel(Convert.ToDouble(-1 * volume), 1.0);
            Bass.BASS_ChannelSetAttribute(BassMixer, BASSAttribute.BASS_ATTRIB_VOL, track_vol);
        }

        private void picVolume_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            var Volume = new Volume(this, Cursor.Position);
            Volume.Show();
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

        private void picVisualizer_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                var g = e.Graphics;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                //always align vertically, horizontal alignment varies by field
                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center
                };

                //draw album art
                if (RESOURCE_ALBUM_ART != null)
                {
                    g.DrawImage(RESOURCE_ALBUM_ART, picAlbumArt.Left, picAlbumArt.Top, picAlbumArt.Width, picAlbumArt.Height);
                }

                //draw user logo
                if (RESOURCE_AUTHOR_LOGO != null)
                {
                    g.DrawImage(RESOURCE_AUTHOR_LOGO, picLogo.Left, picLogo.Top, picLogo.Width, picLogo.Height);
                }
                
                //draw song name
                if (!string.IsNullOrWhiteSpace(txtSong.Text))
                {
                    stringFormat.Alignment = StringAlignment.Near;
                    using (var f = new Font(ActiveFont, (float) numFontName.Value, FontStyle.Bold))
                    {
                        g.DrawString(txtSong.Text, f, new SolidBrush(songColor), songX, songY, stringFormat);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(txtSong1.Text))
                {
                    stringFormat.Alignment = StringAlignment.Near;
                    using (var f = new Font(ActiveFont, (float) numFontName.Value, FontStyle.Bold))
                    {
                        g.DrawString(txtSong1.Text, f, new SolidBrush(songColor), songX, songY - 12, stringFormat);
                        g.DrawString(txtSong2.Text, f, new SolidBrush(songColor), songX, songY + 8, stringFormat);
                    }
                }

                //draw song time
                if (!string.IsNullOrWhiteSpace(txtTime.Text))
                {
                    //limit string to 5 characters i.e 1:23:45
                    var time = txtTime.Text.Length > 7 ? txtTime.Text.Substring(0, 6) : txtTime.Text;
                    stringFormat.Alignment = StringAlignment.Far;
                    using (var f = new Font(ActiveFont, (float) numFontTime.Value, FontStyle.Bold))
                    {
                        g.DrawString(time, f, new SolidBrush(timeColor), TimeLeft, TimeTop, stringFormat);
                    }
                }

                //draw artist name
                if (!string.IsNullOrWhiteSpace(txtArtist.Text))
                {
                    stringFormat.Alignment = StringAlignment.Near;
                    using (var f = new Font(ActiveFont, (float) numFontArtist.Value, FontStyle.Bold))
                    {
                        g.DrawString(txtArtist.Text, f, new SolidBrush(artistColor), artistX, artistY, stringFormat);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(txtArtist1.Text))
                {
                    stringFormat.Alignment = StringAlignment.Near;
                    using (var f = new Font(ActiveFont, (float)numFontArtist.Value, FontStyle.Bold))
                    {
                        g.DrawString(txtArtist1.Text, f, new SolidBrush(artistColor), artistX, artistY - 10, stringFormat);
                        g.DrawString(txtArtist2.Text, f, new SolidBrush(artistColor), artistX, artistY + 10, stringFormat);
                    }
                }

                //redraw album name
                if (!string.IsNullOrWhiteSpace(txtAlbum.Text))
                {
                    stringFormat.Alignment = StringAlignment.Near;
                    using (var f = new Font(ActiveFont, 11, FontStyle.Bold))
                    {
                        g.DrawString(txtAlbum.Text, f, new SolidBrush(albumColor), albumX, albumY, stringFormat);
                    }
                }

                var genre = "";
                var ColorforGenre = C3Light;
                if (chkGenre.Checked)
                {
                    genre = chkSubGenre.Checked && !string.IsNullOrWhiteSpace(txtSubGenre.Text) ? txtGenre.Text + " (" + txtSubGenre.Text + ")" : txtGenre.Text;
                    ColorforGenre = genreColor;
                }
                else if (chkSubGenre.Checked)
                {
                    genre = txtSubGenre.Text;
                    ColorforGenre = genreColor;
                }

                //draw genre
                if (!string.IsNullOrWhiteSpace(genre))
                {
                    stringFormat.Alignment = StringAlignment.Near;
                    using (var f = new Font(ActiveFont, 11, FontStyle.Bold))
                    {
                        g.DrawString(genre, f, new SolidBrush(ColorforGenre), genreX, genreY, stringFormat);
                    }
                }

                //draw Rating
                if (!string.IsNullOrWhiteSpace(Rating) && cboRating.Enabled)
                {
                    stringFormat.Alignment = StringAlignment.Far;
                    using (var f = new Font(ActiveFont, 11, FontStyle.Bold))
                    {
                        g.DrawString(Rating, f, new SolidBrush(RatingColor), ratingX, ratingY, stringFormat);
                    }
                }

                //draw year
                if (!string.IsNullOrWhiteSpace(txtYear.Text))
                {
                    stringFormat.Alignment = StringAlignment.Far;
                    //limit year to 4 digits, i.e 2013
                    string year;
                    string year2;
                    var year1 = txtYear.Text.Length > 4 ? txtYear.Text.Substring(0, 3) : txtYear.Text;
                    if (string.IsNullOrWhiteSpace(txtYear2.Text))
                    {
                        year = year1;
                    }
                    else if (txtYear2.Text.Length > 4)
                    {
                        year2 = txtYear2.Text.Substring(0, 3);
                        year = year1 + " (" + year2 + ")";
                    }
                    else
                    {
                        year2 = txtYear2.Text;
                        year = year1 + " (" + year2 + ")";
                    }
                    using (var f = new Font(ActiveFont, 11, FontStyle.Bold))
                    {
                        g.DrawString(year, f, new SolidBrush(yearColor), yearX, !string.IsNullOrWhiteSpace(Rating) && cboRating.Enabled ? 112f : yearY, stringFormat);
                    }
                }
                
                g.DrawImage(GetDifficultyImage(diffGuitar), diffGuitar.Left, diffGuitar.Top, diffGuitar.Width, diffGuitar.Height);
                g.DrawImage(GetDifficultyImage(diffBass), diffBass.Left, diffBass.Top, diffBass.Width, diffBass.Height);
                g.DrawImage(GetDifficultyImage(diffDrums), diffDrums.Left, diffDrums.Top, diffDrums.Width, diffDrums.Height);
                g.DrawImage(GetDifficultyImage(diffKeys), diffKeys.Left, diffKeys.Top, diffKeys.Width, diffKeys.Height);
                g.DrawImage(GetDifficultyImage(diffVocals), diffVocals.Left, diffVocals.Top, diffVocals.Width, diffVocals.Height);
                if (proGuitar.Tag.ToString() == "1")
                {
                    g.DrawImage(RESOURCE_PRO_GUITAR, proGuitar.Left, proGuitar.Top, proGuitar.Width, proGuitar.Height);
                }
                if (proBass.Tag.ToString() == "1")
                {
                    g.DrawImage(RESOURCE_PRO_BASS, proBass.Left, proBass.Top, proBass.Width, proBass.Height);
                }
                if (proKeys.Tag.ToString() == "1")
                {
                    g.DrawImage(RESOURCE_PRO_KEYS, proKeys.Left, proKeys.Top, proKeys.Width, proKeys.Height);
                }
                if (pic2x.Tag.ToString() == "1")
                {
                    g.DrawImage(RESOURCE_2X, pic2x.Left, pic2x.Top, pic2x.Width, pic2x.Height);
                }
                if (intVocals > 1)
                {
                    g.DrawImage(intVocals == 2 ? RESOURCE_HARM2 : RESOURCE_HARM3, picHarm.Left, picHarm.Top, picHarm.Width, picHarm.Height);
                }

                //draw c3 logo
                if (ShowC3Logo && Tools.IsAuthorized())
                {
                    g.DrawImage(Resources.c3a, 0, picVisualizer.Height - Resources.c3a.Height, Resources.c3a.Width, Resources.c3a.Height);
                    g.DrawImage(Resources.c3b, picVisualizer.Width - Resources.c3b.Width, picVisualizer.Height - Resources.c3b.Height, Resources.c3b.Width, Resources.c3b.Height);
                }

                //draw icons
                if (RESOURCE_ICON1 != null)
                {
                    g.DrawImage(RESOURCE_ICON1, picIcon1.Left, picIcon1.Top, picIcon1.Width, picIcon1.Height);
                }
                if (RESOURCE_ICON2 != null)
                {
                    g.DrawImage(RESOURCE_ICON2, picIcon2.Left, picIcon2.Top, picIcon2.Width, picIcon2.Height);
                }

                if (!UseOverlay || string.IsNullOrWhiteSpace(ThemeName) || RESOURCE_THEME == null) return;
                e.Graphics.DrawImage(RESOURCE_THEME, 0, 0, RESOURCE_THEME.Width, RESOURCE_THEME.Height);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error:\n" + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ControlChanged(object sender, EventArgs e)
        {
            picVisualizer.Invalidate();
        }
        
        private void lblBottom_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || lblBottom.Cursor != Cursors.Hand || string.IsNullOrWhiteSpace(imgLink)) return;
            Process.Start(imgLink);
        }

        private void picIcon1_MouseEnter(object sender, EventArgs e)
        {
            var box = (PictureBox) sender;
            var enabled = (sender == picIcon1 && RESOURCE_ICON1 != null) || (sender == picIcon2 && RESOURCE_ICON2 != null);
            toolTip1.SetToolTip(box, enabled ? "Right-click to hide the icons" : "");
            box.Cursor = enabled ? Cursors.Hand : Cursors.Default;
        }

        private void picLogo_MouseUp(object sender, MouseEventArgs e)
        {
            picLogo.Cursor = Cursors.Hand;
            if (RESOURCE_AUTHOR_LOGO != null)
            {
                picVisualizer.Invalidate();
                return;
            }
            var ofd = new OpenFileDialog
            {
                Title = "Select your logo",
                Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png",
                InitialDirectory = Tools.CurrentFolder
            };
            ofd.ShowDialog();
            if (ofd.FileName != "" && File.Exists(ofd.FileName))
            {
                GetLogo(ofd.FileName);
            }
            picVisualizer.Invalidate();
        }

        private void picLogo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || RESOURCE_AUTHOR_LOGO == null) return;
            mouseX = MousePosition.X;
            mouseY = MousePosition.Y;
            picLogo.Cursor = Cursors.NoMove2D;
        }

        private void txtGenre_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtGenre.Text))
            {
                chkGenre.Checked = true;
            }
            ControlChanged(sender, e);
        }

        private void txtSubGenre_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtSubGenre.Text))
            {
                chkSubGenre.Checked = true;
            }
            ControlChanged(sender, e);
        }
        
        private void txtTime_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTime.Text)) return;
            try
            {
                Convert.ToDateTime(txtTime.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("That's not a valid duration value", "Visualizer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtTime.Text = "";
                txtTime.Focus();
            }
        }

        private void toolStripClearIcon_Click(object sender, EventArgs e)
        {
            RESOURCE_ICON1 = null;
            RESOURCE_ICON2 = null;
            picVisualizer.Invalidate();
        }
    }
}
