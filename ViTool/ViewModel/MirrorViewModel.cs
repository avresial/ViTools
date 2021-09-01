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
        private static int maxOutputLines = 2000;
        private IndicatorColors indicatorColors = new IndicatorColors();
        private Progress<ProgressReportModel> progress;
        public List<double> TimeUsagePerFile { get; set; }
        public MirrorAlgorithm MirrorAlgorithm { get; set; }
        public SolidColorBrush MirrorAlgorithmBrush { get; set; }

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

        public int ProgressPercent { get; set; }
        public double EstimatedTime { get; set; }
        public String MirrorSrc { get; set; }

        #endregion

        #region CTOR

        public MirrorViewModel(MirrorAlgorithm mirrorAlgorithm, Progress<ProgressReportModel> progress, IndicatorColors indicatorColors)
        {
            this.progress = progress;
            MirrorAlgorithm = mirrorAlgorithm;
            this.indicatorColors = indicatorColors;

            EstimatedTime = 0;
            ProgressPercent = 0;
            MirrorSrc = "No directory location";
            TimeUsagePerFile = new List<double>();
            MirrorAlgorithmBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
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
                        ProgressPercent = 0;
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
                ProgressPercent = 0;
                return;
            }

            foreach (string line in e.FilesProcessed)
                Output += line + "\n";

            if (e.InfoMessage != "")
                Output += e.InfoMessage + "\n";

            ProgressPercent = e.PercentageComplete;

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
