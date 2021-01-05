using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace TestMSAL.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            IConfidentialClientApplication app = AsyncHelper.RunSync<IConfidentialClientApplication>(() => MsalAppBuilder.BuildConfidentialClientApplication());
            string oid = ClaimsPrincipal.Current.GetObjectId();
            System.Diagnostics.Debug.WriteLine($"oid value:{oid}");
            string tid = ClaimsPrincipal.Current.GetTenantId();
            System.Diagnostics.Debug.WriteLine($"tid value:{tid}");
            string uid = ClaimsPrincipal.Current.GetHomeObjectId();
            System.Diagnostics.Debug.WriteLine($"uid value:{uid}");
            string utid = ClaimsPrincipal.Current.GetHomeTenantId();
            System.Diagnostics.Debug.WriteLine($"utid value:{utid}");
            string msalaccountid = ClaimsPrincipal.Current.GetMsalAccountId();
            System.Diagnostics.Debug.WriteLine($"msalaccountid value:{msalaccountid}");
            var accounts = app.GetAccountsAsync().Result;
            if (accounts != null)
            {
                IAccount first = accounts.FirstOrDefault();
                if (first != null)
                {
                    System.Diagnostics.Debug.WriteLine($"HomeAccountId value from Index:{first.HomeAccountId}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("GetAccountsAsync returned an empty collection");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetAccountsAsync returned null");
            }
            IAccount account = app.GetAccountAsync(ClaimsPrincipal.Current.GetAccountId()).Result;
            if (account != null)
            {

                System.Diagnostics.Debug.WriteLine($"HomeAccountId value from Index:{account.HomeAccountId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GetAccountAsync returned null");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}