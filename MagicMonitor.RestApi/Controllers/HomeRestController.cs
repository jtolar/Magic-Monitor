using nanoFramework.WebServer;

namespace MagicMonitor.RestApi.Controllers
{
    public class HomeRestController
    {
        [Route(""), Route("home")]
        public void Home(WebServerEventArgs e)
        {

            e.Context.Response.ContentType = "text/html";
            WebServer.OutPutStream(e.Context.Response, "<html><head>" +
                                                       "<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
                                                       "<a href='/text.txt'>Download the Text.txt file</a><br>" +
                                                       "Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
        }
    }
}