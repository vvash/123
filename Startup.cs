using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NewsBlog.Startup))]
namespace NewsBlog
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
