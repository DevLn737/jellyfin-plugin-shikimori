using System.Xml.Serialization;
using MediaBrowser.Common.Plugins;
using Jellyfin.Plugin.Shikimori.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Shikimori
{
    public class ShikimoriPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public IHttpClientFactory HttpClientFactory { get; init; }
        public ShikimoriPlugin(
                IApplicationPaths applicationPaths,
                IXmlSerializer xmlSerialize,
                IHttpClientFactory httpClientFactory)
            : base(applicationPaths, xmlSerialize)
        {
            Instance = this;
            HttpClientFactory = httpClientFactory;
        }

        public const string ProviderName = "Shikimori";
        public const string ProviderId = "Shikimori";
        public const string ShikimoriBaseUrlDefault = "https://shikimori.io";
        public string ShikimoriBaseUrl
        {
            get
            {
                if (String.IsNullOrEmpty(Configuration.ShikimoriBaseUrl))
                    return ShikimoriBaseUrlDefault;
                return Configuration.ShikimoriBaseUrl;
            }
        }

        public override string Name => "Shikimori Next";
        public override Guid Id => Guid.Parse("42730b4b-b30e-45dc-bee0-8f351eadb690");
        
        public static ShikimoriPlugin? Instance { get; private set; }
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = $"{GetType().Namespace}.Configuration.config.html"
                }
            };
        }
    }
}
