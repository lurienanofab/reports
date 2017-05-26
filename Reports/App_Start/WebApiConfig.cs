using System.Web.Http;

namespace Reports
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.MapHttpAttributeRoutes();
        }
    }
}
