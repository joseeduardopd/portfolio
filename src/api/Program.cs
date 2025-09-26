using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using System.Text.Json;
using Api.Endpoints;
using Api.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
// DB Context (EF Core) – Postgres
var conn = builder.Configuration.GetConnectionString("Default")
           ?? builder.Configuration["ConnectionStrings:Default"]
           ?? "Host=db;Database=portfolio;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portfolio API", Version = "v1" });
});

// Auth/JWT
var jwtKey = builder.Configuration["JWT__KEY"] ?? builder.Configuration["JWT:KEY"] ?? "dev-very-secret-key-change";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddAuthorization();

var permittedOrigins = new[]
{
    // Ajustar no deploy para o domínio real do GitHub Pages
    "https://localhost:5173",
    "http://localhost:5173"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("pages", policy =>
        policy.WithOrigins(permittedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseCors("pages");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Portfolio API v1");
    });
}

// Health
app.MapGet("/_health", () => Results.Ok("Healthy"));
// Avatar: salvar em disco (volume) e servir
var avatarRoot = builder.Configuration["AVATAR__ROOT"] ?? "/data/profile";
Directory.CreateDirectory(avatarRoot);
app.MapGet("/api/profile/avatar", () =>
{
    var path = Path.Combine(avatarRoot, "avatar.png");
    if (!System.IO.File.Exists(path)) return Results.NotFound();
    var bytes = System.IO.File.ReadAllBytes(path);
    return Results.File(bytes, "image/png");
});

app.MapPost("/api/profile/avatar", async (HttpRequest request) =>
{
    if (!request.HasFormContentType) return Results.BadRequest(new { message = "multipart/form-data required" });
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null || file.Length == 0) return Results.BadRequest(new { message = "file required" });
    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
    var allowed = new[] { ".png", ".jpg", ".jpeg" };
    if (!allowed.Contains(ext)) return Results.BadRequest(new { message = "allowed: png,jpg,jpeg" });
    var dest = Path.Combine(avatarRoot, "avatar.png");
    using var stream = System.IO.File.Create(dest);
    await file.CopyToAsync(stream);
    return Results.Ok(new { message = "uploaded" });
}).RequireAuthorization();


// Seed em memória (S1)
var profile = new
{
    id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    name = "José Eduardo",
    title = "Desenvolvedor de software",
    summary = "Olá, meu nome é José Eduardo e fui/sou desenvolvedor de software;"
};

var formations = new[]
{
    new { id = Guid.NewGuid(), course = "Sou formado em Análise e Desenvolvimento de Sistemas e atualmente curso pós-graduação em Data Warehouse e Business Intelligence;"}
};

var experiences = new[]
{
    new { id = Guid.NewGuid(), company = "Experiência prévia", role = string.Empty, start = new DateOnly(2010,1,1), end = new DateOnly(2016,12,31), description = "Atuei por cerca de 6 anos como desenvolvedor web, com maior foco em Java backend (Spring Boot, JPA e Hibernate) e, posteriormente, em um período como Full Stack utilizando .NET e Angular. Nesse período, participei do desenvolvimento e manutenção de projetos de grande porte, incluindo:\n\n\t* Sistema de Gerenciamento de Vendas de recarga móvel via POS – Um dos sistemas pioneiros no Brasil na venda de recargas móveis via máquinas de cartão de credito (Rede Tendência – atual Grupo Card)\n\n\t* Sistema de Gestão Agropecuária (Sisgado) – Plataforma para gerenciamento de fazendas e monitoramento de gado pela tags de identificação, com rastreio via GPS e acompanhamento de dados (vacinas, evolução de peso etc.). (BitSis)\n\n\t* Sistema Integrado de Gestão Operacional (S.I.G.O.) – Solução para segurança pública que unifica bases de dados, gera relatórios, permite acompanhamento em tempo real de viaturas no mapa, otimiza o despacho de ocorrências por proximidade e integra a comunicação entre órgãos de segurança pública estaduais. (Compnet)" }
};

