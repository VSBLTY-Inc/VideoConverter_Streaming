using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using VideoConverter_Streaming.Services;

namespace VideoConverter_Streaming
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddHostedService<ConvertVideosWorker>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            var configuration = app.Services.GetRequiredService<IConfiguration>();
            var convertedFiles = configuration["ConvertedVideoFolder"];
            var fileProvider = new PhysicalFileProvider(convertedFiles);
            app.UseStaticFiles( new StaticFileOptions
                {
                    FileProvider = fileProvider,
                    RequestPath = "/convertedFiles"
                }
                );

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}