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

        public MainPanelViewModel(MirrorViewModel mirrorViewModel, TranslateXmlToTxTViewModel translateXmlToTxTViewModel) 
        {
            _MirrorViewModel = mirrorViewModel;
            _TranslateXmlToTxTViewModel = translateXmlToTxTViewModel;
        }
    }
}
