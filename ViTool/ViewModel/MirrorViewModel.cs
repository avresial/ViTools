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
    public class MirrorViewModel : ViewModelBase
    {
        private IndicatorColors indicatorColors = new IndicatorColors();

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
                        MirrorSrc = selectPath("C:\\", "Point to folder with dataset (jpg + xml) \nProgram will create folder called 'yourFolderMirrored' next original one.");
                        MirrorAlgorithmBrush = indicatorColors.busyColor;
                        if (MirrorSrc != null && MirrorSrc != "")
                        {
                            bool result = await Task.Run(() => MirrorAlgorithm.MirrorImgAsync(MirrorSrc));

                            if (result)
                                MirrorAlgorithmBrush = indicatorColors.doneColor;
                            else
                                MirrorAlgorithmBrush = indicatorColors.errorColor;

                        }
                        else
                        {
                            MirrorAlgorithmBrush = indicatorColors.errorColor;
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
