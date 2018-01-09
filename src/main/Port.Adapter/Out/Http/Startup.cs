using Microsoft.AspNetCore.Builder;
using Nancy.Owin;

namespace org.neurul.Cortex.Port.Adapter.Out.Http
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(buildFunc => buildFunc.UseNancy());
        }
    }
}
