﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.ApplicationModel;
using Windows.Globalization;
using Windows.Storage;
using MMN.App.ViewModels;
using MMN.App.Views;
using MMN.Repository;
using MMN.Repository.Rest;
using MMN.Repository.Sql;
using System;

namespace MMN.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

        /// <summary>
        /// Gets main App Window
        /// </summary>
        public static Window Window { get { return m_window; } }
        private static Window m_window;

        /// <summary>
        /// Gets the app-wide MainViewModel singleton instance.
        /// </summary>
        public static MainViewModel ViewModel { get; } = new MainViewModel();

        /// <summary>
        /// Pipeline for interacting with backend service or database.
        /// </summary>
        public static IContosoRepository Repository { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() => InitializeComponent();

        /// <summary>
        /// Invoked when the application is launched normally by the end user.
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            m_window = new MainWindow();

            // Load the database.
            if (ApplicationData.Current.LocalSettings.Values.TryGetValue(
                "data_source", out object dataSource))
            {
                switch (dataSource.ToString())
                {
                    case "Rest": if (!UseRest()) UseSqlite(); break;
                    default: UseSqlite(); break;
                }
            }
            else
            {
                UseSqlite();
            }

            // Prepare the app shell and window content.
            AppShell shell = m_window.Content as AppShell ?? new AppShell();
            shell.Language = ApplicationLanguages.Languages[0];
            m_window.Content = shell;

            if (shell.AppFrame.Content == null)
            {
                // When the navigation stack isn't restored, navigate to the first page
                // suppressing the initial entrance animation.
                shell.AppFrame.Navigate(typeof(CustomerListPage), null,
                    new SuppressNavigationTransitionInfo());
            }

            m_window.Activate();
        }

        /// <summary>
        /// Configures the app to use the Sqlite data source. If no existing Sqlite database exists, 
        /// loads a demo database filled with fake data so the app has content.
        /// </summary>
        public static void UseSqlite()
        {
            string demoDatabasePath;
            string databasePath;

            try
            {
                // For packaged apps
                demoDatabasePath = Path.Combine(Windows.ApplicationModel.Package.Current.InstalledLocation.Path, "Assets", "Contoso.db");
                databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Contoso.db");
            }
            catch
            {
                // For unpackaged apps
                demoDatabasePath = Path.Combine(AppContext.BaseDirectory, "Assets", "Contoso.db");
                databasePath = Path.Combine(AppContext.BaseDirectory, "Contoso.db");
            }

            if (!File.Exists(databasePath))
            {
                try
                {
                    File.Copy(demoDatabasePath, databasePath);
                }
                catch (Exception ex)
                {
                    // Log or handle the error
                    System.Diagnostics.Debug.WriteLine($"Failed to copy database: {ex.Message}");
                }
            }

            var dbOptions = new DbContextOptionsBuilder<ContosoContext>().UseSqlite(
                "Data Source=" + databasePath);
            Repository = new SqlContosoRepository(dbOptions);


            // Ensure the database is created
            using (var context = new ContosoContext(dbOptions.Options))
            {
                context.Database.EnsureCreated();
            }

        }

        /// <summary>
        /// Configures the app to use the REST data source. For convenience, a read-only source is provided. 
        /// You can also deploy your own copy of the REST service locally or to Azure. See the README for details.
        /// </summary>
        public static bool UseRest() 
        {
            try
            {
                var accessToken = Task.Run(async () => await MsalHelper.GetTokenAsync(Constants.WebApiScopes)).Result;

                Repository = new RestContosoRepository($"{Constants.ApiUrl}/api/", accessToken);
            }
            catch (System.Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
