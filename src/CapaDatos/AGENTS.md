# Data privacy rules

- Treat every database row created or configured by a user as private to that user.
- Every data-layer and business-layer method that reads or changes user-owned data must require `idUsuario` (or an entity carrying it) and validate it before accessing the database.
- Every stored procedure must filter reads and constrain writes by `IdUsuario`; do not rely on the UI or an ID being hard to guess.
- User-owned catalog records such as applications and websites must have an owner column and owner-scoped uniqueness. Never query or reuse another user's catalog row.
- Relationship tables must carry the owner and use composite foreign keys so a task cannot reference another user's catalog item.
- Add the owner to every new user-owned table and every new query. Review all callers when changing these APIs.
- Never add a general user search that returns names, emails, hashes, or other account data. Login and recovery flows must return only the account being authenticated or recovered.
- For database changes, update both the fresh-install scripts and an upgrade migration, then verify two users with the same task/catalog values cannot read or mutate each other's records.
