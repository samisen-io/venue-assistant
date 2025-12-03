# VenueAssistant MCP Server - C# Implementation
## Product Requirements Document (PRD)

---

## EXECUTIVE SUMMARY

**Product:** VenueAssistant MCP Server in C#

**Objective:** Build a Model Context Protocol (MCP) server in C# that enables Claude to coordinate venue events autonomously. This will handle vendor matching, timeline generation, risk prediction, budget tracking, and post-event reporting through MCP tool calls.

**Architecture:** 
- MCP Server: C# (.NET 8+)
- Protocol: Model Context Protocol (stdio-based)
- Data: File-based JSON persistence
- AI: Claude API (for future enhancements)
- Deployment: Single-process console application

**Target Users:** Venue managers using Claude.ai to coordinate events

**Timeline:** 3-4 weeks to MVP

**Success Criteria:**
- Claude can call all MCP tools
- Venue managers can coordinate complete events through conversation
- All data persists correctly between sessions
- Zero external dependencies (no database)
- Deployable as single executable

---

## PRODUCT VISION

### Problem
Venue managers spend 40-60 hours per month coordinating vendors via fragmented email chains, spreadsheets, and phone calls. This leads to miscommunications, delays, cost overruns, and poor guest experiences.

### Solution
VenueAssistant MCP Server makes Claude the venue coordinator. Venue managers describe their event once to Claude. Claude:
- Extracts requirements
- Matches vendors automatically
- Generates timelines
- Predicts risks
- Tracks budgets
- Generates post-event reports

All through natural conversation. Zero new software to learn.

### Why C#?
- Strong typing and compile-time safety (critical for data consistency)
- Excellent async/await for handling multiple concurrent events
- Built-in dependency injection (easier to maintain than Node.js)
- File I/O performance (JSON persistence)
- Cross-platform (.NET can run on Windows, Linux, macOS)
- Codex integration is native for C#

---

## TECHNICAL SPECIFICATIONS

### Technology Stack

```
Runtime: .NET 8.0 LTS
Language: C# 12
Framework: Console App (.NET)
Package Manager: NuGet
Key Libraries:
  - System.Text.Json (JSON serialization)
  - System.Diagnostics (logging)
  - System.IO.Abstractions (file operations)
  - Optional: Serilog (advanced logging)
```

### Project Structure

```
VenueAssistant.MCP/
├── VenueAssistant.MCP.csproj
├── Program.cs (entry point, MCP server setup)
├── Models/
│   ├── Venue.cs
│   ├── Vendor.cs
│   ├── Event.cs
│   ├── EventVendor.cs
│   ├── Communication.cs
│   ├── Timeline.cs
│   └── Budget.cs
├── Services/
│   ├── StorageService.cs (file persistence)
│   ├── EventIntakeService.cs
│   ├── VendorMatchingService.cs
│   ├── TimelineGenerationService.cs
│   ├── RiskPredictionService.cs
│   ├── BudgetTrackingService.cs
│   ├── ReportingService.cs
│   └── VendorManagementService.cs
├── Tools/
│   ├── IMcpTool.cs (interface)
│   ├── ExtractEventRequirementsTool.cs
│   ├── MatchVendorsToEventTool.cs
│   ├── GenerateEventTimelineTool.cs
│   ├── PredictVendorRisksTool.cs
│   ├── TrackEventBudgetTool.cs
│   ├── GeneratePostEventReportTool.cs
│   ├── AddVendorTool.cs
│   ├── ListVendorsTool.cs
│   └── UpdateVendorPerformanceTool.cs
├── Utils/
│   ├── VendorRankingCalculator.cs
│   ├── RiskCalculator.cs
│   ├── TimelineCalculator.cs
│   └── IdGenerator.cs
├── Data/ (created at runtime)
│   ├── venues.json
│   ├── vendors.json
│   ├── events.json
│   └── communications.json
└── appsettings.json
```

### Core Models

#### 1. Venue.cs
```csharp
public record Venue
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Name { get; init; }
    public string Address { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string Phone { get; init; }
    public int Capacity { get; init; }
    public List<string> PreferredVendorIds { get; init; } = new();
    public string SubscriptionTier { get; init; } = "starter"; // starter, growth, enterprise
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
```

