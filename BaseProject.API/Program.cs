using Microsoft.EntityFrameworkCore;
using BaseProject.DAO.Data;
using BaseProject.DAO.IRepository;
using BaseProject.DAO.IService;
using BaseProject.DAO.Models;
using BaseProject.DAO.Repository;
using BaseProject.DAO.Service;
using Serilog;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Hangfire;
using BaseProject.DAO.Hangfire;
using Serilog.Sinks.MSSqlServer;
using Hangfire.SqlServer;
using BaseProject.Util;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;

#region Service

#region Others

//PDF Tools
services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

//Azure Service Bus
//services.AddHostedService<ServiceBusReceiver>();
//services.AddScoped<IServiceBusSender, ServiceBusSender>();

//API OpenAI
services.AddScoped<IServiceOpenAI, ServiceOpenAI>();

//API Localidade
services.AddScoped<IServiceLocalidade, ServiceLocalidade>();

//Notification
services.AddScoped<IServiceEmail, ServiceEmail>();
services.AddScoped<IServiceTeamsNotification, ServiceTeamsNotification>();

//Autentication
services.AddScoped<IServiceToken, ServiceToken>();

#endregion

services.AddScoped<IServiceAspNetUser, ServiceAspNetUser>();
services.AddScoped<IServiceDownload, ServiceDownload>();
services.AddScoped<IServiceEmpresa, ServiceEmpresa>();
services.AddScoped<IServiceLogAcessoUsuario, ServiceLogAcessoUsuario>();
services.AddScoped<IServiceUpload, ServiceUpload>();
services.AddScoped<IServiceUsuario, ServiceUsuario>();

#endregion

#region Repository

services.AddScoped<IRepositoryAspNetUser, RepositoryAspNetUser>();
services.AddScoped<IRepositoryDownload, RepositoryDownload>();
services.AddScoped<IRepositoryEmpresa, RepositoryEmpresa>();
services.AddScoped<IRepositoryLogAcessoUsuario, RepositoryLogAcessoUsuario>();
services.AddScoped<IRepositoryUpload, RepositoryUpload>();
services.AddScoped<IRepositoryUsuario, RepositoryUsuario>();

#endregion

var config = builder.Configuration;
var env = builder.Environment;

var connectionString = config.GetConnectionString(env.EnvironmentName);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .MinimumLevel.Error()
    .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions
    {
        TableName = "Log"
    })
    .Enrich.FromLogContext()
    .CreateLogger()
);

services.AddMemoryCache();

services.AddLocalization();

services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString)
);

services.AddHangfire(x => x.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
{
    TryAutoDetectSchemaDependentOptions = false
}));
services.AddHangfireServer();

services.AddDatabaseDeveloperPageExceptionFilter();

services.AddDefaultIdentity<AspNetUser>(options =>
	options.SignIn.RequireConfirmedAccount = false
).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

services.AddDetection();

services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = false;
});

services.AddControllersWithViews()
    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

services.AddHttpContextAccessor();

services.AddRazorPages().AddRazorRuntimeCompilation()
.AddNewtonsoftJson(options =>
{
	options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
	options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
    options.SerializerSettings.DateFormatString = "dd/MM/yyyy";
}).AddJsonOptions(
	options => options.JsonSerializerOptions.PropertyNamingPolicy = null
);

services.Configure<FormOptions>(options =>
{
	options.ValueLengthLimit = int.MaxValue;
	options.MultipartBodyLengthLimit = long.MaxValue;
	options.MultipartBoundaryLengthLimit = int.MaxValue;
	options.MultipartHeadersCountLimit = int.MaxValue;
	options.MultipartHeadersLengthLimit = int.MaxValue;
});

services.Configure<DataProtectionTokenProviderOptions>(options =>
	options.TokenLifespan = TimeSpan.FromMinutes(10)
);

services.AddCors(options =>
{
	options.AddPolicy("CorsPolicy", builder =>
		builder.WithOrigins(config.GetProperty<string>("AppUrl", env.EnvironmentName))
		.SetIsOriginAllowed(origin => true)
		.AllowAnyMethod()
		.AllowAnyHeader()
		.AllowCredentials()
	);
});

services.AddSignalR();

services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.GetValue<string>("Secret"))),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
    };
    x.Events = new JwtBearerEvents

    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["access_token"];

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var response = context.HttpContext.Response;
            return Task.CompletedTask;
        }
    };
});

services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
    options.Events = new CookieAuthenticationEvents()
    {
        OnRedirectToLogin = redirectContext =>
        {
            return Task.CompletedTask;

        },
        OnRedirectToAccessDenied = redirectContext =>
        {
            return Task.CompletedTask;

        },
    };
});

services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("Bearer", "Cookies")
        .Build();
});

services.Configure<IdentityOptions>(options =>
{
    // Password settings  
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings  
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
    options.Lockout.MaxFailedAccessAttempts = 10;
    options.Lockout.AllowedForNewUsers = true;

    // User settings  
    options.User.RequireUniqueEmail = true;
});

services.ConfigureApplicationCookie(options =>
{
	options.Cookie.HttpOnly = false;
	options.Cookie.SameSite = SameSiteMode.None;
});

services.AddHttpClient();

services.AddMvc(opt =>
{
    opt.Filters.Add(typeof(GlobalModelStateValidatorAttribute));
});

var app = builder.Build();

// Cria ou atualiza o banco de dados na inicialização
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
    ForwardedHeaders.XForwardedProto
});

app.UseRequestLocalization(new RequestLocalizationOptions
{
    SupportedCultures = new List<CultureInfo> { new CultureInfo("pt-BR") },
    SupportedUICultures = new List<CultureInfo> { new CultureInfo("pt-BR") },
    DefaultRequestCulture = new RequestCulture("pt-BR")
});

app.UseHangfireDashboard();

var hangfireConfig = new HangfireConfiguration();

hangfireConfig.AddRecurringJobs();

if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
	app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
	MinimumSameSitePolicy = SameSiteMode.None,
	Secure = CookieSecurePolicy.Always,
	HttpOnly = HttpOnlyPolicy.None
});

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseDetection();

app.UseRouting();

app.UseSession();

app.UseCors("CorsPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();

    endpoints.MapControllerRoute(
        name: "areaRoute",
        pattern: "{area:exists}/{controller}/{action}/{id?}");
    
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");
});

app.MapRazorPages();

app.Run();
