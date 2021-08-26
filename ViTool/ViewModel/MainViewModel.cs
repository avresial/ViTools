using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

namespace ViTool.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ;
        }

        private MainPanelViewModel _MainPanelViewModel = new MainPanelViewModel();
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



    }
}