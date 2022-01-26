using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViTool.Models
{
    public class SettingsData
    {
        public string LastOpenedDirectory { get; set; }
        public List<string> SavedClasses { get; set; }

        public SettingsData()
        {
            LastOpenedDirectory = "No Directory";
            SavedClasses = new List<string>();
        }
    }
}
