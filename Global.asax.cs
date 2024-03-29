﻿using Data.Context;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Web;

namespace SysIgreja
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            SqlServerTypes.Utilities.LoadNativeAssemblies(Server.MapPath("~/bin"));
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Data.Migrations.Configuration>());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            UnityConfig.RegisterComponents();
        }

        protected void Application_EndRequest()
        {
            var context = new HttpContextWrapper(this.Context);
            if (context.Response.StatusCode == 302
                && context.Response.SuppressFormsAuthenticationRedirect)
            {
                context.Response.Clear();
                context.Response.StatusCode = 401;
            }
        }
    }
}