#### 2. Vendor.cs
```csharp
public record Vendor
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string VenueId { get; init; }
    public string Name { get; init; }
    public string Category { get; init; } // catering, AV, florals, parking, etc.
    public string ContactEmail { get; init; }
    public string ContactPhone { get; init; }
    public string Website { get; init; } = "";
    public decimal CostPerUnit { get; init; } = 0;
    public int ReliabilityScore { get; init; } = 0; // 0-100
    public int TotalEvents { get; init; } = 0;
    public int OnTimeCount { get; init; } = 0;
    public decimal AverageQualityRating { get; init; } = 0; // 0-5
    public decimal OnTimePercentage { get; init; } = 0; // 0-100
    public string Notes { get; init; } = "";
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
```

#### 3. Event.cs
```csharp
public record Event
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string VenueId { get; init; }
    public string EventName { get; init; }
    public string EventType { get; init; } // gala, conference, wedding, corporate
    public DateTime EventDate { get; init; }
    public TimeOnly EventTime { get; init; }
    public int GuestCount { get; init; }
    public decimal BudgetTotal { get; init; }
    public string Description { get; init; }
    public Dictionary<string, object> SpecialRequirements { get; init; } = new();
    public string Status { get; init; } = "planning"; // planning, confirmed, in_progress, completed
    public string CreatedBy { get; init; }
    public List<TimelineItem> Timeline { get; init; } = new();
    public Dictionary<string, decimal> BudgetBreakdown { get; init; } = new();
    public List<string> AssignedVendorIds { get; init; } = new();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; init; }
}
```

#### 4. EventVendor.cs
```csharp
public record EventVendor
{
    public string EventId { get; init; }
    public string VendorId { get; init; }
    public string Role { get; init; } = "primary"; // primary, backup, alternate
    public string Status { get; init; } = "pending"; // pending, confirmed, declined, failed, completed
    public decimal QuoteAmount { get; init; } = 0;
    public DateTime? QuoteDate { get; init; }
    public decimal? ActualCost { get; init; }
    public DateTime? ConfirmedDate { get; init; }
    public TimeOnly? ArrivalTime { get; init; }
    public TimeOnly? CompletionTime { get; init; }
    public int QualityRating { get; init; } = 0; // 1-5
    public string Notes { get; init; } = "";
    public bool RiskFlag { get; init; } = false;
    public string RiskReason { get; init; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}
```

#### 5. Timeline.cs
```csharp
public record TimelineItem
{
    public string Task { get; init; }
    public double OffsetHoursFromEventStart { get; init; }
    public string Owner { get; init; } = "";
    public string Status { get; init; } = "pending"; // pending, in_progress, completed, delayed
    public DateTime? ScheduledTime { get; init; }
    public DateTime? ActualTime { get; init; }
    public string Notes { get; init; } = "";
}
```

#### 6. Communication.cs
```csharp
public record Communication
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string EventId { get; init; }
    public string VendorId { get; init; }
    public string Type { get; init; } = "email"; // email, sms, api, note
    public string Subject { get; init; } = "";
    public string Body { get; init; }
    public DateTime? SentAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime? ResponseReceivedAt { get; init; }
    public string ResponseContent { get; init; } = "";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
```

---

## MCP SERVER SPECIFICATION

### MCP Protocol Implementation

The server implements the Model Context Protocol (MCP) via stdio. Claude communicates with the server using JSON-RPC messages.

### Tool Definitions

#### Tool 1: extract_event_requirements

