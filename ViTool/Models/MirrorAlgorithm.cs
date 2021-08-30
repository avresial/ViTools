﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ViTool.Models
{
    public class MirrorAlgorithm : ViewModelBase
    {
        private const string imgExt = ".jpg";
        private const string xmlExt = ".xml";

        private string noValidFiles = "No valid files in given Directory";
        private string operationFinished = "Operation Finished";
        private string noValidDirectory = "No Valid Directory";

        private int _MaxOutputLines = 20000;
        public int MaxOutputLines
        {
            get { return _MaxOutputLines; }
            set
            {
                if (_MaxOutputLines == value)
                    return;

                _MaxOutputLines = value;
            }
        }

        private String _Output;
        public String Output
        {
            get { return _Output; }
            set
            {
                if (_Output == value)
                    return;
                if (value.Length > _MaxOutputLines)
                    _Output = "";
                else
                    _Output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        private int _HowMuchLeft;
        public int HowMuchLeft
        {
            get { return _HowMuchLeft; }
            set
            {
                if (_HowMuchLeft == value)
                    return;

                _HowMuchLeft = value;
                RaisePropertyChanged(nameof(HowMuchLeft));
            }
        }

        private int _HowMuchThereIs;
        public int HowMuchThereIs
        {
            get { return _HowMuchThereIs; }
            set
            {
                if (_HowMuchThereIs == value)
                    return;

                _HowMuchThereIs = value;
                RaisePropertyChanged(nameof(HowMuchThereIs));
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
                RaisePropertyChanged(nameof(IsRunning));
            }
        }

        public async Task<bool> MirrorImgAsync(string directory)
        {
            Output = "Loading fles \n";
            IsRunning = true;
            HowMuchLeft = 0;
            HowMuchThereIs = 0;

            string[] files;
            string mirroredImgDirectory = CreateMirroredImgDirectory(directory);

            if (mirroredImgDirectory == null || mirroredImgDirectory == "")
                return false;

            files = Directory.GetFiles(directory);

            HowMuchThereIs = files.Where(x => x.Contains(imgExt)).Count();
            HowMuchLeft = HowMuchThereIs;

            if (HowMuchThereIs == 0)
            {
                Output = noValidFiles;
                IsRunning = false;
                return false;
            }

            //ProcessFiles(files, mirroredImgDirectory);
            await ProcessFilesParalelAsync(files, mirroredImgDirectory);
            Output = operationFinished;
            IsRunning = false;
            return true;
        }

        private string CreateMirroredImgDirectory(string directory)
        {
            string mirroredImgDirectory = null;

            if (directory == null || directory == "")
            {
                Output = noValidDirectory;
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
                    HowMuchLeft--;
                }
            }
        }

        async Task ProcessFilesParalelAsync(string[] files, string mirroredImgDirectory)
        {
            int procesorsCount = Environment.ProcessorCount;

            if (procesorsCount >= 4)
                procesorsCount -= 2;

            await Task.Run(() => Parallel.ForEach<string>(files, new ParallelOptions { MaxDegreeOfParallelism = procesorsCount }, src =>
              {
                  if (Path.GetExtension(src) == xmlExt)
                      SaveXml(src, mirroredImgDirectory, xmlExt, FlipXml(src));

                  if (Path.GetExtension(src) == imgExt)
                  {
                      SaveImg(src, mirroredImgDirectory, imgExt, FlipImg(src));
                      HowMuchLeft--;
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
            Output += "Saveing - " + xmlFilename + "\n";
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
            Output += "Saveing - " + imgFilename + "\n";

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
