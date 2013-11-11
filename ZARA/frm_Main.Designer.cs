namespace ZARA
{
    partial class frm_Main
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_Exit = new System.Windows.Forms.Button();
            this.btn_minimize = new System.Windows.Forms.Button();
            this.btn_control = new System.Windows.Forms.Button();
            this.gb_general = new System.Windows.Forms.GroupBox();
            this.lbl_stat_uploadrate = new System.Windows.Forms.Label();
            this.lbl_stat_uploaded = new System.Windows.Forms.Label();
            this.lbl_stat_downloadrate = new System.Windows.Forms.Label();
            this.lbl_stat_downloaded = new System.Windows.Forms.Label();
            this.lbl_stat_activeconnections = new System.Windows.Forms.Label();
            this.lbl_stat_acceptingthreads = new System.Windows.Forms.Label();
            this.lbl_rq2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lbl_rq1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.gb_connection = new System.Windows.Forms.GroupBox();
            this.txt_username = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_server = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_password = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.gb_general.SuspendLayout();
            this.gb_connection.SuspendLayout();
            this.SuspendLayout();
            // 
            // statTimer
            // 
            this.statTimer.Enabled = true;
            this.statTimer.Interval = 1000;
            this.statTimer.Tick += new System.EventHandler(this.statTimer_Tick);
            // 
            // panel1
            // 
            this.panel1.BackgroundImage = global::ZARA.Properties.Resources.Background;
            this.panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.panel1.Controls.Add(this.btn_Exit);
            this.panel1.Controls.Add(this.btn_minimize);
            this.panel1.Controls.Add(this.btn_control);
            this.panel1.Controls.Add(this.gb_general);
            this.panel1.Controls.Add(this.gb_connection);
            this.panel1.Location = new System.Drawing.Point(-64, -31);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(497, 314);
            this.panel1.TabIndex = 3;
            // 
            // btn_Exit
            // 
            this.btn_Exit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_Exit.Location = new System.Drawing.Point(79, 159);
            this.btn_Exit.Name = "btn_Exit";
            this.btn_Exit.Size = new System.Drawing.Size(73, 23);
            this.btn_Exit.TabIndex = 3;
            this.btn_Exit.Text = "&Exit";
            this.btn_Exit.UseVisualStyleBackColor = true;
            this.btn_Exit.Click += new System.EventHandler(this.btn_Exit_Click);
            // 
            // btn_minimize
            // 
            this.btn_minimize.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_minimize.Location = new System.Drawing.Point(158, 159);
            this.btn_minimize.Name = "btn_minimize";
            this.btn_minimize.Size = new System.Drawing.Size(73, 23);
            this.btn_minimize.TabIndex = 2;
            this.btn_minimize.Text = "&Minimize";
            this.btn_minimize.UseVisualStyleBackColor = true;
            this.btn_minimize.Click += new System.EventHandler(this.btn_minimize_Click);
            // 
            // btn_control
            // 
            this.btn_control.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_control.Location = new System.Drawing.Point(303, 159);
            this.btn_control.Name = "btn_control";
            this.btn_control.Size = new System.Drawing.Size(73, 23);
            this.btn_control.TabIndex = 1;
            this.btn_control.Text = "&Start";
            this.btn_control.UseVisualStyleBackColor = true;
            this.btn_control.Click += new System.EventHandler(this.btn_control_Click);
            // 
            // gb_general
            // 
            this.gb_general.BackColor = System.Drawing.Color.Transparent;
            this.gb_general.Controls.Add(this.lbl_stat_uploadrate);
            this.gb_general.Controls.Add(this.lbl_stat_uploaded);
            this.gb_general.Controls.Add(this.lbl_stat_downloadrate);
            this.gb_general.Controls.Add(this.lbl_stat_downloaded);
            this.gb_general.Controls.Add(this.lbl_stat_activeconnections);
            this.gb_general.Controls.Add(this.lbl_stat_acceptingthreads);
            this.gb_general.Controls.Add(this.lbl_rq2);
            this.gb_general.Controls.Add(this.label7);
            this.gb_general.Controls.Add(this.lbl_rq1);
            this.gb_general.Controls.Add(this.label5);
            this.gb_general.Controls.Add(this.label4);
            this.gb_general.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gb_general.Location = new System.Drawing.Point(79, 41);
            this.gb_general.Name = "gb_general";
            this.gb_general.Size = new System.Drawing.Size(297, 108);
            this.gb_general.TabIndex = 6;
            this.gb_general.TabStop = false;
            this.gb_general.Text = "General Stats";
            this.gb_general.Visible = false;
            // 
            // lbl_stat_uploadrate
            // 
            this.lbl_stat_uploadrate.AutoSize = true;
            this.lbl_stat_uploadrate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(50)))), ((int)(((byte)(0)))));
            this.lbl_stat_uploadrate.Location = new System.Drawing.Point(233, 74);
            this.lbl_stat_uploadrate.Name = "lbl_stat_uploadrate";
            this.lbl_stat_uploadrate.Size = new System.Drawing.Size(30, 13);
            this.lbl_stat_uploadrate.TabIndex = 5;
            this.lbl_stat_uploadrate.Text = "0 KB";
            // 
            // lbl_stat_uploaded
            // 
            this.lbl_stat_uploaded.AutoSize = true;
            this.lbl_stat_uploaded.Location = new System.Drawing.Point(233, 48);
            this.lbl_stat_uploaded.Name = "lbl_stat_uploaded";
            this.lbl_stat_uploaded.Size = new System.Drawing.Size(30, 13);
            this.lbl_stat_uploaded.TabIndex = 5;
            this.lbl_stat_uploaded.Text = "0 KB";
            // 
            // lbl_stat_downloadrate
            // 
            this.lbl_stat_downloadrate.AutoSize = true;
            this.lbl_stat_downloadrate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(170)))), ((int)(((byte)(255)))));
            this.lbl_stat_downloadrate.Location = new System.Drawing.Point(101, 74);
            this.lbl_stat_downloadrate.Name = "lbl_stat_downloadrate";
            this.lbl_stat_downloadrate.Size = new System.Drawing.Size(30, 13);
            this.lbl_stat_downloadrate.TabIndex = 5;
            this.lbl_stat_downloadrate.Text = "0 KB";
            // 
            // lbl_stat_downloaded
            // 
            this.lbl_stat_downloaded.AutoSize = true;
            this.lbl_stat_downloaded.Location = new System.Drawing.Point(101, 48);
            this.lbl_stat_downloaded.Name = "lbl_stat_downloaded";
            this.lbl_stat_downloaded.Size = new System.Drawing.Size(30, 13);
            this.lbl_stat_downloaded.TabIndex = 5;
            this.lbl_stat_downloaded.Text = "0 KB";
            // 
            // lbl_stat_activeconnections
            // 
            this.lbl_stat_activeconnections.AutoSize = true;
            this.lbl_stat_activeconnections.Location = new System.Drawing.Point(233, 22);
            this.lbl_stat_activeconnections.Name = "lbl_stat_activeconnections";
            this.lbl_stat_activeconnections.Size = new System.Drawing.Size(13, 13);
            this.lbl_stat_activeconnections.TabIndex = 4;
            this.lbl_stat_activeconnections.Text = "0";
            // 
            // lbl_stat_acceptingthreads
            // 
            this.lbl_stat_acceptingthreads.AutoSize = true;
            this.lbl_stat_acceptingthreads.Location = new System.Drawing.Point(174, 22);
            this.lbl_stat_acceptingthreads.Name = "lbl_stat_acceptingthreads";
            this.lbl_stat_acceptingthreads.Size = new System.Drawing.Size(13, 13);
            this.lbl_stat_acceptingthreads.TabIndex = 4;
            this.lbl_stat_acceptingthreads.Text = "0";
            // 
            // lbl_rq2
            // 
            this.lbl_rq2.AutoSize = true;
            this.lbl_rq2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(50)))), ((int)(((byte)(0)))));
            this.lbl_rq2.Location = new System.Drawing.Point(157, 74);
            this.lbl_rq2.Name = "lbl_rq2";
            this.lbl_rq2.Size = new System.Drawing.Size(70, 13);
            this.lbl_rq2.TabIndex = 4;
            this.lbl_rq2.Text = "Upload Rate:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(157, 48);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 13);
            this.label7.TabIndex = 4;
            this.label7.Text = "Uploaded:";
            // 
            // lbl_rq1
            // 
            this.lbl_rq1.AutoSize = true;
            this.lbl_rq1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(170)))), ((int)(((byte)(255)))));
            this.lbl_rq1.Location = new System.Drawing.Point(11, 74);
            this.lbl_rq1.Name = "lbl_rq1";
            this.lbl_rq1.Size = new System.Drawing.Size(84, 13);
            this.lbl_rq1.TabIndex = 4;
            this.lbl_rq1.Text = "Download Rate:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(70, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Downloaded:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(154, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Cycles and Connections (A, R):";
            // 
            // gb_connection
            // 
            this.gb_connection.BackColor = System.Drawing.Color.Transparent;
            this.gb_connection.Controls.Add(this.txt_username);
            this.gb_connection.Controls.Add(this.label1);
            this.gb_connection.Controls.Add(this.txt_server);
            this.gb_connection.Controls.Add(this.label2);
            this.gb_connection.Controls.Add(this.txt_password);
            this.gb_connection.Controls.Add(this.label3);
            this.gb_connection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.gb_connection.Location = new System.Drawing.Point(79, 41);
            this.gb_connection.Name = "gb_connection";
            this.gb_connection.Size = new System.Drawing.Size(297, 108);
            this.gb_connection.TabIndex = 0;
            this.gb_connection.TabStop = false;
            this.gb_connection.Text = "Connection Details";
            // 
            // txt_username
            // 
            this.txt_username.Location = new System.Drawing.Point(92, 19);
            this.txt_username.Name = "txt_username";
            this.txt_username.Size = new System.Drawing.Size(187, 20);
            this.txt_username.TabIndex = 1;
            this.txt_username.Leave += new System.EventHandler(this.txt_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label1.Location = new System.Drawing.Point(11, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username:";
            // 
            // txt_server
            // 
            this.txt_server.Location = new System.Drawing.Point(92, 71);
            this.txt_server.Name = "txt_server";
            this.txt_server.Size = new System.Drawing.Size(187, 20);
            this.txt_server.TabIndex = 5;
            this.txt_server.Leave += new System.EventHandler(this.txt_server_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label2.Location = new System.Drawing.Point(11, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Password:";
            // 
            // txt_password
            // 
            this.txt_password.Location = new System.Drawing.Point(92, 45);
            this.txt_password.Name = "txt_password";
            this.txt_password.PasswordChar = '*';
            this.txt_password.Size = new System.Drawing.Size(187, 20);
            this.txt_password.TabIndex = 3;
            this.txt_password.Leave += new System.EventHandler(this.txt_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.label3.Location = new System.Drawing.Point(11, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Server:";
            // 
            // frm_Main
            // 
            this.AcceptButton = this.btn_control;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_minimize;
            this.ClientSize = new System.Drawing.Size(328, 160);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frm_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Z A Я A Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frm_Main_FormClosing);
            this.Load += new System.EventHandler(this.frm_Main_Load);
            this.panel1.ResumeLayout(false);
            this.gb_general.ResumeLayout(false);
            this.gb_general.PerformLayout();
            this.gb_connection.ResumeLayout(false);
            this.gb_connection.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer statTimer;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_Exit;
        private System.Windows.Forms.Button btn_minimize;
        private System.Windows.Forms.Button btn_control;
        private System.Windows.Forms.GroupBox gb_connection;
        private System.Windows.Forms.TextBox txt_username;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_server;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_password;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.Label lbl_stat_uploadrate;
        private System.Windows.Forms.Label lbl_stat_uploaded;
        private System.Windows.Forms.Label lbl_stat_downloadrate;
        private System.Windows.Forms.Label lbl_stat_downloaded;
        private System.Windows.Forms.Label lbl_stat_activeconnections;
        private System.Windows.Forms.Label lbl_stat_acceptingthreads;
        private System.Windows.Forms.Label lbl_rq2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbl_rq1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;

    }
}

