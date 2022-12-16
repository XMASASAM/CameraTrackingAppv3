
namespace CameraTrackingAppv3
{
    partial class UserControl2
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            ""}, -1);
            this.listView1 = new System.Windows.Forms.ListView();
            this.ユーザ名 = new System.Windows.Forms.ColumnHeader();
            this.機種名 = new System.Windows.Forms.ColumnHeader();
            this.IPアドレス = new System.Windows.Forms.ColumnHeader();
            this.MACアドレス = new System.Windows.Forms.ColumnHeader();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ユーザ名,
            this.機種名,
            this.IPアドレス,
            this.MACアドレス});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView1.Location = new System.Drawing.Point(0, 0);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(328, 168);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // ユーザ名
            // 
            this.ユーザ名.Text = "ユーザ名";
            this.ユーザ名.Width = 82;
            // 
            // 機種名
            // 
            this.機種名.Text = "機種名";
            this.機種名.Width = 82;
            // 
            // IPアドレス
            // 
            this.IPアドレス.Text = "IPアドレス";
            this.IPアドレス.Width = 82;
            // 
            // MACアドレス
            // 
            this.MACアドレス.Text = "MACアドレス";
            this.MACアドレス.Width = 82;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // UserControl2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listView1);
            this.Name = "UserControl2";
            this.Size = new System.Drawing.Size(328, 168);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader ユーザ名;
        private System.Windows.Forms.ColumnHeader 機種名;
        private System.Windows.Forms.ColumnHeader IPアドレス;
        private System.Windows.Forms.ColumnHeader MACアドレス;
        private System.Windows.Forms.Timer timer1;
    }
}
