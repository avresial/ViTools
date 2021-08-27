using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
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

        private String _TranslateXmlToTxTSrc = "No directory location";
        public String TranslateXmlToTxTSrc
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
                        TranslateXmlToTxTSrc = selectPath("C:\\", "Point to folder with xml files. \nProgram will create txt files called 'yourFile.txt' next original ones.");
                        TranslateXmlToTxTInfoBrush = indicatorColors.busyColor;
                        if (TranslateXmlToTxTSrc != null && TranslateXmlToTxTSrc != "")
                        {
                            bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(TranslateXmlToTxTSrc, ".xml"));
                            if (result)
                                TranslateXmlToTxTInfoBrush = indicatorColors.doneColor;
                            else
                                TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                        }
                        else
                        {
                            TranslateXmlToTxTInfoBrush = indicatorColors.errorColor;
                            TranslateXmlToTxT.Output = "There is no files";
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
