using System.Drawing;
using System.Windows.Forms;

namespace CameraTrackingAppv3
{

    public partial class UserControl1 : UserControl
    {
        public static string camera_name = "";
        public Graphics PictureGraphics { get; private set; }

        public int PictureWidth { get { return pictureBox1.Width; } }
        public int PictureHeight { get { return pictureBox1.Height; } }

        public UserControl1()
        {
            InitializeComponent();
            PictureGraphics = pictureBox1.CreateGraphics();
            SetCameraName(camera_name);
        }

        public void PictureClear()
        {
            PictureGraphics.Clear(BackColor);
        }

        public void DrawImage(Bitmap bitmap, Point offset, Point size)
        {
            PictureGraphics.DrawImage(bitmap, offset.X, offset.Y, size.X, size.Y);
        }

        public void SetFPS(int fps)
        {
            label1.Text = "FPS:" + fps.ToString();
        }

        public void SetCameraName(string name)
        {
            label2.Text = "起動中:"+name;
            camera_name = name;
        }

        public void VisibleFPS(bool visible)
        {
            label1.Visible = visible;
        }

        public void VisibleCameraName(bool visible)
        {
            label2.Visible = visible;
        }

        public void VisiblePictureBox(bool visible)
        {
            pictureBox1.Visible = visible;
        }

        public int GetPictureBoxTop()
        {
            return pictureBox1.Top;
        }

        public Point GetPictureBoxCenterPoint()
        {

            return new Point(
                pictureBox1.Location.X + (int)(pictureBox1.Size.Width * 0.5),
                pictureBox1.Location.Y + (int)(pictureBox1.Size.Height * 0.5)
                );
        }

        public void DisplayClear()
        {
            VisibleCameraName(false);
            VisibleFPS(false);
            //PictureClear();
        }

    }
}
