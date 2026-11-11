using Accounting.DBUtilities;
using Accounting.DBUtilities.Implementations.Transactions;
using Accounting.DBUtilities.Interfaces.Transactions;
using Accounting.Utilities.Internal;
using Accounting.Utilities.Internal.Implementations;
using Accounting.Utilities.Internal.Interfaces;
using Accounting.Web.Abstract;
using Accounting.Web.Models;
using Accounting.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Serialization;

namespace Accounting.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            WebAppReferenceUtility._configuration = configuration;
            SQLHelper.ConnectionString = configuration.GetConnectionString("AccountingDBConnectionString");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromDays(1);
            });
            services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
            services.AddHttpContextAccessor();
            services.AddControllersWithViews();
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddSingleton<IPDFOptions, ITextSharpUtility>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<ViewRender, ViewRender>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserContextRepository, UserContextRepository>();
            services.AddScoped<IMakePaymentsRepository,MakePaymentsRepository>();
            services.AddTransient<IReportsRepository, ReportsRepository>();
            services.AddTransient<ILU_SQLQueriesRepository, LU_SQLQueriesRepository>();
            services.AddTransient<ILU_SQLInputParametersRepository, LU_SQLInputParametersRepository>();
            services.AddTransient<IReportFileDownloadSettingsRepository, ReportFileDownloadSettingsRepository>();
            services.AddSingleton<IMSOfficeFileOptions, ClosedXMLUtility>();
            services.AddSingleton<ICSVOptions, CSVUtility>();
            services.AddSingleton<IZip, ZipUtility>();
            services.AddScoped<ClientInfoHelper>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPolicyDetailsRepository, PolicyDetailsRepository>();
            services.AddScoped<IClientsRepository, ClientsRepository>();
            services.AddScoped<ICoverPayService, CoverPayService>();
            services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMemoryCache memoryCache)
        {
            Configuration["FileLogger:LogsFolder"] = Path.Combine(env.ContentRootPath, "App_Data", "Logs");
            Configuration["FileLogger:RetentionDays"] = Configuration["LogFileRetentionDays"];
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{PolicyDetails_Id?}");
            });
            WebAppReferenceUtility._memoryCache = memoryCache;
        }
    }
}
