﻿
namespace CameraTrackingAppv3
{
    partial class Form5
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
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button7 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 86);
            this.button1.TabIndex = 0;
            this.button1.Text = "左クリック";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.button1.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.button1.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(192, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(84, 86);
            this.button2.TabIndex = 1;
            this.button2.Text = "ダブルクリック";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            this.button2.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.button2.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(282, 12);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(84, 86);
            this.button3.TabIndex = 2;
            this.button3.Text = "ドラッグ";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            this.button3.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.button3.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(102, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(84, 86);
            this.button4.TabIndex = 3;
            this.button4.Text = "右クリック";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            this.button4.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.button4.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(372, 12);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(84, 86);
            this.button5.TabIndex = 4;
            this.button5.Text = "上スクロール";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            this.button5.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.button5.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(462, 12);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(84, 86);
            this.button6.TabIndex = 5;
            this.button6.Text = "下スクロール";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            this.button6.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.button6.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "label1";
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(573, 12);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(93, 86);
            this.button7.TabIndex = 7;
            this.button7.Text = "中心点の変更";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // Form5
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(679, 130);
            this.ControlBox = false;
            this.Controls.Add(this.button7);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form5";
            this.Text = "Form5";
            this.TopMost = true;
            this.MouseEnter += new System.EventHandler(this.Form5_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.Form5_MouseLeave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button7;
    }
}