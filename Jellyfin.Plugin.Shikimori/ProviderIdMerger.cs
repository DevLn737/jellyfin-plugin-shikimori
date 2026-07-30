namespace Jellyfin.Plugin.Shikimori
{
    internal static class ProviderIdMerger
    {
        public static Dictionary<string, string> Merge(
            IReadOnlyDictionary<string, string>? existingProviderIds,
            string shikimoriId)
        {
            var providerIds = existingProviderIds == null
                ? new Dictionary<string, string>()
                : existingProviderIds.ToDictionary(pair => pair.Key, pair => pair.Value);

            providerIds[ShikimoriPlugin.ProviderId] = shikimoriId;

            return providerIds;
        }
    }
}
