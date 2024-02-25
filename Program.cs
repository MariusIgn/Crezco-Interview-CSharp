using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

HttpClient sharedClient = new()
{
    BaseAddress = new Uri("http://ip-api.com/json/"),
};


var IpDataCache = new List<IpData>();
var IpV4Regex = "^([0-9]{1,3})\\.([0-9]{1,3})\\.([0-9]{1,3})\\.([0-9]{1,3})$";
var IpV6Regex = "(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))";

// Use controllers
// builder.Services.AddControllers();
//app.MapControllers();

// Use minimal APIs
app.MapGet("/minimal-api{ip}", (string ip) =>
{
    ip = ip.Trim();
    if (!Regex.Match(ip, IpV4Regex).Success && !Regex.Match(ip, IpV6Regex).Success)
    {
        return Results.BadRequest("Given IP address is not formatted correctly.");
    }
    var responce = sharedClient.GetAsync(ip).Result;

    var data = System.Text.Json.JsonSerializer.Deserialize<IpData>(responce.Content.ReadAsStream());
    IpDataCache.Add(data);
    return Results.Ok(data);

});

app.MapGet("/report", () =>
{
    var Data = new Dictionary<string, int>();


    var result = IpDataCache.GroupBy(ip => ip.country).Select(g => (g.Key, g.Count()));


    return Results.Ok(result);

});


app.Run();


public class IpData
{
    public string query { get; set; }
    public string status { get; set; }
    public string country { get; set; }
    public string countryCode { get; set; }
    public string region { get; set; }
    public string regionName { get; set; }
    public string city { get; set; }
    public string zip { get; set; }
    public float lat { get; set; }
    public float lon { get; set; }
    public string timezone { get; set; }
    public string isp { get; set; }
    public string org { get; set; }
    public string _as { get; set; }
}
