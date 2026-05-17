using FileHubOrg.Application.Configurations;
using FileHubOrg.Domain.Entities.User;
using FileHubOrg.Domain.Interfaces;
using FileHubOrg.Infrastructure.Data;
using FileHubOrg.Infrastructure.Persistence.Seeders;
using FileHubOrg.Infrastructure.Repositories;
using FileHubOrg.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Register services
builder.Services.AddDbContext<FileHubOrgDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("FileHubOrg.Infrastructure"))
    );




// Identity services
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<FileHubOrgDbContext>()
.AddDefaultTokenProviders();


// Cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<LdapSettings>(builder.Configuration.GetSection("Ldap"));
builder.Services.AddScoped<ILdapAuthenticationService, LdapAuthenticationService>();

// HttpContextAccessor for accessing HTTP context in services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

// Web host environment for file uploads
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);


// ================================
// Register Repositories (Infrastructure Layer)
// ================================
// Automatically scan Infrastructure assembly for classes ending with "Repository"
// and register them as their implemented interfaces with Scoped lifetime.
builder.Services.Scan(scan => scan
    .FromAssemblies(typeof(FileHubOrgDbContext).Assembly)
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);


// ================================
// Register Services (Application Layer)
// ================================
// Automatically scan Application assembly for classes ending with "Service"
// and register them as their implemented interfaces with Scoped lifetime.
builder.Services.Scan(scan => scan
    .FromAssemblies(typeof(FileHubOrg.Application.AssemblyMarker).Assembly)
    .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);

// ================================
// Register Unit of Work
// ================================
// Register UnitOfWork implementation for IUnitOfWork interface.
// This ensures all services can resolve IUnitOfWork via DI.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();



// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    // Apply EF Core migrations automatically
    var db = scope.ServiceProvider.GetRequiredService<FileHubOrgDbContext>(); db.Database.Migrate();

    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<FileHubOrgDbContext>();

    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    
    DepartmentSeeder.SeedAsync(context).GetAwaiter().GetResult();
    RoleSeeder.SeedAsync(roleManager).GetAwaiter().GetResult();

    await UserSeeder.SeedAsync(userManager, roleManager, context);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
