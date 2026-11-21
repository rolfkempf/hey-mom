# PostgreSQL Persistence

- Keep `HeyMomDbContext`, persisted entities, EF configurations and migrations in this project. Put new entities in `Entities` and explicit mappings in `Configurations` as the model grows.
- Keep the dependency pointing from the API to persistence. Keep HTTP contracts, request validation and external provider calls in their API owners.
- Keep entity and mapping changes together. Preserve table and constraint names during code-only moves so the database schema stays stable.
- Once migrations are introduced, add a forward migration for each intentional schema change. Review generated operations for nullability, constraints, indexes, foreign keys and delete behavior. Call out backfills and destructive changes. Apply migrations explicitly outside production API startup.
- Test schema and query behavior against isolated PostgreSQL. Compare database timestamps at PostgreSQL's microsecond precision when values make a round trip.
