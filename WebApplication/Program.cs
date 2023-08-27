using System.Text;
using OpenRiaServices.Client.Benchmarks.Server.Cities;
using OpenRiaServices.Hosting.AspNetCore;


CityDomainService.GetCitiesResult = CreateValidCities(10).ToList();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenRiaServices();
builder.Services.AddControllers();
builder.Services.AddTransient<CityDomainService>();

var app = builder.Build();

// Configure the HTTP request pipeline.

// app.UseAuthorization();
app.MapControllers();

//builder.WebHost.UseUrls(new[] { "https://localhost:7045", "http://localhost:5000" });


// Enable mapping of all requests to root 
app.MapOpenRiaServices(builder =>
{
    builder.AddDomainService<CityDomainService>();
});

/****************************************************************************
// Bytes
var megabyte = new byte[1024 * 1024];
Random.Shared.NextBytes(megabyte);

foreach (var size in new[] { 1, 5, 10, 100 })
{
    // Write 10 megabyte
    app.MapGet($"/{size}/a", async httpContext =>
    {
        // MINDRE GC ??
        using ArrayPoolStream.BufferMemory memory = WriteMemory(megabyte, size);

        httpContext.Response.Headers.ContentLength = memory.Length;
        httpContext.Response.Headers.ContentType = "application/binary";
        await memory.WriteTo(httpContext.Response, default);
    });


    app.MapGet($"/{size}/b", async httpContext =>
    {
        using ArrayPoolStream.BufferMemory memory = WriteMemory(megabyte, size);

        httpContext.Response.Headers.ContentLength = memory.Length;
        httpContext.Response.Headers.ContentType = "application/binary";
        await memory.WriteToAsync(httpContext.Response, default);
    });

    app.MapGet($"/{size}/c", async httpContext =>
    {
        using ArrayPoolStream.BufferMemory memory = WriteMemory(megabyte, size);
        // MINDRE GC ??
        httpContext.Response.Headers.ContentLength = memory.Length;
        httpContext.Response.Headers.ContentType = "application/binary";
        await memory.WriteTo2(httpContext.Response, default);
    });


    app.MapGet($"/{size}/d", async httpContext =>
    {
        httpContext.Response.Headers.ContentType = "application/binary";
        await httpContext.Response.StartAsync();

        var stream = new ArrayPoolStream2(maxBlockSize: 4 * 1024 * 1024);
        stream.Reset(httpContext.Response.BodyWriter, 4096);
        for (int i = 0; i < size; ++i)
            stream.Write(megabyte);

        await stream.Finish(httpContext.Response);
        await httpContext.Response.CompleteAsync();
    });
}

// new ArrayPoolStream.BufferMemory(ArrayPool<byte>.Shared, )

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

static IEnumerable<OpenRiaServices.Client.Benchmarks.Server.Cities.City> CreateValidCities(int num)
{
    for (var i = 0; i < num; i++)
    {
        yield return new OpenRiaServices.Client.Benchmarks.Server.Cities.City { Name = "Name" + ToAlphaKey(i), CountyName = "Country", StateName = "SA" };
    }
}

static string ToAlphaKey(int num)
{
    var sb = new StringBuilder();
    do
    {
        var alpha = (char)('a' + (num % 25));
        sb.Append(alpha);
        num /= 25;
    } while (num > 0);

    return sb.ToString();
}