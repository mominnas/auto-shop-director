using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Azure.Identity;
using Azure.Core;
using System.Threading;

namespace MMN.App.ViewModels
{
    /// <summary>
    /// Handles user authentication and getting user info from the Microsoft Graph API.
    /// </summary>
    public class AuthenticationViewModel : BindableBase
    {
        /// <summary>
        /// Creates a new AuthenticationViewModel for logging users in and getting their info.
        /// </summary>
        public AuthenticationViewModel()
        {
            Task.Run(PrepareAsync);
        }

        private string _name;

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        private string _email;

        /// <summary>
        /// Gets or sets the user's email.
        /// </summary>
        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        private string _title;

        /// <summary>
        /// Gets or sets the user's standard title.
        /// </summary>
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        private string _domain;

        /// <summary>
        /// Gets or sets the user's AAD domain.
        /// </summary>
        public string Domain
        {
            get => _domain;
            set => Set(ref _domain, value);
        }

        private BitmapImage _photo;

        /// <summary>
        /// Gets or sets the user's photo.
        /// </summary>
        public BitmapImage Photo
        {
            get => _photo;
            set => Set(ref _photo, value);
        }

        private string _errorText;

        /// <summary>
        /// Gets or sets error text to show if the login operation fails.
        /// </summary>
        public string ErrorText
        {
            get => _errorText;
            set => Set(ref _errorText, value);
        }

        private bool _showWelcome;

        /// <summary>
        /// Gets or sets whether to show the starting welcome UI. 
        /// </summary>
        public bool ShowWelcome
        {
            get => _showWelcome;
            set => Set(ref _showWelcome, value);
        }

        private bool _showLoading;

        /// <summary>
        /// Gets or sets whether to show the logging in progress UI.
        /// </summary>
        public bool ShowLoading
        {
            get => _showLoading;
            set => Set(ref _showLoading, value);
        }

        private bool _showData;

        /// <summary>
        /// Gets or sets whether to show user data UI.
        /// </summary>
        public bool ShowData
        {
            get => _showData;
            set => Set(ref _showData, value);
        }

        private bool _showError;

        /// <summary>
        /// Gets or sets whether to show the error UI.
        /// </summary>
        public bool ShowError
        {
            get => _showError;
            set => Set(ref _showError, value);
        }

        /// <summary>
        /// Prepares the login sequence. 
        /// </summary>
        public async Task PrepareAsync()
        {
            var accounts = await MsalHelper.GetAccountsAsync();
            if (accounts.Any())
            {
                await LoginAsync();
            }
            else
            {
                SetVisible(vm => vm.ShowWelcome);
            }
        }

        /// <summary>
        /// Logs the user in by requesting a token and using it to query the 
        /// Microsoft Graph API.
        /// </summary>
        public async Task LoginAsync()
        {
            try
            {
                SetVisible(vm => vm.ShowLoading);
                string token = await MsalHelper.GetTokenAsync(Repository.Constants.GraphpApiScopes);
                if (token != null)
                {
                    await SetUserInfoAsync(token);
                    await SetUserPhoto(token);
                    SetVisible(vm => vm.ShowData);
                }
                else
                {
                    SetVisible(vm => vm.ShowError);
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
                SetVisible(vm => vm.ShowError);
            }
        }

        /// <summary>
        /// Gets and processes the user's info from the Microsoft Graph API.
        /// </summary>
        private async Task SetUserInfoAsync(string token)
        {
            var accounts = await MsalHelper.GetAccountsAsync();
            var domain = accounts?.First().Username.Split('@')[1] ?? string.Empty;

            var tokenCredential = new TokenCredentialAdapter(token);
            var graph = new GraphServiceClient(tokenCredential);
            

            var me = await graph.Me.GetAsync();

            App.Window.DispatcherQueue.TryEnqueue(
                DispatcherQueuePriority.Normal, () =>
            {
                Name = me.DisplayName;
                Email = me.Mail;
                Title = me.JobTitle;
                Domain = domain;
            });
        }

        /// <summary>
        /// Gets and processes the user's photo from the Microsoft Graph API. 
        /// </summary>
        private async Task SetUserPhoto(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                string url = "https://graph.microsoft.com/beta/me/photo/$value";
                var result = await client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    return;
                }
                using (Stream stream = await result.Content.ReadAsStreamAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        App.Window.DispatcherQueue.TryEnqueue(
                            DispatcherQueuePriority.Normal, async () =>
                        {
                            Photo = new BitmapImage();
                            await Photo.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Logs the user in.
        /// </summary>
        public async void LoginClick()
        {
            await LoginAsync();
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        public async void LogoutClick()
        {
            var signoutDialog = new ContentDialog()
            {
                Title = "Sign out",
                Content = "Sign out?",
                PrimaryButtonText = "Sign out",
                SecondaryButtonText = "Cancel"

            };
            signoutDialog.PrimaryButtonClick += async (_, _) =>
            {
                await MsalHelper.RemoveCachedTokens();
                SetVisible(vm => vm.ShowWelcome);
            };
            signoutDialog.XamlRoot = App.Window.Content.XamlRoot;

            await signoutDialog.ShowAsync();
        }

        /// <summary>
        /// Shows one part of the login UI sequence and hides all the others.
        /// </summary>
        private void SetVisible(Expression<Func<AuthenticationViewModel, bool>> selector)
        {
            var prop = (PropertyInfo)((MemberExpression)selector.Body).Member;

            App.Window.DispatcherQueue.TryEnqueue(
                DispatcherQueuePriority.High, () =>
            {
                ShowWelcome = false;
                ShowLoading = false;
                ShowData = false;
                ShowError = false;
                prop.SetValue(this, true);
            });
        }
    }

    public class TokenCredentialAdapter : TokenCredential
    {
        private readonly string _token;

        public TokenCredentialAdapter(string token)
        {
            _token = token;
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new AccessToken(_token, DateTimeOffset.UtcNow.AddHours(1)); // Adjust token expiration as needed
        }


        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(new AccessToken(_token, DateTimeOffset.UtcNow.AddHours(1))); // Adjust token expiration as needed
        }

    }
}
