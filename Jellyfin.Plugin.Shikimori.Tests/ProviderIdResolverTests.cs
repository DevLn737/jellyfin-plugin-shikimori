using MediaBrowser.Controller.Providers;

namespace Jellyfin.Plugin.Shikimori.Tests
{
    public class ProviderIdResolverTests
    {
        [Fact]
        public void Resolve_PrefersSavedShikimoriIdOverFilename()
        {
            var info = new SeriesInfo
            {
                Path = "/media/anime/Title [shikimori-1234]/",
                ProviderIds = new Dictionary<string, string>
                {
                    [ShikimoriPlugin.ProviderId] = "6586",
                    ["Tmdb"] = "45999"
                }
            };

            Assert.True(new ProviderIdResolver().TryResolve(info, out var id));
            Assert.Equal(6586L, id);
            Assert.Equal("45999", info.ProviderIds["Tmdb"]);
        }

        [Theory]
        [InlineData("/media/anime/Title [shikimori-6586]/")]
        [InlineData("/media/anime/Title [SHIKIMORI6586]/")]
        [InlineData("/media/anime/[shikimori-1234]/Title [shikimori-6586]/")]
        public void Resolve_FilenameRetainsExistingSyntaxAndRightmostMatch(string path)
        {
            var info = new MovieInfo { Path = path };

            Assert.True(new ProviderIdResolver().TryResolve(info, out var id));
            Assert.Equal(6586L, id);
        }

        [Fact]
        public void Resolve_InvalidSavedIdFallsBackToFilename()
        {
            var info = new SeriesInfo
            {
                Path = "/media/anime/Title [shikimori-6586]/",
                ProviderIds = new Dictionary<string, string> { [ShikimoriPlugin.ProviderId] = "invalid" }
            };

            Assert.True(new ProviderIdResolver().TryResolve(info, out var id));
            Assert.Equal(6586L, id);
        }

        [Fact]
        public void Resolve_DoesNotInterpretOtherProviderIdsAsShikimori()
        {
            var info = new MovieInfo
            {
                Path = "/media/movies/Title [tmdbid-6586]/",
                ProviderIds = new Dictionary<string, string> { ["Tmdb"] = "6586" }
            };

            Assert.False(new ProviderIdResolver().TryResolve(info, out _));
            Assert.Single(info.ProviderIds);
        }
    }
}
