using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Ecowitt.Controller.Model.Mapping
{
    public class SensorConfigurationService
    {
        private SensorMappingConfiguration _configuration = new();
        private readonly ILogger<SensorConfigurationService> _logger;
        private readonly string _configPath;
        
        public SensorConfigurationService(ILogger<SensorConfigurationService> logger, string configPath)
        {
            _logger = logger;
            _configPath = configPath;
            LoadConfigurations();
        }

        public void LoadConfigurations()
        {
            if (!Directory.Exists(_configPath))
            {
                _logger.LogWarning("Configuration directory not found: {ConfigPath}. Creating default ignored properties list.", _configPath);
                _configuration = CreateDefaultConfiguration();
                return;
            }

            _configuration = new SensorMappingConfiguration();
            var fileCount = 0;
            
            foreach (var file in Directory.GetFiles(_configPath, "*.json", SearchOption.AllDirectories))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        _logger.LogWarning("Configuration file is empty: {File}", file);
                        continue;
                    }

                    var fileConfig = JsonSerializer.Deserialize<SensorMappingConfiguration>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    });
                    
                    if (fileConfig == null)
                    {
                        _logger.LogWarning("Failed to deserialize configuration file: {File}", file);
                        continue;
                    }

                    // Merge exact mappings
                    foreach (var mapping in fileConfig.ExactMappings)
                    {
                        if (_configuration.ExactMappings.ContainsKey(mapping.Key))
                        {
                            _logger.LogWarning("Duplicate sensor mapping found for '{PropertyName}'. Using definition from: {File}", mapping.Key, file);
                        }
                        _configuration.ExactMappings[mapping.Key] = mapping.Value;
                    }

                    // Merge ignored properties
                    foreach (var ignored in fileConfig.IgnoredProperties)
                    {
                        _configuration.IgnoredProperties.Add(ignored);
                    }

                    // Add pattern mappings
                    foreach (var pattern in fileConfig.PatternMappings)
                    {
                        try
                        {
                            pattern.CompiledPattern = new Regex(pattern.Pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                            _configuration.PatternMappings.Add(pattern);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Invalid regex pattern in config file {File}: {Pattern}", file, pattern.Pattern);
                        }
                    }

                    fileCount++;
                    _logger.LogDebug("Loaded sensor configuration from {File}", file);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing configuration file: {File}", file);
                }
            }

            // Add default ignored properties if none were loaded
            if (_configuration.IgnoredProperties.Count == 0)
            {
                _configuration.IgnoredProperties = CreateDefaultConfiguration().IgnoredProperties;
            }

            _logger.LogInformation("Loaded {ExactMappings} exact mappings, {PatternMappings} pattern mappings, and {IgnoredProperties} ignored properties from {FileCount} configuration files",
                _configuration.ExactMappings.Count, _configuration.PatternMappings.Count, _configuration.IgnoredProperties.Count, fileCount);
        }

        public SensorMapping? GetMappingForProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;

            // Check if property is explicitly ignored
            if (_configuration.IgnoredProperties.Contains(propertyName))
                return null;

            // Try exact match first
            if (_configuration.ExactMappings.TryGetValue(propertyName, out var mapping))
            {
                return mapping;
            }

            // Try pattern matches
            foreach (var pattern in _configuration.PatternMappings)
            {
                if (pattern.CompiledPattern?.IsMatch(propertyName) == true)
                {
                    // Clone the pattern mapping to customize the display name
                    var customMapping = new SensorMapping
                    {
                        BuilderMethod = pattern.BuilderMethod,
                        Parameters = new List<BuilderParameter>(pattern.Parameters)
                    };

                    // Extract number or other components from property name
                    var match = pattern.CompiledPattern.Match(propertyName);
                    if (match.Groups.Count > 1 && !string.IsNullOrEmpty(pattern.DisplayNameFormat))
                    {
                        var captureGroups = match.Groups.Cast<Group>().Skip(1).Select(g => g.Value).ToArray();
                        try
                        {
                            customMapping.DisplayName = string.Format(pattern.DisplayNameFormat, captureGroups);
                        }
                        catch (FormatException ex)
                        {
                            _logger.LogWarning(ex, "Format error for pattern {Pattern} with property {PropertyName}. Using pattern format as-is.", pattern.Pattern, propertyName);
                            customMapping.DisplayName = pattern.DisplayNameFormat;
                        }
                    }
                    else
                    {
                        customMapping.DisplayName = pattern.DisplayNameFormat;
                    }

                    return customMapping;
                }
            }

            return null;
        }

        private static SensorMappingConfiguration CreateDefaultConfiguration()
        {
            return new SensorMappingConfiguration
            {
                IgnoredProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "PASSKEY", "stationtype", "runtime", "dateutc", "freq", "model",
                    "ws90_ver", "interval", "rssi", "timeutc", "publish_time", "id",
                    "nickname", "devicename", "version"
                }
            };
        }

        public SensorMappingConfiguration Configuration => _configuration;
        public bool IsPropertyIgnored(string propertyName) => _configuration.IgnoredProperties.Contains(propertyName);
    }
}