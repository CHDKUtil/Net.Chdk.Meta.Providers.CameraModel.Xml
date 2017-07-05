using Microsoft.Extensions.DependencyInjection;
using Net.Chdk.Meta.Providers.CameraModel;

namespace Net.Chdk.Meta.Providers.Platform.Xml
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
