using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Models;
using Umbraco.Forms.Core.Persistence.Dtos;
using Umbraco.Forms.Core.Services;
using Umbraco.Forms.Data.Storage;

namespace TwilioFlowToUmbracoForms
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="webHostEnvironment">The web hosting environment.</param>
        /// <param name="config">The configuration.</param>
        /// <remarks>
        /// Only a few services are possible to be injected here https://github.com/dotnet/aspnetcore/issues/9337.
        /// </remarks>
        public Startup(IWebHostEnvironment webHostEnvironment, IConfiguration config)
        {
            _env = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <remarks>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940.
        /// </remarks>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddUmbraco(_env, _config)
                .AddBackOffice()
                .AddWebsite()
                .AddComposers()
                .Build();
        }

        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseUmbraco()
                .WithMiddleware(u =>
                {
                    u.UseBackOffice();
                    u.UseWebsite();
                })
                .WithEndpoints(u =>
                {
                    u.UseInstallerEndpoints();
                    u.UseBackOfficeEndpoints();
                    u.UseWebsiteEndpoints();
                    var builder = u.EndpointRouteBuilder;
                    builder.MapPost("/test", (IFormService formService, IRecordService recordService) =>
                    {
                        var form = formService.Get("VeryImportantSurvey");
                        var fieldDic = RecordFields(form);
                        var record = new Record()
                        {
                            Created = DateTime.Now,
                            Form = form.Id,
                            RecordFields = fieldDic,
                            State = FormState.Submitted
                        };
                        //record.RecordData = record.GenerateRecordDataAsJson();
                        recordService.Submit(record, form);
                    });
                });
        }
        
        private static Dictionary<Guid, RecordField> RecordFields(Form form)
        {
            var fieldDic = new Dictionary<Guid, RecordField>();
            var pineappleOnPizzaField = form.AllFields.First(f => f.Alias == "pineappleOnPizza");
            var pineappleOnPizzaRecordField = new RecordField(pineappleOnPizzaField)
            {
                Values = new List<object> { "10" }
            };

            fieldDic.Add(pineappleOnPizzaField.Id, pineappleOnPizzaRecordField);
            return fieldDic;
        }
    }
}