**Purpose:** Parse natural language conversation to extract event requirements

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "venue_id": {
      "type": "string",
      "description": "ID of the venue"
    },
    "conversation_text": {
      "type": "string",
      "description": "Natural language description of event needs"
    }
  },
  "required": ["venue_id", "conversation_text"]
}
```

**Output:**
```json
{
  "success": true,
  "event": {
    "id": "uuid",
    "event_name": "Corporate Gala",
    "event_type": "gala",
    "guest_count": 300,
    "budget_total": 15000,
    "event_date": "2024-12-15",
    "description": "..."
  },
  "summary": "📋 Event Summary: ...",
  "next_steps": [...]
}
```

**Implementation Logic:**
- Parse conversation text using simple heuristics (regex patterns for numbers, dates, event types)
- Extract: event name, type, date, guest count, budget, required vendors, special needs
- Create Event record in storage
- Generate summary for Claude to confirm

---

#### Tool 2: match_vendors_to_event

**Purpose:** Find and rank best vendors for an event

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "event_id": {
      "type": "string",
      "description": "ID of the event"
    },
    "vendor_categories": {
      "type": "array",
      "items": { "type": "string" },
      "description": "Categories: catering, AV, florals, parking, etc."
    }
  },
  "required": ["event_id", "vendor_categories"]
}
```

**Output:**
```json
{
  "success": true,
  "event_id": "uuid",
  "matches": {
    "catering": [
      {
        "id": "uuid",
        "name": "Premier Catering",
        "reliability_score": 92,
        "estimated_cost": 7500,
        "on_time_rate": 95,
        "match_score": 87,
        "recommendation": "PRIMARY"
      }
    ]
  },
  "summary": "✅ Vendor Matching Results: ..."
}
```

**Implementation Logic:**
1. Retrieve all vendors for venue in requested categories
2. For each vendor, calculate match score:
   - Reliability: 40% weight
   - Cost fit: 30% weight (quote vs. budget per guest)
   - Experience: 20% weight
   - Cost accuracy: 10% weight
3. Sort vendors by match score descending
4. Mark top vendor as PRIMARY, others as BACKUP
5. Return ranked list with scores

**Scoring Algorithm (VendorRankingCalculator):**
```
match_score = 
  (reliability_score × 0.4) +
  (cost_fit_percentage × 0.3) +
  (min(experience_count, 20) × 0.2) +
  (cost_accuracy × 0.1)

where:
  cost_fit = min(100, (budget_per_guest / vendor_cost_per_unit) × 100)
  experience_count = vendor.total_events
```

---

#### Tool 3: generate_event_timeline

**Purpose:** Auto-generate timeline milestones for an event

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "event_id": {
      "type": "string"
    },
    "event_type": {
      "type": "string",
      "description": "gala, conference, wedding, corporate, etc."
    },
    "event_date": {
      "type": "string",
      "description": "ISO format date"
    },
    "setup_hours_before": {
      "type": "number",
      "description": "Hours before event to begin setup"
    }
  },
  "required": ["event_id", "event_type", "event_date"]
}
```

**Output:**
```json
{
  "success": true,
  "event_id": "uuid",
  "timeline": [
    {
      "task": "Vendor Setup Begins",
      "offset_hours": -4,
      "time": "-4h"
    },
    {
      "task": "Guests Arrive",
      "offset_hours": 0,
      "time": "+0h"
    }
  ],
  "summary": "📅 Event Timeline Created: ..."
}
```

**Implementation Logic:**
1. Get event details from storage
2. Load event-type-specific timeline template from TimelineCalculator
3. Calculate absolute times based on event_date
4. Store timeline in Event record
5. Return formatted timeline with offset hours

**Timeline Templates (TimelineCalculator):**

Gala:
- -4h: Vendor Setup Begins
- -2h: Final Walkthrough
- 0h: Guests Arrive
- +0.5h: Cocktail Hour
- +1.5h: Dinner Service
- +3h: Dancing/Entertainment
- +4h: Event Ends
- +5h: Breakdown Complete

Conference:
- -3h: AV Setup & Test
- -2h: Vendor Setup Complete
- 0h: Doors Open
- +0.5h: Breakfast Service
- +1h: Sessions Begin
- +4h: Lunch Service
- +8h: Closing Ceremony
- +9h: Breakdown

Wedding:
- -5h: Vendor Setup & Decorations
- -3h: Catering Setup
- -1h: Guest Arrivals Begin
- 0h: Ceremony Begins
- +1h: Cocktail Hour
- +1.5h: Dinner Service
- +3h: Toasts & Dancing
- +5h: Reception Ends

---

#### Tool 4: predict_vendor_risks

**Purpose:** Analyze vendor history to predict delays/failures

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "event_id": {
      "type": "string"
    },
    "vendor_id": {
      "type": "string"
    }
  },
  "required": ["event_id", "vendor_id"]
}
```

