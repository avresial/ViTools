using System;
using System.Collections.Generic;

namespace ViTool.Models
{
    public class ProgressReportModel
    {
        public string InfoMessage { get; set; } = "";
        public string ErrorMessage { get; set; } = "";
        public int PercentageComplete { get; set; } = 0;
        public int EstimatedTimeInSecounds { get; set; } = 0;
        public List<String> FilesProcessed { get; set; } = new List<string>();
    }
}
