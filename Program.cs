using Empath_AI.Data;
using Empath_AI.Hubs;
using Empath_AI.Repository;
using Empath_AI.Service;
using Empath_AI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net.Http.Headers;
using System.Text;                                                       


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
/*builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ConnectionString"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,              // Try up to 5 times
                maxRetryDelay: TimeSpan.FromSeconds(10),  // Wait up to 10 sec between retries
                errorNumbersToAdd: null        // You can specify SQL error codes if needed
            );
        }));*/


builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables();
var geminiSection = builder.Configuration.GetSection("Gemini");
var baseUrl = geminiSection.GetValue<string>("BaseUrl");
var apiKey = geminiSection.GetValue<string>("ApiKey");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
/*builder.Services.AddAuthentication("Cookies").AddCookie("Cookies", options =>
{
    options.LoginPath = "/Account/login";
    options.AccessDeniedPath = "/";
});*/

var config = builder.Configuration;

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IHeartRateRepository, HeartRateRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMedicalReportRepository, MedicalReportRepository>();
builder.Services.AddScoped<IAccelerometerRepository, AccelerometerRepository>();
builder.Services.AddScoped<IGSRRepository, GSRRepository>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>(client =>
{
    // We'll still pass API key as query param, but set Accept header
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddHttpClient<SocialAuthService>();
builder.Services.AddScoped<Email>();
builder.Services.AddScoped<Token>();
//builder.Services.AddScoped<Bot>();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB
})
.AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddAuthentication().AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = config["JWT:Issure"],
        ValidateAudience = true,
        ValidAudience = config["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"])),
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // SignalR sends token via query string ?access_token=...
            var accessToken = context.Request.Query["access_token"];

            // Check if the request is for our ChatHub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

//////////////////////////////////////////////////////////////////////////////
//builder.Services.AddHostedService<DeleteOldConversationsService>();
//////////////////////////////////////////////////////////////////////////////

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy
            .WithOrigins("http://localhost:5280", "http://127.0.0.1:5280", "null") // allow your frontend origins
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.Configure<Empath_AI.Services.GeminiOptions>(builder.Configuration.GetSection("Gemini"));

var app = builder.Build();

app.UseCors("AllowLocalhost");


// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
*/
app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/Chat");

app.Run();