**Output:**
```json
{
  "success": true,
  "event_id": "uuid",
  "vendor_id": "uuid",
  "vendor_name": "Caterer Name",
  "risk_analysis": {
    "overall_risk_score": 25,
    "risk_level": "LOW",
    "factors": {
      "reliability": {
        "score": 92,
        "assessment": "Excellent track record"
      },
      "timing": {
        "days_until_event": 14,
        "last_minute_risk": "LOW"
      }
    }
  },
  "recommendation": {
    "action": "Proceed normally",
    "reasoning": "This vendor has strong track record. Low risk of issues."
  }
}
```

**Implementation Logic (RiskCalculator):**
1. Get vendor history from storage
2. Calculate risk factors:
   - Reliability Risk = 100 - reliability_score
   - Last Minute Risk = if days < 7: (50 - days × 5) else 0
   - Experience Risk = if total_events < 5: 20 else 0
3. Overall Risk Score = (reliability_risk × 0.4) + (last_minute × 0.3) + (experience × 0.3)
4. Categorize: HIGH (>70), MEDIUM (40-70), LOW (<40)
5. Generate recommendation

---

#### Tool 5: track_event_budget

**Purpose:** Track budget, flag overruns, suggest optimizations

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "event_id": {
      "type": "string"
    },
    "action": {
      "type": "string",
      "enum": ["get_summary", "add_cost", "flag_overrun", "get_optimization_suggestions"]
    },
    "category": {
      "type": "string",
      "description": "catering, AV, florals, etc."
    },
    "amount": {
      "type": "number",
      "description": "Cost amount if action is add_cost"
    }
  },
  "required": ["event_id", "action"]
}
```

**Output:**
```json
{
  "success": true,
  "event_id": "uuid",
  "budget_total": 15000,
  "actual_spent": 14250,
  "remaining": 750,
  "variance": -750,
  "variance_percent": -5.0,
  "breakdown": {
    "catering": 8500,
    "AV": 2900,
    "florals": 2850
  },
  "status": "ON TRACK"
}
```

**Implementation Logic:**
- **get_summary**: Sum budget_breakdown, calculate variance, check status
- **add_cost**: Add/update category cost, recalculate summary
- **flag_overrun**: If variance > 5%, return overrun details with recommendations
- **get_optimization_suggestions**: Analyze costs relative to event size, suggest reductions

---

#### Tool 6: generate_post_event_report

**Purpose:** Create comprehensive report after event completion

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "event_id": {
      "type": "string"
    },
    "completed_at": {
      "type": "string",
      "description": "ISO format timestamp"
    }
  },
  "required": ["event_id"]
}
```

**Output:**
```json
{
  "success": true,
  "event_id": "uuid",
  "report": {
    "event_summary": {
      "name": "Corporate Gala",
      "type": "gala",
      "date": "2024-12-15",
      "guests": 300,
      "completed_at": "2024-12-15T23:00:00Z"
    },
    "financial_summary": {
      "budgeted": 15000,
      "actual": 14850,
      "variance": -150,
      "variance_percent": -1.0
    },
    "lessons_learned": [
      "For future galas with 300 guests:",
      "- Book vendors 6+ weeks in advance",
      "- Set budget buffer of 10%",
      "- Coordinate timeline walkthrough"
    ],
    "recommendations": {
      "vendor_selection": "Review and rate vendors for future reference",
      "timeline": "Update timeline templates based on experience",
      "budget": "Analyze variances to improve budgeting"
    }
  },
  "summary": "📊 Post-Event Report: Corporate Gala..."
}
```

**Implementation Logic:**
1. Get event, timeline, budget, and communications from storage
2. Build financial summary
3. Extract lessons learned (hard-coded templates + Claude API optional)
4. Generate recommendations
5. Update event status to "completed"
6. Return formatted report

---

#### Tool 7: add_vendor

