using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VenueAssistant.MCP.Models;

namespace VenueAssistant.MCP.Services;

/// <summary>
/// Ensures the data directory and base JSON files exist with empty stubs.
/// </summary>
public static class PersistenceBootstrapper
{
    public static void EnsureDataFiles(string dataDirectory, JsonSerializerOptions jsonOptions, bool seedSamples = false)
    {
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        if (seedSamples)
        {
            CreateVenuesFromSeed(Path.Combine(dataDirectory, "venues.json"), "Data/sample-venues.json", jsonOptions);
            CreateIfMissingFromSeed(Path.Combine(dataDirectory, "vendors.json"), "Data/sample-vendors.json");
            CreateIfMissingFromSeed(Path.Combine(dataDirectory, "events.json"), "Data/sample-events.json");
            CreateIfMissing(Path.Combine(dataDirectory, "communications.json"), new List<Communication>(), jsonOptions);
        }
        else
        {
            CreateIfMissing(Path.Combine(dataDirectory, "venues.json"), new Dictionary<string, Venue>(), jsonOptions);
            CreateIfMissing(Path.Combine(dataDirectory, "vendors.json"), new Dictionary<string, Dictionary<string, Vendor>>(), jsonOptions);
            CreateIfMissing(Path.Combine(dataDirectory, "events.json"), new Dictionary<string, Event>(), jsonOptions);
            CreateIfMissing(Path.Combine(dataDirectory, "communications.json"), new List<Communication>(), jsonOptions);
        }
    }

    private static void CreateIfMissing<T>(string path, T seed, JsonSerializerOptions jsonOptions)
    {
        if (File.Exists(path))
        {
            return;
        }

        var json = JsonSerializer.Serialize(seed, jsonOptions);
        File.WriteAllText(path, json);
    }

    private static void CreateIfMissingFromSeed(string targetPath, string seedRelativePath)
    {
        if (!ShouldOverwriteWithSeed(targetPath) && File.Exists(targetPath))
        {
            return;
        }

        if (TryResolveSeedPath(seedRelativePath, out var seedPath))
        {
            File.Copy(seedPath, targetPath, overwrite: true);
            return;
        }

        // Fallback to empty file if seed missing.
        File.WriteAllText(targetPath, "{}");
    }

    private static void CreateVenuesFromSeed(string targetPath, string seedRelativePath, JsonSerializerOptions jsonOptions)
    {
        if (!ShouldOverwriteWithSeed(targetPath) && File.Exists(targetPath))
        {
            return;
        }

        if (!TryResolveSeedPath(seedRelativePath, out var seedPath))
        {
            File.WriteAllText(targetPath, "{}");
            return;
        }

        try
        {
            var seedJson = File.ReadAllText(seedPath);
            var venues = JsonSerializer.Deserialize<List<Venue>>(seedJson, jsonOptions) ?? new List<Venue>();
            var dict = venues.ToDictionary(v => v.Id, v => v, StringComparer.OrdinalIgnoreCase);
            var json = JsonSerializer.Serialize(dict, jsonOptions);
            File.WriteAllText(targetPath, json);
        }
        catch
        {
            File.WriteAllText(targetPath, "{}");
        }
    }

    private static bool TryResolveSeedPath(string seedRelativePath, out string seedPath)
    {
        // 1) Try alongside the built assembly (when data files are copied on publish).
        var candidate = Path.Combine(AppContext.BaseDirectory, seedRelativePath);
        if (File.Exists(candidate))
        {
            seedPath = candidate;
            return true;
        }

        // 2) Try project-root relative (useful for dotnet run from source).
        var projectRelative = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", seedRelativePath));
        if (File.Exists(projectRelative))
        {
            seedPath = projectRelative;
            return true;
        }

        seedPath = string.Empty;
        return false;
    }

    private static bool ShouldOverwriteWithSeed(string targetPath)
    {
        if (!File.Exists(targetPath))
        {
            return true;
        }

        var contents = File.ReadAllText(targetPath);
        var trimmed = contents.Trim();
        return string.IsNullOrWhiteSpace(trimmed) || trimmed == "{}" || trimmed == "[]" || trimmed.StartsWith("[");
    }
}
