using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ViTool.Models;

namespace ViTool.ViewModel
{
    public class MirrorViewModel : ViewModelBase
    {
        private IndicatorColors indicatorColors = new IndicatorColors();
        private Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
        public List<double> TimeUsagePerFile { get; set; } = new List<double>();

        private MirrorAlgorithm _MirrorAlgorithm = new MirrorAlgorithm();
        public MirrorAlgorithm MirrorAlgorithm
        {
            get { return _MirrorAlgorithm; }
            set
            {
                if (_MirrorAlgorithm == value)
                    return;

                _MirrorAlgorithm = value;
                RaisePropertyChanged(nameof(MirrorAlgorithm));
            }
        }

        private SolidColorBrush _MirrorAlgorithmBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        public SolidColorBrush MirrorAlgorithmBrush
        {
            get { return _MirrorAlgorithmBrush; }
            set
            {
                if (_MirrorAlgorithmBrush == value)
                    return;

                _MirrorAlgorithmBrush = value;
                RaisePropertyChanged(nameof(MirrorAlgorithmBrush));
            }
        }

        private string _Output;
        public string Output
        {
            get { return _Output; }
            set
            {
                if (_Output == value)
                    return;

                if (value.Length > 2000)
                    _Output = "";
                else
                    _Output = value;
                RaisePropertyChanged(nameof(Output));
            }
        }

        private int _Progress = 0;
        public int Progress
        {
            get { return _Progress; }
            set
            {
                if (_Progress == value)
                    return;

                _Progress = value;
                RaisePropertyChanged(nameof(Progress));
            }
        }

        private double _EstimatedTime = 0;
        public double EstimatedTime
        {
            get { return _EstimatedTime; }
            set
            {
                if (_EstimatedTime == value)
                    return;

                _EstimatedTime = value;
                RaisePropertyChanged(nameof(EstimatedTime));
            }
        }


        private String _MirrorSrc = "No directory location";
        public String MirrorSrc
        {
            get { return _MirrorSrc; }
            set
            {
                if (_MirrorSrc == value)
                    return;

                _MirrorSrc = value;
                RaisePropertyChanged(nameof(MirrorSrc));
            }
        }

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
                        TimeUsagePerFile.Clear();
                        Progress = 0;
                        EstimatedTime = 0;
                        Output = "";
                        progress.ProgressChanged += ReportProgress;

                        MirrorSrc = selectPath("C:\\", "Point to folder with dataset (jpg + xml) \nProgram will create folder called 'yourFolderMirrored' next original one.");
                        MirrorAlgorithmBrush = indicatorColors.busyColor;

                        if (MirrorSrc != null && MirrorSrc != "")
                        {
                            bool result = await Task.Run(() => MirrorAlgorithm.MirrorImgAsync(MirrorSrc, progress));

                            if (result)
                                MirrorAlgorithmBrush = indicatorColors.doneColor;
                            else
                                MirrorAlgorithmBrush = indicatorColors.errorColor;
                        }
                        else
                        {
                            MirrorAlgorithmBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                        }
                    },
                    () =>
                    {
                        return !MirrorAlgorithm.IsRunning;
                    });
                }

                return _MorrorImg;
            }
        }
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
                Output += e.InfoMessage;

            Progress = e.PercentageComplete;

            EstimatedTime = TimeUsagePerFile.Average() * (e.NumberOfAllFilesToProcess - (e.PercentageComplete * e.NumberOfAllFilesToProcess/100));
        }


        string selectPath(string startingDir, string description)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.Description = description;
            folderDlg.SelectedPath = startingDir;
            folderDlg.ShowNewFolderButton = true;

            DialogResult result = folderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (!Directory.Exists(folderDlg.SelectedPath))
                    return null;
                return folderDlg.SelectedPath;
            }
            else
            {
                return null;
            }
        }
    }
}
