using System;
using AcquiringBanks.Stub;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PaymentGateway.Domain;
using PaymentGateway.Infrastructure;
using PaymentGateway.ReadProjector;
using Swashbuckle.AspNetCore.Swagger;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace PaymentGateway
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
            
            services.AddScoped<IKnowSendRequestToBankSynchrony>(svcProvider => new RequestBankSynchronyMaster(svcProvider.GetService<IOptionsMonitor<AppSettings>>()));
            services.AddScoped<ICommandHandler<RequestPaymentCommand>, PaymentRequestCommandHandler>();

            //Event sourcing
            services.AddScoped<IEventSourcedRepository<Payment>, EventSourcedRepository<Payment>>();
            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddSingleton<IPublishEvents, InMemoryBus>();

            // Some memories
            services.AddSingleton<IKnowAllPaymentRequests, PaymentRequestsMemory>();
            services.AddSingleton<IKnowAllPaymentsIds>(provider => provider.GetService<IMapAcquiringBankToPaymentGateway>());
            services.AddSingleton<IMapAcquiringBankToPaymentGateway, PaymentIdsMemory>();

            // Mediator between Gateway and Acquiring banks
            services.AddScoped<IProcessPayment, PaymentProcessor>();

            // Acquiring bank related
            services.AddScoped<ISelectAdapter, BankAdapterSelector>();
            services.AddScoped<IMapMerchantToBankAdapter, MerchantToBankAdapterMapper>();
            services.AddSingleton<IKnowAllMerchants, MerchantsRepository>();
            services.AddScoped<IConnectToAcquiringBanks, RandomConnectionBehavior>();
            services.AddTransient<IGenerateBankPaymentId, DefaultBankPaymentIdGenerator>();
            services.AddTransient<IProvideBankResponseTime, RandomDelayProvider>();
            services.AddSingleton<IGenerateAcquiringBankPaymentStatus, AcquiringBankPaymentStatusRandomnizer>();
            services.AddTransient<IProvideTimeout, DefaultTimeoutProviderWaitingBankResponse>();

            // Host Read Projectors
            services.AddSingleton<IHostedService, ReadProjections>();

            // Read model
            services.AddSingleton<IPaymentDetailsRepository, PaymentDetailsRepository>();
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment Gateway API");
                c.RoutePrefix = string.Empty;
            });


            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }

    public class DefaultTimeoutProviderWaitingBankResponse : IProvideTimeout
    {
        public TimeSpan GetTimeout()
        {
            return TimeSpan.FromSeconds(2);
        }
    }

    public class RequestBankSynchronyMaster : IKnowSendRequestToBankSynchrony
    {
        private readonly IOptionsMonitor<AppSettings> _optionsMonitor;

        public RequestBankSynchronyMaster(IOptionsMonitor<AppSettings> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public bool SendPaymentRequestAsynchronously()
        {
            return _optionsMonitor.CurrentValue.Executor == ExecutorType.API;
        }
    }

    ///<inheritdoc cref="IProvideBankResponseTime"/>
    /// <summary>
    /// Can be used for performance testing: test Gateway internal latency.
    /// </summary>
    internal class NoDelayProvider : IProvideBankResponseTime
    {
        public TimeSpan Delays()
        {
            return TimeSpan.Zero;
        }
    }

}
