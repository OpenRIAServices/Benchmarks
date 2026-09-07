using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.Serialization.DataContracts;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using OpenRiaServices.Hosting.AspNetCore;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddResponseCompression(o =>
{
    // Run nettling without compression
    o.EnableForHttps = false;
    o.Providers.Add<BrotliCompressionProvider>();
    o.Providers.Add<GzipCompressionProvider>();
    o.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/msbin1", "application/msgpack", "application/msgpack2" });
});
builder.Services.AddOpenRiaServices()
    .ClearSerializationProviders()
    .AddXmlSerialization(false)    
    .AddMessagePackSerialization(true)
    ;

builder.Services.AddControllers();
builder.Services.AddTransient<CityDomainService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// app.UseResponseCompression();

// app.UseAuthorization();
app.MapControllers();

//builder.WebHost.UseUrls(new[] { "https://localhost:7045", "http://localhost:5000" });


// Enable mapping of all requests to root 
app.MapOpenRiaServices(builder =>
{
    builder.AddDomainService<CityDomainService>();
});

/****************************************************************************
*/
app.MapGet("minimalapi/cities", () => CityDomainService.GetCitiesResult);
app.MapGet("/", httpContext =>
{
    var dataSource = httpContext.RequestServices.GetRequiredService<EndpointDataSource>();

    var sb = new StringBuilder();
    sb.Append("<html><body>");
    sb.AppendLine("<p>Endpoints:</p>");
    foreach (var endpoint in dataSource.Endpoints.OfType<RouteEndpoint>().OrderBy(e => e.RoutePattern.RawText, StringComparer.OrdinalIgnoreCase))
    {
        sb.AppendLine(FormattableString.Invariant($"- <a href=\"{endpoint.RoutePattern.RawText}\">{endpoint.RoutePattern.RawText}</a><br />"));
    }

    var response = httpContext.Response;
    response.StatusCode = 200;

    sb.AppendLine("</body></html>");
    response.ContentType = "text/html";
    return response.WriteAsync(sb.ToString());
});


app.Run();
