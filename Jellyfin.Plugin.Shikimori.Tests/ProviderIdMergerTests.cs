namespace Jellyfin.Plugin.Shikimori.Tests
{
    public class ProviderIdMergerTests
    {
        [Fact]
        public void Merge_PreservesExternalIdsAndAddsShikimoriId()
        {
            var existingProviderIds = new Dictionary<string, string>
            {
                ["Tvdb"] = "119141",
                ["Tmdb"] = "45999",
                ["AniList"] = "6586",
                ["AniDB"] = "6966",
                ["Imdb"] = "tt6608398",
                ["CustomProvider"] = "custom-id",
            };

            var result = ProviderIdMerger.Merge(existingProviderIds, "6586");

            Assert.Equal("119141", result["Tvdb"]);
            Assert.Equal("45999", result["Tmdb"]);
            Assert.Equal("6586", result["AniList"]);
            Assert.Equal("6966", result["AniDB"]);
            Assert.Equal("tt6608398", result["Imdb"]);
            Assert.Equal("custom-id", result["CustomProvider"]);
            Assert.Equal("6586", result[ShikimoriPlugin.ProviderId]);
        }

        [Fact]
        public void Merge_ReplacesStaleShikimoriIdWithoutMutatingInput()
        {
            var existingProviderIds = new Dictionary<string, string>
            {
                ["Tmdb"] = "45999",
                [ShikimoriPlugin.ProviderId] = "1234",
            };

            var result = ProviderIdMerger.Merge(existingProviderIds, "6586");

            Assert.Equal("6586", result[ShikimoriPlugin.ProviderId]);
            Assert.Equal("1234", existingProviderIds[ShikimoriPlugin.ProviderId]);
            Assert.NotSame(existingProviderIds, result);
        }

        [Fact]
        public void Merge_WithNoExistingIds_ReturnsOnlyShikimoriId()
        {
            var result = ProviderIdMerger.Merge(null, "6586");

            var providerId = Assert.Single(result);
            Assert.Equal(ShikimoriPlugin.ProviderId, providerId.Key);
            Assert.Equal("6586", providerId.Value);
        }
    }
}