**Purpose:** Add new vendor to venue database

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "venue_id": {
      "type": "string"
    },
    "vendor_name": {
      "type": "string"
    },
    "category": {
      "type": "string",
      "description": "catering, AV, florals, parking, etc."
    },
    "contact_email": {
      "type": "string"
    },
    "contact_phone": {
      "type": "string"
    },
    "cost_per_unit": {
      "type": "number"
    }
  },
  "required": ["venue_id", "vendor_name", "category", "contact_email"]
}
```

**Output:**
```json
{
  "success": true,
  "message": "Added Premier Catering to your vendor database",
  "vendor": {
    "id": "uuid",
    "name": "Premier Catering",
    "category": "catering",
    "contact_email": "info@premier.com"
  }
}
```

**Implementation Logic:**
1. Validate inputs (email format, category exists)
2. Create Vendor object with default scores
3. Save to storage
4. Return confirmation

---

#### Tool 8: list_vendors

**Purpose:** List vendors for a venue

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "venue_id": {
      "type": "string"
    },
    "category": {
      "type": "string"
    }
  },
  "required": ["venue_id"]
}
```

**Output:**
```json
{
  "success": true,
  "venue_id": "uuid",
  "category": "catering",
  "vendors": [
    {
      "id": "uuid",
      "name": "Premier Catering",
      "category": "catering",
      "reliability_score": 92,
      "contact": "info@premier.com",
      "cost_per_unit": 25
    }
  ],
  "total": 1
}
```

**Implementation Logic:**
1. Get venue from storage
2. Filter vendors by category if provided
3. Return list with key fields

---

#### Tool 9: update_vendor_performance

**Purpose:** Update vendor performance after event

**Input Schema:**
```json
{
  "type": "object",
  "properties": {
    "vendor_id": {
      "type": "string"
    },
    "event_id": {
      "type": "string"
    },
    "on_time": {
      "type": "boolean"
    },
    "quality_rating": {
      "type": "number",
      "minimum": 1,
      "maximum": 5
    },
    "cost_accurate": {
      "type": "boolean"
    },
    "notes": {
      "type": "string"
    }
  },
  "required": ["vendor_id", "event_id"]
}
```

**Output:**
```json
{
  "success": true,
  "message": "Updated performance for Premier Catering",
  "updated_metrics": {
    "total_events": 46,
    "on_time_count": 44,
    "on_time_percentage": 95.7,
    "avg_quality_rating": 4.7,
    "reliability_score": 92
  }
}
```

**Implementation Logic:**
1. Get vendor from storage
2. Update metrics:
   - total_events += 1
   - on_time_count += 1 if on_time
   - Calculate on_time_percentage
   - Update quality_rating
3. Recalculate reliability_score
4. Save to storage
5. Return updated metrics

**Reliability Score Calculation:**
```
reliability_score = 
  (on_time_percentage × 0.4) +
  (quality_rating / 5 × 100 × 0.4) +
  (consistency_bonus × 0.2)

where:
  consistency_bonus = min(10, total_events / 5)
```

---

## SERVICE IMPLEMENTATIONS

### 1. StorageService.cs

**Responsibilities:**
- Load/save JSON files
- CRUD operations for all entities
- Thread-safe file operations
- File locking for concurrent access

**Key Methods:**
```csharp
public class StorageService
{
    private string _dataDirectory;
    private Dictionary<string, Venue> _venues;
    private Dictionary<string, Dictionary<string, Vendor>> _vendors; // [venueId][vendorId]
    private Dictionary<string, Event> _events;
    private List<Communication> _communications;
    private readonly object _lockObject = new();

    public void Load();
    public void Save();
    
    public void AddVenue(Venue venue);
    public Venue GetVenue(string venueId);
    public IEnumerable<Venue> GetAllVenues();
    
    public void AddVendor(string venueId, Vendor vendor);
    public Vendor GetVendor(string venueId, string vendorId);
    public IEnumerable<Vendor> GetVendorsByCategory(string venueId, string category);
    public void UpdateVendor(string venueId, string vendorId, Action<Vendor> update);
    
    public void AddEvent(Event eventRecord);
    public Event GetEvent(string eventId);
    public IEnumerable<Event> GetEventsByVenue(string venueId);
    public void UpdateEvent(string eventId, Action<Event> update);
    
    public void AddCommunication(Communication communication);
    public IEnumerable<Communication> GetCommunications(string eventId);
}
```

