using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
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

        public async Task MirrorImgAsync(string directory)
        {
            Output = "Loading fles \n";
            IsRunning = true;
            HowMuchLeft = 0;
            HowMuchThereIs = 0;


            string mirroredImgDirectory = "";
            string imgDirectory;
            string[] imgFiles;
            string imgExt = ".jpg";
            string xmlExt = ".xml";
            string txtExt = ".txt";

            imgDirectory = directory;

            if (imgDirectory == null || imgDirectory == "")
                return;

            mirroredImgDirectory = Path.Combine(Path.GetFullPath(Path.Combine(imgDirectory, @"..\")), Path.GetFileName(imgDirectory) + "Mirrored");

            if (Directory.Exists(mirroredImgDirectory))
                Directory.Delete(mirroredImgDirectory, true);

            Directory.CreateDirectory(mirroredImgDirectory);

            imgFiles = Directory.GetFiles(imgDirectory);

            if (imgFiles.Count() == 0)
                return;

            foreach (string fileSrc in imgFiles)
                if (Path.GetExtension(fileSrc) == imgExt)
                    HowMuchThereIs++;
                
            HowMuchLeft = HowMuchThereIs;

            foreach (string file in imgFiles)
                if (Path.GetExtension(file) != imgExt && Path.GetExtension(file) != xmlExt && Path.GetExtension(file) != txtExt)
                    return;

            ProcessFiles(imgFiles, mirroredImgDirectory, imgExt, xmlExt);
            IsRunning = false;
        }

        void ProcessFiles(string[] imgFiles, string mirroredImgDirectory, string imgExt, string xmlExt)
        {
            int howMuchLeft = imgFiles.Count() / 2;

            foreach (string imgSrc in imgFiles)
            {
                if (Path.GetExtension(imgSrc) == xmlExt)
                {
                    SaveXml(imgSrc, mirroredImgDirectory, xmlExt, FlipXml(imgSrc));
                    howMuchLeft--;
                }

                if (Path.GetExtension(imgSrc) == imgExt)
                {
                    SaveImg(imgSrc, mirroredImgDirectory, imgExt, FlipImg(imgSrc));
                    HowMuchLeft--;
                }
            }
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
