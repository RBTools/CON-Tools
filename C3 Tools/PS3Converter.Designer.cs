namespace C3Tools
{
    partial class PS3Converter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PS3Converter));
            this.lstLog = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportLogFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkMerge = new System.Windows.Forms.CheckBox();
            this.chkSongID = new System.Windows.Forms.CheckBox();
            this.btnBegin = new System.Windows.Forms.Button();
            this.picPin = new System.Windows.Forms.PictureBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mergeSongsToolStrip = new System.Windows.Forms.ToolStripMenuItem();
            this.managePackDTAFile = new System.Windows.Forms.ToolStripMenuItem();
            this.numericIDOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchChangeIDsToNumeric = new System.Windows.Forms.ToolStripMenuItem();
            this.changeIDPrefix = new System.Windows.Forms.ToolStripMenuItem();
            this.changeAuthorID = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSongNumber = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mIDIToEDATEncryptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.type1 = new System.Windows.Forms.ToolStripMenuItem();
            this.type2 = new System.Windows.Forms.ToolStripMenuItem();
            this.wait2Seconds = new System.Windows.Forms.ToolStripMenuItem();
            this.wait5Seconds = new System.Windows.Forms.ToolStripMenuItem();
            this.wait10Seconds = new System.Windows.Forms.ToolStripMenuItem();
            this.type3 = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptReplacementMIDI = new System.Windows.Forms.ToolStripMenuItem();
            this.encryptReplacementMogg = new System.Windows.Forms.ToolStripMenuItem();
            this.fixMoggThatCausesLooping = new System.Windows.Forms.ToolStripMenuItem();
            this.fixLoopingWhenConverting = new System.Windows.Forms.ToolStripMenuItem();
            this.regionOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.regionNTSC = new System.Windows.Forms.ToolStripMenuItem();
            this.regionPAL = new System.Windows.Forms.ToolStripMenuItem();
            this.tutorialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnReset = new System.Windows.Forms.Button();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnFolder = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.chkRAR = new System.Windows.Forms.CheckBox();
            this.picWorking = new System.Windows.Forms.PictureBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPin)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWorking)).BeginInit();
            this.SuspendLayout();
            // 
            // lstLog
            // 
            this.lstLog.AllowDrop = true;
            this.lstLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstLog.BackColor = System.Drawing.Color.White;
            this.lstLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstLog.ContextMenuStrip = this.contextMenuStrip1;
            this.lstLog.FormattingEnabled = true;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.Location = new System.Drawing.Point(12, 144);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(568, 223);
            this.lstLog.TabIndex = 12;
            this.lstLog.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDragDrop);
            this.lstLog.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEnter);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportLogFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(122, 26);
            // 
            // exportLogFileToolStripMenuItem
            // 
            this.exportLogFileToolStripMenuItem.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.exportLogFileToolStripMenuItem.Name = "exportLogFileToolStripMenuItem";
            this.exportLogFileToolStripMenuItem.Size = new System.Drawing.Size(121, 22);
            this.exportLogFileToolStripMenuItem.Text = "Export log file";
            this.exportLogFileToolStripMenuItem.Click += new System.EventHandler(this.exportLogFileToolStripMenuItem_Click);
            // 
            // chkMerge
            // 
            this.chkMerge.AutoSize = true;
            this.chkMerge.Checked = true;
            this.chkMerge.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMerge.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkMerge.Enabled = false;
            this.chkMerge.Location = new System.Drawing.Point(218, 86);
            this.chkMerge.Name = "chkMerge";
            this.chkMerge.Size = new System.Drawing.Size(170, 17);
            this.chkMerge.TabIndex = 63;
            this.chkMerge.Text = "Merge new songs with existing";
            this.toolTip1.SetToolTip(this.chkMerge, "Merge converted songs with your existing customs in the Merged Songs folder");
            this.chkMerge.UseVisualStyleBackColor = true;
            this.chkMerge.CheckedChanged += new System.EventHandler(this.chkMerge_CheckedChanged);
            // 
            // chkSongID
            // 
            this.chkSongID.AutoSize = true;
            this.chkSongID.Checked = true;
            this.chkSongID.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSongID.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkSongID.Location = new System.Drawing.Point(401, 86);
            this.chkSongID.Name = "chkSongID";
            this.chkSongID.Size = new System.Drawing.Size(184, 17);
            this.chkSongID.TabIndex = 64;
            this.chkSongID.Text = "Change song ID to numeric value";
            this.toolTip1.SetToolTip(this.chkSongID, "Change song ID to unique numeric value when converting");
            this.chkSongID.UseVisualStyleBackColor = true;
            // 
            // btnBegin
            // 
            this.btnBegin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBegin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(89)))), ((int)(((byte)(201)))));
            this.btnBegin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBegin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBegin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBegin.ForeColor = System.Drawing.Color.White;
            this.btnBegin.Location = new System.Drawing.Point(516, 109);
            this.btnBegin.Name = "btnBegin";
            this.btnBegin.Size = new System.Drawing.Size(64, 29);
            this.btnBegin.TabIndex = 51;
            this.btnBegin.Text = "&Begin";
            this.toolTip1.SetToolTip(this.btnBegin, "Click to begin");
            this.btnBegin.UseVisualStyleBackColor = false;
            this.btnBegin.Visible = false;
            this.btnBegin.Click += new System.EventHandler(this.btnBegin_Click);
            // 
            // picPin
            // 
            this.picPin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picPin.BackColor = System.Drawing.Color.Transparent;
            this.picPin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picPin.Image = global::C3Tools.Properties.Resources.unpinned;
            this.picPin.Location = new System.Drawing.Point(567, 4);
            this.picPin.Name = "picPin";
            this.picPin.Size = new System.Drawing.Size(20, 20);
            this.picPin.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPin.TabIndex = 65;
            this.picPin.TabStop = false;
            this.picPin.Tag = "unpinned";
            this.toolTip1.SetToolTip(this.picPin, "Click to pin on top");
            this.picPin.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picPin_MouseClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsToolStripMenuItem,
            this.numericIDOptionsToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.regionOptionsToolStripMenuItem,
            this.tutorialToolStripMenuItem,
            this.helpToolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(592, 24);
            this.menuStrip1.TabIndex = 35;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mergeSongsToolStrip,
            this.managePackDTAFile});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // mergeSongsToolStrip
            // 
            this.mergeSongsToolStrip.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.mergeSongsToolStrip.Name = "mergeSongsToolStrip";
            this.mergeSongsToolStrip.Size = new System.Drawing.Size(190, 22);
            this.mergeSongsToolStrip.Text = "Merge songs";
            this.mergeSongsToolStrip.Click += new System.EventHandler(this.mergeSongsToolStrip_Click);
            // 
            // managePackDTAFile
            // 
            this.managePackDTAFile.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.managePackDTAFile.Name = "managePackDTAFile";
            this.managePackDTAFile.Size = new System.Drawing.Size(190, 22);
            this.managePackDTAFile.Text = "Manage pack DTA file";
            this.managePackDTAFile.Click += new System.EventHandler(this.managePackDTAFile_Click);
            // 
            // numericIDOptionsToolStripMenuItem
            // 
            this.numericIDOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.batchChangeIDsToNumeric,
            this.changeIDPrefix,
            this.changeAuthorID,
            this.changeSongNumber});
            this.numericIDOptionsToolStripMenuItem.Name = "numericIDOptionsToolStripMenuItem";
            this.numericIDOptionsToolStripMenuItem.Size = new System.Drawing.Size(124, 20);
            this.numericIDOptionsToolStripMenuItem.Text = "Numeric ID Options";
            // 
            // batchChangeIDsToNumeric
            // 
            this.batchChangeIDsToNumeric.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.batchChangeIDsToNumeric.Name = "batchChangeIDsToNumeric";
            this.batchChangeIDsToNumeric.Size = new System.Drawing.Size(193, 22);
            this.batchChangeIDsToNumeric.Text = "Batch replace song IDs";
            this.batchChangeIDsToNumeric.Click += new System.EventHandler(this.batchChangeIDsToNumeric_Click);
            // 
            // changeIDPrefix
            // 
            this.changeIDPrefix.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.changeIDPrefix.Name = "changeIDPrefix";
            this.changeIDPrefix.Size = new System.Drawing.Size(193, 22);
            this.changeIDPrefix.Text = "Change ID prefix";
            this.changeIDPrefix.Click += new System.EventHandler(this.changeIDPrefix_Click);
            // 
            // changeAuthorID
            // 
            this.changeAuthorID.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.changeAuthorID.Name = "changeAuthorID";
            this.changeAuthorID.Size = new System.Drawing.Size(193, 22);
            this.changeAuthorID.Text = "Change author ID";
            this.changeAuthorID.Click += new System.EventHandler(this.changeAuthorID_Click);
            // 
            // changeSongNumber
            // 
            this.changeSongNumber.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.changeSongNumber.Name = "changeSongNumber";
            this.changeSongNumber.Size = new System.Drawing.Size(193, 22);
            this.changeSongNumber.Text = "Change song number";
            this.changeSongNumber.Click += new System.EventHandler(this.changeSongNumber_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mIDIToEDATEncryptionToolStripMenuItem,
            this.encryptReplacementMIDI,
            this.encryptReplacementMogg,
            this.fixMoggThatCausesLooping,
            this.fixLoopingWhenConverting});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(121, 20);
            this.optionsToolStripMenuItem.Text = "Encryption Options";
            // 
            // mIDIToEDATEncryptionToolStripMenuItem
            // 
            this.mIDIToEDATEncryptionToolStripMenuItem.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.mIDIToEDATEncryptionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.type1,
            this.type2,
            this.type3});
            this.mIDIToEDATEncryptionToolStripMenuItem.Name = "mIDIToEDATEncryptionToolStripMenuItem";
            this.mIDIToEDATEncryptionToolStripMenuItem.Size = new System.Drawing.Size(251, 22);
            this.mIDIToEDATEncryptionToolStripMenuItem.Text = "MIDI to EDAT encryption type";
            // 
            // type1
            // 
            this.type1.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.type1.Checked = true;
            this.type1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.type1.Name = "type1";
            this.type1.Size = new System.Drawing.Size(211, 22);
            this.type1.Text = "Type 1 (edattool.exe)";
            this.type1.Click += new System.EventHandler(this.type1defaultToolStripMenuItem_Click);
            // 
            // type2
            // 
            this.type2.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.type2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wait2Seconds,
            this.wait5Seconds,
            this.wait10Seconds});
            this.type2.Name = "type2";
            this.type2.Size = new System.Drawing.Size(211, 22);
            this.type2.Text = "Type 2 (rebuilder.exe)";
            this.type2.Click += new System.EventHandler(this.type2_Click);
            // 
            // wait2Seconds
            // 
            this.wait2Seconds.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.wait2Seconds.Checked = true;
            this.wait2Seconds.CheckState = System.Windows.Forms.CheckState.Checked;
            this.wait2Seconds.Name = "wait2Seconds";
            this.wait2Seconds.Size = new System.Drawing.Size(159, 22);
            this.wait2Seconds.Text = "Wait 2 seconds";
            this.wait2Seconds.Click += new System.EventHandler(this.wait2Seconds_Click);
            // 
            // wait5Seconds
            // 
            this.wait5Seconds.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.wait5Seconds.Name = "wait5Seconds";
            this.wait5Seconds.Size = new System.Drawing.Size(159, 22);
            this.wait5Seconds.Text = "Wait 5 seconds";
            this.wait5Seconds.Click += new System.EventHandler(this.wait5Seconds_Click);
            // 
            // wait10Seconds
            // 
            this.wait10Seconds.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.wait10Seconds.Name = "wait10Seconds";
            this.wait10Seconds.Size = new System.Drawing.Size(159, 22);
            this.wait10Seconds.Text = "Wait 10 seconds";
            this.wait10Seconds.Click += new System.EventHandler(this.wait10Seconds_Click);
            // 
            // type3
            // 
            this.type3.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.type3.Name = "type3";
            this.type3.Size = new System.Drawing.Size(211, 22);
            this.type3.Text = "Type 3 (make_npdata.exe)";
            this.type3.Click += new System.EventHandler(this.type3_Click);
            // 
            // encryptReplacementMIDI
            // 
            this.encryptReplacementMIDI.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.encryptReplacementMIDI.Name = "encryptReplacementMIDI";
            this.encryptReplacementMIDI.Size = new System.Drawing.Size(251, 22);
            this.encryptReplacementMIDI.Text = "Encrypt replacement MIDI file(s)";
            this.encryptReplacementMIDI.Click += new System.EventHandler(this.encryptReplacementMIDI_Click);
            // 
            // encryptReplacementMogg
            // 
            this.encryptReplacementMogg.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.encryptReplacementMogg.Name = "encryptReplacementMogg";
            this.encryptReplacementMogg.Size = new System.Drawing.Size(251, 22);
            this.encryptReplacementMogg.Text = "Encrypt replacement mogg file";
            this.encryptReplacementMogg.Click += new System.EventHandler(this.encryptReplacementMogg_Click);
            // 
            // fixMoggThatCausesLooping
            // 
            this.fixMoggThatCausesLooping.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.fixMoggThatCausesLooping.Name = "fixMoggThatCausesLooping";
            this.fixMoggThatCausesLooping.Size = new System.Drawing.Size(251, 22);
            this.fixMoggThatCausesLooping.Text = "Fix PS3 mogg that causes looping";
            this.fixMoggThatCausesLooping.Click += new System.EventHandler(this.fixMoggThatCausesLooping_Click);
            // 
            // fixLoopingWhenConverting
            // 
            this.fixLoopingWhenConverting.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.fixLoopingWhenConverting.CheckOnClick = true;
            this.fixLoopingWhenConverting.Name = "fixLoopingWhenConverting";
            this.fixLoopingWhenConverting.Size = new System.Drawing.Size(251, 22);
            this.fixLoopingWhenConverting.Text = "Fix looping when converting";
            // 
            // regionOptionsToolStripMenuItem
            // 
            this.regionOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.regionNTSC,
            this.regionPAL});
            this.regionOptionsToolStripMenuItem.Name = "regionOptionsToolStripMenuItem";
            this.regionOptionsToolStripMenuItem.Size = new System.Drawing.Size(101, 20);
            this.regionOptionsToolStripMenuItem.Text = "Region Options";
            // 
            // regionNTSC
            // 
            this.regionNTSC.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.regionNTSC.Checked = true;
            this.regionNTSC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.regionNTSC.Name = "regionNTSC";
            this.regionNTSC.Size = new System.Drawing.Size(104, 22);
            this.regionNTSC.Text = "NTSC";
            this.regionNTSC.Click += new System.EventHandler(this.regionNTSC_Click);
            // 
            // regionPAL
            // 
            this.regionPAL.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.regionPAL.Name = "regionPAL";
            this.regionPAL.Size = new System.Drawing.Size(104, 22);
            this.regionPAL.Text = "PAL";
            this.regionPAL.Click += new System.EventHandler(this.regionPAL_Click);
            // 
            // tutorialToolStripMenuItem
            // 
            this.tutorialToolStripMenuItem.Name = "tutorialToolStripMenuItem";
            this.tutorialToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.tutorialToolStripMenuItem.Text = "&Tutorial";
            this.tutorialToolStripMenuItem.Click += new System.EventHandler(this.tutorialToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem1.Text = "&Help";
            this.helpToolStripMenuItem1.Click += new System.EventHandler(this.helpToolStripMenuItem1_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(89)))), ((int)(((byte)(201)))));
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.ForeColor = System.Drawing.Color.White;
            this.btnReset.Location = new System.Drawing.Point(12, 109);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 29);
            this.btnReset.TabIndex = 54;
            this.btnReset.Text = "&Reset";
            this.btnReset.UseVisualStyleBackColor = false;
            this.btnReset.Visible = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.BackColor = System.Drawing.Color.White;
            this.txtFolder.Location = new System.Drawing.Point(12, 60);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.ReadOnly = true;
            this.txtFolder.Size = new System.Drawing.Size(568, 20);
            this.txtFolder.TabIndex = 48;
            this.txtFolder.TextChanged += new System.EventHandler(this.txtFolder_TextChanged);
            this.txtFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDragDrop);
            this.txtFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEnter);
            // 
            // btnFolder
            // 
            this.btnFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(89)))), ((int)(((byte)(201)))));
            this.btnFolder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFolder.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFolder.ForeColor = System.Drawing.Color.White;
            this.btnFolder.Location = new System.Drawing.Point(12, 27);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(134, 30);
            this.btnFolder.TabIndex = 49;
            this.btnFolder.Text = "Change &Input Folder";
            this.btnFolder.UseVisualStyleBackColor = false;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(89)))), ((int)(((byte)(201)))));
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(480, 27);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 30);
            this.btnRefresh.TabIndex = 50;
            this.btnRefresh.Text = "Refresh Folder";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Visible = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // chkRAR
            // 
            this.chkRAR.AutoSize = true;
            this.chkRAR.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRAR.Location = new System.Drawing.Point(12, 86);
            this.chkRAR.Name = "chkRAR";
            this.chkRAR.Size = new System.Drawing.Size(189, 17);
            this.chkRAR.TabIndex = 55;
            this.chkRAR.Text = "Create RAR archive for each song";
            this.chkRAR.UseVisualStyleBackColor = true;
            // 
            // picWorking
            // 
            this.picWorking.Image = global::C3Tools.Properties.Resources.working;
            this.picWorking.Location = new System.Drawing.Point(240, 123);
            this.picWorking.Name = "picWorking";
            this.picWorking.Size = new System.Drawing.Size(128, 15);
            this.picWorking.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picWorking.TabIndex = 62;
            this.picWorking.TabStop = false;
            this.picWorking.Visible = false;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // PS3Converter
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.ClientSize = new System.Drawing.Size(592, 379);
            this.Controls.Add(this.picPin);
            this.Controls.Add(this.chkSongID);
            this.Controls.Add(this.chkMerge);
            this.Controls.Add(this.picWorking);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.btnFolder);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnBegin);
            this.Controls.Add(this.chkRAR);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.lstLog);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "PS3Converter";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PS3 Converter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PS3Converter_FormClosing);
            this.Shown += new System.EventHandler(this.PS3Prep_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.HandleDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.HandleDragEnter);
            this.Resize += new System.EventHandler(this.PS3Prep_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPin)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picWorking)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exportLogFileToolStripMenuItem;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnBegin;
        private System.Windows.Forms.CheckBox chkRAR;
        private System.Windows.Forms.PictureBox picWorking;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckBox chkMerge;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mergeSongsToolStrip;
        private System.Windows.Forms.ToolStripMenuItem managePackDTAFile;
        private System.Windows.Forms.ToolStripMenuItem tutorialToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mIDIToEDATEncryptionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem type1;
        private System.Windows.Forms.ToolStripMenuItem type2;
        private System.Windows.Forms.ToolStripMenuItem wait2Seconds;
        private System.Windows.Forms.ToolStripMenuItem wait5Seconds;
        private System.Windows.Forms.ToolStripMenuItem wait10Seconds;
        private System.Windows.Forms.ToolStripMenuItem encryptReplacementMogg;
        private System.Windows.Forms.ToolStripMenuItem encryptReplacementMIDI;
        private System.Windows.Forms.CheckBox chkSongID;
        private System.Windows.Forms.ToolStripMenuItem numericIDOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem batchChangeIDsToNumeric;
        private System.Windows.Forms.ToolStripMenuItem changeIDPrefix;
        private System.Windows.Forms.ToolStripMenuItem changeAuthorID;
        private System.Windows.Forms.ToolStripMenuItem changeSongNumber;
        private System.Windows.Forms.ToolStripMenuItem regionOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem regionNTSC;
        private System.Windows.Forms.ToolStripMenuItem regionPAL;
        private System.Windows.Forms.ToolStripMenuItem type3;
        private System.Windows.Forms.ToolStripMenuItem fixMoggThatCausesLooping;
        private System.Windows.Forms.ToolStripMenuItem fixLoopingWhenConverting;
        private System.Windows.Forms.PictureBox picPin;
    }
}