### 2. EventIntakeService.cs

**Responsibilities:**
- Parse conversation text
- Extract event requirements
- Create Event records

**Key Methods:**
```csharp
public class EventIntakeService
{
    private readonly StorageService _storage;
    
    public EventIntakeResult ExtractFromConversation(string venueId, string conversationText);
    public Event CreateEvent(string venueId, EventRequirements requirements);
    private EventRequirements ParseConversation(string text);
    private string GenerateSummary(Event eventRecord);
}

public record EventRequirements
{
    public string EventName { get; init; }
    public string EventType { get; init; }
    public DateTime EventDate { get; init; }
    public int GuestCount { get; init; }
    public decimal BudgetTotal { get; init; }
    public List<string> RequiredVendorCategories { get; init; }
    public string Description { get; init; }
}
```

**Parsing Logic:**
- Use regex patterns to extract numbers, dates, vendor types
- Match event types: gala, conference, wedding, corporate, party, summit
- Extract budget amounts (look for $ and numbers)
- Extract dates (look for month names + days)
- Extract guest counts

### 3. VendorMatchingService.cs

**Responsibilities:**
- Rank vendors for event
- Calculate match scores

**Key Methods:**
```csharp
public class VendorMatchingService
{
    private readonly StorageService _storage;
    private readonly VendorRankingCalculator _ranker;
    
    public VendorMatches MatchVendors(string eventId, List<string> vendorCategories);
    private List<VendorMatch> RankVendors(List<Vendor> vendors, Event eventRecord);
    private string GenerateMatchSummary(VendorMatches matches, Event eventRecord);
}

public record VendorMatch
{
    public string Id { get; init; }
    public string Name { get; init; }
    public int ReliabilityScore { get; init; }
    public decimal EstimatedCost { get; init; }
    public decimal OnTimeRate { get; init; }
    public double MatchScore { get; init; }
    public string Recommendation { get; init; } // PRIMARY, BACKUP
}
```

### 4. TimelineGenerationService.cs

**Responsibilities:**
- Create event timelines
- Manage timeline items

**Key Methods:**
```csharp
public class TimelineGenerationService
{
    private readonly StorageService _storage;
    private readonly TimelineCalculator _calculator;
    
    public TimelineResult GenerateTimeline(string eventId, string eventType, 
        DateTime eventDate, int setupHoursBefore = 4);
    private List<TimelineItem> CreateTimelineFromTemplate(string eventType, 
        DateTime eventDate, int setupHours);
    private string GenerateTimelineSummary(List<TimelineItem> timeline);
}
```

### 5. RiskPredictionService.cs

**Responsibilities:**
- Analyze vendor risk
- Generate recommendations

**Key Methods:**
```csharp
public class RiskPredictionService
{
    private readonly StorageService _storage;
    private readonly RiskCalculator _calculator;
    
    public RiskAnalysis PredictRisks(string eventId, string vendorId);
    private RiskAnalysis AnalyzeVendorRisk(Vendor vendor, Event eventRecord);
    private string GenerateRecommendation(RiskAnalysis analysis);
}

public record RiskAnalysis
{
    public int OverallRiskScore { get; init; } // 0-100
    public string RiskLevel { get; init; } // HIGH, MEDIUM, LOW
    public Dictionary<string, RiskFactor> Factors { get; init; }
}
```

### 6. BudgetTrackingService.cs

**Responsibilities:**
- Track and manage budgets
- Detect overruns
- Suggest optimizations

**Key Methods:**
```csharp
public class BudgetTrackingService
{
    private readonly StorageService _storage;
    
    public BudgetSummary GetBudgetSummary(string eventId);
    public void AddCost(string eventId, string category, decimal amount);
    public BudgetOverrunAlert CheckForOverrun(string eventId);
    public List<OptimizationSuggestion> GetOptimizations(string eventId);
}

public record BudgetSummary
{
    public decimal BudgetTotal { get; init; }
    public decimal ActualSpent { get; init; }
    public decimal Remaining { get; init; }
    public decimal Variance { get; init; }
    public decimal VariancePercent { get; init; }
    public Dictionary<string, decimal> Breakdown { get; init; }
    public string Status { get; init; } // ON TRACK, OVER BUDGET
}
```

