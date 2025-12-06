# Venue Assistant MCP Server - Claude Desktop Demo

This guide provides a step-by-step script for demonstrating the Venue Assistant MCP Server using **Claude Desktop**.

## Prerequisite: Configure Claude Desktop

Before the demo, ensure your `claude_desktop_config.json` (located in `%APPDATA%\Claude\` on Windows) includes your server:

```json
{
  "mcpServers": {
    "venue-assistant": {
      "url": "http://localhost:5001/mcp"
    }
  }
}
```
*Note: Make sure to restart Claude Desktop after saving this config.*

**IMPORTANT:** Because we are using HTTP/SSE, **you must start the server manually** before opening Claude Desktop.
1. Run `run_demo.bat` (or `dotnet run` in the `VenueAssistant.MCP` folder).
2. Wait for the server to start (it will say listening on `http://localhost:5001`).
3. Open Claude Desktop.

---

## The Demo Script

**Scenario:** You are planning the "Grand Wedding" at the Grand Pavilion.

### Step 1: Ingesting the Event
**Goal:** Show Claude understanding complex natural language and using the `extract_event_requirements` tool.

**👉 Prompt to Copy:**
> I am planning a wedding for 150 guests at the Grand Pavilion on June 15th, 2024. We need high-end catering with a budget of around $50 per person. We also need a live band and floral arrangements. The theme is 'Summer Garden'. Please extract these requirements for me.

**👀 What to Watch:**
Claude should call `extract_event_requirements` and confirm the details (Venue: Grand Pavilion, Budget: $50, etc.).

---

### Step 2: Finding Vendors
**Goal:** Use the `match_vendors_to_event` tool to find the best match.

**👉 Prompt to Copy:**
> Great. Based on those requirements, can you find a suitable catering vendor for this event?

**👀 What to Watch:**
Claude should call `match_vendors_to_event` with the venue ID and catering category. It should recommend a vendor like "Premier Catering".

---

### Step 3: Risk Assessment
**Goal:** Use the `predict_vendor_risks` tool to show "Agentic" reasoning.

**👉 Prompt to Copy:**
> I'm thinking of going with Premier Catering. Can you check if there are any known risks with them for this date?

**👀 What to Watch:**
Claude should call `predict_vendor_risks`. It might say something like "Risk Score is low" or highlight past reliability.

---

### Step 4: Timeline Generation
**Goal:** Use the `generate_event_timeline` tool.

**👉 Prompt to Copy:**
> Let's move forward. Please generate a run-of-show timeline for the wedding. It starts at 2:00 PM and ends at 10:00 PM. Include catering, music, and photography services.

**👀 What to Watch:**
Claude should call `generate_event_timeline` and output a formatted schedule (e.g., "14:00 - Guest Arrival", "18:00 - Dinner Service").

---

### Step 5: Budget Tracking
**Goal:** Use the `track_event_budget` tool.

**👉 Prompt to Copy:**
> I've just signed the contracts. I'm spending $6,000 on catering and $2,000 on the venue. My total budget is $10,000. Can you track this and tell me how much I have left?

**👀 What to Watch:**
Claude should call `track_event_budget` and calculate the remaining budget ($2,000).

---

### Step 6: Post-Event Reporting
**Goal:** Use the `generate_post_event_report` tool to close the loop.

**👉 Prompt to Copy:**
> Fast forward to after the event. It went well, but the florist was late. We had 145 attendees. Please generate a post-event report.

**👀 What to Watch:**
Claude should call `generate_post_event_report`, logging the attendance and the issue with the florist.
