
/************************************************************************************************
The MIT License (MIT)

Copyright (c) 2015 Microsoft Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
***********************************************************************************************/

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TestMSAL
{
    public static class MsalAppBuilder
    {
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];

        private static string authority = aadInstance + tenantId + "/v2.0";
        private static String RedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static String ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];

        public static string GetAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string oid = claimsPrincipal.GetObjectId();
            string tid = claimsPrincipal.GetTenantId();
            return $"{oid}.{tid}";
        }

        public static async Task<IConfidentialClientApplication> BuildConfidentialClientApplication()
        {
            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(clientId)
                  .WithClientSecret(ClientSecret)
                  .WithRedirectUri(RedirectUri)
                  .WithAuthority(authority)
                  .Build();

            // After the ConfidentialClientApplication is created, we overwrite its default UserTokenCache serialization with our implementation
            IMsalTokenCacheProvider memoryTokenCacheProvider = CreateTokenCacheSerializer();
            await memoryTokenCacheProvider.InitializeAsync(clientapp.UserTokenCache);
            return clientapp;
        }

        public static async Task RemoveAccount()
        {
            IConfidentialClientApplication clientapp = ConfidentialClientApplicationBuilder.Create(clientId)
                  .WithClientSecret(ClientSecret)
                  .WithRedirectUri(RedirectUri)
                  .WithAuthority(authority)
                  .Build();

            // We only clear the user's tokens.
            IMsalTokenCacheProvider memoryTokenCacheProvider = CreateTokenCacheSerializer();
            await memoryTokenCacheProvider.InitializeAsync(clientapp.UserTokenCache);
            var userAccount = await clientapp.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId());
            if (userAccount != null)
            {
                await clientapp.RemoveAsync(userAccount);
            }
        }
        private static IMsalTokenCacheProvider CreateTokenCacheSerializer()
        {
            IServiceCollection services = new ServiceCollection();

            // In memory token cache. Other forms of serialization are possible.
            // See https://github.com/AzureAD/microsoft-identity-web/wiki/asp-net 
            services.AddInMemoryTokenCaches();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IMsalTokenCacheProvider msalTokenCacheProvider = serviceProvider.GetRequiredService<IMsalTokenCacheProvider>();
            return msalTokenCacheProvider;
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}

