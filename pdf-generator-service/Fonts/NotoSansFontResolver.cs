using PdfSharp.Fonts;

namespace pdf_generator_service.Fonts
{
    public class NotoSansFontResolver : IFontResolver
    {
        private readonly ILogger<NotoSansFontResolver>? _logger;
        private readonly Dictionary<string, byte[]> _fontCache = new();

        public NotoSansFontResolver(ILogger<NotoSansFontResolver>? _logger = null)
        {
            this._logger = _logger;
            LoadFonts();
        }

        private void LoadFonts()
        {
            try
            {
                var fontsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts");
                _logger?.LogInformation("Fonts path: {FontsPath}", fontsPath);

                // Load Noto Sans TC Regular
                LoadFont(fontsPath, "static/NotoSansTC-Regular.ttf", "notosans");

                // Load Noto Sans TC Bold
                LoadFont(fontsPath, "static/NotoSansTC-Bold.ttf", "notosans-bold");

                // Load Noto Sans TC Light (optional)
                LoadFont(fontsPath, "static/NotoSansTC-Light.ttf", "notosans-light");

                _logger?.LogInformation("Fonts loaded successfully, total: {Count}", _fontCache.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while loading fonts");
            }
        }

        private void LoadFont(string basePath, string fileName, string cacheKey)
        {
            var fontPath = Path.Combine(basePath, fileName);

            if (File.Exists(fontPath))
            {
                _fontCache[cacheKey] = File.ReadAllBytes(fontPath);
                _logger?.LogInformation("Font loaded successfully: {FileName} -> {CacheKey}", fileName, cacheKey);
            }
            else
            {
                _logger?.LogError("Font file not found: {Path}", fontPath);

                // Use regular weight as fallback if bold is not found
                if (cacheKey.Contains("bold") && _fontCache.ContainsKey("notosans"))
                {
                    _fontCache[cacheKey] = _fontCache["notosans"];
                    _logger?.LogInformation("Using Regular weight as fallback for Bold");
                }
            }
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Normalize font name
            var normalizedName = familyName.ToLower().Replace(" ", "");

            // Map font
            var fontKey = normalizedName switch
            {
                "notosanstc" or "noto sans tc" or "notosans" => GetFontKey(isBold, isItalic),
                // Default to Noto Sans TC
                _ => GetFontKey(isBold, isItalic)
            };

            _logger?.LogDebug("Resolving font: {FamilyName} (Bold: {Bold}) -> {FontKey}",
                familyName, isBold, fontKey);

            return new FontResolverInfo(fontKey);
        }

        private string GetFontKey(bool isBold, bool isItalic)
        {
            if (isBold)
                return "notosans-bold";
            return "notosans";
        }

        public byte[]? GetFont(string faceName)
        {
            if (_fontCache.TryGetValue(faceName, out var fontData))
            {
                return fontData;
            }

            _logger?.LogWarning("Font not found: {FaceName}", faceName);
            return null;
        }
    }
}