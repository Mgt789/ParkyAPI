using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ParkyAPI.Data;
using ParkyAPI.Repository;
using ParkyAPI.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ParkyAPI.ParkyMapper;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ParkyAPI
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
            //Add CORS
            services.AddCors();

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //nationalpark repository
            services.AddScoped<INationalParkRepository, NationalParkRepository>();
            //trail repository
            services.AddScoped<ITrailRepository, TrailRepository>();

            //user authentication addon
            services.AddScoped<IUserRepository, UserRepository>();

            //adding automapper
            services.AddAutoMapper(typeof(ParkyMappings));

            //version controller
            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });

            //version controller API call
            services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            //AppSetting class call
            var appSettingsSection = Configuration.GetSection("AppSettings");
            var appSettings=appSettingsSection.Get<AppSettings>();

            services.Configure<AppSettings>(appSettingsSection);
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            //JWTBearer Support add
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
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });



            //swagger connection
            //services.AddSwaggerGen(options =>
            //{
            //    //first Api
            //    options.SwaggerDoc("ParkyOpenAPISpec",
            //        new Microsoft.OpenApi.Models.OpenApiInfo()
            //        {
            //            Title = "Parky API",
            //            Version = "1",
            //            Description = "Udemy Parky API",
            //            Contact = new Microsoft.OpenApi.Models.OpenApiContact()
            //            {
            //                Email = "chandan.m@quadwave.com",
            //                Name = "Chandan Meher",
            //                Url = new Uri("https://mgt789.github.io/portfolio/")
            //            },
            //            License=new Microsoft.OpenApi.Models.OpenApiLicense()
            //            {
            //                Name="MIT License",
            //                Url=new Uri("https://en.wikipedia.org/wiki/MIT_License")
            //            }
            //        }); 

            //    //second Api
            //     //options.SwaggerDoc("ParkyOpenAPISpecTrails",
            //     //   new Microsoft.OpenApi.Models.OpenApiInfo()
            //     //   {
            //     //       Title = "Parky API Trails",
            //     //       Version = "1",
            //     //       Description = "Udemy Parky API Trails",
            //     //       Contact = new Microsoft.OpenApi.Models.OpenApiContact()
            //     //       {
            //     //           Email = "chandan.m@quadwave.com",
            //     //           Name = "Chandan Meher",
            //     //           Url = new Uri("https://mgt789.github.io/portfolio/")
            //     //       },
            //     //       License=new Microsoft.OpenApi.Models.OpenApiLicense()
            //     //       {
            //     //           Name="MIT License",
            //     //           Url=new Uri("https://en.wikipedia.org/wiki/MIT_License")
            //     //       }
            //     //   }); 

            //    //for xml comment we need this paragraph
            //    var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            //    var cmlCommentsFullPath=Path.Combine(AppContext.BaseDirectory,xmlCommentFile);
            //    options.IncludeXmlComments(cmlCommentsFullPath);
            //});
            
            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                    options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
                        desc.GroupName.ToUpperInvariant());

                options.RoutePrefix = "";
            });
            
            //app.UseSwaggerUI(options =>
            //{
            //    options.SwaggerEndpoint("/swagger/ParkyOpenAPISpec/swagger.json", "Parky API");
            //    //options.SwaggerEndpoint("/swagger/ParkyOpenAPISpecNP/swagger.json", "Parky API NP");
            //    //options.SwaggerEndpoint("/swagger/ParkyOpenAPISpecTrails/swagger.json", "Parky API Trails");
            //    options.RoutePrefix = "";
            //});

            app.UseRouting();

            app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
