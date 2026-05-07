using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Abp.Localization.Dictionaries;

namespace ClientPlatform.Localization;

/// <summary>
/// Provides localization dictionaries from embedded JSON files.
/// </summary>
public class JsonEmbeddedFileLocalizationDictionaryProvider : ILocalizationDictionaryProvider
{
    private readonly Assembly _assembly;
    private readonly string _rootNamespace;
    private readonly Dictionary<string, ILocalizationDictionary> _dictionaries;

    /// <summary>
    /// Creates a new <see cref="JsonEmbeddedFileLocalizationDictionaryProvider"/> object.
    /// </summary>
    /// <param name="assembly">Assembly that contains embedded json files</param>
    /// <param name="rootNamespace">
    /// Namespace of the embedded json dictionary files
    /// </param>
    public JsonEmbeddedFileLocalizationDictionaryProvider(Assembly assembly, string rootNamespace)
    {
        _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        _rootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
        _dictionaries = new Dictionary<string, ILocalizationDictionary>();
    }

    public ILocalizationDictionary DefaultDictionary
    {
        get
        {
            try
            {
                return GetDictionary(CultureInfo.GetCultureInfo("en"));
            }
            catch
            {
                // Return empty dictionary if default culture not found
                return new LocalizationDictionary(CultureInfo.GetCultureInfo("en"));
            }
        }
    }

    public IDictionary<string, ILocalizationDictionary> Dictionaries => _dictionaries;

    public void Initialize(string sourceName)
    {
        // Pre-load all available dictionaries during initialization
        // This is required by ABP Framework to ensure dictionaries are available
        var allResources = _assembly.GetManifestResourceNames();
        var jsonResources = allResources.Where(r => 
            r.EndsWith(".json", StringComparison.OrdinalIgnoreCase) &&
            (r.Contains("ClientPlatform", StringComparison.OrdinalIgnoreCase) || 
             r.Contains("Localization", StringComparison.OrdinalIgnoreCase))
        ).ToList();

        foreach (var resourceName in jsonResources)
        {
            try
            {
                // Extract culture from resource name
                var culture = ExtractCultureFromResourceName(resourceName);
                if (culture != null && !_dictionaries.ContainsKey(culture.Name))
                {
                    var dictionary = LoadDictionaryFromResource(resourceName, culture);
                    if (dictionary != null)
                    {
                        _dictionaries[culture.Name] = dictionary;
                    }
                }
            }
            catch
            {
                // Skip resources that can't be loaded
                continue;
            }
        }
    }

