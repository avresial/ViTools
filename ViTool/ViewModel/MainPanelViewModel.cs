using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using ViTool.Models;

namespace ViTool.ViewModel
{

    public class MainPanelViewModel : ViewModelBase
    {


        private IndicatorColors indicatorColors;
        private Settings settings;

        public string SelectedClass { get; set; }
        public string NewClass { get; set; }

        private string _SelectedFile;
        public string SelectedFile
        {
            get { return _SelectedFile; }
            set
            {
                if (_SelectedFile == value || value == null)
                    return;

                _SelectedFile = Path.Combine(DirectoryPath, value);
                if (Path.GetExtension(_SelectedFile) == ".txt" || Path.GetExtension(_SelectedFile) == ".xml" || Path.GetExtension(_SelectedFile).Contains("names"))
                {
                    StreamReader OpenFile = new StreamReader(_SelectedFile);
                    string otput = OpenFile.ReadToEnd();
                    Output = otput;
                }
                else if (Path.GetExtension(_SelectedFile) == ".jpg" || Path.GetExtension(_SelectedFile) == ".png")
                {
                    Output = "";
                    DrawRedRectangle();
                }
                else
                {
                    Output = $"Unknown file type - {value}";
                }

                RaisePropertyChanged(nameof(SelectedFile));
            }
        }

        private string _Output;
        public string Output
        {
            get { return _Output; }
            set
            {
                if (value != "") ImagePreview = null;

                _Output = value;
            }
        }

        private string _DirectoryPath = "";
        public string DirectoryPath
        {
            get { return _DirectoryPath; }
            set
            {
                if (_DirectoryPath == value) return;

                if (value == null || value == "") FilesList.Clear();

                _DirectoryPath = value;
                RaisePropertyChanged(nameof(DirectoryPath));
            }
        }

        public int ProgressPercent { get; set; }
        public int EstimatedTime { get; set; }

        private ObservableCollection<string> _FilesList = new ObservableCollection<string>();
        public ObservableCollection<string> FilesList
        {
            get { return _FilesList; }
            set
            {
                if (_FilesList == value) return;

                _FilesList = value;
            }
        }
        public ObservableCollection<string> ListOfClasses { get; set; } = new ObservableCollection<string>();

        public TranslateXmlToTxTAlgorithm TranslateXmlToTxT { get; set; }

        private ImageSource _ImagePreview;
        public ImageSource ImagePreview
        {
            get { return _ImagePreview; }
            set
            {
                if (_ImagePreview == value)
                    return;

                _ImagePreview = value;
                RaisePropertyChanged(nameof(ImagePreview));
            }
        }

