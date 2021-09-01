using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ViTool.Models;

namespace ViTool.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class MirrorViewModel : ViewModelBase
    {
        #region Properties
        int maxOutputLines = 2000;
        private IndicatorColors indicatorColors = new IndicatorColors();
        private Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
        public List<double> TimeUsagePerFile { get; set; } = new List<double>();
        public MirrorAlgorithm MirrorAlgorithm { get; set; } = new MirrorAlgorithm();
        public SolidColorBrush MirrorAlgorithmBrush { get; set; } = new SolidColorBrush(Color.FromRgb(220, 220, 220));

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
        public double EstimatedTime { get; set; } = 0;
        public String MirrorSrc { get; set; } = "No directory location";

        #endregion

        #region CTOR

        public MirrorViewModel()
        {
            progress.ProgressChanged += ReportProgress;
        }

        #endregion

        #region Commands

        private RelayCommand _MorrorImg;
        public RelayCommand MorrorImg
        {
            get
            {
                if (_MorrorImg == null)
                {
                    _MorrorImg = new RelayCommand(
                    async () =>
                    {
                        Output = "";
                        Progress = 0;
                        EstimatedTime = 0;
                        TimeUsagePerFile.Clear();

                        MirrorAlgorithmBrush = indicatorColors.busyColor;
                        MirrorSrc = selectPath("C:\\", "Point to folder with dataset (jpg + xml) \nProgram will create folder called 'yourFolderMirrored' next original one.");

                        if (MirrorSrc == null || MirrorSrc == "")
                        {
                            MirrorAlgorithmBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                            return;
                        }

                        bool result = await Task.Run(() => MirrorAlgorithm.MirrorImgAsync(MirrorSrc, progress));
                        MirrorAlgorithmBrush = result ? indicatorColors.doneColor : indicatorColors.errorColor;

                    },
                    () =>
                    {
                        return !MirrorAlgorithm.IsRunning;
                    });
                }

                return _MorrorImg;
            }
        }

        #endregion

        private void ReportProgress(object sender, ProgressReportModel e)
        {
            TimeUsagePerFile.Add(e.TimeConsumedByProcessedFiles);

            if (e.ErrorMessage != "")
            {
                Output += e.ErrorMessage;
                EstimatedTime = 0;
                Progress = 0;
                return;
            }

            foreach (string line in e.FilesProcessed)
                Output += line + "\n";

            if (e.InfoMessage != "")
                Output += e.InfoMessage + "\n";

            Progress = e.PercentageComplete;

            EstimatedTime = TimeUsagePerFile.Average() * (e.NumberOfAllFilesToProcess - (e.PercentageComplete * e.NumberOfAllFilesToProcess / 100));
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
