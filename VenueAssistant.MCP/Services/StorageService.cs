using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using VenueAssistant.MCP.Models;
using VenueAssistant.MCP.Serialization;

namespace VenueAssistant.MCP.Services;

/// <summary>
/// File-based JSON persistence layer with simple thread-safe CRUD operations.
/// </summary>
public class StorageService
{
    private readonly string _dataDirectory;
    private readonly object _lockObject = new();
    private readonly JsonSerializerOptions _jsonOptions;

    private Dictionary<string, Venue> _venues = new();
    private Dictionary<string, Dictionary<string, Vendor>> _vendors = new();
    private Dictionary<string, Event> _events = new();
    private List<Communication> _communications = new();

    public StorageService(string dataDirectory)
    {
        _dataDirectory = dataDirectory;
        _jsonOptions = Program.JsonOptions ?? CreateDefaultOptions();
    }

    public void Load()
    {
        lock (_lockObject)
        {
            EnsureDataDirectory();
            _venues = LoadFile("venues.json", new Dictionary<string, Venue>());
            _vendors = LoadFile("vendors.json", new Dictionary<string, Dictionary<string, Vendor>>());
            _events = LoadFile("events.json", new Dictionary<string, Event>());
            _communications = LoadFile("communications.json", new List<Communication>());
        }
    }

    public void Save()
    {
        lock (_lockObject)
        {
            SaveUnlocked();
        }
    }

    public void AddVenue(Venue venue)
    {
        lock (_lockObject)
        {
            var now = DateTime.UtcNow;
            var record = venue with { CreatedAt = now, UpdatedAt = now };
            _venues[record.Id] = record;
            SaveUnlocked();
        }
    }

    public Venue? GetVenue(string venueId)
    {
        lock (_lockObject)
        {
            return _venues.TryGetValue(venueId, out var venue) ? venue : null;
        }
    }

    public IEnumerable<Venue> GetAllVenues()
    {
        lock (_lockObject)
        {
            return _venues.Values.ToList();
        }
    }

    public void AddVendor(string venueId, Vendor vendor)
    {
        lock (_lockObject)
        {
            if (!_vendors.TryGetValue(venueId, out var venueVendors))
            {
                venueVendors = new Dictionary<string, Vendor>();
                _vendors[venueId] = venueVendors;
            }

            var now = DateTime.UtcNow;
            var record = vendor with { CreatedAt = now, UpdatedAt = now };
            venueVendors[record.Id] = record;
            SaveUnlocked();
        }
    }

    public Vendor? GetVendor(string venueId, string vendorId)
    {
        lock (_lockObject)
        {
            return _vendors.TryGetValue(venueId, out var venueVendors) && venueVendors.TryGetValue(vendorId, out var vendor)
                ? vendor
                : null;
        }
    }

    public IEnumerable<Vendor> GetVendorsByCategory(string venueId, string? category = null)
    {
        lock (_lockObject)
        {
            if (!_vendors.TryGetValue(venueId, out var venueVendors))
            {
                return Enumerable.Empty<Vendor>();
            }

            return venueVendors.Values
                .Where(v => category == null || string.Equals(v.Category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    public void UpdateVendor(string venueId, string vendorId, Func<Vendor, Vendor> update)
    {
        lock (_lockObject)
        {
            if (!_vendors.TryGetValue(venueId, out var venueVendors) || !venueVendors.TryGetValue(vendorId, out var vendor))
            {
                return;
            }

            var updated = update(vendor) with { UpdatedAt = DateTime.UtcNow };
            venueVendors[vendorId] = updated;
            SaveUnlocked();
        }
    }

    public void AddEvent(Event eventRecord)
    {
        lock (_lockObject)
        {
            var now = DateTime.UtcNow;
            var record = eventRecord with { CreatedAt = now, UpdatedAt = now };
            _events[record.Id] = record;
            SaveUnlocked();
        }
    }

    public Event? GetEvent(string eventId)
    {
        lock (_lockObject)
        {
            return _events.TryGetValue(eventId, out var evt) ? evt : null;
        }
    }

    public IEnumerable<Event> GetEventsByVenue(string venueId)
    {
        lock (_lockObject)
        {
            return _events.Values.Where(e => e.VenueId == venueId).ToList();
        }
    }

    public void UpdateEvent(string eventId, Func<Event, Event> update)
    {
        lock (_lockObject)
        {
            if (!_events.TryGetValue(eventId, out var evt))
            {
                return;
            }

            var updated = update(evt) with { UpdatedAt = DateTime.UtcNow };
            _events[eventId] = updated;
            SaveUnlocked();
        }
    }

    public void AddCommunication(Communication communication)
    {
        lock (_lockObject)
        {
            _communications.Add(communication);
            SaveUnlocked();
        }
    }

    public IEnumerable<Communication> GetCommunications(string eventId)
    {
        lock (_lockObject)
        {
            return _communications.Where(c => c.EventId == eventId).ToList();
        }
    }

    private void SaveUnlocked()
    {
        EnsureDataDirectory();
        WriteFile("venues.json", _venues);
        WriteFile("vendors.json", _vendors);
        WriteFile("events.json", _events);
        WriteFile("communications.json", _communications);
    }

    private T LoadFile<T>(string fileName, T fallback)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        if (!File.Exists(path))
        {
            return fallback;
        }

        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
        {
            return fallback;
        }

        return JsonSerializer.Deserialize<T>(json, _jsonOptions) ?? fallback;
    }

    private void WriteFile<T>(string fileName, T content)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        var json = JsonSerializer.Serialize(content, _jsonOptions);
        File.WriteAllText(path, json);
    }

    private void EnsureDataDirectory()
    {
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
    }

    private static JsonSerializerOptions CreateDefaultOptions() =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(), new TimeOnlyJsonConverter() }
        };
}
