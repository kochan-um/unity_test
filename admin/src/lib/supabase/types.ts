export type Json =
  | string
  | number
  | boolean
  | null
  | { [key: string]: Json | undefined }
  | Json[];

export type Database = {
  public: {
    Tables: {
      items: {
        Row: {
          id: string;
          name: string;
          description: string | null;
          category: string | null;
          rarity: string | null;
          stackable: boolean;
          max_stack: number | null;
          icon_url: string | null;
          is_deleted: boolean | null;
          created_at: string | null;
          updated_at: string | null;
        };
        Insert: {
          id?: string;
          name: string;
          description?: string | null;
          category?: string | null;
          rarity?: string | null;
          stackable?: boolean;
          max_stack?: number | null;
          icon_url?: string | null;
          is_deleted?: boolean | null;
          created_at?: string | null;
          updated_at?: string | null;
        };
        Update: {
          name?: string;
          description?: string | null;
          category?: string | null;
          rarity?: string | null;
          stackable?: boolean;
          max_stack?: number | null;
          icon_url?: string | null;
          is_deleted?: boolean | null;
          updated_at?: string | null;
        };
        Relationships: [];
      };
      player_profiles: {
        Row: {
          id: number;
          user_id: string;
          display_name: string;
          score: number | null;
          level: number | null;
          created_at: string | null;
          updated_at: string | null;
        };
        Insert: {
          id?: number;
          user_id: string;
          display_name: string;
          score?: number | null;
          level?: number | null;
          created_at?: string | null;
          updated_at?: string | null;
        };
        Update: {
          display_name?: string;
          score?: number | null;
          level?: number | null;
          updated_at?: string | null;
        };
        Relationships: [];
      };
      inventory_items: {
        Row: {
          user_id: string;
          slot_index: number;
          item_id: string;
          quantity: number;
        };
        Insert: {
          user_id: string;
          slot_index: number;
          item_id: string;
          quantity: number;
        };
        Update: {
          slot_index?: number;
          item_id?: string;
          quantity?: number;
        };
        Relationships: [];
      };
      chat_sessions: {
        Row: {
          id: string;
          admin_user_id: string;
          title: string | null;
          created_at: string;
          updated_at: string;
        };
        Insert: {
          id?: string;
          admin_user_id: string;
          title?: string | null;
          created_at?: string;
          updated_at?: string;
        };
        Update: {
          title?: string | null;
          updated_at?: string;
        };
        Relationships: [];
      };
      chat_messages: {
        Row: {
          id: string;
          session_id: string;
          role: "user" | "assistant";
          content: string;
          images: Json | null;
          tool_calls: Json | null;
          tool_results: Json | null;
          created_at: string;
        };
        Insert: {
          id?: string;
          session_id: string;
          role: "user" | "assistant";
          content: string;
          images?: Json | null;
          tool_calls?: Json | null;
          tool_results?: Json | null;
          created_at?: string;
        };
        Update: {
          content?: string;
          tool_calls?: Json | null;
          tool_results?: Json | null;
        };
        Relationships: [];
      };
      admin_rate_limits: {
        Row: {
          admin_user_id: string;
          date: string;
          token_count: number;
        };
        Insert: {
          admin_user_id: string;
          date: string;
          token_count: number;
        };
        Update: {
          token_count?: number;
        };
        Relationships: [];
      };
    };
    Views: Record<string, never>;
    Functions: Record<string, never>;
    Enums: Record<string, never>;
    CompositeTypes: Record<string, never>;
  };
};
