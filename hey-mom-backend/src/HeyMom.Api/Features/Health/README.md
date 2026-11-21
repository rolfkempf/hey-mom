# Health

`GET /api/health` returns `{"status":"healthy"}` when PostgreSQL accepts a
connection. A failed connection returns HTTP 503 with an empty body. The
query opens a read-only connectivity check without changing data.