### 7. ReportingService.cs

**Responsibilities:**
- Generate post-event reports
- Extract lessons learned

**Key Methods:**
```csharp
public class ReportingService
{
    private readonly StorageService _storage;
    
    public PostEventReport GenerateReport(string eventId, DateTime completedAt);
    private List<string> ExtractLessonsLearned(Event eventRecord);
    private Dictionary<string, string> GenerateRecommendations(Event eventRecord);
    private string GenerateReportSummary(PostEventReport report);
}

public record PostEventReport
{
    public EventSummary EventSummary { get; init; }
    public FinancialSummary FinancialSummary { get; init; }
    public List<string> LessonsLearned { get; init; }
    public Dictionary<string, string> Recommendations { get; init; }
}
```

### 8. VendorManagementService.cs

**Responsibilities:**
- CRUD operations for vendors

**Key Methods:**
```csharp
public class VendorManagementService
{
    private readonly StorageService _storage;
    
    public Vendor AddVendor(string venueId, VendorCreationRequest request);
    public IEnumerable<Vendor> ListVendors(string venueId, string category = null);
    public void UpdatePerformance(string vendorId, string eventId, 
        PerformanceUpdate update);
}
```

---

## MCP SERVER SETUP (Program.cs)

```csharp
// Pseudocode structure for Program.cs
public class Program
{
    public static async Task Main(string[] args)
    {
        // Initialize services
        var storage = new StorageService("./data");
        var eventIntake = new EventIntakeService(storage);
        var vendorMatching = new VendorMatchingService(storage);
        var timeline = new TimelineGenerationService(storage);
        var risk = new RiskPredictionService(storage);
        var budget = new BudgetTrackingService(storage);
        var reporting = new ReportingService(storage);
        var vendorMgmt = new VendorManagementService(storage);

        // Initialize MCP server
        var mcpServer = new McpServer();
        
        // Register tools
        mcpServer.RegisterTool("extract_event_requirements", 
            inputSchema, (args) => HandleExtractEventRequirements(args, eventIntake));
        mcpServer.RegisterTool("match_vendors_to_event", 
            inputSchema, (args) => HandleMatchVendors(args, vendorMatching));
        // ... register remaining tools
        
        // Start server on stdio
        await mcpServer.StartAsync();
    }
}
```

---

## KEY IMPLEMENTATION DETAILS

### 1. JSON Serialization

Use `System.Text.Json` with custom converters for DateTime handling:

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    WriteIndented = true,
    Converters = {
        new JsonStringEnumConverter(),
        new DateTimeConverter(),
        new TimeOnlyConverter()
    }
};
```

### 2. File-Based Persistence

Store data in JSON files in `./data/` directory:
- `venues.json` - Array of Venue objects
- `vendors.json` - Dictionary [venueId][vendorId]
- `events.json` - Dictionary [eventId]
- `communications.json` - Array of Communication objects

Load on startup, save after each modification (within lock).

### 3. ID Generation

Use `Guid.NewGuid().ToString()` for all IDs. Deterministic not needed since file-based.

### 4. Error Handling

All tool handlers should return standard response:
```csharp
public record ToolResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public object Data { get; init; }
    public string Error { get; init; } = "";
}
```

### 5. Async/Await

Use async all the way:
- File I/O: `File.ReadAllTextAsync`, `File.WriteAllTextAsync`
- Tool handlers: async Tasks
- MCP message processing: async

---

## TESTING REQUIREMENTS

### Unit Tests (xUnit)

```
Tests/
├── Services/
│   ├── EventIntakeServiceTests.cs
│   ├── VendorMatchingServiceTests.cs
│   ├── TimelineGenerationServiceTests.cs
│   ├── RiskPredictionServiceTests.cs
│   ├── BudgetTrackingServiceTests.cs
│   └── ReportingServiceTests.cs
├── Utils/
│   ├── VendorRankingCalculatorTests.cs
│   ├── RiskCalculatorTests.cs
│   └── TimelineCalculatorTests.cs
└── Integration/
    ├── StorageServiceTests.cs
    └── EndToEndTests.cs