        public SolidColorBrush ProgressBrush { get; set; } = new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));

        public Progress<ProgressReportModel> progress;

        private MirrorViewModel _MirrorViewModel;
        public MirrorViewModel MirrorViewModel
        {
            get { return _MirrorViewModel; }
            set
            {
                if (_MirrorViewModel == value)
                    return;

                _MirrorViewModel = value;
                RaisePropertyChanged(nameof(MirrorViewModel));
            }
        }


        public MainPanelViewModel(MirrorViewModel mirrorViewModel, Progress<ProgressReportModel> progress, IndicatorColors indicatorColors,
            TranslateXmlToTxTAlgorithm translateXmlToTxT, Settings settings)
        {
            this.progress = progress;
            this.indicatorColors = indicatorColors;
            this.settings = settings;
            TranslateXmlToTxT = translateXmlToTxT;
            _MirrorViewModel = mirrorViewModel;

            SettingsData currentSettingsData = settings.ReadSettings();
            DirectoryPath = currentSettingsData.LastOpenedDirectory;

            foreach (string newClass in currentSettingsData.SavedClasses) ListOfClasses.Add(newClass);
            if (DirectoryPath != null && DirectoryPath != "" && DirectoryPath != "No Directory") LoadData(DirectoryPath);

        }


        private RelayCommand _LoadDataRelayCommand;
        public RelayCommand LoadDataRelayCommand
        {
            get
            {
                if (_LoadDataRelayCommand == null)
                {
                    _LoadDataRelayCommand = new RelayCommand(
                     () =>
                        {
                            DirectoryPath = SelectPath(DirectoryPath, "Select Directory with jpg and/or xmls");
                            if (DirectoryPath == null || DirectoryPath == "") return;
                            LoadData(DirectoryPath);
                            SaveSettings();
                        },
                    () =>
                    {
                        return true;
                    });
                }

                return _LoadDataRelayCommand;
            }
        }

        private RelayCommand _CreateTxtFromXml;
        public RelayCommand CreateTxtFromXml
        {
            get
            {
                if (_CreateTxtFromXml == null)
                {
                    _CreateTxtFromXml = new RelayCommand(
                    async () =>
                    {
                        Output = "";

                        SettingsData currentSettingsData = settings.ReadSettings();
                        ProgressBrush = indicatorColors.busyColor;

                        if (DirectoryPath == null || DirectoryPath == "")
                        {
                            DirectoryPath = "No Directory";
                            ProgressBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                            return;
                        }

                        bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(DirectoryPath, ".xml", ListOfClasses.ToList(), progress));

                        ProgressBrush = result ? indicatorColors.doneColor : indicatorColors.errorColor;

                        SaveSettings();
                        LoadData(DirectoryPath);
                    },
                    () =>
                    {
                        if (DirectoryPath == null || DirectoryPath == "") return false;
                        if (ListOfClasses.Count == 0) return false;
                        return !TranslateXmlToTxT.IsRunning;
                    });
                }

                return _CreateTxtFromXml;
            }
        }

        private RelayCommand _AddClass;
        public RelayCommand AddClass
        {
            get
            {
                if (_AddClass == null)
                {
                    _AddClass = new RelayCommand(
                    () =>
                    {
                        if (NewClass == null || NewClass == "") return;

                        ListOfClasses.Add(NewClass);
                        NewClass = "";

                        SaveSettings();
                    },
                    () =>
                    {
                        return true;
                    });
                }

                return _AddClass;
            }
        }

        private RelayCommand _DeleteClass;
        public RelayCommand DeleteClass
        {
            get
            {
                if (_DeleteClass == null)
                {
                    _DeleteClass = new RelayCommand(
                    () =>
                    {
                        if (SelectedClass == null || SelectedClass == "") return;

                        ListOfClasses.Remove(SelectedClass);

                        SaveSettings();
                    },
                    () =>
                    {
                        return true;
                    });
                }

                return _DeleteClass;
            }
        }

        private RelayCommand _Clear;
        public RelayCommand Clear
        {
            get
            {
                if (_Clear == null)
                {
                    _Clear = new RelayCommand(
                    () =>
                    {
                        ListOfClasses.Clear();

                        SaveSettings();
                    },
                    () =>
                    {
                        return true;
                    });
                }

                return _Clear;
            }
        }

        private RelayCommand _GetDatasetStatistics;
        public RelayCommand GetDatasetStatistics
        {
            get
            {
                if (_GetDatasetStatistics == null)
                {
                    _GetDatasetStatistics = new RelayCommand(
                    () =>
                    {
                        Output = AdditionalOperations.CountDefects(DirectoryPath).GetCounter();
                    },
                    () =>
                    {
                        if (DirectoryPath == null || DirectoryPath == "") return false;
                        return true;
                    });
                }

                return _GetDatasetStatistics;
            }
        }

        private RelayCommand _ClearData;
        public RelayCommand ClearData
        {
            get
            {
                if (_ClearData == null)
                {
                    _ClearData = new RelayCommand(
                    () =>
                    {
                        AdditionalOperations.DeleteObjectsWithNoDefects(DirectoryPath);
                        LoadData(DirectoryPath);
                    },
                    () =>
                    {
                        if (DirectoryPath == null || DirectoryPath == "") return false;
                        return true;
                    });
                }

                return _ClearData;
            }
        }


        private string SelectPath(string startingDir, string description)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = description;
            folderDlg.SelectedPath = startingDir;
            folderDlg.ShowNewFolderButton = true;

            DialogResult result = folderDlg.ShowDialog();

            if (result != DialogResult.OK) return null;

            if (!Directory.Exists(folderDlg.SelectedPath)) return null;

            return folderDlg.SelectedPath;
        }

        private void SaveSettings()
        {
            SettingsData settingsData = new SettingsData();
            settingsData.SavedClasses = ListOfClasses.ToList();
            settingsData.LastOpenedDirectory = DirectoryPath;
            settings.SaveSettings(settingsData);
        }
        private void LoadData(string directory)
        {
            Output = "";
            FilesList.Clear();

            DirectoryInfo d = new DirectoryInfo(directory);
            try
            {
                FileInfo[] Files = d.GetFiles();
                foreach (FileInfo file in Files)
                {
                    if (Path.GetExtension(file.FullName).Contains("names"))
                    {
                        ListOfClasses.Clear();
                        string[] logFile = File.ReadAllLines(file.FullName);

                        foreach (string yoloClass in new List<string>(logFile)) ListOfClasses.Add(yoloClass);
                    }
                    FilesList.Add(file.Name);
                }

            }
            catch (Exception ex)
            {

                Output = ex.Message;
            }


        }

        private void DrawRedRectangle()
        {
            string TXTfileName = Path.GetFileNameWithoutExtension(_SelectedFile) + ".txt";
            string TXTFile = Path.Combine(DirectoryPath, TXTfileName);
            if (File.Exists(TXTFile))
            {
                DrawRectangleFromTxtFile(TXTFile);
                return;
            }

            string XMLfileName = Path.GetFileNameWithoutExtension(_SelectedFile) + ".xml";
            string xmlFile = Path.Combine(DirectoryPath, XMLfileName);
            if (File.Exists(xmlFile))
            {
                DrawRectangleFromXmlFile(XMLfileName, xmlFile);
                return;
            }

            ImagePreview = new BitmapImage(new Uri(_SelectedFile));

        }

        private void DrawRectangleFromTxtFile(string TXTFilePath)
        {
            Bitmap bitmap = new Bitmap(_SelectedFile);

            var logFile = File.ReadAllLines(TXTFilePath);
            List<string> logList = new List<string>(logFile);

            Graphics gr = Graphics.FromImage(bitmap);
            System.Drawing.Color penColor = System.Drawing.Color.Red;
            System.Drawing.Pen pen = new System.Drawing.Pen(penColor, 5);

            foreach (string defect in logList)
            {
                if (defect == null || defect == "") continue;
                // [class numer] [Left] [Top] [Width] [Height]
                Rectangle rectangle = new Rectangle();


                string[] defectsData = defect.Split(' ');
                int defectIndex = int.Parse(defectsData[0]);
                string defectName = ListOfClasses[defectIndex];

                rectangle.X = (int)((double.Parse(defectsData[1]) - (double.Parse(defectsData[3]) / 2)) * bitmap.Width);
                rectangle.Y = (int)((double.Parse(defectsData[2]) - (double.Parse(defectsData[4]) / 2)) * bitmap.Height);

                rectangle.Width = (int)(double.Parse(defectsData[3]) * bitmap.Width);
                rectangle.Height = (int)(double.Parse(defectsData[4]) * bitmap.Height);


                gr.DrawRectangle(pen, rectangle);
                gr.DrawString(s: defectName + ":    ", font: new Font(new Font("Times New Roman", 15.0f),
                    FontStyle.Bold), brush: new SolidBrush(pen.Color), point: new Point(rectangle.X, rectangle.Y + 25));


                Output += defect + "\n";
            }
            gr.Dispose();
            ImagePreview = AdditionalOperations.ToBitmapImage(bitmap);
            bitmap.Dispose();
        }
        private void DrawRectangleFromXmlFile(string XMLfileName, string xmlFile)
        {
            Bitmap bitmap = new Bitmap(_SelectedFile);

            if (File.Exists(xmlFile))
            {
                StreamReader OpenFile = new StreamReader(xmlFile);
                Output = OpenFile.ReadToEnd();

                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(DirectoryPath, XMLfileName));
                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/annotation/object");

                Graphics gr = Graphics.FromImage(bitmap);
                System.Drawing.Color penColor = System.Drawing.Color.Red;
                System.Drawing.Pen pen = new System.Drawing.Pen(penColor, 5);

                foreach (XmlNode currentNode in nodes)
                {
                    int xmin = int.Parse(currentNode.SelectSingleNode("bndbox/xmin").InnerText);
                    int xmax = int.Parse(currentNode.SelectSingleNode("bndbox/xmax").InnerText);
                    int ymin = int.Parse(currentNode.SelectSingleNode("bndbox/ymin").InnerText);
                    int ymax = int.Parse(currentNode.SelectSingleNode("bndbox/ymax").InnerText);

                    Rectangle rectangle = new Rectangle();
                    rectangle.X = xmin;
                    rectangle.Y = ymin;
                    rectangle.Width = xmax - xmin;
                    rectangle.Height = ymax - ymin;

                    string defectType = currentNode.SelectSingleNode("name").InnerText;

                    gr.DrawRectangle(pen, rectangle);
                    gr.DrawString(s: defectType + ":    ", font: new Font(new Font("Times New Roman", 15.0f),
                        FontStyle.Bold), brush: new SolidBrush(pen.Color), point: new Point(rectangle.X, rectangle.Y + 25));

                }
                gr.Dispose();
            }
            else
            {
                Output = $"No XML available at src {xmlFile}";
            }

            ImagePreview = AdditionalOperations.ToBitmapImage(bitmap);
            bitmap.Dispose();
        }
    }
}
