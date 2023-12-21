using System.Net.Http.Headers;
using System.Text;
using DatabaseGpt.Web.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using TinyHelpers.Extensions;

namespace DatabaseGpt.Web.Swagger;

public class SwaggerBasicAuthenticationMiddleware(RequestDelegate next, IOptions<SwaggerSettings> settingsOptions)
{
    private readonly SwaggerSettings settings = settingsOptions.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") && settings.UserName.HasValue() && settings.Password.HasValue())
        {
            string authenticationHeader = context.Request.Headers[HeaderNames.Authorization];
            if (authenticationHeader?.StartsWith("Basic ") ?? false)
            {
                // Get the credentials from request header
                var header = AuthenticationHeaderValue.Parse(authenticationHeader);
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter)).Split(':', count: 2);
                var userName = credentials.ElementAtOrDefault(0);
                var password = credentials.ElementAtOrDefault(1);

                // validate credentials
                if (userName == settings.UserName && password == settings.Password)
                {
                    await next.Invoke(context).ConfigureAwait(false);
                    return;
                }
            }

            context.Response.Headers.WWWAuthenticate = new StringValues("Basic");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            await next.Invoke(context).ConfigureAwait(false);
        }
    }
}