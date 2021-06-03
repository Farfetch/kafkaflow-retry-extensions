using KafkaFlow.Retry.MongoDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KafkaFlow.Retry.API.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseKafkaFlowRetryEndpoints();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(sp =>
                new MongoDbDataProviderFactory()
                    .TryCreate(
                        new MongoDbSettings
                        {
                            ConnectionString = "mongodb://localhost:27017/SVC_KAFKA_FLOW_RETRY_DURABLE",
                            DatabaseName = "SVC_KAFKA_FLOW_RETRY_DURABLE",
                            RetryQueueCollectionName = "RetryQueues",
                            RetryQueueItemCollectionName = "RetryQueueItems"
                        }
                    ).Result
                );

            services.AddControllers();
        }
    }
}