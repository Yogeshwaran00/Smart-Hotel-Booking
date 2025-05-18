using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartHotelBooking.DbContext;
using SmartHotelBooking.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Configure Identity services
builder.Services.AddIdentity<AccountUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
}).AddEntityFrameworkStores<ApplicationDbContext>()
  .AddDefaultTokenProviders();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});


builder.Services.AddControllersWithViews();
//builder.Services.Configure<StripeSettings>(
//    builder.Configuration.GetSection("Stripe"));
//StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
}).AddCookie();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/User/Login";
    options.LogoutPath = "/User/Logout";
    options.AccessDeniedPath = "/User/AccessDenied";
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeAnAdmin", policy => policy.RequireClaim("Role", "Admin"));
    options.AddPolicy("MustBeAnMember", policy => policy.RequireClaim("Role", "Member"));
    options.AddPolicy("MustBeAnHotelManager", policy => policy.RequireClaim("Role", "Manager"));
    options.AddPolicy("AdminOrMember", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Role", "Admin") || context.User.HasClaim("Role", "Member")));
    options.AddPolicy("AdminOrManager", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Role", "Admin") || context.User.HasClaim("Role", "Manager")));
    options.AddPolicy("ManagerOrMember", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Role", "Manager") || context.User.HasClaim("Role", "Member")));
    options.AddPolicy("ManagerOrMemberOrAdmin", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("Role", "Manager") || context.User.HasClaim("Role", "Member") || context.User.HasClaim("Role", "Admin")));
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseRouting();
app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RoleBasedRedirectionMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}"
);

app.Run();