    private CultureInfo ExtractCultureFromResourceName(string resourceName)
    {
        // Extract culture from resource name patterns:
        // - ClientPlatform.json -> en
        // - ClientPlatform-zh-Hans.json -> zh-Hans
        // - ClientPlatform-pt-BR.json -> pt-BR
        
        if (resourceName.Contains("ClientPlatform.json", StringComparison.OrdinalIgnoreCase) ||
            resourceName.EndsWith("ClientPlatform.json", StringComparison.OrdinalIgnoreCase))
        {
            return CultureInfo.GetCultureInfo("en");
        }

        // Try to extract culture code from pattern: ClientPlatform-{culture}.json
        var match = System.Text.RegularExpressions.Regex.Match(
            resourceName, 
            @"ClientPlatform-([a-z]{2}(?:-[A-Z]{2,})?)\.json",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        if (match.Success && match.Groups.Count > 1)
        {
            var cultureCode = match.Groups[1].Value;
            try
            {
                return CultureInfo.GetCultureInfo(cultureCode);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private ILocalizationDictionary LoadDictionaryFromResource(string resourceName, CultureInfo culture)
    {
        var resourceStream = _assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            return null;
        }

        try
        {
            using var reader = new StreamReader(resourceStream);
            var jsonContent = reader.ReadToEnd();
            return ParseJsonContent(jsonContent, culture);
        }
        catch
        {
            return null;
        }
    }

    public void Extend(ILocalizationDictionary dictionary)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        var cultureCode = dictionary.CultureInfo.Name;
        if (_dictionaries.TryGetValue(cultureCode, out var existingDictionary))
        {
            foreach (var item in dictionary.GetAllStrings())
            {
                existingDictionary[item.Name] = item.Value;
            }
        }
        else
        {
            _dictionaries[cultureCode] = dictionary;
        }
    }

    public ILocalizationDictionary GetDictionary(string cultureName)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
        return GetDictionary(cultureInfo);
    }

    public ILocalizationDictionary GetDictionary(CultureInfo culture)
    {
        if (culture == null)
        {
            throw new ArgumentNullException(nameof(culture));
        }

        var cultureCode = culture.Name;

        if (_dictionaries.TryGetValue(cultureCode, out var existingDictionary))
        {
            return existingDictionary;
        }

        var dictionary = CreateDictionaryForCulture(culture);
        if (dictionary != null)
        {
            _dictionaries[cultureCode] = dictionary;
        }

        return dictionary;
    }

    protected virtual ILocalizationDictionary CreateDictionaryForCulture(CultureInfo culture)
    {
        var resourceName = FindResourceName(culture);
        
        if (resourceName == null)
        {
            // Try to get default culture (en) if specific culture not found
            if (culture.Name != "en")
            {
                return CreateDictionaryForCulture(CultureInfo.GetCultureInfo("en"));
            }
            
            // Debug: List all available resources
            var allResources = _assembly.GetManifestResourceNames();
            var relevantResources = allResources.Where(r => 
                r.Contains("ClientPlatform", StringComparison.OrdinalIgnoreCase) || 
                r.Contains("Localization", StringComparison.OrdinalIgnoreCase) ||
                r.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            throw new Exception(
                $"Could not find JSON localization resource for culture '{culture.Name}'. " +
                $"Searched for patterns containing 'ClientPlatform' and '{culture.Name}'. " +
                $"Available resources: {string.Join(", ", relevantResources)}");
        }

        var resourceStream = _assembly.GetManifestResourceStream(resourceName);
        if (resourceStream == null)
        {
            throw new Exception($"Resource stream is null for resource: {resourceName}");
        }

        try
        {
            using var reader = new StreamReader(resourceStream);
            var jsonContent = reader.ReadToEnd();
            var dictionary = ParseJsonContent(jsonContent, culture);
            return dictionary;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse JSON localization file: {resourceName}", ex);
        }
    }

    protected virtual string FindResourceName(CultureInfo culture)
    {
        var cultureCode = culture.Name;
        var baseName = "ClientPlatform";
        var allResources = _assembly.GetManifestResourceNames();
        
        // Build exact filename patterns to search for
        string fileName;
        if (cultureCode == "en")
        {
            fileName = $"{baseName}.json";
        }
        else
        {
            fileName = $"{baseName}-{cultureCode}.json";
        }

        // First, try exact match with rootNamespace
        var exactMatch = $"{_rootNamespace}.{fileName}";
        if (allResources.Contains(exactMatch, StringComparer.OrdinalIgnoreCase))
        {
            return allResources.First(r => r.Equals(exactMatch, StringComparison.OrdinalIgnoreCase));
        }

        // Try to find by filename ending
        var resource = allResources.FirstOrDefault(r => 
            r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) &&
            (r.Contains("ClientPlatform", StringComparison.OrdinalIgnoreCase) || 
             r.Contains("Localization", StringComparison.OrdinalIgnoreCase))
        );

        if (resource != null)
        {
            return resource;
        }

        // Last resort: search for any resource containing the filename
        resource = allResources.FirstOrDefault(r => 
            r.Contains(fileName, StringComparison.OrdinalIgnoreCase)
        );

        return resource;
    }

    protected virtual ILocalizationDictionary ParseJsonContent(string jsonContent, CultureInfo culture)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        var jsonDoc = JsonDocument.Parse(jsonContent, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        var root = jsonDoc.RootElement;

        // Get culture from JSON or use provided culture
        var cultureName = root.TryGetProperty("culture", out var cultureProp)
            ? cultureProp.GetString()
            : culture.Name;

        var dictionary = new LocalizationDictionary(CultureInfo.GetCultureInfo(cultureName));

        // Parse texts
        if (root.TryGetProperty("texts", out var textsProp))
        {
            if (textsProp.ValueKind == JsonValueKind.Object)
            {
                // Format: { "texts": { "key": "value", ... } }
                foreach (var text in textsProp.EnumerateObject())
                {
                    var key = text.Name;
                    var value = text.Value.GetString() ?? string.Empty;
                    dictionary[key] = value;
                }
            }
            else if (textsProp.ValueKind == JsonValueKind.Array)
            {
                // Format: { "texts": [ { "name": "key", "value": "value" }, ... ] }
                foreach (var textItem in textsProp.EnumerateArray())
                {
                    if (textItem.TryGetProperty("name", out var nameProp) &&
                        textItem.TryGetProperty("value", out var valueProp))
                    {
                        var key = nameProp.GetString();
                        var value = valueProp.GetString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(key))
                        {
                            dictionary[key] = value;
                        }
                    }
                }
            }
        }

        return dictionary;
    }
}

