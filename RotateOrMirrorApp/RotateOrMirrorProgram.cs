using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace RotateOrMirrorApp
{
    class RotateOrMirrorProgram
    {
        [STAThread]
        static void Main(string[] args)
        {
            //            RotateAndFlip();
            translateXmlToTxT("R:\\Graw\\Defektoskopia\\VI2Defect", ".xml");
        }


        static void translateXmlToTxT(string directory, string xmlExt, bool askForNewDirectory = false)
        {
            if (askForNewDirectory)
            {
                FolderBrowserDialog folderDlg = new FolderBrowserDialog();

                folderDlg.SelectedPath = directory;
                folderDlg.ShowNewFolderButton = true;

                DialogResult result = folderDlg.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (!Directory.Exists(folderDlg.SelectedPath))
                        return;
                    directory = folderDlg.SelectedPath;
                }
                else 
                {
                    return;
                }
            }

            string[] Files = Directory.GetFiles(directory);

            foreach (string fileSrc in Files)
            {
                if (Path.GetExtension(fileSrc) == xmlExt)
                {
                    List<TxtDefectRow> defects = new List<TxtDefectRow>();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileSrc);
                    int frameWidth = int.Parse(doc.DocumentElement.SelectSingleNode("/annotation/size/width").InnerText);
                    int frameHeight = int.Parse(doc.DocumentElement.SelectSingleNode("/annotation/size/height").InnerText);

                    var nodes = doc.DocumentElement.SelectNodes("/annotation/object");

                    foreach (XmlNode currentNode in nodes)
                    {
                        TxtDefectRow defectRow = new TxtDefectRow();

                        int xmin = int.Parse(currentNode.SelectSingleNode("bndbox/xmin").InnerText);
                        int xmax = int.Parse(currentNode.SelectSingleNode("bndbox/xmax").InnerText);
                        int ymin = int.Parse(currentNode.SelectSingleNode("bndbox/ymin").InnerText);
                        int ymax = int.Parse(currentNode.SelectSingleNode("bndbox/ymax").InnerText);

                        defectRow.Left = Math.Round(((double)(xmax + xmin) / 2) / (double)frameWidth, 5);
                        defectRow.Top = Math.Round(((double)(ymax + ymin) / 2) / (double)frameHeight, 5);

                        defectRow.Width = Math.Round((double)(xmax - xmin) / frameWidth, 5);
                        defectRow.Height = Math.Round((double)(ymax - ymin) / frameHeight, 5);


                        string defectType = doc.DocumentElement.SelectSingleNode("/annotation/object/name").InnerText;

                        object Classes = Enum.Parse(typeof(Classes), defectType);
                        defectRow.DefectType = (int)Classes;

                        defects.Add(defectRow);
                    }



                    string xmlFilename = Path.GetFileNameWithoutExtension(fileSrc);
                    xmlFilename += ".txt";
                    string mirroredXmlSrc = Path.Combine(directory, xmlFilename);
                    SaveToTxt(defects, mirroredXmlSrc);
                    Console.WriteLine("writing " + mirroredXmlSrc);
                }
            }
        }

        static void SaveToTxt(List<TxtDefectRow> defectRows, string filename)
        {
            List<string> lines = new List<string>();

            foreach (TxtDefectRow txtDefectRow in defectRows)
            {
                string row = txtDefectRow.DefectType + " " + txtDefectRow.Top + " " + txtDefectRow.Left + " " + txtDefectRow.Width + " " + txtDefectRow.Height;
                lines.Add(row);
            }


            using (StreamWriter file = new StreamWriter(filename))
                foreach (string line in lines)
                    file.WriteLineAsync(line);
        }

        static void RotateAndFlip()
        {
            string sinitialImgDirectory = "R:\\Graw\\Defektoskopia";
            string mirroredImgDirectory = "";
            string imgDirectory;
            string[] imgFiles;
            string imgExt = ".jpg";
            string xmlExt = ".xml";

            Console.WriteLine("Start");

            imgDirectory = GetImgsDir(sinitialImgDirectory);

            if (imgDirectory == null || imgDirectory == "")
                return;

            mirroredImgDirectory = Path.Combine(Path.GetFullPath(Path.Combine(imgDirectory, @"..\")), Path.GetFileName(imgDirectory) + "Mirrored");

            if (Directory.Exists(mirroredImgDirectory))
                Directory.Delete(mirroredImgDirectory, true);

            Directory.CreateDirectory(mirroredImgDirectory);

            imgFiles = Directory.GetFiles(imgDirectory);

            if (imgFiles.Count() == 0)
                return;

            foreach (string file in imgFiles)
                if (Path.GetExtension(file) != imgExt && Path.GetExtension(file) != xmlExt)
                    return;

            ProcessFiles(imgFiles, mirroredImgDirectory, imgExt, xmlExt);

            Console.WriteLine("Job Done");
            Console.ReadLine();

        }

        static string GetImgsDir(string startingImgDirectory)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();

            folderDlg.SelectedPath = startingImgDirectory;
            folderDlg.ShowNewFolderButton = true;

            DialogResult result = folderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!Directory.Exists(folderDlg.SelectedPath))
                    return null;

                return folderDlg.SelectedPath;
            }

            return null;
        }

        static void ProcessFiles(string[] imgFiles, string mirroredImgDirectory, string imgExt, string xmlExt)
        {
            int howMuchLeft = imgFiles.Count() / 2;

            foreach (string imgSrc in imgFiles)
            {
                if (Path.GetExtension(imgSrc) == xmlExt)
                {
                    SaveXml(imgSrc, mirroredImgDirectory, xmlExt, FlipXml(imgSrc));
                    howMuchLeft--;
                    Console.WriteLine("Left - " + howMuchLeft);
                }

                if (Path.GetExtension(imgSrc) == imgExt)
                    SaveImg(imgSrc, mirroredImgDirectory, imgExt, FlipImg(imgSrc));
            }
        }

        static XmlDocument FlipXml(string imgSrc)
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

        static void SaveXml(string imgSrc, string mirroredImgDirectory, string xmlExt, XmlDocument doc)
        {
            string xmlFilename = Path.GetFileNameWithoutExtension(imgSrc);
            xmlFilename += "Mirrored" + xmlExt;
            string mirroredXmlSrc = Path.Combine(mirroredImgDirectory, xmlFilename);
            doc.Save(mirroredXmlSrc);
            Console.WriteLine("Saveing - " + xmlFilename);
        }

        static Bitmap FlipImg(string imgSrc)
        {
            Bitmap newBitmap = GetImg(imgSrc);

            if (newBitmap == null)
                return newBitmap;

            newBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return newBitmap;
        }

        static void SaveImg(string imgSrc, string mirroredImgDirectory, string imgExt, Bitmap img)
        {

            string imgFilename = Path.GetFileNameWithoutExtension(imgSrc);
            imgFilename += "Mirrored" + imgExt;

            string mirroredImgSrc = Path.Combine(mirroredImgDirectory, imgFilename);
            img.Save(mirroredImgSrc);
            Console.WriteLine("Saveing - " + imgFilename);

        }

        static Bitmap GetImg(string imgFiles)
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
