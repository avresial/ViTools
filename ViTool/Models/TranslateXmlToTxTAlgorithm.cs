using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace ViTool.Models
{
    public class TranslateXmlToTxTAlgorithm
    {
        private List<String> Output = new List<string>();

        private string noValidFiles = "No valid files in given Directory";
        private string operationFinished = "Operation Finished";

        private int _HowMuchThereIs;
        public int HowMuchThereIs
        {
            get { return _HowMuchThereIs; }
            set
            {
                if (_HowMuchThereIs == value)
                    return;

                _HowMuchThereIs = value;
            }
        }

        public bool IsRunning { get; set; } = false;

        public async Task<bool> TranslateXmlToTxTAsync(string directory, string xmlExt, List<string> classes, IProgress<ProgressReportModel> progress)
        {
            IsRunning = true;
            Output.Clear();
            HowMuchThereIs = 0;

            string[] files = Directory.GetFiles(directory);

            HowMuchThereIs = files.Where(x => x.Contains(xmlExt)).Count();

            if (HowMuchThereIs == 0)
            {
                progress.Report(new ProgressReportModel() { ErrorMessage = "No valid files in given Directory" });
                IsRunning = false;
                return false;
            }

            await ProcessFilesParalelAsync(directory, xmlExt, classes, files, progress);
            SendProgressReport(progress);

            IsRunning = false;
            return true;
        }

        private void SendProgressReport(IProgress<ProgressReportModel> progress)
        {
            ProgressReportModel progressReportModel = new ProgressReportModel();
            progressReportModel.NumberOfAllFilesToProcess = HowMuchThereIs;
            progressReportModel.PercentageComplete = 100;
            progressReportModel.InfoMessage = operationFinished;
            progress.Report(progressReportModel);
        }

        private async Task ProcessFilesParalelAsync(string directory, string xmlExt, List<string> classes, string[] files, IProgress<ProgressReportModel> progress)
        {
            int timeForOneRecord;
            int procesorsCount = Environment.ProcessorCount;

            if (procesorsCount >= 4)
                procesorsCount -= 2;

            await Task.Run(() => Parallel.ForEach<string>(files, new ParallelOptions { MaxDegreeOfParallelism = procesorsCount }, src =>
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                ProgressReportModel progressReportModel = new ProgressReportModel();

                if (Path.GetExtension(src) != xmlExt)
                    return;

                XmlDocument doc = new XmlDocument();
                doc.Load(src);

                int frameWidth = int.Parse(doc.DocumentElement.SelectSingleNode("/annotation/size/width").InnerText);
                int frameHeight = int.Parse(doc.DocumentElement.SelectSingleNode("/annotation/size/height").InnerText);

                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/annotation/object");

                List<TxtDefectRow> defects = CreateMirroredDefects(classes, doc, frameWidth, frameHeight, nodes);

                if (defects.Count == 0)
                    return;

                string xmlFilename = Path.GetFileNameWithoutExtension(src) + ".txt";
                string mirroredXmlSrc = Path.Combine(directory, xmlFilename);

                SaveToTxt(defects, mirroredXmlSrc);
                Output.Add(mirroredXmlSrc);

                watch.Stop();

                progressReportModel.FilesProcessed.Add(mirroredXmlSrc);
                progressReportModel.TimeConsumedByProcessedFiles = (int)((HowMuchThereIs - Output.Count) * watch.Elapsed.TotalMilliseconds * 0.001);
                progressReportModel.PercentageComplete = (Output.Count * 100) / HowMuchThereIs;
                progress.Report(progressReportModel);
            }));
        }

        private List<TxtDefectRow> CreateMirroredDefects(List<string> classes, XmlDocument doc, int frameWidth, int frameHeight, XmlNodeList nodes)
        {
            List<TxtDefectRow> defects = new List<TxtDefectRow>();
            foreach (XmlNode currentNode in nodes)
            {
                TxtDefectRow defectRow = CreateMirroredDefect(classes, doc, frameWidth, frameHeight, currentNode);
                if (defectRow != null)
                    defects.Add(defectRow);
            }

            return defects;
        }

        private TxtDefectRow CreateMirroredDefect(List<string> classes, XmlDocument doc, int frameWidth, int frameHeight, XmlNode node)
        {
            TxtDefectRow defectRow = new TxtDefectRow();

            int xmin = int.Parse(node.SelectSingleNode("bndbox/xmin").InnerText);
            int xmax = int.Parse(node.SelectSingleNode("bndbox/xmax").InnerText);
            int ymin = int.Parse(node.SelectSingleNode("bndbox/ymin").InnerText);
            int ymax = int.Parse(node.SelectSingleNode("bndbox/ymax").InnerText);

            defectRow.Left = Math.Round(((double)(xmax + xmin) / 2) / (double)frameWidth, 5);
            defectRow.Top = Math.Round(((double)(ymax + ymin) / 2) / (double)frameHeight, 5);
            defectRow.Width = Math.Round((double)(xmax - xmin) / frameWidth, 5);
            defectRow.Height = Math.Round((double)(ymax - ymin) / frameHeight, 5);

            string defectType = doc.DocumentElement.SelectSingleNode("/annotation/object/name").InnerText;

            defectRow.DefectType = -1;

            for (int i = 0; i < classes.Count; i++)
                if (classes[i] == defectType)
                    defectRow.DefectType = i;

            if (defectRow.DefectType != -1)
                return defectRow;

            return null;
        }


        void SaveToTxt(List<TxtDefectRow> defectRows, string filename)
        {
            List<string> lines = new List<string>();

            foreach (TxtDefectRow txtDefectRow in defectRows)
                lines.Add(txtDefectRow.DefectType + " " + txtDefectRow.Left + " " + txtDefectRow.Top + " " + txtDefectRow.Width + " " + txtDefectRow.Height);

            using (StreamWriter file = new StreamWriter(filename))
                foreach (string line in lines)
                    file.WriteLineAsync(line);
        }
    }
}
