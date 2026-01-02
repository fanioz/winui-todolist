using Microsoft.UI.Xaml;
using todolist.Services;
using todolist.ViewModels;

namespace todolist
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>
        /// Gets the main ViewModel for the application.
        /// </summary>
        public static TodoViewModel ViewModel { get; private set; } = null!;

        /// <summary>
        /// Gets the local storage service instance.
        /// </summary>
        public static ILocalStorageService StorageService { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.
        /// </summary>
        public App()
        {
            InitializeComponent();
            
            // Set up dependency injection
            StorageService = new LocalStorageService();
            ViewModel = new TodoViewModel(StorageService);
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
