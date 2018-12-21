namespace G_H_Transplantation_For_AMAT
{
    partial class Select_Operation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Select_Operation));
            this.alne = new System.Windows.Forms.Button();
            this.ex = new System.Windows.Forms.Button();
            this.label27 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.denglu_ipstatus = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.denglu_conip = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.timer4 = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.neco = new System.Windows.Forms.Button();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // alne
            // 
            this.alne.BackColor = System.Drawing.Color.Transparent;
            this.alne.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("alne.BackgroundImage")));
            this.alne.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.alne.FlatAppearance.BorderSize = 0;
            this.alne.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.alne.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.alne.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.alne.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alne.ForeColor = System.Drawing.Color.White;
            this.alne.Location = new System.Drawing.Point(25, 257);
            this.alne.Name = "alne";
            this.alne.Size = new System.Drawing.Size(210, 90);
            this.alne.TabIndex = 6;
            this.alne.Text = "Append G-HWDEF\r\nTo All Parts";
            this.alne.UseVisualStyleBackColor = false;
            this.alne.Click += new System.EventHandler(this.alne_Click);
            // 
            // ex
            // 
            this.ex.BackColor = System.Drawing.Color.Transparent;
            this.ex.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ex.BackgroundImage")));
            this.ex.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ex.Enabled = false;
            this.ex.FlatAppearance.BorderSize = 0;
            this.ex.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.ex.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.ex.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ex.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ex.ForeColor = System.Drawing.Color.White;
            this.ex.Location = new System.Drawing.Point(489, 257);
            this.ex.Name = "ex";
            this.ex.Size = new System.Drawing.Size(210, 90);
            this.ex.TabIndex = 8;
            this.ex.Text = "RB/Controller/Aligner\r\nExchange";
            this.ex.UseVisualStyleBackColor = false;
            this.ex.Click += new System.EventHandler(this.ex_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Cambria", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label27.Location = new System.Drawing.Point(34, 82);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(294, 22);
            this.label27.TabIndex = 9;
            this.label27.Text = "Select operation which you want.\r\n";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Cambria", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(34, 109);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(561, 110);
            this.label1.TabIndex = 10;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // denglu_ipstatus
            // 
            this.denglu_ipstatus.BackColor = System.Drawing.Color.Yellow;
            this.denglu_ipstatus.Font = new System.Drawing.Font("Cambria", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.denglu_ipstatus.ForeColor = System.Drawing.Color.Red;
            this.denglu_ipstatus.Location = new System.Drawing.Point(565, 11);
            this.denglu_ipstatus.Name = "denglu_ipstatus";
            this.denglu_ipstatus.Padding = new System.Windows.Forms.Padding(12, 5, 0, 0);
            this.denglu_ipstatus.Size = new System.Drawing.Size(110, 30);
            this.denglu_ipstatus.TabIndex = 15;
            this.denglu_ipstatus.Text = "OFFLINE";
            // 
            // label28
            // 
            this.label28.BackColor = System.Drawing.Color.Gainsboro;
            this.label28.Location = new System.Drawing.Point(562, 8);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(116, 36);
            this.label28.TabIndex = 16;
            // 
            // denglu_conip
            // 
            this.denglu_conip.Font = new System.Drawing.Font("メイリオ", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.denglu_conip.Location = new System.Drawing.Point(111, 14);
            this.denglu_conip.Name = "denglu_conip";
            this.denglu_conip.ReadOnly = true;
            this.denglu_conip.Size = new System.Drawing.Size(259, 27);
            this.denglu_conip.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "Controller IP";
            // 
            // timer4
            // 
            this.timer4.Enabled = true;
            this.timer4.Interval = 1000;
            this.timer4.Tick += new System.EventHandler(this.timer4_Tick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightCyan;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.denglu_ipstatus);
            this.panel1.Controls.Add(this.label28);
            this.panel1.Controls.Add(this.denglu_conip);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(690, 55);
            this.panel1.TabIndex = 17;
            // 
            // neco
            // 
            this.neco.BackColor = System.Drawing.Color.Transparent;
            this.neco.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("neco.BackgroundImage")));
            this.neco.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.neco.Enabled = false;
            this.neco.FlatAppearance.BorderSize = 0;
            this.neco.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.neco.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.neco.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.neco.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.neco.ForeColor = System.Drawing.Color.White;
            this.neco.Location = new System.Drawing.Point(260, 257);
            this.neco.Name = "neco";
            this.neco.Size = new System.Drawing.Size(210, 90);
            this.neco.TabIndex = 7;
            this.neco.Text = "New Combination";
            this.neco.UseVisualStyleBackColor = false;
            this.neco.Click += new System.EventHandler(this.neco_Click);
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox6.Image")));
            this.pictureBox6.Location = new System.Drawing.Point(33, 132);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(24, 22);
            this.pictureBox6.TabIndex = 29;
            this.pictureBox6.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(33, 154);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 22);
            this.pictureBox1.TabIndex = 30;
            this.pictureBox1.TabStop = false;
            // 
            // Select_Operation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(717, 362);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBox6);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.ex);
            this.Controls.Add(this.neco);
            this.Controls.Add(this.alne);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(733, 400);
            this.MinimumSize = new System.Drawing.Size(733, 400);
            this.Name = "Select_Operation";
            this.Text = "G-H Transplantation For AMAT Ver.1.5.0.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectOperation_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SelectOperation_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button alne;
        private System.Windows.Forms.Button ex;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label denglu_ipstatus;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.TextBox denglu_conip;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Timer timer4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button neco;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}