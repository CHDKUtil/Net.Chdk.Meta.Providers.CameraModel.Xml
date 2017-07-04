using Microsoft.Extensions.DependencyInjection;

namespace Net.Chdk.Meta.Providers.CameraModel.Xml
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddXmlPlatformProvider(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton<IPlatformProvider, XmlPlatformProvider>();
        }
    }
}
