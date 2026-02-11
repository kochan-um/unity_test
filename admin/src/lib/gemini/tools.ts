import { Tool, SchemaType } from "@google/generative-ai";

export const TOOLS: Tool[] = [
  {
    functionDeclarations: [
      {
        name: "search_items",
        description:
          "Search for game items by query, category, or rarity. Returns a list of matching items.",
        parameters: {
          type: SchemaType.OBJECT,
          properties: {
            query: {
              type: SchemaType.STRING,
              description:
                "Search query for item name or description (e.g., 'sword', 'fire')",
            },
            category: {
              type: SchemaType.STRING,
              description:
                "Filter by category (e.g., 'weapon', 'armor', 'potion', 'quest')",
            },
            rarity: {
              type: SchemaType.STRING,
              description:
                "Filter by rarity level (e.g., 'common', 'uncommon', 'rare', 'epic', 'legendary')",
            },
          },
          required: ["query"],
        },
      },
      {
        name: "get_item_detail",
        description: "Get detailed information about a specific item by ID",
        parameters: {
          type: SchemaType.OBJECT,
          properties: {
            item_id: {
              type: SchemaType.STRING,
              description: "The unique identifier of the item",
            },
          },
          required: ["item_id"],
        },
      },
      {
        name: "search_players",
        description:
          "Search for players by username, email, or other criteria. Returns player profiles.",
        parameters: {
          type: SchemaType.OBJECT,
          properties: {
            query: {
              type: SchemaType.STRING,
              description: "Search query for player name or identifier",
            },
          },
          required: ["query"],
        },
      },
      {
        name: "get_player_detail",
        description:
          "Get detailed profile information for a specific player including stats",
        parameters: {
          type: SchemaType.OBJECT,
          properties: {
            player_id: {
              type: SchemaType.STRING,
              description: "The player's user ID or player profile ID",
            },
          },
          required: ["player_id"],
        },
      },
      {
        name: "get_player_inventory",
        description:
          "Get the inventory contents for a specific player with item details",
        parameters: {
          type: SchemaType.OBJECT,
          properties: {
            player_id: {
              type: SchemaType.STRING,
              description: "The player's user ID",
            },
          },
          required: ["player_id"],
        },
      },
      {
        name: "get_dashboard_stats",
        description: "Get overall game statistics and metrics",
        parameters: {
          type: SchemaType.OBJECT,
          properties: {},
          required: [],
        },
      },
    ],
  },
];
