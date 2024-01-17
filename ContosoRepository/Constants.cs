namespace Contoso.Repository
{
    /// <summary>
    /// Contains constant values you'll need to insert before running the sample. 
    /// Note: this file is provided for convenience only. In a production app,
    /// never store sensitive information in code or share server-side keys with
    /// the client.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Base URL for the app's API service. A live test service is provided for convenience; 
        /// however, you cannot modify any data on the server or deploy your own updates. 
        /// To see the full functionality, deploy Contoso.Service using your own Azure account.
        /// </summary>
        public const string ApiUrl = @"http://customers-orders-api-prod.azurewebsites.net";

        /// <summary>
        /// The Azure Active Directory (AAD) client id.
        /// </summary>
        public const string AccountClientId = "<TODO: Insert Azure client Id>";

        /// <summary>
        /// The Azure Active Directory (AAD) rest api client id.
        /// </summary>
        public const string WebApiClientId = "< TODO: Insert Azure client Id>";

        /// <summary>
        /// Connection string for a server-side SQL database.
        /// </summary>
        public const string SqlAzureConnectionString = "<TODO: Insert connection string>";

        // Cache settings
        public const string CacheFileName = "contosoapp_msal_cache.txt";

        // Graph Api Scopes
        public static readonly string[] GraphpApiScopes = new[] { "https://graph.microsoft.com/User.Read" };

        // Downstream Api Scopes
        public static readonly string[] WebApiScopes = new[] { $"api://{WebApiClientId}/Contoso.ReadWrite" };
        
    }
}
