using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ViTool.Models;

namespace ViTool.ViewModel
{

    public class MainPanelViewModel : ViewModelBase
    {
        private SolidColorBrush busyColor = new SolidColorBrush(Color.FromRgb(255, 255, 0));
        private SolidColorBrush doneColor = new SolidColorBrush(Color.FromRgb(0, 204, 0));
        private SolidColorBrush errorColor = new SolidColorBrush(Color.FromRgb(216, 31, 42));

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

        public MainPanelViewModel()
        {

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
                        TranslateXmlToTxTSrc = selectPath("C:\\");
                        TranslateXmlToTxTInfoBrush = busyColor;
                        if (TranslateXmlToTxTSrc != null && TranslateXmlToTxTSrc != "")
                        {
                            bool result = await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(TranslateXmlToTxTSrc, ".xml"));
                            if (result)
                                TranslateXmlToTxTInfoBrush = doneColor;
                            else
                                TranslateXmlToTxTInfoBrush = errorColor;
                        }
                        else
                        {
                            TranslateXmlToTxTInfoBrush = errorColor;
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
                        MirrorSrc = selectPath("C:\\");
                        MirrorAlgorithmBrush = busyColor;
                        if (MirrorSrc != null && MirrorSrc != "")
                        {
                            bool result = await Task.Run(() => MirrorAlgorithm.MirrorImgAsync(MirrorSrc));

                            if (result)
                                MirrorAlgorithmBrush = doneColor;
                            else
                                MirrorAlgorithmBrush = errorColor;

                        }
                        else
                        {
                            MirrorAlgorithmBrush = errorColor;
                            MirrorAlgorithm.Output = "There is no files";
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

        string selectPath(string startingDir)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();

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
