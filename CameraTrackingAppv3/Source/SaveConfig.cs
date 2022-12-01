using System;
using System.Collections.Generic;
using System.Text;
using OpenCvSharp;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace CameraTrackingAppv3
{
    [Serializable()]
    public class SettingsConfig:IDisposable
    {
        static readonly string save_path = Utils.PathResource + "\\settings.config";
        public Vec2d[] Range_of_motion { get; set; } = null;

        public string CameraID { get; set; } = "";

        static public string GetPathSave { get { return save_path; } }

        
        public SettingsConfig()
        {

        }
        public SettingsConfig(Vec2d[] range_of_motion,string camera_id)
        {
            this.Range_of_motion = range_of_motion;
            this.CameraID = camera_id;
        }

        public SettingsConfig(SettingsConfig config)
        {
            Set(config);
        }

        public void Set(SettingsConfig config)
        {
            this.Range_of_motion = config.Range_of_motion;
            this.CameraID = config.CameraID;
        }


        public bool Save()
        {
            bool ok = System.IO.Directory.Exists(Utils.PathResource);
            BinaryFormatter bf1 = new BinaryFormatter();

            System.IO.FileStream fs1 = new System.IO.FileStream(save_path, System.IO.FileMode.Create);

            bf1.Serialize(fs1, this);
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
                settings = (SettingsConfig)bf2.Deserialize(fs2);
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

        public override bool Equals(object obj)
        {
            if (!(obj is SettingsConfig)) return false;

            SettingsConfig a = (SettingsConfig)obj;

            return (CameraID.Equals(a.CameraID) && Range_of_motion.Equals(a.Range_of_motion));
                

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Dispose()
        {
        }


        public void Adapt()
        {
            CursorControl.SetRangeOfMotion(Utils.Config.Range_of_motion);
            CursorControl.IsRangeOfMotion = true;
        }

    }
}
