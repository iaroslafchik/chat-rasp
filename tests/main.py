#!/usr/bin/env python3
"""
Test script for vLLM + MCP tool calling using OpenAI Chat Completions API.

Assumptions:
- vLLM is running at http://localhost:8000
- vLLM was started with: --enable-auto-tool-choice --tool-call-parser lfm2
  --tool-server http://omgtu-schedule-tools-server:8080/app
- The MCP server is running and exposes the tools described below.
"""

import json
from openai import OpenAI

# ----------------------------------------------------------------------
# 1. Configure the OpenAI client (pointing to vLLM)
# ----------------------------------------------------------------------
client = OpenAI(
    base_url="http://localhost:8000/v1",
    api_key="EMPTY",  # vLLM doesn't require an API key
)

# ----------------------------------------------------------------------
# 2. Define the tools in OpenAI Chat Completions format
#    (each tool must have a "function" wrapper)
# ----------------------------------------------------------------------
tools = [
    {
        "type": "function",
        "function": {
            "name": "Search",
            "description": "Search for schedule entities (group, person, auditorium) to get their IDs.",
            "parameters": {
                "type": "object",
                "properties": {
                    "term": {"type": "string", "description": "Search query (e.g., group name, teacher last name)"},
                    "type": {"type": "string", "enum": ["person", "auditorium", "student", "group"], "description": "Type of entity to search"}
                },
                "required": ["term", "type"],
                "additionalProperties": False,
            },
            "strict": True,
        }
    },
    {
        "type": "function",
        "function": {
            "name": "GetGroupSchedule",
            "description": "Get schedule for a study group as CSV.",
            "parameters": {
                "type": "object",
                "properties": {
                    "id": {"type": "integer", "description": "Group identifier (obtain from Search tool)"},
                    "start": {"type": "string", "description": "Start date in YYYY.MM.DD format"},
                    "finish": {"type": "string", "description": "End date in YYYY.MM.DD format"},
                    "lng": {"type": "integer", "description": "Language: 1 = Russian", "default": 1}
                },
                "required": ["id", "start", "finish"],
                "additionalProperties": False,
            },
            "strict": True,
        }
    },
    {
        "type": "function",
        "function": {
            "name": "GetPersonSchedule",
            "description": "Get schedule for a teacher as CSV.",
            "parameters": {
                "type": "object",
                "properties": {
                    "id": {"type": "integer", "description": "Teacher identifier (from Search tool)"},
                    "start": {"type": "string", "description": "Start date in YYYY.MM.DD format"},
                    "finish": {"type": "string", "description": "End date in YYYY.MM.DD format"},
                    "lng": {"type": "integer", "description": "Language: 1 = Russian", "default": 1}
                },
                "required": ["id", "start", "finish"],
                "additionalProperties": False,
            },
            "strict": True,
        }
    },
    {
        "type": "function",
        "function": {
            "name": "GetAuditoriumSchedule",
            "description": "Get schedule for a classroom as CSV.",
            "parameters": {
                "type": "object",
                "properties": {
                    "id": {"type": "integer", "description": "Auditorium identifier (from Search tool)"},
                    "start": {"type": "string", "description": "Start date in YYYY.MM.DD format"},
                    "finish": {"type": "string", "description": "End date in YYYY.MM.DD format"},
                    "lng": {"type": "integer", "description": "Language: 1 = Russian", "default": 1}
                },
                "required": ["id", "start", "finish"],
                "additionalProperties": False,
            },
            "strict": True,
        }
    },
    {
        "type": "function",
        "function": {
            "name": "GetGroupScheduleIcs",
            "description": "Get group schedule as ICS calendar file.",
            "parameters": {
                "type": "object",
                "properties": {
                    "id": {"type": "integer", "description": "Group identifier"},
                    "start": {"type": "string", "description": "Start date in YYYY.MM.DD"},
                    "finish": {"type": "string", "description": "End date in YYYY.MM.DD"},
                    "lng": {"type": "integer", "description": "Language: 1 = Russian", "default": 1}
                },
                "required": ["id", "start", "finish"],
                "additionalProperties": False,
            },
            "strict": True,
        }
    }
]

# ----------------------------------------------------------------------
# 3. Send a user query that requires tool use (Chat Completions API)
# ----------------------------------------------------------------------
messages = [
    {
        "role": "user",
        "content": "Найди id для группы ПЭ-231н"
    }
]

print("📤 Sending request to vLLM using Chat Completions API...")

try:
    response = client.chat.completions.create(
        model="LiquidAI/LFM2.5-1.2B-Thinking",
        messages=messages,
        tools=tools,
        tool_choice="auto",
        max_tokens=1024,
    )

    print("✅ Full response object:")
    print(response.model_dump_json(indent=2))

    # Extract and display tool calls if any
    message = response.choices[0].message
    if message.tool_calls:
        print("\n🔧 Tool calls detected:")
        for tc in message.tool_calls:
            print(f"  - {tc.function.name}({tc.function.arguments})")
    else:
        # For reasoning models, content may be None – check reasoning_content
        content = message.content
        if content is None and hasattr(message, 'reasoning_content'):
            content = message.reasoning_content
        print(f"\n💬 Model response (no tool calls): {content}")

except Exception as e:
    print(f"❌ Error: {e}")
    print("\n💡 Troubleshooting tips:")
    print("  - Ensure vLLM is running with --enable-auto-tool-choice and correct --tool-call-parser")
    print("  - Verify that your vLLM version supports tool calling (use vLLM >= 0.6.0)")
    print("  - If using a custom parser plugin, ensure it's compatible with the model")
