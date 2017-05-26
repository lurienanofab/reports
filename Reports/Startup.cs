using LNF.Impl;
using Microsoft.Owin;
using Owin;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

[assembly: OwinStartup(typeof(Reports.Startup))]

namespace Reports
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseDataAccess();

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var config = new HttpConfiguration();
            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }
    }
}
