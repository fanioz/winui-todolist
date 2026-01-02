using Microsoft.UI.Xaml;

namespace todolist
{
    /// <summary>
    /// Main window for the To-Do List application.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set window size
            var appWindow = this.AppWindow;
            appWindow.Resize(new Windows.Graphics.SizeInt32(500, 600));
            
            // Set title
            Title = "To-Do List";
            
            // Navigate to the main page
            RootFrame.Navigate(typeof(MainPage));
        }
    }
}
