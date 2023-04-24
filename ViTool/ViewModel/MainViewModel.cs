using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ViTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private MainPanelViewModel _MainPanelViewModel;
        public MainPanelViewModel MainPanelViewModel
        {
            get { return _MainPanelViewModel; }
            set
            {
                if (_MainPanelViewModel == value)
                    return;

                _MainPanelViewModel = value;
                RaisePropertyChanged(nameof(MainPanelViewModel));
            }
        }

        public MainViewModel(MainPanelViewModel mainPanelViewModel)
        {
            _MainPanelViewModel = mainPanelViewModel; 
        }
    }
}