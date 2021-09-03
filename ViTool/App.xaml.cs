using Autofac;
using System.Windows;
using ViTool.IOC;
using ViTool.ViewModel;

namespace ViTool
{   
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var container = ContainerConfig.Configure();

            var scope = container.BeginLifetimeScope();
            var mainViewModel = scope.Resolve<MainViewModel>();

            MainWindow mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();
        }
    }
}