```

**Critical Test Cases:**

1. **EventIntakeService:**
   - Parse conversation with guests, date, budget
   - Extract event type correctly
   - Handle missing information

2. **VendorMatchingService:**
   - Rank vendors by match score
   - Filter by category
   - Mark primary vs. backup

3. **RiskPredictionService:**
   - Calculate risk scores correctly
   - Identify HIGH/MEDIUM/LOW risk
   - Account for days until event

4. **BudgetTrackingService:**
   - Add costs and track total
   - Calculate variance correctly
   - Detect overruns

5. **StorageService:**
   - Load/save JSON files
   - Thread-safe CRUD operations
   - Handle concurrent access

### Integration Tests

Test complete workflows:
1. Create venue → Add vendors → Create event → Match vendors → Generate timeline
2. Create event → Add costs → Flag overrun → Get optimizations
3. Complete event → Generate report → Extract lessons

---

## DEPLOYMENT

### Build & Run

```bash
# Build
dotnet build

# Run
dotnet run

# Or as executable
dotnet publish -c Release -r win-x64
./VenueAssistant.MCP.exe
```

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./VenueAssistant.MCP"]
```

---

## LOGGING & DEBUGGING

Enable logging via `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "VenueAssistant": "Debug"
    }
  }
}
```

Log to:
- Console (development)
- `logs/venue-assistant.log` (production)

---

## SUCCESS CRITERIA

- ✅ All 9 MCP tools implemented and callable
- ✅ Claude can coordinate complete events via conversation
- ✅ Data persists correctly between sessions
- ✅ All unit tests passing (>90% coverage)
- ✅ Can run on Windows, Linux, macOS
- ✅ Deployable as single executable
- ✅ Response times <500ms for all operations
- ✅ Handles 100+ venues and 1000+ events without performance degradation

---

## TIMELINE

**Week 1:** Models, StorageService, basic tools (extract_event_requirements, match_vendors)
**Week 2:** Remaining tools (timeline, risk, budget, reporting), VendorManagementService
**Week 3:** MCP server setup, tool registration, integration, testing
**Week 4:** Deployment, documentation, final testing with Claude

---

## DELIVERABLES

1. **Source Code** - Complete C# project with all services and tools
2. **Unit Tests** - >90% code coverage with xUnit tests
3. **Documentation** - README, API docs, deployment guide
4. **Executables** - Self-contained binaries for Windows, Linux, macOS
5. **Demo Data** - Sample venues/vendors for testing
6. **System Prompt** - Claude prompt for interacting with MCP server

---

## APPENDIX: MCP Protocol Basics

### Tool Call (Claude → Server)
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "extract_event_requirements",
    "arguments": {
      "venue_id": "abc123",
      "conversation_text": "I need to coordinate a 300-person gala..."
    }
  }
}
```

### Tool Response (Server → Claude)
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [
      {
        "type": "text",
        "text": "{\"success\": true, \"event\": {...}}"
      }
    ]
  }
}
```

### Tool List Response
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "tools": [
      {
        "name": "extract_event_requirements",
        "description": "...",
        "inputSchema": {
          "type": "object",
          "properties": {...},
          "required": [...]
        }
      }
    ]
  }
}
```

---

## NOTES FOR CODEX

1. **Strong Typing:** Use records and explicit types. Avoid dynamic objects.
2. **Error Handling:** Validate all inputs. Return clear error messages.
3. **Concurrency:** Use locks in StorageService for thread safety.
4. **JSON:** Use System.Text.Json with snake_case naming policy.
5. **Logging:** Inject ILogger into services, log important operations.
6. **DI:** Use dependency injection for all services.
7. **Async:** Use async/await throughout, no blocking calls.
8. **Testing:** Write tests before implementation where possible.
9. **Documentation:** Add XML comments to all public methods.
10. **MCP Protocol:** Implement stdio-based JSON-RPC communication.