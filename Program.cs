using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RemediEmr.Class;
using RemediEmr.Repositry;
using Newtonsoft.Json.Serialization;
using System;
using System.Text;
using RemediEmr.Data.Class;
using System.Net.WebSockets;
using System.Text.Json;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });

builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDbContext")));

var jwtSecretKey = "PatientInfo@Tedsys1234545tedsyspatient"; // Replace with your actual secret key
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });


// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<CommonRepositry>();
builder.Services.AddScoped<PatientRepositry>();
builder.Services.AddScoped<OPNurseRepositry>();
builder.Services.AddScoped<DietCommonRepository>();
builder.Services.AddScoped<PhysioRepository>();
builder.Services.AddScoped<PrescriptionRepositry>();
builder.Services.AddScoped<JwtService.JwtHandler>();
builder.Services.AddScoped<ComplaintRepository>();
builder.Services.AddScoped<SelfRegistrationRepositry>();
builder.Services.AddScoped<FeedbackRepositry>();
builder.Services.AddScoped<TvTokenRepositry>();
builder.Services.AddScoped<WebSocketHandler>();
builder.Services.AddScoped<WebSocketHandlerDoctorToken>(); // Use Scoped or Transient here
builder.Services.AddHostedService<TokenUpdateService>();
builder.Services.AddHostedService<DoctorTvTokenUpdateService>();
builder.Services.AddScoped<DoctorTvTokenRepositry>();



var app = builder.Build();
app.UseWebSockets();

//app.UseWebSockets();

app.Map("/Radiology", async (HttpContext context, WebSocketHandler socketHandler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid WebSocket request.");
        return;
    }

    string deviceId = context.Request.Query["deviceId"].ToString();
    if (string.IsNullOrEmpty(deviceId))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("DeviceId is required.");
        return;
    }

    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
    await socketHandler.HandleWebSocketAsync(webSocket, deviceId);
});

app.Map("/DoctorToken", async (HttpContext context, WebSocketHandlerDoctorToken socketHandler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid WebSocket request.");
        return;
    }

    string deviceId = context.Request.Query["deviceId"].ToString();
    if (string.IsNullOrEmpty(deviceId))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("DeviceId is required.");
        return;
    }

    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
    await socketHandler.HandleWebSocketAsync(webSocket, deviceId);
});

//Map WebSocket Handler
//app.Map("/", async (HttpContext context, WebSocketHandler socketHandler) =>
//{
//    if (context.WebSockets.IsWebSocketRequest)
//    {
//        // Extract the deviceId from the query string
//        var deviceId = context.Request.Query["deviceId"].ToString();

//        if (string.IsNullOrEmpty(deviceId))
//        {
//            context.Response.StatusCode = 400;  // Bad request if deviceId is not present
//            await context.Response.WriteAsync("DeviceId is missing.");
//            return;
//        }

//        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

//        // Pass deviceId to the WebSocket handler
//        await socketHandler.HandleWebSocketAsync(webSocket, deviceId);
//    }
//    else
//    {
//        context.Response.StatusCode = 400;
//    }
//});

//app.Use(async (context, next) =>
//{
//    if (context.Request.Path == "/") // The path for your WebSocket connection
//    {
//        if (context.WebSockets.IsWebSocketRequest)
//        {
//            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

//            // Resolve WebSocketHandler from DI container
//            using var scope = app.Services.CreateScope();
//            var socketHandler = scope.ServiceProvider.GetRequiredService<WebSocketHandler>();

//            // Use the socket handler to manage the WebSocket connection
//            await socketHandler.HandleWebSocketAsync(webSocket);
//        }
//        else
//        {
//            context.Response.StatusCode = StatusCodes.Status400BadRequest;
//        }
//    }
//    else
//    {
//        await next();
//    }
//});

async Task HandleWebSocketConnection(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

    while (!result.CloseStatus.HasValue)
    {
        string requestMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(requestMessage);
        string deviceId = requestData["deviceId"];

        // Fetch data from the database
        var tokens = await GetTokenDetails(deviceId);

        // Send data back to the client
        var responseMessage = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tokens));
        await webSocket.SendAsync(new ArraySegment<byte>(responseMessage, 0, responseMessage.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

        // Wait for the next message
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
}
async Task<List<dynamic>> GetTokenDetails(string deviceId)
{
    using var scope = app.Services.CreateScope();
    var tvTokenRepo = scope.ServiceProvider.GetRequiredService<TvTokenRepositry>();

    return await tvTokenRepo.GetTokenDetails(deviceId);
}



// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
