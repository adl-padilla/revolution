using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace RevolutionWeb
{
    public class Global : System.Web.HttpApplication
    {
        public static bool UseMinified=true;

        public static string JQuery
        {
            get
            {
                if (UseMinified)
                    return "Scripts/jquery-3.3.1.min.js";
                else
                    return "Scripts/jquery-3.3.1.js";
            }
        }
        public static string SignalR
        {
            get
            {
                if (UseMinified)
                    return "Scripts/jquery.signalR-2.3.0.min.js";
                else
                    return "Scripts/jquery.signalR-2.3.0.js";
            }
        }


        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}