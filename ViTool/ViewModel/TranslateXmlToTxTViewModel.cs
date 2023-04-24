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
        private IndicatorColors indicatorColors;
        public Progress<ProgressReportModel> progress;

        private Settings settings;
        public TranslateXmlToTxTAlgorithm TranslateXmlToTxT { get; set; }
        public SolidColorBrush TranslateXmlToTxTInfoBrush { get; set; }
        public string TranslateXmlToTxTSrc { get; set; }

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
        /// <summary>
        /// 0 - 100
        /// </summary>
        public int ProgressPercent { get; set; }
        public int EstimatedTime { get; set; }
        public string SelectedClass { get; set; }
        public string NewClass { get; set; }
        public ObservableCollection<string> ListOfClasses { get; set; }

        #endregion

        #region CTOR
        public TranslateXmlToTxTViewModel(TranslateXmlToTxTAlgorithm translateXmlToTxT, Progress<ProgressReportModel> progress, IndicatorColors indicatorColors, Settings settings)
        {
            this.settings = settings;
            this.progress = progress;
            this.TranslateXmlToTxT = translateXmlToTxT;
            this.indicatorColors = indicatorColors;

            SettingsData currentSettingsData = settings.ReadSettings();

            EstimatedTime = 0;
            ProgressPercent = 0;
            TranslateXmlToTxTSrc = currentSettingsData.LastOpenedDirectory;
            TranslateXmlToTxTInfoBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            ListOfClasses = new ObservableCollection<string>();

            foreach (string newClass in currentSettingsData.SavedClasses)
                ListOfClasses.Add(newClass);

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

                        SettingsData currentSettingsData = settings.ReadSettings();
                        TranslateXmlToTxTInfoBrush = indicatorColors.busyColor;
                        TranslateXmlToTxTSrc = selectPath(currentSettingsData.LastOpenedDirectory, "Point to folder with xml files. \nProgram will create txt files called 'yourFile.txt' next original ones.");

                        if (TranslateXmlToTxTSrc == null || TranslateXmlToTxTSrc == "")
                        {
                            TranslateXmlToTxTSrc = "No Directory";
                            TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                            return;
                        }

                        bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(TranslateXmlToTxTSrc, ".xml", ListOfClasses.ToList(), progress));

                        TranslateXmlToTxTInfoBrush = result ? indicatorColors.doneColor : indicatorColors.errorColor;

                        SaveSettings();

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

        private void SaveSettings()
        {
            SettingsData settingsData = new SettingsData();
            settingsData.SavedClasses = ListOfClasses.ToList();
            settingsData.LastOpenedDirectory = TranslateXmlToTxTSrc;
            settings.SaveSettings(settingsData);
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
                        settingsData.LastOpenedDirectory = TranslateXmlToTxTSrc;
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

            ProgressPercent = e.PercentageComplete;
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
