using FileAccessApp.Components;
using FileAccessApp.Handlers;
using FileAccessApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Swagger.Bootstrap;
using System.Text;

namespace FileAccessApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddOpenApi();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerBootstrap(options =>
            {
                options.UseExperimentalFeatures = true;
            });


            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = builder.Configuration["JWT:Issuer"],
                        ValidAudience = builder.Configuration["JWT:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)
                        )
                    };
                });

            builder.Services.AddAuthorization();


            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddScoped<FileService>();
            builder.Services.AddScoped<LoginHandler>();
            builder.Services.AddScoped<AuthService>();

            //var port = builder.Configuration["HTTP_PORT"] ?? "8080";

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri($"http://localhost:8080")
            });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.MapOpenApi();
            app.UseSwaggerBootstrap(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "OpenApiDocument v1");
            });

            //app.Urls.Add($"http://0.0.0.0:{port}");
            //app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapControllers();

            app.Run();
        }
    }
}
