using System.Xml.Serialization;
using Jellyfin.Plugin.Shikimori.Configuration;

namespace Jellyfin.Plugin.Shikimori.Tests
{
    public class PluginConfigurationTests
    {
        [Fact]
        public void LegacyXml_RoundTripsEverySavedSetting()
        {
            const string legacyXml = """
                <?xml version="1.0" encoding="utf-8"?>
                <PluginConfiguration>
                  <SearchLimit>37</SearchLimit>
                  <TitlePreference>Japanese</TitlePreference>
                  <OriginalTitlePreference>Russian</OriginalTitlePreference>
                  <SearchTitlePreference>Romaji</SearchTitlePreference>
                  <GenreTitleLanguagePreference>English</GenreTitleLanguagePreference>
                  <ShowCensored>true</ShowCensored>
                  <ShikimoriBaseUrl>https://shikimori.io</ShikimoriBaseUrl>
                </PluginConfiguration>
                """;

            var serializer = new XmlSerializer(typeof(PluginConfiguration));
            using var reader = new StringReader(legacyXml);
            var restored = Assert.IsType<PluginConfiguration>(serializer.Deserialize(reader));

            using var writer = new StringWriter();
            serializer.Serialize(writer, restored);
            using var roundTripReader = new StringReader(writer.ToString());
            var roundTrip = Assert.IsType<PluginConfiguration>(serializer.Deserialize(roundTripReader));

            Assert.Equal(37, roundTrip.SearchLimit);
            Assert.Equal(TitlePreferenceType.Japanese, roundTrip.TitlePreference);
            Assert.Equal(TitlePreferenceType.Russian, roundTrip.OriginalTitlePreference);
            Assert.Equal(TitlePreferenceType.Romaji, roundTrip.SearchTitlePreference);
            Assert.Equal(GenreTitleLanguagePreferenceType.English, roundTrip.GenreTitleLanguagePreference);
            Assert.True(roundTrip.ShowCensored);
            Assert.Equal("https://shikimori.io", roundTrip.ShikimoriBaseUrl);
        }

        [Fact]
        public void LegacyXml_WithMissingFields_PreservesExistingDefaults()
        {
            var serializer = new XmlSerializer(typeof(PluginConfiguration));
            using var reader = new StringReader("<PluginConfiguration />");

            var restored = Assert.IsType<PluginConfiguration>(serializer.Deserialize(reader));

            Assert.Equal(10, restored.SearchLimit);
            Assert.Equal(TitlePreferenceType.Russian, restored.TitlePreference);
            Assert.Equal(TitlePreferenceType.Romaji, restored.OriginalTitlePreference);
            Assert.Equal(TitlePreferenceType.Russian, restored.SearchTitlePreference);
            Assert.Equal(GenreTitleLanguagePreferenceType.Russian, restored.GenreTitleLanguagePreference);
            Assert.False(restored.ShowCensored);
            Assert.Equal("https://shikimori.io", restored.ShikimoriBaseUrl);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(50)]
        public void SearchLimit_AcceptsApiBoundaries(int value)
        {
            var configuration = new PluginConfiguration { SearchLimit = value };

            Assert.Equal(value, configuration.SearchLimit);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(51)]
        public void SearchLimit_RejectsInvalidValuesWithoutResettingSavedValue(int value)
        {
            var configuration = new PluginConfiguration { SearchLimit = 37 };

            configuration.SearchLimit = value;

            Assert.Equal(37, configuration.SearchLimit);
        }
    }
}
