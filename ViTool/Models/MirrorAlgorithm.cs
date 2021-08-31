using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ViTool.Models
{
    public class MirrorAlgorithm
    {
        private const string imgExt = ".jpg";
        private const string xmlExt = ".xml";

        private string noValidFiles = "No valid files in given Directory";
        private string operationFinished = "Operation Finished";
        private string noValidDirectory = "No Valid Directory";

        private List<String> Output = new List<string>();
       
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

        private bool _IsRunning = false;
        public bool IsRunning
        {
            get { return _IsRunning; }
            set
            {
                if (_IsRunning == value)
                    return;

                _IsRunning = value;
            }
        }

        public async Task<bool> MirrorImgAsync(string directory, IProgress<ProgressReportModel> progress)
        {
            Output.Clear();
            //Output.Add("Loading fles");
            IsRunning = true;
            HowMuchThereIs = 0;

            string[] files;
            string mirroredImgDirectory = CreateMirroredImgDirectory(directory);

            if (mirroredImgDirectory == null || mirroredImgDirectory == "")
                return false;

            files = Directory.GetFiles(directory);

            HowMuchThereIs = files.Where(x => x.Contains(imgExt)).Count();

            if (HowMuchThereIs == 0)
            {
                Output.Add(noValidFiles);
                IsRunning = false;
                return false;
            }

            //ProcessFiles(files, mirroredImgDirectory);
            await ProcessFilesParalelAsync(files, mirroredImgDirectory, progress);
            //Output.Add(operationFinished);
            IsRunning = false;
            return true;
        }

        private string CreateMirroredImgDirectory(string directory)
        {
            string mirroredImgDirectory = null;

            if (directory == null || directory == "")
            {
                //Output.Add(noValidDirectory);
                IsRunning = false;
                return null;
            }

            mirroredImgDirectory = Path.Combine(Path.GetFullPath(Path.Combine(directory, @"..\")), Path.GetFileName(directory) + "Mirrored");

            if (Directory.Exists(mirroredImgDirectory))
                Directory.Delete(mirroredImgDirectory, true);

            Directory.CreateDirectory(mirroredImgDirectory);

            return mirroredImgDirectory;
        }

        void ProcessFiles(string[] files, string mirroredImgDirectory)
        {
            foreach (string src in files)
            {
                if (Path.GetExtension(src) == xmlExt)
                    SaveXml(src, mirroredImgDirectory, xmlExt, FlipXml(src));

                if (Path.GetExtension(src) == imgExt)
                {
                    SaveImg(src, mirroredImgDirectory, imgExt, FlipImg(src));
                }
            }
        }

        async Task ProcessFilesParalelAsync(string[] files, string mirroredImgDirectory, IProgress<ProgressReportModel> progress)
        {
            int procesorsCount = Environment.ProcessorCount;

            if (procesorsCount >= 4)
                procesorsCount -= 2;

            await Task.Run(() => Parallel.ForEach<string>(files, new ParallelOptions { MaxDegreeOfParallelism = procesorsCount }, src =>
              {
                  ProgressReportModel progressReportModel = new ProgressReportModel();


                  if (Path.GetExtension(src) == xmlExt)
                      SaveXml(src, mirroredImgDirectory, xmlExt, FlipXml(src));

                  if (Path.GetExtension(src) == imgExt)
                  {
                      Stopwatch watch = new Stopwatch();
                      watch.Start();
                      SaveImg(src, mirroredImgDirectory, imgExt, FlipImg(src));

                      Output.Add(src);

                      watch.Stop();

                      progressReportModel.FilesProcessed.Add(src);
                      progressReportModel.NumberOfAllFilesToProcess = HowMuchThereIs;
                      progressReportModel.PercentageComplete = (Output.Count * 100) / HowMuchThereIs;
                      progressReportModel.TimeConsumedByProcessedFiles = watch.Elapsed.TotalMilliseconds * 0.001;

                      progress.Report(progressReportModel);
                  }

              }));
        }

        XmlDocument FlipXml(string imgSrc)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(imgSrc);
            int frameWidth = int.Parse(doc.DocumentElement.SelectSingleNode("/annotation/size/width").InnerText);

            var nodes = doc.DocumentElement.SelectNodes("/annotation/object");

            foreach (XmlNode currentNode in nodes)
            {
                int newXmin = frameWidth - int.Parse(currentNode.SelectSingleNode("bndbox/xmin").InnerText);
                XmlNode node = currentNode.SelectSingleNode("bndbox/xmin");
                node.InnerText = newXmin.ToString();

                int test = int.Parse(currentNode.SelectSingleNode("bndbox/xmax").InnerText);
                int newXmax = frameWidth - test;
                XmlNode xmax = currentNode.SelectSingleNode("bndbox/xmax");
                xmax.InnerText = newXmax.ToString();
            }
            return doc;
        }

        void SaveXml(string imgSrc, string mirroredImgDirectory, string xmlExt, XmlDocument doc)
        {
            string xmlFilename = Path.GetFileNameWithoutExtension(imgSrc);
            xmlFilename += "Mirrored" + xmlExt;
            string mirroredXmlSrc = Path.Combine(mirroredImgDirectory, xmlFilename);
            doc.Save(mirroredXmlSrc);
            //Output.Add("Saveing - " + xmlFilename);
        }

        Bitmap FlipImg(string imgSrc)
        {
            Bitmap newBitmap = GetImg(imgSrc);

            if (newBitmap == null)
                return newBitmap;

            newBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return newBitmap;
        }

        void SaveImg(string imgSrc, string mirroredImgDirectory, string imgExt, Bitmap img)
        {

            string imgFilename = Path.GetFileNameWithoutExtension(imgSrc);
            imgFilename += "Mirrored" + imgExt;

            string mirroredImgSrc = Path.Combine(mirroredImgDirectory, imgFilename);
            img.Save(mirroredImgSrc);
            //Output.Add("Saveing - " + imgFilename);

        }

        Bitmap GetImg(string imgFiles)
        {
            Bitmap bitmap1 = null;

            try
            {
                return (Bitmap)Bitmap.FromFile(imgFiles);
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("There was an error. Check the path to the bitmap.");
            }

            return bitmap1;
        }
    }
}
