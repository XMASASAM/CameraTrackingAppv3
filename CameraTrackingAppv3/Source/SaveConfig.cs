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

        public void  Set(SettingProps props)
        {
            this.RangeOfMotion = props.RangeOfMotion;
            this.CameraID = props.CameraID;
        }

    }

    
    public class SettingsConfig:IDisposable
    {
        static readonly string save_path = Utils.PathResource + "\\settings.config";

        public SettingProps Property;

        public VideoCapture VideoCapture { get; set; } = null;

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
            this.VideoCapture = config.VideoCapture;
        }

        public bool Save()
        {
            bool ok = System.IO.Directory.Exists(Utils.PathResource);
            BinaryFormatter bf1 = new BinaryFormatter();

            System.IO.FileStream fs1 = new System.IO.FileStream(save_path, System.IO.FileMode.Create);

            bf1.Serialize(fs1, Property);
            fs1.Close();
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
            CursorControl.SetRangeOfMotion(config.Property.RangeOfMotion);
            CursorControl.IsRangeOfMotion = true;

        }

    }
}
