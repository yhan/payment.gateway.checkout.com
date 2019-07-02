using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcquiringBanks.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.API.ReadProjector;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using SimpleCQRS;
using Swashbuckle.AspNetCore.Swagger;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace PaymentGateway.API
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
            services.AddOptions();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    //options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
            });


            services.AddScoped<IGenerateGuid, DefaultGuidGenerator>();
            services.AddScoped<IEventSourcedRepository<Payment>, EventSourcedRepository<Payment>>();
            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddSingleton<IPublishEvents, InMemoryBus>();
            services.AddSingleton<IKnowAllPaymentRequests, InMemoryPaymentRequests>();
            services.AddScoped<IProcessPayment, PaymentProcessor>(); 
            services.AddScoped<ITalkToAcquiringBank, AcquiringBankFacade>();
            services.AddScoped<IAmAcquiringBank, AcquiringBankSimulator>();
            services.AddScoped<IConnectToAcquiringBanks, RandomConnectionBehavior>();
            
            services.AddTransient<IGenerateBankPaymentId, DefaultBankPaymentIdGenerator>();
            services.AddTransient<IProvideRandomBankResponseTime, NoDelayProvider>();

            services.AddSingleton<IRandomnizeAcquiringBankPaymentStatus, AcquiringBankPaymentStatusRandomnizer>();

            services.AddSingleton<IHostedService, ReadProjections>();
            services.AddSingleton<IPaymentDetailsRepository, PaymentDetailsRepository>();
            
            services.AddSingleton<IMapAcquiringBankToPaymentGateway, PaymentIdsMemory>();
            services.AddSingleton<IKnowAllPaymentsIds>(provider => provider.GetService<IMapAcquiringBankToPaymentGateway>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });


            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }


    internal class NoDelayProvider : IProvideRandomBankResponseTime
    {
        public TimeSpan Delays()
        {
            return TimeSpan.Zero;
        }
    }

}
