# GastosApp - Project Information

## PostgreSQL Database Setup

### Create Unaccent Extension

```sql
CREATE EXTENSION IF NOT EXISTS unaccent WITH SCHEMA public;
```

### Configure Extension for Each Schema

Redirect to the public function for each schema:

```sql
CREATE OR REPLACE FUNCTION rc.unaccent(text)
RETURNS text
LANGUAGE SQL
AS $$
  SELECT public.unaccent($1)
$$;
```

### List Extensions

```sql
SELECT extname, nspname AS schema_name
FROM pg_extension e
JOIN pg_namespace n ON e.extnamespace = n.oid;
```

### Move Extension Between Schemas

```sql
ALTER EXTENSION unaccent SET SCHEMA public;
```

## Deployment

Deployment is triggered automatically from GitHub when synchronizing the `main` branch.

## Resources

**Android Icons Generator**: [https://maskable.app/editor](https://maskable.app/editor)