using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace CAT.Middleware
{
    public class OnlineEditorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public OnlineEditorMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _httpClient = new HttpClient();
            _configuration = configuration; // Injected IConfiguration
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.ToString().ToLower();
            if (requestPath.Contains("/onlineeditor/"))
            {
                try
                {
                    var targetServerBaseUrl = _configuration["TargetServerBaseUrl"];
                    var targetUrl = targetServerBaseUrl + context.Request.Path.ToString();

                    // Add the query string to the target URL, if it exists
                    if (!string.IsNullOrEmpty(context.Request.QueryString.Value))
                    {
                        targetUrl += context.Request.QueryString.Value;
                    }

                    // Create a new request to the target server
                    var targetRequest = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUrl);
                    // Copy headers and content from the original request
                    foreach (var (key, value) in context.Request.Headers)
                    {
                        targetRequest.Headers.TryAddWithoutValidation(key, value.ToArray());
                    }

                    // Copy content and set the Content-Type header from the original request
                    if (context.Request.ContentLength.HasValue)
                    {
                        var content = new StreamContent(context.Request.Body);
                        targetRequest.Content = content;
                        if (context.Request.Headers.TryGetValue("Content-Type", out var contentType))
                        {
                            targetRequest.Content.Headers.TryAddWithoutValidation("Content-Type", contentType.ToArray());
                        }
                    }

                    // Send the request to the target server and get the response
                    var targetResponse = await _httpClient.SendAsync(targetRequest);

                    //// Copy the target server's response to the original response
                    context.Response.StatusCode = (int)targetResponse.StatusCode;
                    var sHeader = "";
                    foreach (var (key, value) in targetResponse.Headers)
                    {
                        //context.Response.Headers[key] = value.ToArray();
                        sHeader += "key: " + String.Join(',', value.ToArray()) + "\n";
                    }

                    try { File.WriteAllText("/data/contents/OE.log", sHeader); } catch (Exception) { }

                    await targetResponse.Content.CopyToAsync(context.Response.Body);
                    return;
                }
                catch (Exception ex)
                {
                    // Read the contents of the custom error HTML page
                    //var errorHtml = File.ReadAllText("Error.html"); // Replace with the actual path to your HTML error page

                    // Set the response status code to 500 (Internal Server Error)
                    context.Response.StatusCode = 500;

                    // Set the Content-Type header to indicate HTML content
                    context.Response.Headers["Content-Type"] = "text/html";

                    // Write the error HTML as the response content
                    await context.Response.WriteAsync($"<html>{" error: " + ex.ToString()}</html>");
                }
            }
            else
            {
                // If the path does not contain "/OnlineEditor/", continue with the next middleware
                await _next(context);
            }
        }
    }
}
