using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml;
using ViTool.Enums;

namespace ViTool.Models
{
    public static class AdditionalOperations
    {

        public static YoloObjectCounter CountDefects(string directoryWithAnnotationFiles, AnnotationTypes annotationTypes, List<string> classesList)
        {
            if (directoryWithAnnotationFiles == null || directoryWithAnnotationFiles == "") new YoloObjectCounter();
            YoloObjectCounter yoloObjectCounter = new YoloObjectCounter();

            DirectoryInfo d = new DirectoryInfo(directoryWithAnnotationFiles);
            FileInfo[] files = null;
            switch (annotationTypes)
            {
                case AnnotationTypes.TXT:
                    files = d.GetFiles("*.txt");
                    break;

                case AnnotationTypes.XML:
                    files = d.GetFiles("*.xml");
                    break;
            }

            foreach (FileInfo file in files) yoloObjectCounter.AddCounters(CountDefectsOnSingleFile(file.FullName, classesList));

            return yoloObjectCounter;
        }

        private static YoloObjectCounter CountDefectsOnSingleFile(string pathToSingleXMLFile, List<string> classesList)
        {
            YoloObjectCounter yoloObjectCounter = null;

            if (Path.GetExtension(pathToSingleXMLFile) == ".xml") yoloObjectCounter = CountObiectsFromXML(pathToSingleXMLFile);
            if (Path.GetExtension(pathToSingleXMLFile) == ".txt") yoloObjectCounter = CountObiectsFromTXT(pathToSingleXMLFile, classesList);

            return yoloObjectCounter;
        }

        private static YoloObjectCounter CountObiectsFromXML(string pathToSingleXMLFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathToSingleXMLFile);

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/annotation/object");

            YoloObjectCounter yoloObjectCounter = ConvertXMLNodeToYoloObjectCounter(nodes);
            return yoloObjectCounter;
        }

        private static YoloObjectCounter CountObiectsFromTXT(string pathToSingleTXTFile, List<string> classesList)
        {
            if (pathToSingleTXTFile.ToLower().Contains("classes")) return null;
            if (pathToSingleTXTFile.ToLower().Contains("names")) return null;

            YoloObjectCounter yoloObjectCounter = new YoloObjectCounter();
            string[] fileLines = File.ReadAllLines(pathToSingleTXTFile);

            foreach (string line in fileLines)
            {
                if (line == null || line == string.Empty) continue;
                
                string[] rows = line.Split(' ');
                if (rows.Length != 5) continue;

                int classId = int.Parse(rows[0]);

                if (classesList.Count >= classId) yoloObjectCounter.CountObject(classesList[classId]);
                else yoloObjectCounter.CountObject(rows[0].ToString());
            }

            return yoloObjectCounter;
        }

        private static YoloObjectCounter ConvertXMLNodeToYoloObjectCounter(XmlNodeList xmlNodeList)
        {
            YoloObjectCounter yoloObjectCounter = new YoloObjectCounter();

            foreach (XmlNode node in xmlNodeList)
            {
                XmlNode objectNode = node.SelectSingleNode("name");
                string defectName = objectNode.InnerText;
                yoloObjectCounter.CountObject(defectName);
            }

            return yoloObjectCounter;
        }

        public static void DeleteObjectsWithNoDefects(string path)
        {
            if (path == null || path == "") return;

            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] files = d.GetFiles();
            foreach (FileInfo file in files) DeleteObject(file.FullName);
        }
        private static void DeleteObject(string xmlPath)
        {

            if (DeleteFileWithNoPair(xmlPath))
                return;

            string extension = Path.GetExtension(xmlPath);

            if (extension != ".xml")
                return;

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            var nodes = doc.DocumentElement.SelectNodes("/annotation/object");

            if (nodes.Count != 0)
                return;

            if (File.Exists(xmlPath))
            {
                File.Delete(xmlPath);
                Console.WriteLine("deleting - " + xmlPath);
            }

            if (File.Exists(xmlPath.Replace(extension, ".jpg")))
            {
                File.Delete(xmlPath.Replace(extension, ".jpg"));
                Console.WriteLine("deleting - " + xmlPath.Replace(extension, ".jpg"));
            }
        }
        private static bool DeleteFileWithNoPair(string path)
        {
            string extension = Path.GetExtension(path);

            if (extension == ".xml")
            {
                if (!File.Exists(path.Replace(extension, ".jpg")))
                {
                    File.Delete(path);
                    return true;
                }
            }


            if (extension == ".jpg")
            {
                if (!File.Exists(path.Replace(extension, ".xml")))
                {
                    File.Delete(path);
                    return true;

                }
            }

            return false;
        }


        public static BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

    }
}
