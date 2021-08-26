using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ViTool.Models;

namespace ViTool.ViewModel
{

    public class MainPanelViewModel : ViewModelBase
    {

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

        private String _TranslateXmlToTxTSrc;
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
                        TranslateXmlToTxTSrc = selectPath("R:\\Graw\\Defektoskopia\\VI2Defect");
                        if (TranslateXmlToTxTSrc != null && TranslateXmlToTxTSrc != "")
                            await Task.Run(() => TranslateXmlToTxT.TranslateXmlToTxTAsync(TranslateXmlToTxTSrc, ".xml"));
                    },
                    () =>
                    {
                        return true;
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
                    () =>
                    {
                        ;
                    },
                    () =>
                    {
                        return true;
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
