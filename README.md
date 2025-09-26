# Portfolio • .NET + Blazor WASM + Minimal API + Postgres

Este repositório contém meu currículo/portfólio com uma landing interativa (terminal) e um CRUD demo simples (em sessão), consumindo uma API real.

## Visão geral
- Frontend: Blazor WebAssembly (SPA estática)
- API: ASP.NET Core Minimal API
- Banco: PostgreSQL (EF Core, `jsonb` para steps do terminal)
- Infra: Docker Compose (API + DB); avatar em volume (`/data/profile/avatar.png`)

## Rodando com Docker Compose
```bash
cd infra
docker compose up -d --build
```
- Front (dev local): http://localhost:5173
- API: http://localhost:8081 (Swagger em /swagger)
- DB: postgres://postgres:postgres@localhost:5433/portfolio

Parar:
```bash
docker compose down
```

## Rodando o Front local (sem Docker)
```bash
cd src/frontend.wasm
dotnet restore
dotnet run --urls http://localhost:5173
```

## Principais Endpoints (API)
- GET /_health → "Healthy"
- GET /api/profile/avatar → avatar (image/png)
- GET /api/terminal/commands → lista de comandos (para o terminal)
- GET /api/terminal/commands/{key} → comando por chave (retorna `steps: string[]`)
- [Admin/JWT]
  - POST /api/terminal/commands
  - PUT /api/terminal/commands/{id}
  - PATCH /api/terminal/commands/{id}/order
  - PATCH /api/terminal/commands/{id}/enabled
  - DELETE /api/terminal/commands/{id}

## Como validar
1. API/DB
```bash
cd infra
docker compose up -d --build api
```
2. Front
```bash
cd ../src/frontend.wasm
dotnet run --urls http://localhost:5173
```
3. Acesse http://localhost:5173
   - Terminal: execute comandos e observe animações (digitação, clear) e resultados.
   - CRUD demo: clique em “Eu sei fazer CRUD! 😎 → Clique aqui” e teste criar/editar/excluir pessoas (persistência em sessão).
4. Avatar
```bash
cd infra
docker compose cp ../avatar.png api:/data/profile/avatar.png
```
   - Recarregue a Home (Ctrl+F5). A rota http://localhost:8081/api/profile/avatar deve servir a imagem.

5. Logs da API
```bash
cd infra
docker compose logs -f api | cat
```

## Estrutura
```
/infra                # docker compose (api + db)
/src
  /api               # Minimal API (.NET)
  /frontend.wasm     # Blazor WebAssembly (SPA)
```

## Notas de implementação
- Terminal consome exclusivamente `/api/terminal/commands/{key}` e reexecuta o último comando ao detectar mudanças (polling leve) usando quebra de cache `?ts=<unix>`.
- Comandos persistem no Postgres; `steps` são `jsonb`.
- CRUD demo em `/demo/crud` usa apenas sessionStorage (não persiste em banco).

