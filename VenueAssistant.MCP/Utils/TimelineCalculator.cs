using System;
using System.Collections.Generic;
using VenueAssistant.MCP.Models;

namespace VenueAssistant.MCP.Utils;

/// <summary>
/// Provides event-type timeline templates and materializes them to concrete timeline items.
/// </summary>
public class TimelineCalculator
{
    private readonly Dictionary<string, List<(string Task, double OffsetHours)>> _templates =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["gala"] = new()
            {
                ("Vendor Setup Begins", -4),
                ("Final Walkthrough", -2),
                ("Guests Arrive", 0),
                ("Cocktail Hour", 0.5),
                ("Dinner Service", 1.5),
                ("Dancing/Entertainment", 3),
                ("Event Ends", 4),
                ("Breakdown Complete", 5)
            },
            ["conference"] = new()
            {
                ("AV Setup & Test", -3),
                ("Vendor Setup Complete", -2),
                ("Doors Open", 0),
                ("Breakfast Service", 0.5),
                ("Sessions Begin", 1),
                ("Lunch Service", 4),
                ("Closing Ceremony", 8),
                ("Breakdown", 9)
            },
            ["wedding"] = new()
            {
                ("Vendor Setup & Decorations", -5),
                ("Catering Setup", -3),
                ("Guest Arrivals Begin", -1),
                ("Ceremony Begins", 0),
                ("Cocktail Hour", 1),
                ("Dinner Service", 1.5),
                ("Toasts & Dancing", 3),
                ("Reception Ends", 5)
            }
        };

    private readonly List<(string Task, double OffsetHours)> _defaultTemplate = new()
    {
        ("Setup Begins", -3),
        ("Event Starts", 0),
        ("Wrap Up", 4),
        ("Breakdown Complete", 5)
    };

    public List<TimelineItem> BuildTimeline(string eventType, DateTime eventStart, int setupHoursBefore = 4)
    {
        var template = ResolveTemplate(eventType, setupHoursBefore);
        var items = new List<TimelineItem>(template.Count);

        foreach (var (task, offset) in template)
        {
            items.Add(new TimelineItem
            {
                Task = task,
                OffsetHoursFromEventStart = offset,
                ScheduledTime = eventStart.AddHours(offset)
            });
        }

        return items;
    }

    private List<(string Task, double OffsetHours)> ResolveTemplate(string eventType, int setupHoursBefore)
    {
        if (!string.IsNullOrWhiteSpace(eventType) && _templates.TryGetValue(eventType, out var template))
        {
            return template;
        }

        // Fallback uses provided setup hours to anchor first task.
        return new List<(string Task, double OffsetHours)>
        {
            ("Setup Begins", -Math.Abs(setupHoursBefore)),
            ("Event Starts", 0),
            ("Wrap Up", 4),
            ("Breakdown Complete", 5)
        };
    }
}