var certifications = new[]
{
    new { id = Guid.NewGuid(), name = ".NET", institution = "Org Z", year = 2023 }
};

// Novo conteúdo: Transição de carreira (linhas)
string[] career = new []
{
    "No final de 2016, precisei interromper minha carreira devido a um problema de saúde. Nesse período, trabalhar de forma remota era uma condição essencial para mim, mas ainda pouco comum na área, especialmente na cidade em que eu moro, Campo Grande-MS.",
    "Foi nesse momento que conheci a possibilidade de jogar poker online profissionalmente, um mercado até então desconhecido para quem jogava apenas de maneira recreativa com os amigos. Em 2017 entrei para um time de poker e atuei como jogador profissional por aproximadamente 5 anos. Essa experiência me proporcionou o desenvolvimento de competências fundamentais como disciplina, controle emocional, gestão de tempo e capacidade de tomar decisões sob pressão.",
    string.Empty,
    "Durante a pandemia, observei a consolidação do trabalho remoto em diversos setores, incluindo TI. Esse movimento despertou interesse em retomar minha carreira como desenvolvedor e desde então, venho estruturando minha transição:",
    "* concluí minha graduação em Análise e Desenvolvimento de Sistemas",
    "* iniciei uma pós-graduação em Data Warehouse e Business Intelligence",
    string.Empty,
    "Atualmente, busco uma oportunidade como Trainee ou Júnior para reingressar no mercado de desenvolvimento de software."
};

var commands = new List<TerminalCommand>
{
    TerminalCommand.Seed("Quem sou eu", "Quem sou eu", "Who am I", new[]{ Step.Print("Seu Nome — Desenvolvedor .NET"), Step.Print("Resumo em uma linha…") }),
    TerminalCommand.Seed("formacao", "Formação", "Education", new[]{ Step.Print("Ciência da Computação — Universidade X (2016–2020)") }),
    TerminalCommand.Seed("experiencia previa", "Experiência prévia", "Experience", new[]{ Step.Print("Software Engineer @ Empresa Y (2021–Present)") }),
    TerminalCommand.Seed("cursos e certificacoes", "Cursos e Certificações", "Courses & Certifications", new[]{ Step.Print("Certificação .NET — Org Z (2023)") }),
};

// Endpoints públicos
app.MapGet("/api/profile", () => Results.Ok(profile));
app.MapGet("/api/formation", () => Results.Ok(formations));
app.MapGet("/api/experience", () => Results.Ok(experiences));
app.MapGet("/api/certifications", () => Results.Ok(certifications));
app.MapGet("/api/career", () => Results.Ok(career));

// Inicialização do banco + seed (S1):
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.TerminalCommands.Any())
    {
        var seedEntities = commands.Select(c => new TerminalCommandEntity
        {
            Id = Guid.NewGuid(),
            Key = c.Key,
            LabelPt = c.LabelPt,
            LabelEn = c.LabelEn,
            Order = c.Order,
            Enabled = c.Enabled,
            Steps = JsonSerializer.Serialize(c.Steps.Select(s => s.Value ?? string.Empty))
        }).ToList();
        db.TerminalCommands.AddRange(seedEntities);
        db.SaveChanges();
    }
}

// GET via DB (retorna Steps como array)
app.MapGet("/api/terminal/commands", async (AppDbContext db) =>
{
    var list = await db.TerminalCommands.AsNoTracking().OrderBy(x => x.Order).ToListAsync();
    var dtos = list.Select(e => TerminalMapper.ToReadDto(e)).ToList();
    return Results.Ok(dtos);
});

app.MapGet("/api/terminal/commands/{key}", async (string key, AppDbContext db) =>
{
    var item = await db.TerminalCommands.AsNoTracking()
        .FirstOrDefaultAsync(c => c.Key.ToLower() == key.ToLower());
    return item is null ? Results.NotFound() : Results.Ok(TerminalMapper.ToReadDto(item));
});

