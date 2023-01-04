using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace CameraTrackingAppv3
{

    [Serializable()]
    public class SettingProps
    {
        public RangeOfMotionProps RangeOfMotion;
        public string CameraID = "";
        public double MoveMag = 1;
        public double ThresholdMag = 3;
        public int CameraAngle = 0;
        public double AxisXMag = 1;
        public double AxisYMag = 1;
        public int PortNumber = 62355;
        public double ClickInterval = 0.5;
        public double DoubleClickInterval = 0.5;
        public int InfraredFirstThreshold = 180;
        public int InfraredFirstErodeIteration = 1;
        public int InfraredTrackErodeIteration = 0;
        public double TrackingTargetMean=0;
        public double TrackingTargetAround=0;

        public void  Set(SettingProps props)
        {
            RangeOfMotion = props.RangeOfMotion;
            CameraID = props.CameraID;
            CameraAngle = props.CameraAngle;
            MoveMag = props.MoveMag;
            ThresholdMag = props.ThresholdMag;
            AxisXMag = props.AxisXMag;
            AxisYMag = props.AxisYMag;
            PortNumber = props.PortNumber;
            ClickInterval = props.ClickInterval;
            DoubleClickInterval = props.DoubleClickInterval;
            InfraredFirstThreshold = props.InfraredFirstThreshold;
            InfraredFirstErodeIteration = props.InfraredFirstErodeIteration;
            InfraredTrackErodeIteration = props.InfraredTrackErodeIteration;
            TrackingTargetMean = props.TrackingTargetMean;
            TrackingTargetAround = props.TrackingTargetAround;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SettingProps)) return false;
            SettingProps p = (SettingProps)obj;

            return RangeOfMotion.Equals(p.RangeOfMotion) &&
                CameraID.Equals(p.CameraID) &&
                MoveMag == p.MoveMag &&
                ThresholdMag == p.ThresholdMag &&
                CameraAngle == p.CameraAngle &&
                AxisXMag == p.AxisXMag &&
                AxisYMag == p.AxisYMag &&
                PortNumber == p.PortNumber &&
                ClickInterval == p.ClickInterval &&
                DoubleClickInterval == p.DoubleClickInterval &&
                InfraredFirstThreshold == p.InfraredFirstThreshold &&
                InfraredFirstErodeIteration == p.InfraredFirstErodeIteration &&
                InfraredTrackErodeIteration == p.InfraredTrackErodeIteration &&
                TrackingTargetMean == p.TrackingTargetMean &&
                TrackingTargetAround == p.TrackingTargetAround;

        }


    }

    
    public class SettingsConfig:IDisposable
    {
        static readonly string save_path = Utils.PathResource + "\\settings.config";
        static readonly string tracking_target_image_path = Utils.PathResource + "\\infraref_tracking_target_image.png";
        public SettingProps Property;

        public VideoCapture VideoCapture { get; set; } = null;
        Mat tracking_target_mat = null;
        bool f_change_tracking_target_mat = false;
        public Mat TrackingTargetImage { 
            get { 
                return tracking_target_mat; 
            }
            set {
                if (tracking_target_mat != null && !tracking_target_mat.IsDisposed&&!tracking_target_mat.Empty())
                {
                    if (tracking_target_mat == value)
                        return;

                    tracking_target_mat.Dispose();
                }

                f_change_tracking_target_mat = true;
                tracking_target_mat = value;
            } }
        public bool IsHaveTargetImage { get {
                return TrackingTargetImage != null && !TrackingTargetImage.Empty();
            } 
        }

        static public string GetPathSave { get { return save_path; } }

        
        public SettingsConfig()
        {
            Property = new SettingProps();
        }

        public SettingsConfig(SettingProps props)
        {
            Property = props;
           // RangeOfMotion = new RangeOfMotionProps(Property.RangeOfMotionPoints);
        }
        public SettingsConfig(SettingsConfig config)
        {
            Property = new SettingProps();
            Set(config);
            //RangeOfMotion = new RangeOfMotionProps(Property.RangeOfMotionPoints);

        }

        public void Set(SettingsConfig config)
        {
            Property.Set(config.Property);
            TrackingTargetImage = config.TrackingTargetImage;
            this.VideoCapture = config.VideoCapture;
        }

        public bool Save()
        {
            bool ok = System.IO.Directory.Exists(Utils.PathResource);
            BinaryFormatter bf1 = new BinaryFormatter();

            System.IO.FileStream fs1 = new System.IO.FileStream(save_path, System.IO.FileMode.Create);

            bf1.Serialize(fs1, Property);
            fs1.Close();

            if (IsHaveTargetImage && f_change_tracking_target_mat)
            {
                f_change_tracking_target_mat = false;
                Cv2.ImWrite(tracking_target_image_path,TrackingTargetImage);
            }

            return ok;
        }

        static public bool Load(out SettingsConfig settings)
        {
            bool ok = true;
            settings = new SettingsConfig();
            if (!System.IO.File.Exists(save_path)) return false;

            BinaryFormatter bf2 = new BinaryFormatter();

            System.IO.FileStream fs2 = new System.IO.FileStream(save_path, System.IO.FileMode.Open);
            try
            {
                var prop = (SettingProps)bf2.Deserialize(fs2);
                settings = new SettingsConfig(prop);
                Utils.WriteLine("正常に設定をロードできました");

            }
            catch (Exception e)
            {
                ok = false;
                Utils.WriteLine("警告!!!!:正常に設定をロードできませんでした");
            }

            fs2.Close();

            if (settings.Property.PortNumber == 0)
            {
                settings.Property.PortNumber = 62355;
            }


            if(System.IO.File.Exists(tracking_target_image_path))
            {
                using(Mat mat = Cv2.ImRead(tracking_target_image_path))
                settings.tracking_target_mat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);

            }


            return ok;
        }

      /*  public override bool Equals(object obj)
        {
            if (!(obj is SettingsConfig)) return false;

            SettingsConfig a = (SettingsConfig)obj;

            return (Property.CameraID.Equals(a.Property.CameraID) && Property.RangeOfMotion.Equals(a.Property.RangeOfMotion));
                

        }*/

     /*   public override int GetHashCode()
        {
            return base.GetHashCode();
        }*/

        public void Dispose()
        {
        }


        static public void Adapt(SettingsConfig config)
        {
            if (config.VideoCapture == null)
                throw new Exception("videoCaptureがnullのため適応させられません");
            CursorControl.SetRangeOfMotion(config.Property.RangeOfMotion);
            CursorControl.IsRangeOfMotion = true;
            Main.SetRotate(config.Property.CameraAngle);
            Main.GetConnect().Init(config.Property.PortNumber);
            CursorControl.DwellThresholdTime = (int)(1000*config.Property.ClickInterval);
            MouseControl.WaitTimeDouble = (int)config.Property.DoubleClickInterval;
        }

    }
}
