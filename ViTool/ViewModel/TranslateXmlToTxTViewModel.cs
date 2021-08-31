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
    public class TranslateXmlToTxTViewModel : ViewModelBase
    {
        private IndicatorColors indicatorColors = new IndicatorColors();
        private Progress<ProgressReportModel> progress = new Progress<ProgressReportModel>();
        int maxOutputLines = 2000;

        private TranslateXmlToTxTAlgorithm _TranslateXmlToTxT = new TranslateXmlToTxTAlgorithm();
        public TranslateXmlToTxTAlgorithm TranslateXmlToTxT
        {
            get { return _TranslateXmlToTxT; }
            set
            {
                if (_TranslateXmlToTxT == value)
                    return;

                _TranslateXmlToTxT = value;
                RaisePropertyChanged(nameof(TranslateXmlToTxT));
            }
        }

        private SolidColorBrush _TranslateXmlToTxTInfoBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        public SolidColorBrush TranslateXmlToTxTInfoBrush
        {
            get { return _TranslateXmlToTxTInfoBrush; }
            set
            {
                if (_TranslateXmlToTxTInfoBrush == value)
                    return;

                _TranslateXmlToTxTInfoBrush = value;
                RaisePropertyChanged(nameof(TranslateXmlToTxTInfoBrush));
            }
        }

        private string _TranslateXmlToTxTSrc = "No directory location";
        public string TranslateXmlToTxTSrc
        {
            get { return _TranslateXmlToTxTSrc; }
            set
            {
                if (_TranslateXmlToTxTSrc == value)
                    return;

                _TranslateXmlToTxTSrc = value;
                RaisePropertyChanged(nameof(TranslateXmlToTxTSrc));
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

                if (value.Length > maxOutputLines)
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

        private int _EstimatedTime = 0;
        public int EstimatedTime
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

        private string _SelectedClass;
        public string SelectedClass
        {
            get { return _SelectedClass; }
            set
            {
                if (_SelectedClass == value)
                    return;

                _SelectedClass = value;
                RaisePropertyChanged(nameof(SelectedClass));
            }
        }

        private string _NewClass;
        public string NewClass
        {
            get { return _NewClass; }
            set
            {
                if (_NewClass == value)
                    return;

                _NewClass = value;
                RaisePropertyChanged(nameof(NewClass));
            }
        }

        private ObservableCollection<string> _ListOfClasses = new ObservableCollection<string>() { "HCH", "LowFreqAnomaly", "Imprint", "Break", "ChippedBreak" };
        public ObservableCollection<string> ListOfClasses
        {
            get { return _ListOfClasses; }
            set
            {
                if (_ListOfClasses == value)
                    return;

                _ListOfClasses = value;
                RaisePropertyChanged(nameof(ListOfClasses));
            }
        }

        public TranslateXmlToTxTViewModel()
        {
            progress.ProgressChanged += ReportProgress;
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
                        
                      
                        TranslateXmlToTxTSrc = selectPath("C:\\", "Point to folder with xml files. \nProgram will create txt files called 'yourFile.txt' next original ones.");
                        TranslateXmlToTxTInfoBrush = indicatorColors.busyColor;

                        if (TranslateXmlToTxTSrc != null && TranslateXmlToTxTSrc != "")
                        {
                            bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(TranslateXmlToTxTSrc, ".xml", ListOfClasses.ToList(), progress));
                            
                            if (result)
                                TranslateXmlToTxTInfoBrush = indicatorColors.doneColor;
                            else
                                TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                        }
                        else
                        {
                            TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                            Output += "There is no files";
                        }
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
                        if (NewClass != null && NewClass != "")
                        {
                            ListOfClasses.Add(NewClass);
                            NewClass = "";
                        }
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

        private void ReportProgress(object sender, ProgressReportModel e)
        {
            if (e.ErrorMessage != "")
            {
                Output += e.ErrorMessage;
                //EstimatedTime = 0;
                return;
            }

            foreach (string line in e.FilesProcessed)
                Output += line + "\n";

            if (e.InfoMessage != "")
                Output +=e.InfoMessage;

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
