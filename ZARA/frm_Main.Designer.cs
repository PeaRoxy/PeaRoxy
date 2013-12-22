namespace ZARA
{
    sealed partial class frm_Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frm_Main));
            this.statTimer = new System.Windows.Forms.Timer(this.components);
            this.txt_username = new System.Windows.Forms.TextBox();
            this.pnl_main = new System.Windows.Forms.Panel();
            this.pnl_status = new System.Windows.Forms.Panel();
            this.btn_exit = new System.Windows.Forms.Button();
            this.btn_minimize = new System.Windows.Forms.Button();
            this.panel6 = new System.Windows.Forms.Panel();
            this.btn_disconnect = new System.Windows.Forms.Button();
            this.lbl_stat_uploaded_v = new ZARA.MyLabel();
            this.lbl_stat_uploaded = new ZARA.MyLabel();
            this.lbl_stat_downloaded_v = new ZARA.MyLabel();
            this.lbl_stat_downloaded = new ZARA.MyLabel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.lbl_status = new ZARA.MyLabel();
            this.label6 = new ZARA.MyLabel();
            this.pb_status = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pnl_login = new System.Windows.Forms.Panel();
            this.label13 = new ZARA.MyLabel();
            this.btn_login = new System.Windows.Forms.Button();
            this.pnl_host = new System.Windows.Forms.Panel();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.txt_server = new System.Windows.Forms.TextBox();
            this.pnl_password = new System.Windows.Forms.Panel();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.txt_password = new System.Windows.Forms.TextBox();
            this.pnl_username = new System.Windows.Forms.Panel();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pnl_details = new System.Windows.Forms.Panel();
            this.label4 = new ZARA.MyLabel();
            this.label3 = new ZARA.MyLabel();
            this.label12 = new ZARA.MyLabel();
            this.label9 = new ZARA.MyLabel();
            this.lbl_stat_acceptingthreads = new ZARA.MyLabel();
            this.label7 = new ZARA.MyLabel();
            this.lbl_stat_activeconnections = new ZARA.MyLabel();
            this.panel9 = new System.Windows.Forms.Panel();
            this.label2 = new ZARA.MyLabel();
            this.label1 = new ZARA.MyLabel();
            this.cpb_stat_uploadrate = new CircularProgressBar.CircularProgressBar();
            this.cpb_stat_downloadrate = new CircularProgressBar.CircularProgressBar();
            this.pnl_main.SuspendLayout();
            this.pnl_status.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_status)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.pnl_login.SuspendLayout();
            this.pnl_host.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            this.pnl_password.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.pnl_username.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.pnl_details.SuspendLayout();
            this.SuspendLayout();
            // 
            // statTimer
            // 
            this.statTimer.Enabled = true;
            this.statTimer.Interval = 1000;
            this.statTimer.Tick += new System.EventHandler(this.StatTimerTick);
            // 
            // txt_username
            // 
            this.txt_username.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_username.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.txt_username.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_username.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.txt_username.ForeColor = System.Drawing.Color.White;
            this.txt_username.Location = new System.Drawing.Point(31, 5);
            this.txt_username.Name = "txt_username";
            this.txt_username.Size = new System.Drawing.Size(131, 16);
            this.txt_username.TabIndex = 0;
            this.txt_username.Text = "Username";
            this.txt_username.Leave += new System.EventHandler(this.TxtLeave);
            // 
            // pnl_main
            // 
            this.pnl_main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.pnl_main.Controls.Add(this.pnl_status);
            this.pnl_main.Controls.Add(this.pnl_login);
            this.pnl_main.Controls.Add(this.pnl_details);
            this.pnl_main.Location = new System.Drawing.Point(0, 0);
            this.pnl_main.Name = "pnl_main";
            this.pnl_main.Size = new System.Drawing.Size(680, 300);
            this.pnl_main.TabIndex = 1;
            // 
            // pnl_status
            // 
            this.pnl_status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_status.Controls.Add(this.btn_exit);
            this.pnl_status.Controls.Add(this.btn_minimize);
            this.pnl_status.Controls.Add(this.panel6);
            this.pnl_status.Controls.Add(this.btn_disconnect);
            this.pnl_status.Controls.Add(this.lbl_stat_uploaded_v);
            this.pnl_status.Controls.Add(this.lbl_stat_uploaded);
            this.pnl_status.Controls.Add(this.lbl_stat_downloaded_v);
            this.pnl_status.Controls.Add(this.lbl_stat_downloaded);
            this.pnl_status.Controls.Add(this.panel5);
            this.pnl_status.Controls.Add(this.lbl_status);
            this.pnl_status.Controls.Add(this.label6);
            this.pnl_status.Controls.Add(this.pb_status);
            this.pnl_status.Controls.Add(this.pictureBox2);
            this.pnl_status.Controls.Add(this.pictureBox1);
            this.pnl_status.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnl_status.Location = new System.Drawing.Point(200, 0);
            this.pnl_status.Name = "pnl_status";
            this.pnl_status.Size = new System.Drawing.Size(280, 300);
            this.pnl_status.TabIndex = 1;
            this.pnl_status.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // btn_exit
            // 
            this.btn_exit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_exit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(146)))), ((int)(((byte)(211)))));
            this.btn_exit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_exit.FlatAppearance.BorderSize = 0;
            this.btn_exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(137)))), ((int)(((byte)(199)))));
            this.btn_exit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(160)))), ((int)(((byte)(231)))));
            this.btn_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_exit.Font = new System.Drawing.Font("Arial Black", 8.3F, System.Drawing.FontStyle.Bold);
            this.btn_exit.ForeColor = System.Drawing.Color.White;
            this.btn_exit.Location = new System.Drawing.Point(137, 246);
            this.btn_exit.Name = "btn_exit";
            this.btn_exit.Size = new System.Drawing.Size(107, 33);
            this.btn_exit.TabIndex = 8;
            this.btn_exit.Text = "EXIT";
            this.btn_exit.UseVisualStyleBackColor = false;
            this.btn_exit.Click += new System.EventHandler(this.BtnExitClick);
            // 
            // btn_minimize
            // 
            this.btn_minimize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_minimize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(146)))), ((int)(((byte)(211)))));
            this.btn_minimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_minimize.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_minimize.FlatAppearance.BorderSize = 0;
            this.btn_minimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(137)))), ((int)(((byte)(199)))));
            this.btn_minimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(160)))), ((int)(((byte)(231)))));
            this.btn_minimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_minimize.Font = new System.Drawing.Font("Arial Black", 8.3F, System.Drawing.FontStyle.Bold);
            this.btn_minimize.ForeColor = System.Drawing.Color.White;
            this.btn_minimize.Location = new System.Drawing.Point(26, 246);
            this.btn_minimize.Name = "btn_minimize";
            this.btn_minimize.Size = new System.Drawing.Size(107, 33);
            this.btn_minimize.TabIndex = 7;
            this.btn_minimize.Text = "MINIMIZE";
            this.btn_minimize.UseVisualStyleBackColor = false;
            this.btn_minimize.Click += new System.EventHandler(this.BtnMinimizeClick);
            // 
            // panel6
            // 
            this.panel6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel6.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.panel6.Location = new System.Drawing.Point(26, 178);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(223, 1);
            this.panel6.TabIndex = 10;
            this.panel6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // btn_disconnect
            // 
            this.btn_disconnect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_disconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(146)))), ((int)(((byte)(211)))));
            this.btn_disconnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_disconnect.Enabled = false;
            this.btn_disconnect.FlatAppearance.BorderSize = 0;
            this.btn_disconnect.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(137)))), ((int)(((byte)(199)))));
            this.btn_disconnect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(160)))), ((int)(((byte)(231)))));
            this.btn_disconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_disconnect.Font = new System.Drawing.Font("Arial Black", 8.3F, System.Drawing.FontStyle.Bold);
            this.btn_disconnect.ForeColor = System.Drawing.Color.White;
            this.btn_disconnect.Location = new System.Drawing.Point(26, 198);
            this.btn_disconnect.Name = "btn_disconnect";
            this.btn_disconnect.Size = new System.Drawing.Size(218, 33);
            this.btn_disconnect.TabIndex = 6;
            this.btn_disconnect.Text = "DISCONNECT";
            this.btn_disconnect.UseVisualStyleBackColor = false;
            this.btn_disconnect.Click += new System.EventHandler(this.BtnDisconnectClick);
            // 
            // lbl_stat_uploaded_v
            // 
            this.lbl_stat_uploaded_v.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_stat_uploaded_v.AutoSize = true;
            this.lbl_stat_uploaded_v.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_stat_uploaded_v.Font = new System.Drawing.Font("Arial Black", 6F, System.Drawing.FontStyle.Bold);
            this.lbl_stat_uploaded_v.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.lbl_stat_uploaded_v.Location = new System.Drawing.Point(185, 158);
            this.lbl_stat_uploaded_v.Name = "lbl_stat_uploaded_v";
            this.lbl_stat_uploaded_v.Size = new System.Drawing.Size(21, 11);
            this.lbl_stat_uploaded_v.TabIndex = 5;
            this.lbl_stat_uploaded_v.Text = "Bps";
            this.lbl_stat_uploaded_v.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // lbl_stat_uploaded
            // 
            this.lbl_stat_uploaded.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_stat_uploaded.AutoSize = true;
            this.lbl_stat_uploaded.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_stat_uploaded.Font = new System.Drawing.Font("Arial Black", 11F, System.Drawing.FontStyle.Bold);
            this.lbl_stat_uploaded.ForeColor = System.Drawing.Color.White;
            this.lbl_stat_uploaded.Location = new System.Drawing.Point(183, 140);
            this.lbl_stat_uploaded.Name = "lbl_stat_uploaded";
            this.lbl_stat_uploaded.Size = new System.Drawing.Size(35, 22);
            this.lbl_stat_uploaded.TabIndex = 4;
            this.lbl_stat_uploaded.Text = "0.0";
            this.lbl_stat_uploaded.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // lbl_stat_downloaded_v
            // 
            this.lbl_stat_downloaded_v.AutoSize = true;
            this.lbl_stat_downloaded_v.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_stat_downloaded_v.Font = new System.Drawing.Font("Arial Black", 6F, System.Drawing.FontStyle.Bold);
            this.lbl_stat_downloaded_v.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.lbl_stat_downloaded_v.Location = new System.Drawing.Point(69, 158);
            this.lbl_stat_downloaded_v.Name = "lbl_stat_downloaded_v";
            this.lbl_stat_downloaded_v.Size = new System.Drawing.Size(21, 11);
            this.lbl_stat_downloaded_v.TabIndex = 3;
            this.lbl_stat_downloaded_v.Text = "Bps";
            this.lbl_stat_downloaded_v.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // lbl_stat_downloaded
            // 
            this.lbl_stat_downloaded.AutoSize = true;
            this.lbl_stat_downloaded.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_stat_downloaded.Font = new System.Drawing.Font("Arial Black", 11F, System.Drawing.FontStyle.Bold);
            this.lbl_stat_downloaded.ForeColor = System.Drawing.Color.White;
            this.lbl_stat_downloaded.Location = new System.Drawing.Point(67, 140);
            this.lbl_stat_downloaded.Name = "lbl_stat_downloaded";
            this.lbl_stat_downloaded.Size = new System.Drawing.Size(35, 22);
            this.lbl_stat_downloaded.TabIndex = 2;
            this.lbl_stat_downloaded.Text = "0.0";
            this.lbl_stat_downloaded.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.panel5.Location = new System.Drawing.Point(26, 133);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(223, 1);
            this.panel5.TabIndex = 9;
            this.panel5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // lbl_status
            // 
            this.lbl_status.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_status.AutoSize = true;
            this.lbl_status.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_status.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Bold);
            this.lbl_status.ForeColor = System.Drawing.Color.White;
            this.lbl_status.Location = new System.Drawing.Point(102, 76);
            this.lbl_status.Name = "lbl_status";
            this.lbl_status.Size = new System.Drawing.Size(167, 24);
            this.lbl_status.TabIndex = 1;
            this.lbl_status.Text = "DISCONNECTED";
            this.lbl_status.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.DisabledForeColor = System.Drawing.Color.Empty;
            this.label6.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(16, 22);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 18);
            this.label6.TabIndex = 0;
            this.label6.Text = "STATUS";
            this.label6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pb_status
            // 
            this.pb_status.Image = global::ZARA.Properties.Resources.disconnected;
            this.pb_status.Location = new System.Drawing.Point(19, 49);
            this.pb_status.Name = "pb_status";
            this.pb_status.Size = new System.Drawing.Size(77, 77);
            this.pb_status.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_status.TabIndex = 4;
            this.pb_status.TabStop = false;
            this.pb_status.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox2.Image = global::ZARA.Properties.Resources.upload;
            this.pictureBox2.Location = new System.Drawing.Point(145, 140);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(32, 32);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox2.TabIndex = 3;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::ZARA.Properties.Resources.download;
            this.pictureBox1.Location = new System.Drawing.Point(30, 140);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pnl_login
            // 
            this.pnl_login.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(39)))), ((int)(((byte)(37)))));
            this.pnl_login.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_login.Controls.Add(this.label13);
            this.pnl_login.Controls.Add(this.btn_login);
            this.pnl_login.Controls.Add(this.pnl_host);
            this.pnl_login.Controls.Add(this.pnl_password);
            this.pnl_login.Controls.Add(this.pnl_username);
            this.pnl_login.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnl_login.Location = new System.Drawing.Point(480, 0);
            this.pnl_login.Name = "pnl_login";
            this.pnl_login.Size = new System.Drawing.Size(200, 300);
            this.pnl_login.TabIndex = 0;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.DisabledForeColor = System.Drawing.Color.Empty;
            this.label13.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.ForeColor = System.Drawing.Color.White;
            this.label13.Location = new System.Drawing.Point(16, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(55, 18);
            this.label13.TabIndex = 0;
            this.label13.Text = "LOGIN";
            this.label13.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // btn_login
            // 
            this.btn_login.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_login.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(146)))), ((int)(((byte)(211)))));
            this.btn_login.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_login.FlatAppearance.BorderSize = 0;
            this.btn_login.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(137)))), ((int)(((byte)(199)))));
            this.btn_login.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(160)))), ((int)(((byte)(231)))));
            this.btn_login.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_login.Font = new System.Drawing.Font("Arial Black", 8.3F, System.Drawing.FontStyle.Bold);
            this.btn_login.ForeColor = System.Drawing.Color.White;
            this.btn_login.Location = new System.Drawing.Point(15, 199);
            this.btn_login.Name = "btn_login";
            this.btn_login.Size = new System.Drawing.Size(167, 33);
            this.btn_login.TabIndex = 4;
            this.btn_login.Text = "CONNECT";
            this.btn_login.UseVisualStyleBackColor = false;
            this.btn_login.Click += new System.EventHandler(this.BtnLoginClick);
            // 
            // pnl_host
            // 
            this.pnl_host.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_host.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.pnl_host.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_host.Controls.Add(this.pictureBox6);
            this.pnl_host.Controls.Add(this.txt_server);
            this.pnl_host.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pnl_host.Location = new System.Drawing.Point(15, 143);
            this.pnl_host.Name = "pnl_host";
            this.pnl_host.Size = new System.Drawing.Size(167, 28);
            this.pnl_host.TabIndex = 3;
            this.pnl_host.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pictureBox6
            // 
            this.pictureBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox6.Image = global::ZARA.Properties.Resources.host;
            this.pictureBox6.Location = new System.Drawing.Point(3, 3);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(20, 20);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox6.TabIndex = 15;
            this.pictureBox6.TabStop = false;
            this.pictureBox6.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // txt_server
            // 
            this.txt_server.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_server.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.txt_server.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_server.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.txt_server.ForeColor = System.Drawing.Color.White;
            this.txt_server.Location = new System.Drawing.Point(31, 5);
            this.txt_server.Name = "txt_server";
            this.txt_server.Size = new System.Drawing.Size(131, 16);
            this.txt_server.TabIndex = 0;
            this.txt_server.Text = "Host";
            this.txt_server.Leave += new System.EventHandler(this.TxtServerLeave);
            // 
            // pnl_password
            // 
            this.pnl_password.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_password.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.pnl_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_password.Controls.Add(this.pictureBox5);
            this.pnl_password.Controls.Add(this.txt_password);
            this.pnl_password.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pnl_password.Location = new System.Drawing.Point(15, 104);
            this.pnl_password.Name = "pnl_password";
            this.pnl_password.Size = new System.Drawing.Size(167, 28);
            this.pnl_password.TabIndex = 2;
            this.pnl_password.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox5.Image = global::ZARA.Properties.Resources.password;
            this.pictureBox5.Location = new System.Drawing.Point(3, 3);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(20, 20);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox5.TabIndex = 15;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // txt_password
            // 
            this.txt_password.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_password.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.txt_password.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txt_password.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.txt_password.ForeColor = System.Drawing.Color.White;
            this.txt_password.Location = new System.Drawing.Point(31, 5);
            this.txt_password.Name = "txt_password";
            this.txt_password.Size = new System.Drawing.Size(131, 16);
            this.txt_password.TabIndex = 0;
            this.txt_password.Text = "Password";
            this.txt_password.Enter += new System.EventHandler(this.TxtPasswordEnter);
            // 
            // pnl_username
            // 
            this.pnl_username.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnl_username.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(62)))));
            this.pnl_username.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_username.Controls.Add(this.pictureBox4);
            this.pnl_username.Controls.Add(this.txt_username);
            this.pnl_username.ForeColor = System.Drawing.SystemColors.ControlText;
            this.pnl_username.Location = new System.Drawing.Point(15, 65);
            this.pnl_username.Name = "pnl_username";
            this.pnl_username.Size = new System.Drawing.Size(167, 28);
            this.pnl_username.TabIndex = 1;
            this.pnl_username.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pictureBox4
            // 
            this.pictureBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBox4.Image = global::ZARA.Properties.Resources.user;
            this.pictureBox4.Location = new System.Drawing.Point(3, 3);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(20, 20);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 15;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // pnl_details
            // 
            this.pnl_details.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(39)))), ((int)(((byte)(37)))));
            this.pnl_details.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnl_details.Controls.Add(this.label4);
            this.pnl_details.Controls.Add(this.label3);
            this.pnl_details.Controls.Add(this.label12);
            this.pnl_details.Controls.Add(this.label9);
            this.pnl_details.Controls.Add(this.lbl_stat_acceptingthreads);
            this.pnl_details.Controls.Add(this.label7);
            this.pnl_details.Controls.Add(this.lbl_stat_activeconnections);
            this.pnl_details.Controls.Add(this.panel9);
            this.pnl_details.Controls.Add(this.label2);
            this.pnl_details.Controls.Add(this.label1);
            this.pnl_details.Controls.Add(this.cpb_stat_uploadrate);
            this.pnl_details.Controls.Add(this.cpb_stat_downloadrate);
            this.pnl_details.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnl_details.ForeColor = System.Drawing.Color.White;
            this.pnl_details.Location = new System.Drawing.Point(0, 0);
            this.pnl_details.Name = "pnl_details";
            this.pnl_details.Size = new System.Drawing.Size(200, 300);
            this.pnl_details.TabIndex = 2;
            this.pnl_details.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label4
            // 
            this.label4.DisabledForeColor = System.Drawing.Color.Empty;
            this.label4.Font = new System.Drawing.Font("Arial Black", 6F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.label4.Location = new System.Drawing.Point(102, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 11);
            this.label4.TabIndex = 4;
            this.label4.Text = "UPLOAD";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label3
            // 
            this.label3.DisabledForeColor = System.Drawing.Color.Empty;
            this.label3.Font = new System.Drawing.Font("Arial Black", 6F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.label3.Location = new System.Drawing.Point(17, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 11);
            this.label3.TabIndex = 3;
            this.label3.Text = "DOWNLOAD";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.DisabledForeColor = System.Drawing.Color.Empty;
            this.label12.Font = new System.Drawing.Font("Arial Black", 6F, System.Drawing.FontStyle.Bold);
            this.label12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.label12.Location = new System.Drawing.Point(108, 259);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(73, 11);
            this.label12.TabIndex = 10;
            this.label12.Text = "CONNECTIONS";
            this.label12.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.DisabledForeColor = System.Drawing.Color.Empty;
            this.label9.Font = new System.Drawing.Font("Arial Black", 6F, System.Drawing.FontStyle.Bold);
            this.label9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.label9.Location = new System.Drawing.Point(108, 198);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(73, 11);
            this.label9.TabIndex = 7;
            this.label9.Text = "CONNECTIONS";
            this.label9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // lbl_stat_acceptingthreads
            // 
            this.lbl_stat_acceptingthreads.AutoSize = true;
            this.lbl_stat_acceptingthreads.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_stat_acceptingthreads.Font = new System.Drawing.Font("Arial Black", 18F, System.Drawing.FontStyle.Bold);
            this.lbl_stat_acceptingthreads.ForeColor = System.Drawing.Color.White;
            this.lbl_stat_acceptingthreads.Location = new System.Drawing.Point(35, 242);
            this.lbl_stat_acceptingthreads.Name = "lbl_stat_acceptingthreads";
            this.lbl_stat_acceptingthreads.Size = new System.Drawing.Size(31, 33);
            this.lbl_stat_acceptingthreads.TabIndex = 9;
            this.lbl_stat_acceptingthreads.Text = "0";
            this.lbl_stat_acceptingthreads.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.DisabledForeColor = System.Drawing.Color.Empty;
            this.label7.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(16, 224);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 18);
            this.label7.TabIndex = 8;
            this.label7.Text = "CONNECTING";
            this.label7.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // lbl_stat_activeconnections
            // 
            this.lbl_stat_activeconnections.AutoSize = true;
            this.lbl_stat_activeconnections.DisabledForeColor = System.Drawing.Color.Empty;
            this.lbl_stat_activeconnections.Font = new System.Drawing.Font("Arial Black", 18F, System.Drawing.FontStyle.Bold);
            this.lbl_stat_activeconnections.ForeColor = System.Drawing.Color.White;
            this.lbl_stat_activeconnections.Location = new System.Drawing.Point(35, 181);
            this.lbl_stat_activeconnections.Name = "lbl_stat_activeconnections";
            this.lbl_stat_activeconnections.Size = new System.Drawing.Size(31, 33);
            this.lbl_stat_activeconnections.TabIndex = 6;
            this.lbl_stat_activeconnections.Text = "0";
            this.lbl_stat_activeconnections.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // panel9
            // 
            this.panel9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel9.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(53)))), ((int)(((byte)(53)))));
            this.panel9.Location = new System.Drawing.Point(26, 154);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(148, 1);
            this.panel9.TabIndex = 11;
            this.panel9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.DisabledForeColor = System.Drawing.Color.Empty;
            this.label2.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(16, 163);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 18);
            this.label2.TabIndex = 5;
            this.label2.Text = "CONNECTED";
            this.label2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.DisabledForeColor = System.Drawing.Color.Empty;
            this.label1.Font = new System.Drawing.Font("Arial Black", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "DETAILS";
            this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // cpb_stat_uploadrate
            // 
            this.cpb_stat_uploadrate.AnimatorDuration = 1000;
            this.cpb_stat_uploadrate.AnimatorFunction = null;
            this.cpb_stat_uploadrate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(39)))), ((int)(((byte)(37)))));
            this.cpb_stat_uploadrate.Caption = null;
            this.cpb_stat_uploadrate.CaptionMargin = 3;
            this.cpb_stat_uploadrate.Font = new System.Drawing.Font("Arial Black", 8F, System.Drawing.FontStyle.Bold);
            this.cpb_stat_uploadrate.ForeColor = System.Drawing.Color.White;
            this.cpb_stat_uploadrate.InnerCircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(57)))), ((int)(((byte)(3)))));
            this.cpb_stat_uploadrate.InnerCircleMargin = 2;
            this.cpb_stat_uploadrate.InnerCircleWidth = 3;
            this.cpb_stat_uploadrate.Location = new System.Drawing.Point(102, 49);
            this.cpb_stat_uploadrate.MaxValue = 100F;
            this.cpb_stat_uploadrate.MinValue = 0F;
            this.cpb_stat_uploadrate.Name = "cpb_stat_uploadrate";
            this.cpb_stat_uploadrate.OuterCircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.cpb_stat_uploadrate.OuterCircleMargin = 1;
            this.cpb_stat_uploadrate.OuterCircleWidth = 1;
            this.cpb_stat_uploadrate.ProgressCircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(0)))));
            this.cpb_stat_uploadrate.ProgressCircleStartAngle = 270;
            this.cpb_stat_uploadrate.ProgressCircleWidth = 9;
            this.cpb_stat_uploadrate.Size = new System.Drawing.Size(82, 82);
            this.cpb_stat_uploadrate.SubText = null;
            this.cpb_stat_uploadrate.SubTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cpb_stat_uploadrate.SupSubFont = new System.Drawing.Font("Arial", 6F, System.Drawing.FontStyle.Bold);
            this.cpb_stat_uploadrate.SupText = null;
            this.cpb_stat_uploadrate.SupTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cpb_stat_uploadrate.TabIndex = 2;
            this.cpb_stat_uploadrate.Value = 0F;
            this.cpb_stat_uploadrate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // cpb_stat_downloadrate
            // 
            this.cpb_stat_downloadrate.AnimatorDuration = 1000;
            this.cpb_stat_downloadrate.AnimatorFunction = null;
            this.cpb_stat_downloadrate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(39)))), ((int)(((byte)(37)))));
            this.cpb_stat_downloadrate.Caption = "";
            this.cpb_stat_downloadrate.CaptionMargin = 3;
            this.cpb_stat_downloadrate.Font = new System.Drawing.Font("Arial Black", 8F, System.Drawing.FontStyle.Bold);
            this.cpb_stat_downloadrate.ForeColor = System.Drawing.Color.White;
            this.cpb_stat_downloadrate.InnerCircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(118)))), ((int)(((byte)(57)))), ((int)(((byte)(3)))));
            this.cpb_stat_downloadrate.InnerCircleMargin = 2;
            this.cpb_stat_downloadrate.InnerCircleWidth = 3;
            this.cpb_stat_downloadrate.Location = new System.Drawing.Point(16, 49);
            this.cpb_stat_downloadrate.MaxValue = 100F;
            this.cpb_stat_downloadrate.MinValue = 0F;
            this.cpb_stat_downloadrate.Name = "cpb_stat_downloadrate";
            this.cpb_stat_downloadrate.OuterCircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.cpb_stat_downloadrate.OuterCircleMargin = 1;
            this.cpb_stat_downloadrate.OuterCircleWidth = 1;
            this.cpb_stat_downloadrate.ProgressCircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(87)))), ((int)(((byte)(0)))));
            this.cpb_stat_downloadrate.ProgressCircleStartAngle = 270;
            this.cpb_stat_downloadrate.ProgressCircleWidth = 9;
            this.cpb_stat_downloadrate.Size = new System.Drawing.Size(82, 82);
            this.cpb_stat_downloadrate.SubText = "";
            this.cpb_stat_downloadrate.SubTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cpb_stat_downloadrate.SupSubFont = new System.Drawing.Font("Arial", 6F, System.Drawing.FontStyle.Bold);
            this.cpb_stat_downloadrate.SupText = "";
            this.cpb_stat_downloadrate.SupTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.cpb_stat_downloadrate.TabIndex = 1;
            this.cpb_stat_downloadrate.Value = 0F;
            this.cpb_stat_downloadrate.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            // 
            // frm_Main
            // 
            this.AcceptButton = this.btn_login;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.btn_minimize;
            this.ClientSize = new System.Drawing.Size(680, 300);
            this.ControlBox = false;
            this.Controls.Add(this.pnl_main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frm_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMainFormClosing);
            this.Load += new System.EventHandler(this.FrmMainLoad);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DragMouseDown);
            this.pnl_main.ResumeLayout(false);
            this.pnl_status.ResumeLayout(false);
            this.pnl_status.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_status)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.pnl_login.ResumeLayout(false);
            this.pnl_login.PerformLayout();
            this.pnl_host.ResumeLayout(false);
            this.pnl_host.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            this.pnl_password.ResumeLayout(false);
            this.pnl_password.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.pnl_username.ResumeLayout(false);
            this.pnl_username.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.pnl_details.ResumeLayout(false);
            this.pnl_details.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer statTimer;
        private System.Windows.Forms.TextBox txt_username;
        private System.Windows.Forms.Panel pnl_main;
        private System.Windows.Forms.Panel pnl_status;
        private System.Windows.Forms.Panel pnl_details;
        private System.Windows.Forms.Panel pnl_login;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.PictureBox pb_status;
        private MyLabel lbl_status;
        private MyLabel label6;
        private System.Windows.Forms.Panel panel5;
        private MyLabel lbl_stat_downloaded_v;
        private MyLabel lbl_stat_downloaded;
        private MyLabel lbl_stat_uploaded_v;
        private MyLabel lbl_stat_uploaded;
        private System.Windows.Forms.Button btn_disconnect;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button btn_exit;
        private System.Windows.Forms.Button btn_minimize;
        private System.Windows.Forms.Panel pnl_username;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Panel pnl_host;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.TextBox txt_server;
        private System.Windows.Forms.Panel pnl_password;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.TextBox txt_password;
        private System.Windows.Forms.Button btn_login;
        private CircularProgressBar.CircularProgressBar cpb_stat_downloadrate;
        private CircularProgressBar.CircularProgressBar cpb_stat_uploadrate;
        private MyLabel label1;
        private System.Windows.Forms.Panel panel9;
        private MyLabel label2;
        private MyLabel label12;
        private MyLabel label9;
        private MyLabel lbl_stat_acceptingthreads;
        private MyLabel label7;
        private MyLabel lbl_stat_activeconnections;
        private MyLabel label13;
        private MyLabel label4;
        private MyLabel label3;

    }
}

