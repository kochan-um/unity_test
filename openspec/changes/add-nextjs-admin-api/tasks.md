## 1. 繝励Ο繧ｸ繧ｧ繧ｯ繝亥・譛溷喧
- [x] 1.1 `admin/` 繝・ぅ繝ｬ繧ｯ繝医Μ縺ｫ Next.js App Router 繝励Ο繧ｸ繧ｧ繧ｯ繝医ｒ菴懈・・・ypeScript, ESLint, Tailwind CSS辟｡縺暦ｼ戡PI蟆ら畑・・- [x] 1.2 蠢・ｦ√↑萓晏ｭ倥ヱ繝・こ繝ｼ繧ｸ繧偵う繝ｳ繧ｹ繝医・繝ｫ・・supabase/supabase-js, zod・・- [x] 1.3 tsconfig.json 縺ｧstrict mode縺ｨ繝代せ繧ｨ繧､繝ｪ繧｢繧ｹ・・@/`・峨ｒ險ｭ螳・- [x] 1.4 .env.local 縺ｮ繝・Φ繝励Ξ繝ｼ繝医ｒ菴懈・・・UPABASE_URL, SUPABASE_SERVICE_ROLE_KEY, SESSION_SECRET・・- [x] 1.5 .gitignore 縺ｫ .env.local 繧定ｿｽ蜉

## 2. Supabase謗･邯壼渕逶､
- [x] 2.1 `src/lib/supabase/admin.ts` 窶・service_role key 繧剃ｽｿ逕ｨ縺吶ｋ Supabase Admin Client 縺ｮ繧ｷ繝ｳ繧ｰ繝ｫ繝医Φ繧貞ｮ溯｣・- [x] 2.2 `src/lib/supabase/types.ts` 窶・Supabase Database 縺ｮ蝙句ｮ夂ｾｩ繧剃ｽ懈・・・upabase CLI 縺ｮ `supabase gen types` 縺ｧ逕滓・縲√∪縺溘・謇句虚螳夂ｾｩ・・- [x] 2.3 謗･邯壹ユ繧ｹ繝育畑縺ｮ繝倥Ν繧ｹ繝√ぉ繝・け繧ｨ繝ｳ繝峨・繧､繝ｳ繝・`GET /api/health` 繧貞ｮ溯｣・
## 3. 隱崎ｨｼAPI
- [x] 3.1 `src/lib/auth/session.ts` 窶・HTTPOnly Cookie 繝吶・繧ｹ縺ｮ繧ｻ繝・す繝ｧ繝ｳ邂｡逅・Θ繝ｼ繝・ぅ繝ｪ繝・ぅ繧貞ｮ溯｣・ｼ・ncrypt/decrypt, set/get/delete cookie・・- [x] 3.2 `src/lib/validation/schemas.ts` 窶・隱崎ｨｼ繝ｪ繧ｯ繧ｨ繧ｹ繝育畑縺ｮ Zod 繧ｹ繧ｭ繝ｼ繝槭ｒ螳夂ｾｩ・・oginSchema: email + password・・- [x] 3.3 `POST /api/auth/login` 窶・Supabase Auth 縺ｧ繝｡繝ｼ繝ｫ/繝代せ繝ｯ繝ｼ繝芽ｪ崎ｨｼ 竊・admin 繝ｭ繝ｼ繝ｫ讀懆ｨｼ 竊・繧ｻ繝・す繝ｧ繝ｳ Cookie 險ｭ螳・- [x] 3.4 `POST /api/auth/logout` 窶・繧ｻ繝・す繝ｧ繝ｳ Cookie 繧貞炎髯､
- [x] 3.5 `GET /api/auth/session` 窶・迴ｾ蝨ｨ縺ｮ繧ｻ繝・す繝ｧ繝ｳ諠・ｱ繧定ｿ泌唆・郁ｪ崎ｨｼ貂医∩縺九√Θ繝ｼ繧ｶ繝ｼ諠・ｱ・・- [x] 3.6 `src/middleware.ts` 窶・Next.js Middleware 縺ｧ /api/* 繝ｫ繝ｼ繝医・隱崎ｨｼ繝√ぉ繝・け・・api/auth/login 縺ｨ /api/health 繧帝勁螟厄ｼ・
## 4. 繧｢繧､繝・Β繝槭せ繧ｿAPI
- [x] 4.1 `src/lib/validation/schemas.ts` 縺ｫ繧｢繧､繝・Β逕ｨ Zod 繧ｹ繧ｭ繝ｼ繝槭ｒ霑ｽ蜉・・reateItemSchema, updateItemSchema, listItemsQuerySchema・・- [x] 4.2 `GET /api/items` 窶・繧｢繧､繝・Β荳隕ｧ蜿門ｾ暦ｼ医・繝ｼ繧ｸ繝阪・繧ｷ繝ｧ繝ｳ: page/limit縲√ヵ繧｣繝ｫ繧ｿ: category/rarity縲√た繝ｼ繝・ name/created_at・・- [x] 4.3 `POST /api/items` 窶・繧｢繧､繝・Β菴懈・・・ame, description, category, rarity, stackable, maxStack, iconUrl 遲会ｼ・- [x] 4.4 `GET /api/items/[id]` 窶・繧｢繧､繝・Β隧ｳ邏ｰ蜿門ｾ・- [x] 4.5 `PUT /api/items/[id]` 窶・繧｢繧､繝・Β譖ｴ譁ｰ
- [x] 4.6 `DELETE /api/items/[id]` 窶・繧｢繧､繝・Β蜑企勁・郁ｫ也炊蜑企勁: is_deleted 繝輔Λ繧ｰ・・
## 5. 繝励Ξ繧､繝､繝ｼ繝・・繧ｿ邂｡逅・PI
- [x] 5.1 `src/lib/validation/schemas.ts` 縺ｫ繝励Ξ繧､繝､繝ｼ逕ｨ Zod 繧ｹ繧ｭ繝ｼ繝槭ｒ霑ｽ蜉
- [x] 5.2 `GET /api/players` 窶・繝励Ξ繧､繝､繝ｼ荳隕ｧ蜿門ｾ暦ｼ医・繝ｼ繧ｸ繝阪・繧ｷ繝ｧ繝ｳ縲∵､懃ｴ｢: username/email・・- [x] 5.3 `GET /api/players/[id]` 窶・繝励Ξ繧､繝､繝ｼ隧ｳ邏ｰ蜿門ｾ暦ｼ医・繝ｭ繝輔ぅ繝ｼ繝ｫ縲∫ｵｱ險域ュ蝣ｱ・・- [x] 5.4 `GET /api/players/[id]/inventory` 窶・繝励Ξ繧､繝､繝ｼ縺ｮ繧､繝ｳ繝吶Φ繝医Μ荳隕ｧ蜿門ｾ暦ｼ医い繧､繝・Β蜷咲ｵ仙粋・・- [x] 5.5 `PATCH /api/players/[id]` 窶・繝励Ξ繧､繝､繝ｼ繝・・繧ｿ菫ｮ豁｣・育ｮ｡逅・・↓繧医ｋ謇句虚菫ｮ豁｣縲∝､画峩繝ｭ繧ｰ險倬鹸・・
## 6. 繝繝・す繝･繝懊・繝陰PI
- [x] 6.1 `GET /api/dashboard/stats` 窶・繝繝・す繝･繝懊・繝臥ｵｱ險茨ｼ育ｷ上・繝ｬ繧､繝､繝ｼ謨ｰ縲∵悽譌･縺ｮ繧｢繧ｯ繝・ぅ繝匁焚縲√い繧､繝・Β邱乗焚縲∫峩霑醍匳骭ｲ繝励Ξ繧､繝､繝ｼ・・- [x] 6.2 `GET /api/dashboard/events` 窶・SSE繧ｨ繝ｳ繝峨・繧､繝ｳ繝茨ｼ・upabase Realtime 繧定ｳｼ隱ｭ縺励√・繝ｬ繧､繝､繝ｼ繝ｭ繧ｰ繧､繝ｳ/繝ｭ繧ｰ繧｢繧ｦ繝医√い繧､繝・Β螟画峩繧､繝吶Φ繝医ｒ驟堺ｿ｡・・
## 7. 蜈ｱ騾壼渕逶､
- [x] 7.1 API 繝ｬ繧ｹ繝昴Φ繧ｹ縺ｮ繝輔か繝ｼ繝槭ャ繝育ｵｱ荳・・{ data, error, meta }` 蠖｢蠑擾ｼ・- [x] 7.2 繧ｨ繝ｩ繝ｼ繝上Φ繝峨Μ繝ｳ繧ｰ縺ｮ蜈ｱ騾壼喧・・00/401/403/404/500 縺ｮ繝ｬ繧ｹ繝昴Φ繧ｹ逕滓・繝倥Ν繝代・・・- [x] 7.3 繝ｪ繧ｯ繧ｨ繧ｹ繝医ヰ繝ｪ繝・・繧ｷ繝ｧ繝ｳ縺ｮ蜈ｱ騾壼喧・・od 繝代・繧ｹ + 繧ｨ繝ｩ繝ｼ繝輔か繝ｼ繝槭ャ繝医・繝倥Ν繝代・髢｢謨ｰ・・- [x] 7.4 繝壹・繧ｸ繝阪・繧ｷ繝ｧ繝ｳ縺ｮ繝倥Ν繝代・・・otalCount, page, limit, totalPages 縺ｮ險育ｮ暦ｼ・
## 8. 繝・・繝ｭ繧､繝ｻCI/CD
- [x] 8.1 Vercel 繝励Ο繧ｸ繧ｧ繧ｯ繝郁ｨｭ螳夲ｼ・admin/` 繧偵Ν繝ｼ繝医ョ繧｣繝ｬ繧ｯ繝医Μ縺ｫ謖・ｮ壹∫腸蠅・､画焚險ｭ螳夲ｼ・- [x] 8.2 `.github/workflows/` 縺ｫ邂｡逅・判髱｢API縺ｮ繝・・繝ｭ繧､繝ｯ繝ｼ繧ｯ繝輔Ο繝ｼ繧定ｿｽ蜉・医∪縺溘・譌｢蟄倥Ρ繝ｼ繧ｯ繝輔Ο繝ｼ繧呈僑蠑ｵ・・- [x] 8.3 譛ｬ逡ｪ迺ｰ蠅・〒縺ｮ蜍穂ｽ懃｢ｺ隱搾ｼ郁ｪ崎ｨｼ繝輔Ο繝ｼ縲，RUD謫堺ｽ懊ヾSE謗･邯夲ｼ・
