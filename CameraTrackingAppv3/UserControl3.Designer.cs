
namespace CameraTrackingAppv3
{
    partial class UserControl3
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.userControl21 = new CameraTrackingAppv3.UserControl2();
            this.userControl22 = new CameraTrackingAppv3.UserControl2();
            this.userControl23 = new CameraTrackingAppv3.UserControl2();
            this.userControl24 = new CameraTrackingAppv3.UserControl2();
            this.SuspendLayout();
            // 
            // userControl21
            // 
            this.userControl21.Location = new System.Drawing.Point(-1, 3);
            this.userControl21.Name = "userControl21";
            this.userControl21.Size = new System.Drawing.Size(99, 128);
            this.userControl21.TabIndex = 0;
            // 
            // userControl22
            // 
            this.userControl22.Location = new System.Drawing.Point(97, 3);
            this.userControl22.Name = "userControl22";
            this.userControl22.Size = new System.Drawing.Size(99, 128);
            this.userControl22.TabIndex = 1;
            // 
            // userControl23
            // 
            this.userControl23.Location = new System.Drawing.Point(195, 3);
            this.userControl23.Name = "userControl23";
            this.userControl23.Size = new System.Drawing.Size(99, 128);
            this.userControl23.TabIndex = 2;
            // 
            // userControl24
            // 
            this.userControl24.Location = new System.Drawing.Point(293, 3);
            this.userControl24.Name = "userControl24";
            this.userControl24.Size = new System.Drawing.Size(99, 128);
            this.userControl24.TabIndex = 3;
            // 
            // UserControl3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.userControl24);
            this.Controls.Add(this.userControl23);
            this.Controls.Add(this.userControl22);
            this.Controls.Add(this.userControl21);
            this.Name = "UserControl3";
            this.Size = new System.Drawing.Size(392, 131);
            this.ResumeLayout(false);

        }

        #endregion

        private UserControl2 userControl21;
        private UserControl2 userControl22;
        private UserControl2 userControl23;
        private UserControl2 userControl24;
    }
}
