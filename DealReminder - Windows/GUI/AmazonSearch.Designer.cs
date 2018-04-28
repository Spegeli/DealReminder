namespace DealReminder_Windows.GUI
{
    partial class AmazonSearch
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            this.metroButton5 = new MetroFramework.Controls.MetroButton();
            this.metroComboBox1 = new MetroFramework.Controls.MetroComboBox();
            this.metroTextBox1 = new MetroFramework.Controls.MetroTextBox();
            this.metroComboBox2 = new MetroFramework.Controls.MetroComboBox();
            this.metroGrid1 = new MetroFramework.Controls.MetroGrid();
            this.DG3_CheckBox = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DG3_PreviewImage = new System.Windows.Forms.DataGridViewImageColumn();
            this.DG3_ASIN_ISBN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DG3_Name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DG3_PreisNeu = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DG3_PreisGebraucht = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metroComboBox3 = new MetroFramework.Controls.MetroComboBox();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // metroButton5
            // 
            this.metroButton5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButton5.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.metroButton5.Highlight = true;
            this.metroButton5.Location = new System.Drawing.Point(670, 63);
            this.metroButton5.Name = "metroButton5";
            this.metroButton5.Size = new System.Drawing.Size(65, 29);
            this.metroButton5.TabIndex = 12;
            this.metroButton5.Text = " Suchen";
            this.metroButton5.UseSelectable = true;
            this.metroButton5.Click += new System.EventHandler(this.metroButton5_Click);
            // 
            // metroComboBox1
            // 
            this.metroComboBox1.FormattingEnabled = true;
            this.metroComboBox1.ItemHeight = 23;
            this.metroComboBox1.Items.AddRange(new object[] {
            "All",
            "Apparel",
            "Automotive",
            "Baby",
            "Beauty",
            "Blended",
            "Books",
            "Classical",
            "DigitalMusic",
            "DVD",
            "Electronics",
            "ForeignBooks",
            "GourmetFood",
            "Grocery",
            "HealthPersonalCare",
            "Hobbies",
            "HomeGarden",
            "Industrial",
            "Jewelry",
            "KindleStore",
            "Kitchen",
            "Magazines",
            "Merchants",
            "Miscellaneous",
            "MP3Downloads",
            "Music",
            "MusicalInstruments",
            "MusicTracks",
            "OfficeProducts",
            "OutdoorLiving",
            "PCHardware",
            "PetSupplies",
            "Photo",
            "Software",
            "SoftwareVideoGames",
            "SportingGoods",
            "Tools",
            "Toys",
            "VHS",
            "Video",
            "VideoGames",
            "Watches",
            "Wireless",
            "WirelessAccessories"});
            this.metroComboBox1.Location = new System.Drawing.Point(98, 63);
            this.metroComboBox1.Name = "metroComboBox1";
            this.metroComboBox1.PromptText = "KATEGORIE";
            this.metroComboBox1.Size = new System.Drawing.Size(167, 29);
            this.metroComboBox1.TabIndex = 11;
            this.metroComboBox1.UseSelectable = true;
            // 
            // metroTextBox1
            // 
            this.metroTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            // 
            // 
            // 
            this.metroTextBox1.CustomButton.Image = null;
            this.metroTextBox1.CustomButton.Location = new System.Drawing.Point(255, 1);
            this.metroTextBox1.CustomButton.Name = "";
            this.metroTextBox1.CustomButton.Size = new System.Drawing.Size(27, 27);
            this.metroTextBox1.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.metroTextBox1.CustomButton.TabIndex = 1;
            this.metroTextBox1.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.metroTextBox1.CustomButton.UseSelectable = true;
            this.metroTextBox1.CustomButton.Visible = false;
            this.metroTextBox1.DisplayIcon = true;
            this.metroTextBox1.FontSize = MetroFramework.MetroTextBoxSize.Medium;
            this.metroTextBox1.Lines = new string[0];
            this.metroTextBox1.Location = new System.Drawing.Point(271, 63);
            this.metroTextBox1.MaxLength = 1000;
            this.metroTextBox1.Name = "metroTextBox1";
            this.metroTextBox1.PasswordChar = '\0';
            this.metroTextBox1.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.metroTextBox1.SelectedText = "";
            this.metroTextBox1.SelectionLength = 0;
            this.metroTextBox1.SelectionStart = 0;
            this.metroTextBox1.ShortcutsEnabled = true;
            this.metroTextBox1.Size = new System.Drawing.Size(318, 29);
            this.metroTextBox1.TabIndex = 10;
            this.metroTextBox1.UseSelectable = true;
            this.metroTextBox1.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.metroTextBox1.WaterMarkFont = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            // 
            // metroComboBox2
            // 
            this.metroComboBox2.FormattingEnabled = true;
            this.metroComboBox2.ItemHeight = 23;
            this.metroComboBox2.Items.AddRange(new object[] {
            "DE",
            "IT",
            "FR",
            "ES",
            "UK"});
            this.metroComboBox2.Location = new System.Drawing.Point(23, 63);
            this.metroComboBox2.Name = "metroComboBox2";
            this.metroComboBox2.PromptText = "STORE";
            this.metroComboBox2.Size = new System.Drawing.Size(69, 29);
            this.metroComboBox2.TabIndex = 14;
            this.metroComboBox2.UseSelectable = true;
            // 
            // metroGrid1
            // 
            this.metroGrid1.AllowUserToAddRows = false;
            this.metroGrid1.AllowUserToDeleteRows = false;
            this.metroGrid1.AllowUserToResizeRows = false;
            this.metroGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroGrid1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.metroGrid1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.metroGrid1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.metroGrid1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Sunken;
            dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle7.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle7.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle7.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.metroGrid1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
            this.metroGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.metroGrid1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.DG3_CheckBox,
            this.DG3_PreviewImage,
            this.DG3_ASIN_ISBN,
            this.DG3_Name,
            this.DG3_PreisNeu,
            this.DG3_PreisGebraucht});
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle11.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle11.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle11.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle11.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.metroGrid1.DefaultCellStyle = dataGridViewCellStyle11;
            this.metroGrid1.EnableHeadersVisualStyles = false;
            this.metroGrid1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.metroGrid1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid1.Location = new System.Drawing.Point(23, 98);
            this.metroGrid1.Name = "metroGrid1";
            this.metroGrid1.ReadOnly = true;
            this.metroGrid1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle12.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle12.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid1.RowHeadersDefaultCellStyle = dataGridViewCellStyle12;
            this.metroGrid1.RowHeadersVisible = false;
            this.metroGrid1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.metroGrid1.RowTemplate.Height = 75;
            this.metroGrid1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.metroGrid1.Size = new System.Drawing.Size(712, 409);
            this.metroGrid1.TabIndex = 15;
            this.metroGrid1.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.metroGrid1_CellMouseClick);
            this.metroGrid1.CellMouseDoubleClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.metroGrid1_CellMouseDoubleClick);
            // 
            // DG3_CheckBox
            // 
            this.DG3_CheckBox.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DG3_CheckBox.HeaderText = "";
            this.DG3_CheckBox.Name = "DG3_CheckBox";
            this.DG3_CheckBox.ReadOnly = true;
            this.DG3_CheckBox.Width = 40;
            // 
            // DG3_PreviewImage
            // 
            this.DG3_PreviewImage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.DG3_PreviewImage.HeaderText = "Vorschau";
            this.DG3_PreviewImage.Name = "DG3_PreviewImage";
            this.DG3_PreviewImage.ReadOnly = true;
            this.DG3_PreviewImage.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DG3_PreviewImage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.DG3_PreviewImage.Width = 75;
            // 
            // DG3_ASIN_ISBN
            // 
            this.DG3_ASIN_ISBN.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.DG3_ASIN_ISBN.DefaultCellStyle = dataGridViewCellStyle8;
            this.DG3_ASIN_ISBN.HeaderText = "ASIN / ISBN";
            this.DG3_ASIN_ISBN.Name = "DG3_ASIN_ISBN";
            this.DG3_ASIN_ISBN.ReadOnly = true;
            this.DG3_ASIN_ISBN.Width = 90;
            // 
            // DG3_Name
            // 
            this.DG3_Name.FillWeight = 44.51458F;
            this.DG3_Name.HeaderText = "Name";
            this.DG3_Name.Name = "DG3_Name";
            this.DG3_Name.ReadOnly = true;
            // 
            // DG3_PreisNeu
            // 
            this.DG3_PreisNeu.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.DG3_PreisNeu.DefaultCellStyle = dataGridViewCellStyle9;
            this.DG3_PreisNeu.FillWeight = 44.51458F;
            this.DG3_PreisNeu.HeaderText = "Preis: Neu";
            this.DG3_PreisNeu.Name = "DG3_PreisNeu";
            this.DG3_PreisNeu.ReadOnly = true;
            this.DG3_PreisNeu.Width = 90;
            // 
            // DG3_PreisGebraucht
            // 
            this.DG3_PreisGebraucht.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.DG3_PreisGebraucht.DefaultCellStyle = dataGridViewCellStyle10;
            this.DG3_PreisGebraucht.HeaderText = "Preis: Gebraucht";
            this.DG3_PreisGebraucht.Name = "DG3_PreisGebraucht";
            this.DG3_PreisGebraucht.ReadOnly = true;
            this.DG3_PreisGebraucht.Width = 125;
            // 
            // metroComboBox3
            // 
            this.metroComboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.metroComboBox3.FormattingEnabled = true;
            this.metroComboBox3.ItemHeight = 23;
            this.metroComboBox3.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5"});
            this.metroComboBox3.Location = new System.Drawing.Point(595, 63);
            this.metroComboBox3.Name = "metroComboBox3";
            this.metroComboBox3.PromptText = "SEITE";
            this.metroComboBox3.Size = new System.Drawing.Size(69, 29);
            this.metroComboBox3.TabIndex = 16;
            this.metroComboBox3.UseSelectable = true;
            // 
            // metroButton1
            // 
            this.metroButton1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroButton1.Location = new System.Drawing.Point(23, 543);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(712, 23);
            this.metroButton1.TabIndex = 17;
            this.metroButton1.Text = "Markierte Einträge hinzufügen";
            this.metroButton1.UseSelectable = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::DealReminder_Windows.Properties.Resources.Icon_Info;
            this.pictureBox1.Location = new System.Drawing.Point(699, 23);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(36, 34);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 18;
            this.pictureBox1.TabStop = false;
            // 
            // metroLabel1
            // 
            this.metroLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroLabel1.Location = new System.Drawing.Point(23, 515);
            this.metroLabel1.Name = "metroLabel1";
            this.metroLabel1.Size = new System.Drawing.Size(712, 20);
            this.metroLabel1.TabIndex = 19;
            this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // AmazonSearch
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 586);
            this.Controls.Add(this.metroLabel1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.metroComboBox3);
            this.Controls.Add(this.metroGrid1);
            this.Controls.Add(this.metroComboBox2);
            this.Controls.Add(this.metroButton5);
            this.Controls.Add(this.metroComboBox1);
            this.Controls.Add(this.metroTextBox1);
            this.Icon = global::DealReminder_Windows.Properties.Resources.Icon_DealReminder;
            this.Name = "AmazonSearch";
            this.ShadowType = MetroFramework.Forms.MetroFormShadowType.AeroShadow;
            this.Text = "AmazonSearch";
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public MetroFramework.Controls.MetroButton metroButton5;
        public MetroFramework.Controls.MetroComboBox metroComboBox1;
        public MetroFramework.Controls.MetroTextBox metroTextBox1;
        public MetroFramework.Controls.MetroComboBox metroComboBox2;
        public MetroFramework.Controls.MetroGrid metroGrid1;
        private System.Windows.Forms.DataGridViewCheckBoxColumn DG3_CheckBox;
        private System.Windows.Forms.DataGridViewImageColumn DG3_PreviewImage;
        private System.Windows.Forms.DataGridViewTextBoxColumn DG3_ASIN_ISBN;
        private System.Windows.Forms.DataGridViewTextBoxColumn DG3_Name;
        private System.Windows.Forms.DataGridViewTextBoxColumn DG3_PreisNeu;
        private System.Windows.Forms.DataGridViewTextBoxColumn DG3_PreisGebraucht;
        public MetroFramework.Controls.MetroComboBox metroComboBox3;
        private MetroFramework.Controls.MetroButton metroButton1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private MetroFramework.Controls.MetroLabel metroLabel1;
    }
}