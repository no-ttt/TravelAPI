using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiExcetionMiddleware;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Fluent;
using PuppeteerSharp;
using WebAPI.Lib;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:8888")
                          .AllowCredentials()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TravelNote", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp";
            });

            services.AddControllersWithViews(options =>
            {
                options.Filters.Add(typeof(LogInActionFilter),
                                    int.MinValue);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseCors("CorsPolicy");
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                 name: "default",
                 pattern: "{controller}/{id?}");
            });
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
            });
        }

        public class IgnoreLogInActionFilter : ActionFilterAttribute {}

        public class LogInActionFilter : IActionFilter
        {
            public void OnActionExecuting(ActionExecutingContext context)
            {
                // 執行動作前的處理
                var ignore = context.ActionDescriptor.FilterDescriptors.Select(f => f.Filter).OfType<IgnoreLogInActionFilter>().Any();
                if (ignore) return;

                string passportCode = context.HttpContext.Request.Cookies["passportCode"];

                if (passportCode == null) return;

                string strSql = @"
                    exec xps_RefreshToken @PassportCode output
                    select @PassportCode as PassportCode
                ";

                var p = new DynamicParameters();
                p.Add("@PassportCode", passportCode);

                using (var db = new AppDb())
                {
                    string RefreshToken = db.Connection.QueryFirstOrDefault<String>(strSql, p);
                    context.HttpContext.Response.Cookies.Append("passportCode", RefreshToken);
                }
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                // 執行動作後的處理
            }
        }
    }
}
