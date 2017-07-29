using KegokProj.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace KegokProj
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //FileWatcher fW = new FileWatcher();
            //fW.Start();
            //MultiWatcher mW = new MultiWatcher(Server.MapPath("~/Content"));
        }
    }
}
