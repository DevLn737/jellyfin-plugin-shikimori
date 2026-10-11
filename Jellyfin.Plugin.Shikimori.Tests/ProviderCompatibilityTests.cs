using System.Net;
using System.Text.Json;
using System.Xml.Serialization;
using Jellyfin.Plugin.Shikimori.Configuration;
using Jellyfin.Plugin.Shikimori.Providers;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jellyfin.Plugin.Shikimori.Tests
{
    public class ProviderCompatibilityTests
    {
        [Fact]
        public void PluginIdentityAndConfigurationLocation_RemainCompatible()
        {
            using var fixture = new PluginFixture();

            Assert.Equal(Guid.Parse("7edb2a28-5b8a-4fe8-ae11-d941315eb862"), fixture.Plugin.Id);
            Assert.Equal(new Version(6, 0, 0, 0), fixture.Plugin.Version);
            Assert.Equal("Shikimori", fixture.Plugin.Name);
            Assert.Equal("Shikimori", ShikimoriPlugin.ProviderId);
            Assert.Equal("Jellyfin.Plugin.Shikimori.xml", fixture.Plugin.ConfigurationFileName);

            var page = Assert.Single(fixture.Plugin.GetPages());
            using var embeddedPage = typeof(ShikimoriPlugin).Assembly.GetManifestResourceStream(page.EmbeddedResourcePath);
            Assert.NotNull(embeddedPage);
        }

        [Fact]
        public void ReleaseMetadata_MatchesRuntimeIdentityAndVersion()
        {
            using var fixture = new PluginFixture();
            using var metadata = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "meta.json")));
            var manifest = metadata.RootElement;

            Assert.Equal(fixture.Plugin.Id, Guid.Parse(manifest.GetProperty("guid").GetString()!));
            Assert.Equal(fixture.Plugin.Version, Version.Parse(manifest.GetProperty("version").GetString()!));
            Assert.Equal(fixture.Plugin.Name, manifest.GetProperty("name").GetString());
            Assert.Equal("12.2.0.0", manifest.GetProperty("targetAbi").GetString());
            Assert.Equal("Active", manifest.GetProperty("status").GetString());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task MetadataById_PreservesExternalIdsAndConfiguredLanguages(bool movie)
        {
            using var fixture = new PluginFixture();
            fixture.Plugin.UpdateConfiguration(new PluginConfiguration
            {
                TitlePreference = TitlePreferenceType.Japanese,
                OriginalTitlePreference = TitlePreferenceType.Romaji,
                GenreTitleLanguagePreference = GenreTitleLanguagePreferenceType.English
            });
            var ids = ExistingIds(includeShikimori: true);

            if (movie)
            {
                var result = await fixture.MovieProvider.GetMetadata(new MovieInfo { ProviderIds = ids }, CancellationToken.None);
                Assert.True(result.HasMetadata);
                Assert.Equal("日本語", result.Item.Name);
                Assert.Equal("Original title", result.Item.OriginalTitle);
                Assert.Equal(new[] { "Drama" }, result.Item.Genres);
                AssertPreservedIds(result.Item.ProviderIds, ids);
            }
            else
            {
                var result = await fixture.SeriesProvider.GetMetadata(new SeriesInfo { ProviderIds = ids }, CancellationToken.None);
                Assert.True(result.HasMetadata);
                Assert.Equal("日本語", result.Item.Name);
                Assert.Equal("Original title", result.Item.OriginalTitle);
                Assert.Equal(new[] { "Drama" }, result.Item.Genres);
                AssertPreservedIds(result.Item.ProviderIds, ids);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task SearchByIdOrName_PreservesExternalIds(bool movie, bool byId)
        {
            using var fixture = new PluginFixture();
            var ids = ExistingIds(includeShikimori: byId);

            var results = movie
                ? await fixture.MovieProvider.GetSearchResults(new MovieInfo { Name = byId ? null : "Title", ProviderIds = ids }, CancellationToken.None)
                : await fixture.SeriesProvider.GetSearchResults(new SeriesInfo { Name = byId ? null : "Title", ProviderIds = ids }, CancellationToken.None);

            var result = Assert.Single(results);
            Assert.Equal("Русское название", result.Name);
            AssertPreservedIds(result.ProviderIds, ids);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task EmptyMetadataResponse_DoesNotBlankExistingIds(bool movie)
        {
            using var fixture = new PluginFixture("{\"data\":{\"animes\":[]}}");
            var ids = ExistingIds(includeShikimori: true);

            if (movie)
            {
                var result = await fixture.MovieProvider.GetMetadata(new MovieInfo { Name = "Title", ProviderIds = ids }, CancellationToken.None);
                Assert.False(result.HasMetadata);
            }
            else
            {
                var result = await fixture.SeriesProvider.GetMetadata(new SeriesInfo { Name = "Title", ProviderIds = ids }, CancellationToken.None);
                Assert.False(result.HasMetadata);
            }

            Assert.Equal("1234", ids[ShikimoriPlugin.ProviderId]);
            Assert.Equal("45999", ids["Tmdb"]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Metadata_PropagatesCancellation(bool movie)
        {
            using var fixture = new PluginFixture();
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var ids = ExistingIds(includeShikimori: true);

            if (movie)
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    fixture.MovieProvider.GetMetadata(new MovieInfo { ProviderIds = ids }, cancellation.Token));
            }
            else
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                    fixture.SeriesProvider.GetMetadata(new SeriesInfo { ProviderIds = ids }, cancellation.Token));
            }
        }

        private static Dictionary<string, string> ExistingIds(bool includeShikimori)
        {
            var ids = new Dictionary<string, string>
            {
                ["Tvdb"] = "119141",
                ["Tmdb"] = "45999",
                ["AniList"] = "6586",
                ["AniDB"] = "6966",
                ["Imdb"] = "tt6608398",
                ["CustomProvider"] = "custom-id"
            };
            if (includeShikimori)
            {
                ids[ShikimoriPlugin.ProviderId] = "1234";
            }

            return ids;
        }

        private static void AssertPreservedIds(IReadOnlyDictionary<string, string> actual, IReadOnlyDictionary<string, string> original)
        {
            foreach (var pair in original.Where(pair => pair.Key != ShikimoriPlugin.ProviderId))
            {
                Assert.Equal(pair.Value, actual[pair.Key]);
            }

            Assert.Equal("6586", actual[ShikimoriPlugin.ProviderId]);
            if (original.ContainsKey(ShikimoriPlugin.ProviderId))
            {
                Assert.Equal("1234", original[ShikimoriPlugin.ProviderId]);
            }
        }

        private sealed class PluginFixture : IDisposable
        {
            private const string Response = """
                {"data":{"animes":[{"id":6586,"name":"Original title","russian":"Русское название",
                "japanese":"日本語","kind":"tv","status":"ongoing","score":8.5,"rating":"pg_13",
                "genres":[{"id":1,"name":"Drama","russian":"Драма"}],
                "studios":[{"id":1,"name":"Studio"}],"airedOn":{"year":2010,"month":1,"day":1},
                "poster":{"originalUrl":"https://example.invalid/poster.jpg","previewUrl":"https://example.invalid/preview.jpg"}}]}}
                """;

            private readonly string _directory = Path.Combine(AppContext.BaseDirectory, "fixtures", Guid.NewGuid().ToString("N"));
            private readonly FixtureClientFactory _clientFactory;

            public PluginFixture(string response = Response)
            {
                _clientFactory = new FixtureClientFactory(response);
                Plugin = new ShikimoriPlugin(new FixturePaths(_directory), new FixtureXmlSerializer(), _clientFactory);
                var manager = new ShikimoriClientManager(NullLogger<ShikimoriClientManager>.Instance);
                var resolver = new ProviderIdResolver();
                MovieProvider = new ShikimoriMovieProvider(NullLogger<ShikimoriMovieProvider>.Instance, manager, resolver);
                SeriesProvider = new ShikimoriSeriesProvider(NullLogger<ShikimoriSeriesProvider>.Instance, manager, resolver);
            }

            public ShikimoriPlugin Plugin { get; }
            public ShikimoriMovieProvider MovieProvider { get; }
            public ShikimoriSeriesProvider SeriesProvider { get; }

            public void Dispose()
            {
                _clientFactory.Dispose();
                if (Directory.Exists(_directory))
                {
                    Directory.Delete(_directory, recursive: true);
                }
            }
        }

        private sealed class FixtureClientFactory : IHttpClientFactory, IDisposable
        {
            private readonly HttpClient _client;

            public FixtureClientFactory(string response)
            {
                _client = new HttpClient(new FixtureHttpHandler(response));
            }

            public HttpClient CreateClient(string name) => _client;
            public void Dispose() => _client.Dispose();
        }

        private sealed class FixtureHttpHandler(string response) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Assert.Equal(HttpMethod.Post, request.Method);
                Assert.Equal("https://shikimori.io/api/graphql", request.RequestUri!.AbsoluteUri);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(response)
                });
            }
        }

        private sealed class FixturePaths(string root) : IApplicationPaths
        {
            public string ProgramDataPath => root;
            public string WebPath => root;
            public string ProgramSystemPath => root;
            public string DataPath => root;
            public string ImageCachePath => root;
            public string PluginsPath => Path.Combine(root, "plugins");
            public string PluginConfigurationsPath => Path.Combine(PluginsPath, "configurations");
            public string LogDirectoryPath => root;
            public string ConfigurationDirectoryPath => root;
            public string SystemConfigurationFilePath => Path.Combine(root, "system.xml");
            public string CachePath => root;
            public string TempDirectory => root;
            public string VirtualDataPath => root;
            public string TrickplayPath => root;
            public string BackupPath => root;
            public void MakeSanityCheckOrThrow() => throw new NotSupportedException();
            public void CreateAndCheckMarker(string path, string markerName, bool recursive) => throw new NotSupportedException();
        }

        private sealed class FixtureXmlSerializer : IXmlSerializer
        {
            public object DeserializeFromStream(Type type, Stream stream) => new XmlSerializer(type).Deserialize(stream)!;
            public void SerializeToStream(object obj, Stream stream) => new XmlSerializer(obj.GetType()).Serialize(stream, obj);
            public void SerializeToFile(object obj, string file)
            {
                using var stream = File.Create(file);
                SerializeToStream(obj, stream);
            }
            public object DeserializeFromFile(Type type, string file)
            {
                using var stream = File.OpenRead(file);
                return DeserializeFromStream(type, stream);
            }
            public object DeserializeFromBytes(Type type, byte[] buffer)
            {
                using var stream = new MemoryStream(buffer);
                return DeserializeFromStream(type, stream);
            }
        }
    }
}
