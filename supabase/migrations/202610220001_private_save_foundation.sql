-- REVIEW ONLY: not deployed. Does not implement a multiplayer economy or friendship sharing.
-- Apply with the Supabase migration tooling after reviewing in a disposable project.
begin;
create table public.profiles (
  id uuid primary key references auth.users(id) on delete cascade,
  display_name text not null check (char_length(display_name) between 1 and 40),
  created_at timestamptz not null default now()
);
create table public.offline_saves (
  owner_id uuid not null references auth.users(id) on delete cascade,
  slot_id uuid not null,
  schema_version integer not null default 1 check (schema_version = 1),
  revision bigint not null default 1 check (revision > 0),
  payload jsonb not null check (jsonb_typeof(payload) = 'object' and octet_length(payload::text) <= 2097152),
  updated_at timestamptz not null default now(),
  primary key (owner_id, slot_id)
);
create table public.life_summaries (
  id uuid primary key default gen_random_uuid(),
  owner_id uuid not null references auth.users(id) on delete cascade,
  kind text not null default 'unverified_offline_summary' check (kind = 'unverified_offline_summary'),
  display_name text not null check (char_length(display_name) between 1 and 40),
  chapter integer not null check (chapter > 0),
  completed boolean not null default false,
  created_at timestamptz not null default now()
);
create index life_summaries_owner on public.life_summaries(owner_id);
alter table public.profiles enable row level security;
alter table public.offline_saves enable row level security;
alter table public.life_summaries enable row level security;
revoke all on public.profiles, public.offline_saves, public.life_summaries from anon;
grant select, insert, update, delete on public.profiles, public.offline_saves, public.life_summaries to authenticated;
create policy "Own profile only" on public.profiles for all to authenticated
  using ((select auth.uid()) = id) with check ((select auth.uid()) = id);
create policy "Own private saves only" on public.offline_saves for all to authenticated
  using ((select auth.uid()) = owner_id) with check ((select auth.uid()) = owner_id);
-- Private by default. Add explicit friendship/consent policies in a later migration.
create policy "Own summaries only" on public.life_summaries for all to authenticated
  using ((select auth.uid()) = owner_id) with check ((select auth.uid()) = owner_id);
-- Conflict-safe client sync must UPDATE ... WHERE revision = expected_revision and
-- increment revision. Never resolve an offline conflict by overwriting silently.
-- Online money, assets, roles and authority MUST NOT be inferred from these records.
commit;
