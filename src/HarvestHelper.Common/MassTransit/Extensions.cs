using System;
using System.Reflection;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HarvestHelper.Common.Settings;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using GreenPipes.Configurators;

namespace HarvestHelper.Common.MassTransit
{
    public static class Extensions
    {
        private const string RabbitMq = "RABBITMQ";
        private const string ServiceBus = "SERVICEBUS";

        public static IServiceCollection AddMassTransitWithMessageBroker(
            this IServiceCollection services,
            IConfiguration config,
            Action<IRetryConfigurator> configureRetries = null)
        {
            var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            switch (serviceSettings.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    services.AddMassTransitWithServiceBus(configureRetries);
                    break;
                case RabbitMq:
                default:
                    services.AddMassTransitWithRabbitMq(configureRetries);
                    break;                
            }

            return services;
        }

        public static IServiceCollection AddMassTransitWithRabbitMq(
            this IServiceCollection services,
            Action<IRetryConfigurator> configureRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingHarvestHelperRabbitMq(configureRetries);
            });

            services.AddMassTransitHostedService();

            return services;
        }

        public static IServiceCollection AddMassTransitWithServiceBus(
            this IServiceCollection services,
            Action<IRetryConfigurator> configureRetries = null)
        {
            services.AddMassTransit(configure =>
            {
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.UsingHarvestHelperAzureServiceBus(configureRetries);
            });

            services.AddMassTransitHostedService();

            return services;
        }        

        public static void UsingHarvestHelperMessageBroker(
            this IServiceCollectionBusConfigurator configure,
            IConfiguration config,
            Action<IRetryConfigurator> configureRetries = null)
        {
            var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

            switch (serviceSettings.MessageBroker?.ToUpper())
            {
                case ServiceBus:
                    configure.UsingHarvestHelperAzureServiceBus(configureRetries);
                    break;
                case RabbitMq:
                default:
                    configure.UsingHarvestHelperRabbitMq(configureRetries);
                    break;                
            }
        }

        public static void UsingHarvestHelperRabbitMq(
            this IServiceCollectionBusConfigurator configure,
            Action<IRetryConfigurator> configureRetries = null)
        {
            configure.UsingRabbitMq((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var rabbitMQSettings = configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(rabbitMQSettings.Host);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                configurator.UseMessageRetry(configureRetries);
            });
        }

        public static void UsingHarvestHelperAzureServiceBus(
            this IServiceCollectionBusConfigurator configure,
            Action<IRetryConfigurator> configureRetries = null)
        {
            configure.UsingAzureServiceBus((context, configurator) =>
            {
                var configuration = context.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var serviceBusSettings = configuration.GetSection(nameof(ServiceBusSettings)).Get<ServiceBusSettings>();
                configurator.Host(serviceBusSettings.ConnectionString);
                configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));

                if (configureRetries == null)
                {
                    configureRetries = (retryConfigurator) => retryConfigurator.Interval(3, TimeSpan.FromSeconds(5));
                }

                configurator.UseMessageRetry(configureRetries);
            });
        }        
    }
}