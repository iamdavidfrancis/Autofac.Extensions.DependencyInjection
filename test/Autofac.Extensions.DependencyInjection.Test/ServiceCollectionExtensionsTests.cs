using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public sealed class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAutofacReturnsProvidedServiceCollection()
        {
            var collection = new ServiceCollection();

            var returnedCollection = collection.AddAutofac();

            Assert.Same(collection, returnedCollection);
        }

        [Fact]
        public void AddAutofacAddsAutofacServiceProviderFactoryToCollection()
        {
            var collection = new ServiceCollection();

            collection.AddAutofac();

            var service = collection.FirstOrDefault(s => s.ServiceType == typeof(IServiceProviderFactory<ContainerBuilder>));
            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
        }

        [Fact]
        public void AddAutofacPassesConfigurationActionToAutofacServiceProviderFactory()
        {
            var collection = new ServiceCollection();

            collection.AddAutofac(config => config.Register(c => "Foo"));

            var serviceProvider = collection.BuildServiceProvider();
            var factory = (IServiceProviderFactory<ContainerBuilder>)serviceProvider.GetService(typeof(IServiceProviderFactory<ContainerBuilder>));
            var builder = factory.CreateBuilder(collection);
            Assert.Equal("Foo", builder.Build().Resolve<string>());
        }

        [Fact]
        public void AddAutofacPassesConfigurationActionWithInstancePerMatchingLifetimeScopeAndConfigureLifetimeScopeToAutofacServiceProviderFactory()
        {
            var rootLifetimeScopeName = "TestScope";

            var collection = new ServiceCollection();

            collection.AddAutofac(config => config.Register(c => new List<string> { "Foo" }).InstancePerMatchingLifetimeScope(rootLifetimeScopeName), container => container.BeginLifetimeScope(rootLifetimeScopeName));

            var serviceProvider = collection.BuildServiceProvider();
            var factory = (IServiceProviderFactory<ContainerBuilder>)serviceProvider.GetService(typeof(IServiceProviderFactory<ContainerBuilder>));
            var builder = factory.CreateBuilder(collection);
            var applicationServiceProvider = factory.CreateServiceProvider(builder);

            var result = applicationServiceProvider.GetService<List<string>>();

            Assert.NotNull(result);
        }

        [Fact]
        public void AddAutofacPassesConfigurationActionWithInstancePerMatchingLifetimeScopeToAutofacServiceProviderFactory()
        {
            var rootLifetimeScopeName = "TestScope";

            var collection = new ServiceCollection();

            collection.AddAutofac(config => config.Register(c => new List<string> { "Foo" }).InstancePerMatchingLifetimeScope(rootLifetimeScopeName));

            var serviceProvider = collection.BuildServiceProvider();
            var factory = (IServiceProviderFactory<ContainerBuilder>)serviceProvider.GetService(typeof(IServiceProviderFactory<ContainerBuilder>));
            var builder = factory.CreateBuilder(collection);
            var applicationServiceProvider = factory.CreateServiceProvider(builder);

            Assert.Throws<Core.DependencyResolutionException>(() => applicationServiceProvider.GetService<List<string>>());
        }
    }
}