using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security.Notifications;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace TestMSAL
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = aadInstance + tenantId + "/v2.0";
        private static string WellKnownMetadata = $"{authority}/.well-known/openid-configuration";
        private static String RedirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
        private static String ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
        public void ConfigureAuth(IAppBuilder app)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //this is critical otherwise claims like sub get removed and replaced with KnownClaimsTypes.NameIdentifier
            //which in this case is messing up antiforgery token which we set in Global.asax.cs as below
            //AntiForgeryConfig.UniqueClaimTypeIdentifier = JwtClaimTypes.Subject;
            //and messing up our caching which uses subject as the unique identifier.
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromMinutes(15),
                CookieManager = new SystemWebChunkingCookieManager(),

            });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = RedirectUri,
                    MetadataAddress = WellKnownMetadata,
                    ClientSecret = ClientSecret,
                    Scope = "openid profile offline_access",
                    RequireHttpsMetadata = true,
                    UseTokenLifetime = false,
                    RedeemCode = true,
                    ResponseType = "code",
                    ResponseMode = "query",
                    RefreshOnIssuerKeyNotFound = true,
                    AuthenticationType = OpenIdConnectAuthenticationDefaults.AuthenticationType,
                    SignInAsAuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthorizationCodeReceived = OnAuthorizationCodeReceivedAsync
                    },
                }
                );
        }
        async Task OnAuthorizationCodeReceivedAsync(AuthorizationCodeReceivedNotification notification)
        {
 
            IConfidentialClientApplication clientApp = await MsalAppBuilder.BuildConfidentialClientApplication();
			AuthenticationResult result = await clientApp.AcquireTokenByAuthorizationCode(new[] { "openid", "profile", "offline_access" }, notification.Code)
				.ExecuteAsync();
			notification.HandleCodeRedemption(result.AccessToken, result.IdToken);
            notification.TokenEndpointResponse.Scope = String.Join(" ", result.Scopes);
			var accounts = await clientApp.GetAccountsAsync();
			IAccount first = accounts.First();
			System.Diagnostics.Debug.WriteLine($"HomeAccountId value from OnAuthorizationCodeReceivedAsync:{first.HomeAccountId}");
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
