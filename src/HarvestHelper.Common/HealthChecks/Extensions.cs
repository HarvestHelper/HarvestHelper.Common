using HarvestHelper.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace HarvestHelper.Common.HealthChecks
{
    public static class Extensions
    {
        private const string MongoCheckName = "mongodb";
        private const string ReadyTagName = "ready";
        private const string LiveTagName = "live";
        private const string HealthEndpoint = "health";
        private const int DefaultSeconds = 3;


        public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, TimeSpan? timeout = default)
        {
            return builder.Add(new HealthCheckRegistration(
                        MongoCheckName,
                        serviceProvider =>
                        {
                            var configuration = serviceProvider.GetService<IConfiguration>();
                            var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();

                            var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);
                            return new MongoDbHealthCheck(mongoClient);
                        },
                        HealthStatus.Unhealthy,
                        new[] { ReadyTagName },
                        TimeSpan.FromSeconds(DefaultSeconds)
                    ));
        }

        public static void MapHarvestHelperHealthChecks(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapHealthChecks($"/{HealthEndpoint}/{ReadyTagName}", new HealthCheckOptions(){
                Predicate = (check) => check.Tags.Contains(ReadyTagName)
            });
            endpoints.MapHealthChecks($"/{HealthEndpoint}/{LiveTagName}", new HealthCheckOptions(){
                Predicate = (check) => false
            });
        }
    }
}