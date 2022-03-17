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

        private ObservableCollection<string> _FilesList = new ObservableCollection<string>();
        private IndicatorColors indicatorColors;
        private Settings settings;

        private System.Drawing.Bitmap _Image;
        public System.Drawing.Bitmap Image
        {
            get { return _Image; }
            set
            {
                if (_Image == value)
                    return;

                _Image = value;
                RaisePropertyChanged(nameof(Image));
            }
        }

        public string SelectedClass { get; set; }
        public string NewClass { get; set; }
        public int FilesCount { get; set; }

        private string _SelectedFile;
        public string SelectedFile
        {
            get { return _SelectedFile; }
            set
            {
                if (_SelectedFile == value || value == null)
                    return;

                _SelectedFile = Path.Combine(DirectoryPath, value);
                if (Path.GetExtension(_SelectedFile) == ".txt" || Path.GetExtension(_SelectedFile) == ".xml")
                {
                    var OpenFile = new System.IO.StreamReader(_SelectedFile);
                    Output = OpenFile.ReadToEnd();

                }
                else
                {
                    Output = "";
                    DrawRedRectangle();




                }

                RaisePropertyChanged(nameof(SelectedFile));
            }
        }


        public Progress<ProgressReportModel> progress;

        public ObservableCollection<string> FilesList
        {
            get { return _FilesList; }
            set
            {
                if (_FilesList == value) return;

                _FilesList = value;
            }
        }
        public ObservableCollection<string> ListOfClasses { get; set; }

        public TranslateXmlToTxTAlgorithm TranslateXmlToTxT { get; set; }

        public int ProgressPercent { get; set; }
        public int EstimatedTime { get; set; }

        private string _Output;
        public string Output
        {
            get { return _Output; }
            set
            {
                if (_Output == value)
                    return;

                if (value != "") Img = null;

                _Output = value;
            }
        }

        private string _DirectoryPath = "SomePath";
        public string DirectoryPath
        {
            get { return _DirectoryPath; }
            set
            {
                if (_DirectoryPath == value)
                    return;

                _DirectoryPath = value;
                RaisePropertyChanged(nameof(DirectoryPath));
            }
        }
        private ImageSource _Img;
        public ImageSource Img
        {
            get { return _Img; }
            set
            {
                if (_Img == value)
                    return;

                _Img = value;
                RaisePropertyChanged(nameof(Img));
            }
        }


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

        private TranslateXmlToTxTViewModel _TranslateXmlToTxTViewModel;
        public TranslateXmlToTxTViewModel TranslateXmlToTxTViewModel
        {
            get { return _TranslateXmlToTxTViewModel; }
            set
            {
                if (_TranslateXmlToTxTViewModel == value)
                    return;

                _TranslateXmlToTxTViewModel = value;
                RaisePropertyChanged(nameof(TranslateXmlToTxTViewModel));
            }
        }

        public SolidColorBrush TranslateXmlToTxTInfoBrush { get; set; }


        public MainPanelViewModel(MirrorViewModel mirrorViewModel, Progress<ProgressReportModel> progress, TranslateXmlToTxTViewModel translateXmlToTxTViewModel, IndicatorColors indicatorColors, TranslateXmlToTxTAlgorithm translateXmlToTxT, Settings settings)
        {
            this.progress = progress;
            this.indicatorColors = indicatorColors;
            this.settings = settings;
            TranslateXmlToTxT = translateXmlToTxT;
            _MirrorViewModel = mirrorViewModel;
            _TranslateXmlToTxTViewModel = translateXmlToTxTViewModel;
            TranslateXmlToTxTInfoBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 220, 220));
            ListOfClasses = new ObservableCollection<string>();

            SettingsData currentSettingsData = settings.ReadSettings();
            DirectoryPath = currentSettingsData.LastOpenedDirectory;
            foreach (string newClass in currentSettingsData.SavedClasses) ListOfClasses.Add(newClass);


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
                            DirectoryPath = selectPath(DirectoryPath, "xd");
                            if (DirectoryPath == null || DirectoryPath == "") return;
                            LoadData();

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
                        TranslateXmlToTxTInfoBrush = indicatorColors.busyColor;

                        if (DirectoryPath == null || DirectoryPath == "")
                        {
                            DirectoryPath = "No Directory";
                            TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                            return;
                        }

                        bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(DirectoryPath, ".xml", ListOfClasses.ToList(), progress));

                        TranslateXmlToTxTInfoBrush = result ? indicatorColors.doneColor : indicatorColors.errorColor;

                        SaveSettings();
                        LoadData();
                    },
                    () =>
                    {
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
                        if (NewClass == null || NewClass == "")
                            return;

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
                        if (SelectedClass == null || SelectedClass == "")
                            return;

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

                        SettingsData settingsData = new SettingsData();
                        settingsData.SavedClasses = ListOfClasses.ToList();
                        settingsData.LastOpenedDirectory = DirectoryPath;
                        settings.SaveSettings(settingsData);
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
                        Output = CountDefects(DirectoryPath).GetCounter();
                    },
                    () =>
                    {
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
                        DeleteObjectsWithNoDefects(DirectoryPath);
                        LoadData();
                    },
                    () =>
                    {
                        return true;
                    });
                }

                return _ClearData;
            }
        }



        private string selectPath(string startingDir, string description)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = description;
            folderDlg.SelectedPath = startingDir;
            folderDlg.ShowNewFolderButton = true;

            DialogResult result = folderDlg.ShowDialog();

            if (result != DialogResult.OK)
                return null;

            if (!Directory.Exists(folderDlg.SelectedPath))
                return null;

            return folderDlg.SelectedPath;
        }

        private void SaveSettings()
        {
            SettingsData settingsData = new SettingsData();
            settingsData.SavedClasses = ListOfClasses.ToList();
            settingsData.LastOpenedDirectory = DirectoryPath;
            settings.SaveSettings(settingsData);
        }
        private void LoadData()
        {
            Output = "";
            FilesList.Clear();
            FilesCount = 0;

            DirectoryInfo d = new DirectoryInfo(DirectoryPath);

            DirectoryInfo[] directories = d.GetDirectories();
            foreach (DirectoryInfo directory in directories) FilesList.Add(directory.Name);

            FileInfo[] Files = d.GetFiles();
            foreach (FileInfo file in Files) FilesList.Add(file.Name);

            FilesCount += Files.Length;
            FilesCount += directories.Length;
        }



        public static YoloObjectCounter CountDefects(string directoryWithXMLs)
        {
            YoloObjectCounter yoloObjectCounter = new YoloObjectCounter();

            DirectoryInfo d = new DirectoryInfo(directoryWithXMLs);
            FileInfo[] files = d.GetFiles("*.xml");
            foreach (FileInfo file in files)
                yoloObjectCounter.AddCounters(CountDefectsOnSingleFile(file.FullName));

            return yoloObjectCounter;
        }

        private static YoloObjectCounter CountDefectsOnSingleFile(string pathToSingleXMLFile)
        {
            if (Path.GetExtension(pathToSingleXMLFile) != ".xml") return null;

            XmlDocument doc = new XmlDocument();
            doc.Load(pathToSingleXMLFile);

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/annotation/object");

            YoloObjectCounter yoloObjectCounter = ConvertXMLNodeToYoloObjectCounter(nodes);

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
            DirectoryInfo d = new DirectoryInfo(path);
            FileInfo[] files = d.GetFiles();
            foreach (FileInfo file in files)
                DeleteObject(file.FullName);
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


        public BitmapImage ToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        void DrawRedRectangle()
        {
            string fileName = Path.GetFileNameWithoutExtension(_SelectedFile) + ".xml";

            Bitmap bitmap = new Bitmap(_SelectedFile);

            if (File.Exists(Path.Combine(DirectoryPath, fileName)))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(DirectoryPath, fileName));
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
                    gr.DrawString(s: defectType + ":    " , font: new Font(new Font("Times New Roman", 12.0f),
                        FontStyle.Bold), brush: new SolidBrush(pen.Color), point: new Point(rectangle.X, rectangle.Y + 25));

                }

                ;
            }
            Img = ToBitmapImage(bitmap);

        }
    }
}
