using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using ViTool.Models;

namespace ViTool.ViewModel
{
    public class MainPanelViewModel : ViewModelBase
    {

        private ObservableCollection<string> _FilesList = new ObservableCollection<string>();
        public ObservableCollection<string> FilesList
        {
            get { return _FilesList; }
            set
            {
                if (_FilesList == value) return;

                _FilesList = value;
            }
        }



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


        public MainPanelViewModel(MirrorViewModel mirrorViewModel, TranslateXmlToTxTViewModel translateXmlToTxTViewModel)
        {
            _MirrorViewModel = mirrorViewModel;
            _TranslateXmlToTxTViewModel = translateXmlToTxTViewModel;
            TranslateXmlToTxTInfoBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        }

        private RelayCommand _LoadData;
        public RelayCommand LoadData
        {
            get
            {
                if (_LoadData == null)
                {
                    _LoadData = new RelayCommand(
                     () =>
                        {
                            Output = "";
                            FilesList.Clear();

                            DirectoryPath = selectPath(DirectoryPath, "xd");
                            if (DirectoryPath == null || DirectoryPath == "") return;

                            DirectoryInfo d = new DirectoryInfo(DirectoryPath);

                            DirectoryInfo[] directories = d.GetDirectories();
                            foreach (DirectoryInfo directory in directories) FilesList.Add(directory.FullName);

                            FileInfo[] Files = d.GetFiles();
                            foreach (FileInfo file in Files) FilesList.Add(file.FullName);




                        },
                    () =>
                    {
                        return true;
                    });
                }

                return _LoadData;
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
    }
}
