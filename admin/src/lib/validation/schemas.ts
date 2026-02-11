import { z } from "zod";

const positiveInt = z.coerce.number().int().min(1);
const limitedInt = positiveInt.max(100);

export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
});

export const listItemsQuerySchema = z.object({
  page: positiveInt.default(1),
  limit: limitedInt.default(20),
  category: z.string().min(1).optional(),
  rarity: z.string().min(1).optional(),
  search: z.string().min(1).optional(),
  sort: z.enum(["created_at", "name"]).optional(),
  order: z.enum(["asc", "desc"]).optional(),
});

export const createItemSchema = z
  .object({
    name: z.string().min(1).max(120),
    description: z.string().max(1000).optional().nullable(),
    category: z.string().min(1).max(60).optional().nullable(),
    rarity: z.string().min(1).max(60).optional().nullable(),
    stackable: z.boolean().default(false),
    maxStack: z.coerce.number().int().min(1).max(999).optional(),
    iconUrl: z.string().url().optional().nullable(),
  })
  .superRefine((value, ctx) => {
    if (value.stackable && !value.maxStack) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["maxStack"],
        message: "maxStack is required when stackable is true.",
      });
    }

    if (!value.stackable && value.maxStack && value.maxStack !== 1) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["maxStack"],
        message: "maxStack must be 1 when stackable is false.",
      });
    }
  });

export const updateItemSchema = z
  .object({
    name: z.string().min(1).max(120).optional(),
    description: z.string().max(1000).optional().nullable(),
    category: z.string().min(1).max(60).optional().nullable(),
    rarity: z.string().min(1).max(60).optional().nullable(),
    stackable: z.boolean().optional(),
    maxStack: z.coerce.number().int().min(1).max(999).optional(),
    iconUrl: z.string().url().optional().nullable(),
  })
  .superRefine((value, ctx) => {
    if (value.stackable === true && !value.maxStack) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["maxStack"],
        message: "maxStack is required when stackable is true.",
      });
    }

    if (value.stackable === false && value.maxStack && value.maxStack !== 1) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["maxStack"],
        message: "maxStack must be 1 when stackable is false.",
      });
    }
  });

export const listPlayersQuerySchema = z.object({
  page: positiveInt.default(1),
  limit: limitedInt.default(20),
  search: z.string().min(1).optional(),
});

export const updatePlayerSchema = z.object({
  displayName: z.string().min(1).max(120).optional(),
  score: z.coerce.number().int().min(0).optional(),
  level: z.coerce.number().int().min(1).optional(),
});

export const createChatSessionSchema = z.object({
  title: z.string().min(1).max(255).optional(),
});

export const sendMessageSchema = z.object({
  content: z.string().min(1).max(10000),
  images: z
    .array(
      z.object({
        data: z.string(),
        mimeType: z.enum(["image/png", "image/jpeg", "image/webp"]),
      })
    )
    .max(4)
    .optional(),
});

export function formatZodError(error: z.ZodError) {
  return error.issues.map((issue) => ({
    field: issue.path.join("."),
    message: issue.message,
  }));
}
