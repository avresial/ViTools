using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ViTool.Models;

namespace ViTool.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class TranslateXmlToTxTViewModel : ViewModelBase
    {
        #region Properties
        private int maxOutputLines = 2000;
        private IndicatorColors indicatorColors = new IndicatorColors();
        private Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();

        public TranslateXmlToTxTAlgorithm TranslateXmlToTxT { get; set; } = new TranslateXmlToTxTAlgorithm();
        public SolidColorBrush TranslateXmlToTxTInfoBrush { get; set; } = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        public string TranslateXmlToTxTSrc { get; set; } = "No directory location";

        private string _Output;
        public string Output
        {
            get { return _Output; }
            set
            {
                if (_Output == value)
                    return;

                if (value.Length > maxOutputLines)
                    _Output = "";
                else
                    _Output = value;
            }
        }

        public int Progress { get; set; } = 0;
        public int EstimatedTime { get; set; } = 0;
        public string SelectedClass { get; set; }

        public string NewClass { get; set; }
        public ObservableCollection<string> ListOfClasses { get; set; } = new ObservableCollection<string>() { "HCH", "LowFreqAnomaly", "Imprint", "Break", "ChippedBreak" };

        #endregion

        #region CTOR
        public TranslateXmlToTxTViewModel()
        {
            progress.ProgressChanged += ReportProgress;
        }
        #endregion

        #region Commands
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

                        TranslateXmlToTxTInfoBrush = indicatorColors.busyColor;
                        TranslateXmlToTxTSrc = selectPath("C:\\", "Point to folder with xml files. \nProgram will create txt files called 'yourFile.txt' next original ones.");

                        if (TranslateXmlToTxTSrc == null || TranslateXmlToTxTSrc == "")
                        {
                            TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                            return;
                        }

                        bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(TranslateXmlToTxTSrc, ".xml", ListOfClasses.ToList(), progress));

                        TranslateXmlToTxTInfoBrush = result ? indicatorColors.doneColor : indicatorColors.errorColor;

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
                    },
                    () =>
                    {
                        return true;
                    });
                }

                return _Clear;
            }
        }
        #endregion

        private void ReportProgress(object sender, ProgressReportModel e)
        {
            if (e.ErrorMessage != "")
            {
                Output += e.ErrorMessage;
                return;
            }

            foreach (string line in e.FilesProcessed)
                Output += line + "\n";

            if (e.InfoMessage != "")
                Output += e.InfoMessage;

            Progress = e.PercentageComplete;
            EstimatedTime = (int)e.TimeConsumedByProcessedFiles;
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
    }
}
