using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nancy.Owin;
using org.neurul.Cortex.Port.Adapter.Common;
using System;

namespace org.neurul.Cortex.Port.Adapter.In.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: transfer to sentry
            // services.AddAuthentication("Bearer")
            //    .AddIdentityServerAuthentication(options =>
            //    {
            //        options.Authority = Environment.GetEnvironmentVariable(EnvironmentVariableKeys.TokenIssuerAddress);
            //        options.RequireHttpsMetadata = false;
            //        options.ApiSecret = "secret";
            //        options.ApiName = "cortex-in";
            //    });
        }

        public void Configure(IApplicationBuilder app)
        {
            // TODO: transfer to sentry 
            // app.UseAuthentication();
            app.UseOwin(buildFunc => buildFunc.UseNancy());
        }
    }
}
