using Microsoft.AspNetCore.Builder;
using Nancy.Owin;

namespace org.neurul.Brain.Port.Adapter.In.Http
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseOwin(buildFunc => buildFunc.UseNancy());
        }
    }
}