// CRUD básico (sem auth neste passo)
app.MapPost("/api/terminal/commands", async (AppDbContext db, TerminalCommandCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Key) || string.IsNullOrWhiteSpace(dto.LabelPt) || string.IsNullOrWhiteSpace(dto.LabelEn))
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["message"] = ["Campos obrigatórios: key, labelPt, labelEn"] });
    if (await db.TerminalCommands.AnyAsync(x => x.Key.ToLower() == dto.Key.ToLower()))
        return Results.Conflict(new { message = "Key já existe" });

    var entity = new TerminalCommandEntity
    {
        Id = Guid.NewGuid(),
        Key = dto.Key,
        LabelPt = dto.LabelPt,
        LabelEn = dto.LabelEn,
        Order = dto.Order,
        Enabled = dto.Enabled,
        Steps = JsonSerializer.Serialize(dto.Steps ?? new List<string>())
    };
    db.TerminalCommands.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/terminal/commands/{entity.Id}", entity);
}).RequireAuthorization();

app.MapPut("/api/terminal/commands/{id:guid}", async (Guid id, AppDbContext db, TerminalCommandUpdateDto dto) =>
{
    var entity = await db.TerminalCommands.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null) return Results.NotFound();
    if (!string.IsNullOrWhiteSpace(dto.Key) && !dto.Key.Equals(entity.Key, StringComparison.OrdinalIgnoreCase))
    {
        if (await db.TerminalCommands.AnyAsync(x => x.Key.ToLower() == dto.Key!.ToLower()))
            return Results.Conflict(new { message = "Key já existe" });
        entity.Key = dto.Key!;
    }
    if (!string.IsNullOrWhiteSpace(dto.LabelPt)) entity.LabelPt = dto.LabelPt!;
    if (!string.IsNullOrWhiteSpace(dto.LabelEn)) entity.LabelEn = dto.LabelEn!;
    if (dto.Order.HasValue) entity.Order = dto.Order.Value;
    if (dto.Enabled.HasValue) entity.Enabled = dto.Enabled.Value;
    if (dto.Steps is not null) entity.Steps = JsonSerializer.Serialize(dto.Steps);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/api/terminal/commands/{id:guid}/order", async (Guid id, AppDbContext db, OrderPatch body) =>
{
    var entity = await db.TerminalCommands.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null) return Results.NotFound();
    entity.Order = body.Order;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapPatch("/api/terminal/commands/{id:guid}/enabled", async (Guid id, AppDbContext db, EnabledPatch body) =>
{
    var entity = await db.TerminalCommands.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null) return Results.NotFound();
    entity.Enabled = body.Enabled;
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.MapDelete("/api/terminal/commands/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var entity = await db.TerminalCommands.FirstOrDefaultAsync(x => x.Id == id);
    if (entity is null) return Results.NotFound();
    db.TerminalCommands.Remove(entity);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

// Endpoint de contato removido do escopo atual (uso de links diretos)

// Auth
app.MapPost("/api/auth/login", (IConfiguration cfg, LoginInput input) =>
{
    var adminEmail = cfg["ADMIN__EMAIL"] ?? cfg["Admin:Email"] ?? "admin@example.com";
    var adminPassword = cfg["ADMIN__PASSWORD"] ?? cfg["Admin:Password"] ?? "admin123";
    if (!string.Equals(input.Email?.Trim(), adminEmail, StringComparison.OrdinalIgnoreCase) || input.Password != adminPassword)
        return Results.Unauthorized();

    var key = cfg["JWT__KEY"] ?? cfg["JWT:KEY"] ?? "dev-very-secret-key-change";
    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddHours(8);
    var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
        claims: new[] { new System.Security.Claims.Claim("role", "admin") },
        expires: expires,
        signingCredentials: creds
    );
    var jwt = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = jwt, expiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds });
});

app.Run();

// Tipos movidos para Api.Domain

public sealed record LoginInput(string Email, string Password);


