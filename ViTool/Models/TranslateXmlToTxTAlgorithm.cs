﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ViTool.Models
{
    public class TranslateXmlToTxTAlgorithm : ViewModelBase
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

        private int _HowMuchLeft ;
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

        public async Task TranslateXmlToTxTAsync(string directory, string xmlExt)
        {
            Output = "";
            HowMuchThereIs=0;
            HowMuchLeft=0;
          
            string[] Files = Directory.GetFiles(directory);

            foreach (string fileSrc in Files)
            {
                if (Path.GetExtension(fileSrc) == xmlExt)
                {
                    HowMuchThereIs++;
                    HowMuchLeft++;
                }
                
            }

            

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
                    Output += "writing " + mirroredXmlSrc + "\n";
                    HowMuchLeft--;
                }
            }
        }

        void SaveToTxt(List<TxtDefectRow> defectRows, string filename)
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

    }
}
