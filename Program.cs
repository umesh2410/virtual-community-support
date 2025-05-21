using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Threading.Tasks;
using VirtualCommunitySupport.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Environment.EnvironmentName = "Development"; // Force Development environment

// Add services to the container
builder.Services.AddControllers();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add Custom Services
builder.Services.AddSingleton<DataService>();
builder.Services.AddScoped<InitiativeService>();
builder.Services.AddScoped<OpportunityService>();
builder.Services.AddScoped<StoryService>();
builder.Services.AddScoped<ContactService>();
builder.Services.AddScoped<AuthService>();

// For now, we'll focus on the API only and handle the Angular app separately
// builder.Services.AddSpaStaticFiles(configuration =>
// {
//     configuration.RootPath = "ClientApp/dist";
// });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enable static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// We are temporarily disabling SPA integration to focus on API functionality
// app.UseSpa(spa =>
// {
//     spa.Options.SourcePath = "ClientApp";
// 
//     if (app.Environment.IsDevelopment())
//     {
//         // This will start the Angular development server when the ASP.NET app starts
//         spa.UseAngularCliServer(npmScript: "start");
//         
//         // Alternatively, if you want to use your own Angular CLI server
//         // spa.UseProxyToSpaDevelopmentServer("http://localhost:5000");
//     }
// });

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dataService = services.GetRequiredService<DataService>();
    await Task.Run(() => dataService.SeedInitialData());
}

// Run the application
app.Run();
