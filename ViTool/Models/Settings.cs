using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViTool.Models
{
    public class Settings
    {

        private readonly string SettingsFileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VITools\\Settings.json";
        private string MainDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\VITools";


        public SettingsData ReadSettings()
        {
            string json;

            if (!Directory.Exists(MainDataDirectory))
                Directory.CreateDirectory(MainDataDirectory);

            if (!File.Exists(SettingsFileName))
                CreateSettingsFile();

            using (StreamReader streamReader = new StreamReader(SettingsFileName))
                json = streamReader.ReadToEnd();


            if (json == null || !IsValidJson(json) || json == "")
            {
                File.Delete(SettingsFileName);
                CreateSettingsFile();
                using (StreamReader streamReader = new StreamReader(SettingsFileName))
                    json = streamReader.ReadToEnd();
            }


            SettingsData test = JsonConvert.DeserializeObject<SettingsData>(json);
            return test;

        }

        public void SaveSettings(SettingsData newSettings)
        {
            if (newSettings.LastOpenedDirectory == null)
                newSettings.LastOpenedDirectory = "No directory location";
            

            string json = JsonConvert.SerializeObject(newSettings);

            using (TextWriter tw = new StreamWriter(SettingsFileName))
            {
                tw.WriteLine(json);
                tw.Close();
            };
        }

        private void CreateSettingsFile()
        {
            if (!Directory.Exists(MainDataDirectory))
                Directory.CreateDirectory(MainDataDirectory);

            File.Create(SettingsFileName).Close();

            SaveSettings(new SettingsData());

        }

        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();

            bool isValid;

            try
            {
                var obj = JToken.Parse(strInput);
                return true;
            }
            catch (JsonReaderException jex)
            {
                //Exception in parsing json
                Console.WriteLine(jex.Message);


                isValid = false;
            }
            catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                isValid = false;
            }

            if (!isValid)
                if (File.Exists(SettingsFileName))
                    File.Delete(SettingsFileName);

            return isValid;
        }


    }
}
