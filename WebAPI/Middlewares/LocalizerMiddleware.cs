using System.Globalization;

namespace WebAPI.Middlewares
{
    public class LocalizerMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Set the culture key based on the request header
            var cultureKey = context.Request.Headers["Accept-Language"];

            // If there is supplied a culture
            if (!string.IsNullOrEmpty(cultureKey))
            {
                // Check if the culture exists
                if (DoesCultureExist(cultureKey))
                {
                    // Set the culture Info
                    var culture = new CultureInfo(cultureKey);

                    // Set the culture in the current thread responsible for that request
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }
            }

            // Await the next request
            await next(context);
        }

        private static bool DoesCultureExist(string cultureName)
        {
            // Return the culture where the culture equals the culture name set
            return CultureInfo.GetCultures(CultureTypes.AllCultures).
                Any(culture => string.Equals(culture.Name, cultureName,
                                             StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
