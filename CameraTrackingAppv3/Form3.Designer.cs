﻿
namespace CameraTrackingAppv3
{
    partial class Form3
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.userControl11 = new CameraTrackingAppv3.UserControl1();
            this.button3 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(167, 214);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 55);
            this.button1.TabIndex = 1;
            this.button1.Text = "設定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(11, 214);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(150, 55);
            this.button2.TabIndex = 2;
            this.button2.Text = "カメラ非表示";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(12, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(306, 87);
            this.button4.TabIndex = 4;
            this.button4.Text = "操作開始";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // userControl11
            // 
            this.userControl11.Location = new System.Drawing.Point(11, 275);
            this.userControl11.Name = "userControl11";
            this.userControl11.Size = new System.Drawing.Size(306, 285);
            this.userControl11.TabIndex = 5;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 105);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(306, 47);
            this.button3.TabIndex = 6;
            this.button3.Text = "終了";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(12, 161);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(306, 47);
            this.button5.TabIndex = 7;
            this.button5.Text = "待機";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 572);
            this.ControlBox = false;
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.userControl11);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form3";
            this.Text = "Form3";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form3_FormClosed);
            this.Load += new System.EventHandler(this.Form3_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button4;
        private UserControl1 userControl11;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button5;
    }
}