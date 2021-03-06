using System;
using System.Threading;
using CacheManager.Core;
using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Issue15
{
    public static class ConfigureServices
    {
        private static readonly Lazy<IServiceProvider> _serviceProviderBuilder =
            new Lazy<IServiceProvider>(getServiceProvider, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// A lazy loaded thread-safe singleton
        /// </summary>
        public static IServiceProvider Instance { get; } = _serviceProviderBuilder.Value;

        public static IEFCacheServiceProvider GetEFCacheServiceProvider()
        {
            return Instance.GetService<IEFCacheServiceProvider>();
        }

        private static IServiceProvider getServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddEFSecondLevelCache();

            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                        .WithJsonSerializer()
                        .WithMicrosoftMemoryCacheHandle()
                        .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
                        .DisablePerformanceCounters()
                        .DisableStatistics()
                        .Build());

            var serviceProvider = services.BuildServiceProvider();
            EFServiceProvider.ApplicationServices = serviceProvider; // app.UseEFSecondLevelCache();

            return serviceProvider;
        }
    }
}
