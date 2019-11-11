namespace ImageMatch
{
    partial class Run
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.m_pic_ShowImage = new System.Windows.Forms.PictureBox();
            this.m_btnOpenCamera = new System.Windows.Forms.Button();
            this.m_btnCloseCamera = new System.Windows.Forms.Button();
            this.m_btnStartGrab = new System.Windows.Forms.Button();
            this.m_btnStopGrab = new System.Windows.Forms.Button();
            this.m_btnCorrect = new System.Windows.Forms.Button();
            this.m_checkChange = new System.Windows.Forms.CheckBox();
            this.m_picShowCorrectImage = new System.Windows.Forms.PictureBox();
            this.m_btnScreenShot = new System.Windows.Forms.Button();
            this.m_btnSaveScreenShot = new System.Windows.Forms.Button();
            this.m_btnMatch = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.m_btnWaitResponse = new System.Windows.Forms.Button();
            this.m_txtboxTargetStatus = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.m_pic_ShowImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picShowCorrectImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // m_pic_ShowImage
            // 
            this.m_pic_ShowImage.Location = new System.Drawing.Point(12, 12);
            this.m_pic_ShowImage.Name = "m_pic_ShowImage";
            this.m_pic_ShowImage.Size = new System.Drawing.Size(438, 352);
            this.m_pic_ShowImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_pic_ShowImage.TabIndex = 1;
            this.m_pic_ShowImage.TabStop = false;
            this.m_pic_ShowImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_pic_ShowImage_MouseDown);
            // 
            // m_btnOpenCamera
            // 
            this.m_btnOpenCamera.Location = new System.Drawing.Point(774, 12);
            this.m_btnOpenCamera.Name = "m_btnOpenCamera";
            this.m_btnOpenCamera.Size = new System.Drawing.Size(75, 23);
            this.m_btnOpenCamera.TabIndex = 2;
            this.m_btnOpenCamera.Text = "打开设备";
            this.m_btnOpenCamera.UseVisualStyleBackColor = true;
            this.m_btnOpenCamera.Click += new System.EventHandler(this.m_btnOpenCamera_Click);
            // 
            // m_btnCloseCamera
            // 
            this.m_btnCloseCamera.Location = new System.Drawing.Point(864, 11);
            this.m_btnCloseCamera.Name = "m_btnCloseCamera";
            this.m_btnCloseCamera.Size = new System.Drawing.Size(75, 23);
            this.m_btnCloseCamera.TabIndex = 3;
            this.m_btnCloseCamera.Text = "关闭设备";
            this.m_btnCloseCamera.UseVisualStyleBackColor = true;
            this.m_btnCloseCamera.Click += new System.EventHandler(this.m_btnCloseCamera_Click);
            // 
            // m_btnStartGrab
            // 
            this.m_btnStartGrab.Location = new System.Drawing.Point(774, 55);
            this.m_btnStartGrab.Name = "m_btnStartGrab";
            this.m_btnStartGrab.Size = new System.Drawing.Size(75, 23);
            this.m_btnStartGrab.TabIndex = 4;
            this.m_btnStartGrab.Text = "开始采集";
            this.m_btnStartGrab.UseVisualStyleBackColor = true;
            this.m_btnStartGrab.Click += new System.EventHandler(this.m_btnStartGrab_Click);
            // 
            // m_btnStopGrab
            // 
            this.m_btnStopGrab.Location = new System.Drawing.Point(864, 55);
            this.m_btnStopGrab.Name = "m_btnStopGrab";
            this.m_btnStopGrab.Size = new System.Drawing.Size(75, 23);
            this.m_btnStopGrab.TabIndex = 5;
            this.m_btnStopGrab.Text = "停止采集";
            this.m_btnStopGrab.UseVisualStyleBackColor = true;
            this.m_btnStopGrab.Click += new System.EventHandler(this.m_btnStopGrab_Click);
            // 
            // m_btnCorrect
            // 
            this.m_btnCorrect.Location = new System.Drawing.Point(774, 117);
            this.m_btnCorrect.Name = "m_btnCorrect";
            this.m_btnCorrect.Size = new System.Drawing.Size(75, 23);
            this.m_btnCorrect.TabIndex = 6;
            this.m_btnCorrect.Text = "校正";
            this.m_btnCorrect.UseVisualStyleBackColor = true;
            this.m_btnCorrect.Click += new System.EventHandler(this.m_btnCorrect_Click);
            // 
            // m_checkChange
            // 
            this.m_checkChange.AutoSize = true;
            this.m_checkChange.Location = new System.Drawing.Point(774, 95);
            this.m_checkChange.Name = "m_checkChange";
            this.m_checkChange.Size = new System.Drawing.Size(72, 16);
            this.m_checkChange.TabIndex = 7;
            this.m_checkChange.Text = "更换终端";
            this.m_checkChange.UseVisualStyleBackColor = true;
            this.m_checkChange.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // m_picShowCorrectImage
            // 
            this.m_picShowCorrectImage.Location = new System.Drawing.Point(456, 14);
            this.m_picShowCorrectImage.Name = "m_picShowCorrectImage";
            this.m_picShowCorrectImage.Size = new System.Drawing.Size(256, 413);
            this.m_picShowCorrectImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.m_picShowCorrectImage.TabIndex = 8;
            this.m_picShowCorrectImage.TabStop = false;
            this.m_picShowCorrectImage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.m_picShowCorrectImage_MouseDown);
            // 
            // m_btnScreenShot
            // 
            this.m_btnScreenShot.Location = new System.Drawing.Point(774, 160);
            this.m_btnScreenShot.Name = "m_btnScreenShot";
            this.m_btnScreenShot.Size = new System.Drawing.Size(75, 23);
            this.m_btnScreenShot.TabIndex = 9;
            this.m_btnScreenShot.Text = "截取图像块";
            this.m_btnScreenShot.UseVisualStyleBackColor = true;
            this.m_btnScreenShot.Click += new System.EventHandler(this.m_btnScreenShot_Click);
            // 
            // m_btnSaveScreenShot
            // 
            this.m_btnSaveScreenShot.Location = new System.Drawing.Point(864, 160);
            this.m_btnSaveScreenShot.Name = "m_btnSaveScreenShot";
            this.m_btnSaveScreenShot.Size = new System.Drawing.Size(75, 23);
            this.m_btnSaveScreenShot.TabIndex = 10;
            this.m_btnSaveScreenShot.Text = "保存截图";
            this.m_btnSaveScreenShot.UseVisualStyleBackColor = true;
            this.m_btnSaveScreenShot.Click += new System.EventHandler(this.m_btnSaveScreenShot_Click_1);
            // 
            // m_btnMatch
            // 
            this.m_btnMatch.Location = new System.Drawing.Point(774, 207);
            this.m_btnMatch.Name = "m_btnMatch";
            this.m_btnMatch.Size = new System.Drawing.Size(75, 23);
            this.m_btnMatch.TabIndex = 11;
            this.m_btnMatch.Text = "匹配";
            this.m_btnMatch.UseVisualStyleBackColor = true;
            this.m_btnMatch.Click += new System.EventHandler(this.m_btnMatch_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(737, 295);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(151, 226);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 13;
            this.pictureBox2.TabStop = false;
            // 
            // m_btnWaitResponse
            // 
            this.m_btnWaitResponse.Location = new System.Drawing.Point(864, 207);
            this.m_btnWaitResponse.Name = "m_btnWaitResponse";
            this.m_btnWaitResponse.Size = new System.Drawing.Size(75, 23);
            this.m_btnWaitResponse.TabIndex = 14;
            this.m_btnWaitResponse.Text = "等待响应";
            this.m_btnWaitResponse.UseVisualStyleBackColor = true;
            this.m_btnWaitResponse.Click += new System.EventHandler(this.m_btnWaitResponse_Click);
            // 
            // m_txtboxTargetStatus
            // 
            this.m_txtboxTargetStatus.Location = new System.Drawing.Point(774, 253);
            this.m_txtboxTargetStatus.Name = "m_txtboxTargetStatus";
            this.m_txtboxTargetStatus.Size = new System.Drawing.Size(100, 21);
            this.m_txtboxTargetStatus.TabIndex = 15;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 586);
            this.Controls.Add(this.m_txtboxTargetStatus);
            this.Controls.Add(this.m_btnWaitResponse);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.m_btnMatch);
            this.Controls.Add(this.m_btnSaveScreenShot);
            this.Controls.Add(this.m_btnScreenShot);
            this.Controls.Add(this.m_picShowCorrectImage);
            this.Controls.Add(this.m_checkChange);
            this.Controls.Add(this.m_btnCorrect);
            this.Controls.Add(this.m_btnStopGrab);
            this.Controls.Add(this.m_btnStartGrab);
            this.Controls.Add(this.m_btnCloseCamera);
            this.Controls.Add(this.m_btnOpenCamera);
            this.Controls.Add(this.m_pic_ShowImage);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.m_pic_ShowImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_picShowCorrectImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox m_pic_ShowImage;
        private System.Windows.Forms.Button m_btnOpenCamera;
        private System.Windows.Forms.Button m_btnCloseCamera;
        private System.Windows.Forms.Button m_btnStartGrab;
        private System.Windows.Forms.Button m_btnStopGrab;
        private System.Windows.Forms.Button m_btnCorrect;
        private System.Windows.Forms.CheckBox m_checkChange;
        private System.Windows.Forms.PictureBox m_picShowCorrectImage;
        private System.Windows.Forms.Button m_btnScreenShot;
        private System.Windows.Forms.Button m_btnSaveScreenShot;
        private System.Windows.Forms.Button m_btnMatch;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button m_btnWaitResponse;
        private System.Windows.Forms.TextBox m_txtboxTargetStatus;

    }
}

