using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace portal_instadose_com_v3
{
    public class Global : System.Web.HttpApplication
    {

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
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();
            var source = "unknown";
            var message = "Unknown: Null exception";
            if (exc != null && exc.InnerException != null)
            {
                message = exc.InnerException.Message;
                source = exc.InnerException.Source;
            }
            // Handle HTTP errors
            if (exc != null && exc.GetType() == typeof(HttpException))
            {
                // The Complete Error Handling Example generates
                // some errors using URLs with "NoCatch" in them;
                // ignore these here to simulate what would happen
                // if a global.asax handler were not implemented.
                if (exc.Message.Contains("NoCatch") || exc.Message.Contains("maxUrlLength"))
                    return;
                else
                {
                    Instadose.Basics.WriteLogEntry(message, Request.LogonUserIdentity.Name, "PORTAL: " + source, Instadose.Basics.MessageLogType.Notice);
                }
                //Redirect HTTP errors to HttpError page
                Server.Transfer("~/ErrorPages/HttpErrorPage.aspx");
            }
            else
                Instadose.Basics.WriteLogEntry(message, Request.LogonUserIdentity.Name, "PORTAL: " + source, Instadose.Basics.MessageLogType.Notice);

            Server.ClearError();
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}