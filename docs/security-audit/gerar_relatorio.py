# -*- coding: utf-8 -*-
"""
Gerador do Relatório de Auditoria de Segurança - SuperHeroesApi
Requer: reportlab, matplotlib (instalados no venv local docs/security-audit/.venv)
Uso: .venv\\Scripts\\python.exe gerar_relatorio.py
"""
import os
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

from reportlab.lib.pagesizes import A4
from reportlab.lib.units import cm
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_LEFT, TA_CENTER, TA_JUSTIFY
from reportlab.platypus import (
    BaseDocTemplate, PageTemplate, Frame, Paragraph, Spacer, Table, TableStyle,
    Image, PageBreak, KeepTogether, HRFlowable
)
from reportlab.platypus.flowables import Flowable

HERE = os.path.dirname(os.path.abspath(__file__))
OUT_PDF = os.path.join(HERE, "relatorio-auditoria-seguranca.pdf")

COR_CRITICA = "#B91C1C"
COR_ALTA = "#EA580C"
COR_MEDIA = "#D97706"
COR_BAIXA = "#2563EB"
COR_FORTE = "#059669"
COR_INFO = "#6B7280"

# ---------------------------------------------------------------------------
# Dados da auditoria
# ---------------------------------------------------------------------------

ACHADOS = [
    {
        "id": "F1",
        "severidade": "CRÍTICA",
        "cor": COR_CRITICA,
        "categoria": "2. Permissão/Autorização (equivalente adaptado)",
        "arquivo": "src/SuperHeroesApi.WebAPI/Controllers/HeroesController.cs:79,112,149",
        "titulo": "Ausência total de autenticação/autorização nos endpoints de escrita",
        "descricao": (
            "Os endpoints POST /api/heroes (linha 79), PUT /api/heroes/{id} (linha 112) e "
            "DELETE /api/heroes/{id} (linha 149) não possuem nenhum atributo [Authorize] nem "
            "qualquer outro controle de acesso. O projeto não registra nenhum esquema de "
            "autenticação (sem AddAuthentication/AddJwtBearer/AddIdentity em Program.cs ou "
            "InfrastructureService.cs). app.UseAuthorization() é chamado (Program.cs:81), mas "
            "não há política, esquema, papel (role) ou claim configurado para ser avaliado — "
            "logo, a chamada é um no-op de segurança. Qualquer cliente anônimo na rede pode "
            "criar, alterar ou excluir qualquer herói."
        ),
        "codigo": "[HttpPost]\npublic async Task<ActionResult<HeroDto>> CreateHero(...)\n// sem [Authorize]",
        "explorabilidade": (
            "Diretamente explorável hoje: basta enviar POST/PUT/DELETE para a API sem nenhum "
            "cabeçalho de autenticação. Não depende de feature flag."
        ),
    },
    {
        "id": "F2",
        "severidade": "ALTA",
        "cor": COR_ALTA,
        "categoria": "Configuração insegura (CORS) — correlata à categoria 4",
        "arquivo": "src/SuperHeroesApi.WebAPI/Program.cs:48-55,79",
        "titulo": "Política de CORS totalmente aberta definida como padrão, com nome de política divergente no middleware",
        "descricao": (
            "AddCors define uma AddDefaultPolicy com AllowAnyOrigin().AllowAnyMethod()."
            "AllowAnyHeader() (Program.cs:48-55) — extremamente permissiva. Em seguida, "
            "app.UseCors(\"VueAppPolicy\") (linha 79) tenta aplicar uma política nomeada "
            "'VueAppPolicy' que nunca foi registrada, deixando a política padrão permissiva "
            "sem efeito no momento. O risco é latente e de alta severidade: a correção óbvia "
            "que qualquer desenvolvedor fará (trocar para app.UseCors() sem nome, ou registrar "
            "'VueAppPolicy' com a mesma regra AllowAny*) ativa imediatamente CORS totalmente "
            "aberto para todos os métodos (incluindo POST/PUT/DELETE) e todas as origens, "
            "permitindo que qualquer site controle a API a partir do navegador da vítima."
        ),
        "codigo": (
            "options.AddDefaultPolicy(policy => {\n"
            "  policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();\n"
            "});\n...\napp.UseCors(\"VueAppPolicy\"); // política inexistente"
        ),
        "explorabilidade": (
            "Não ativa hoje devido ao nome divergente (bug funcional que hoje age como rede de "
            "segurança acidental), mas é uma condição insegura pronta para ser 'corrigida' de "
            "forma perigosa. Reportado como configuração insegura latente."
        ),
    },
    {
        "id": "F3",
        "severidade": "MÉDIA",
        "cor": COR_MEDIA,
        "categoria": "Configuração insegura / vazamento de dados em logs",
        "arquivo": "src/SuperHeroesApi.Infrastructure/Extensions/InfrastructureService.cs:19-20",
        "titulo": "EnableSensitiveDataLogging() habilitado incondicionalmente (sem gate de ambiente)",
        "descricao": (
            "O DbContext é registrado com .UseInMemoryDatabase(\"SuperHeroesDB\")."
            ".EnableSensitiveDataLogging() sem nenhuma checagem de ambiente "
            "(builder.Environment.IsDevelopment()). Essa opção instrui o EF Core a incluir "
            "valores de parâmetros de consulta (dados pessoais dos registros) nas mensagens de "
            "log e exceção. Hoje o provedor é InMemory (risco baixo), mas se o provedor for "
            "trocado para um banco real em produção sem remover essa linha, dados sensíveis "
            "podem vazar para logs de aplicação/observabilidade."
        ),
        "codigo": "options.UseInMemoryDatabase(\"SuperHeroesDB\")\n  .EnableSensitiveDataLogging());",
        "explorabilidade": (
            "Requer troca futura do provedor InMemory por um banco real em produção mantendo "
            "essa configuração — risco condicional, mas fácil de ocorrer por descuido."
        ),
    },
]

# Achados 'não aplicável' documentados explicitamente
NAO_APLICAVEIS = [
    ("1. Banco sem tranca (RLS/tenant)", "Não aplicável: a API não possui conceito de usuário, "
     "organização ou tenant. Heroes/Superpowers são um catálogo global único, sem coluna de "
     "propriedade (owner_id/tenant_id). Não há, portanto, mecanismo de isolamento a auditar; "
     "a ausência de autenticação em si já está coberta no achado F1."),
    ("3. IDOR", "Não aplicável no sentido clássico: como não existe modelo de posse/usuário, "
     "não há 'objeto de outro usuário' a ser acessado indevidamente. O risco real equivalente "
     "(qualquer chamador acessa/altera/exclui qualquer registro por ID) já está coberto por F1, "
     "pois a causa raiz é a ausência total de autenticação, não uma falha de verificação de posse."),
    ("4. Chaves expostas (hardcode)", "Nenhuma chave de API, senha, token JWT, segredo de webhook "
     "ou credencial hardcoded foi encontrada em código-fonte, appsettings.json, "
     "appsettings.Development.json ou InfrastructureService.cs. Não há arquivos Docker/CI/Helm/"
     "Terraform no repositório. O banco de dados é EF Core InMemory (UseInMemoryDatabase), sem "
     "connection string ou credenciais reais. Nenhum default inseguro de variável de ambiente "
     "(${VAR:-default}) foi localizado."),
    ("5. Inputs sem tratamento (XSS)", "Não aplicável: o repositório contém apenas uma API REST "
     "que retorna JSON puro via ActionResult/DTOs (serialização automática, sem geração de HTML). "
     "Não há projeto de frontend neste repositório, não há renderização de templates/e-mails, "
     "não há uso de innerHTML/dangerouslySetInnerHTML/v-html, eval ou new Function no código "
     "auditado."),
]

PONTOS_FORTES = [
    ("Swagger/OpenAPI restrito ao ambiente de desenvolvimento",
     "Program.cs:66-73 — app.UseSwagger()/UseSwaggerUI() só é registrado dentro de "
     "if (app.Environment.IsDevelopment()), evitando exposição do contrato da API em produção."),
    ("HTTPS Redirection habilitado",
     "Program.cs:77 — app.UseHttpsRedirection() força o uso de HTTPS."),
    ("Validação de modelo (Data Annotations) em todos os DTOs de escrita",
     "CreateHeroDto.cs e UpdateHeroDto.cs aplicam [Required], [StringLength] e [Range], e os "
     "controllers verificam ModelState.IsValid (HeroesController.cs:85,118) antes de processar."),
    ("Validação de ID no path antes de qualquer consulta",
     "HeroesController.cs:53,116,153 — GetHeroById, UpdateHero e DeleteHero rejeitam id <= 0 com "
     "BadRequest antes de tocar o repositório."),
    ("Regras de negócio centralizadas na entidade de domínio",
     "Hero.cs — ValidateBusinessRules() garante datas e medidas consistentes antes da persistência, "
     "reduzindo risco de dados inconsistentes explorados posteriormente."),
    ("Nenhum segredo, senha ou chave de API hardcoded encontrado no código-fonte",
     "Verificado em appsettings.json, appsettings.Development.json e todo o código de "
     "Infrastructure/WebAPI (busca por 'password', 'secret', 'apikey', 'token', 'connectionstring')."),
    ("Nenhum ponto de renderização de HTML/template não sanitizado",
     "A API expõe exclusivamente respostas JSON tipadas via DTOs — não há superfície de XSS "
     "server-side no código auditado."),
]

RECOMENDACOES = [
    ("P1", "Implementar autenticação (ex.: JWT Bearer) e autorização ([Authorize]/políticas de "
     "papel) em todos os endpoints de escrita (POST/PUT/DELETE) de HeroesController, e reavaliar "
     "se GET também deve exigir autenticação conforme o modelo de negócio."),
    ("P1", "Corrigir a divergência de nome de política de CORS: registrar explicitamente a "
     "política 'VueAppPolicy' com WithOrigins(<domínios confiáveis>).WithMethods(...)."
     "WithHeaders(...) em vez de AllowAny*, e remover a AddDefaultPolicy totalmente aberta."),
    ("P2", "Remover ou condicionar EnableSensitiveDataLogging() a builder.Environment."
     "IsDevelopment(), garantindo que nunca seja habilitado em produção."),
    ("P2", "Adicionar testes automatizados de autorização (ex.: teste que POST/PUT/DELETE sem "
     "token retornam 401/403) para evitar regressão após a correção de P1."),
    ("P3", "Ao evoluir para persistência real (SQL Server/Postgres), aplicar o princípio de "
     "isolamento de dados adequado ao modelo de negócio (ex.: se heróis passarem a pertencer a "
     "usuários/organizações, implementar filtro obrigatório por proprietário em todas as "
     "consultas e RLS no banco, se aplicável)."),
    ("P3", "Adicionar cabeçalhos de segurança HTTP padrão (HSTS, X-Content-Type-Options, "
     "Content-Security-Policy) via middleware, já que o pipeline atual não os define."),
]

TOTAL_POR_SEVERIDADE = {
    "CRÍTICA": sum(1 for a in ACHADOS if a["severidade"] == "CRÍTICA"),
    "ALTA": sum(1 for a in ACHADOS if a["severidade"] == "ALTA"),
    "MÉDIA": sum(1 for a in ACHADOS if a["severidade"] == "MÉDIA"),
    "BAIXA": sum(1 for a in ACHADOS if a["severidade"] == "BAIXA"),
}

TOTAL_POR_CATEGORIA = {
    "1. Isolamento tenant": 0,
    "2. Permissão/Autorização": 1,
    "3. IDOR": 0,
    "4. Chaves expostas": 0,
    "5. XSS": 0,
    "Config. insegura (CORS/logs)": 2,
}

ISSUES = [
    {
        "titulo": "[Segurança] API sem autenticação/autorização permite criar, alterar e excluir heróis anonimamente",
        "labels": "security, critical",
        "descricao": (
            "Nenhum endpoint de escrita da HeroesController exige autenticação ou autorização. "
            "Não há esquema de autenticação registrado no pipeline (Program.cs) nem atributos "
            "[Authorize] nos controllers. app.UseAuthorization() está presente, mas sem nenhuma "
            "política configurada, não tem efeito prático."
        ),
        "evidencia": (
            "src/SuperHeroesApi.WebAPI/Controllers/HeroesController.cs:79\n"
            "[HttpPost]\npublic async Task<ActionResult<HeroDto>> CreateHero(...)\n\n"
            "src/SuperHeroesApi.WebAPI/Controllers/HeroesController.cs:112\n"
            "[HttpPut(\"{id}\")]\npublic async Task<ActionResult<HeroDto>> UpdateHero(...)\n\n"
            "src/SuperHeroesApi.WebAPI/Controllers/HeroesController.cs:149\n"
            "[HttpDelete(\"{id}\")]\npublic async Task<ActionResult> DeleteHero(...)"
        ),
        "impacto": (
            "Qualquer usuário anônimo pode adulterar ou destruir todo o catálogo de heróis "
            "(integridade e disponibilidade dos dados comprometidas). Em um cenário real com "
            "dados de negócio, isso representa comprometimento total de escrita da aplicação."
        ),
        "correcao": (
            "1) Adicionar autenticação (ex.: JWT Bearer via AddAuthentication().AddJwtBearer()).\n"
            "2) Adicionar [Authorize] (com papéis/políticas apropriadas) em CreateHero, "
            "UpdateHero e DeleteHero.\n"
            "3) Reavaliar se os endpoints GET também devem exigir autenticação."
        ),
        "criterios": [
            "POST /api/heroes sem token retorna 401",
            "PUT /api/heroes/{id} sem token retorna 401",
            "DELETE /api/heroes/{id} sem token retorna 401",
            "Requisições com token válido e papel adequado continuam funcionando (teste de regressão)",
            "Testes automatizados cobrindo os casos acima adicionados ao projeto de testes",
        ],
    },
    {
        "titulo": "[Segurança] Política de CORS aberta (AllowAny*) e nome de política divergente no middleware",
        "labels": "security, high",
        "descricao": (
            "AddDefaultPolicy define CORS totalmente aberto (AllowAnyOrigin/AllowAnyMethod/"
            "AllowAnyHeader), mas app.UseCors() referencia uma política nomeada 'VueAppPolicy' "
            "que nunca foi registrada. Hoje isso faz com que nenhuma política seja aplicada, mas "
            "é uma armadilha: a correção mais provável (renomear ou remover o parâmetro) ativa "
            "CORS totalmente aberto para todos os métodos e origens."
        ),
        "evidencia": (
            "src/SuperHeroesApi.WebAPI/Program.cs:48-55\n"
            "options.AddDefaultPolicy(policy => {\n"
            "  policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();\n"
            "});\n\n"
            "src/SuperHeroesApi.WebAPI/Program.cs:79\n"
            "app.UseCors(\"VueAppPolicy\");"
        ),
        "impacto": (
            "Se corrigido ingenuamente, qualquer site poderá fazer requisições cross-origin "
            "(incluindo POST/PUT/DELETE) contra a API a partir do navegador de qualquer usuário, "
            "ampliando a superfície de ataque, inclusive de CSRF via fetch/XHR."
        ),
        "correcao": (
            "Registrar explicitamente uma política nomeada 'VueAppPolicy' com "
            "WithOrigins(<domínios confiáveis do frontend>), WithMethods(<métodos necessários>) "
            "e WithHeaders(<headers necessários>), removendo AllowAnyOrigin/AllowAnyMethod/"
            "AllowAnyHeader e a AddDefaultPolicy totalmente aberta."
        ),
        "criterios": [
            "Política CORS nomeada 'VueAppPolicy' registrada e referenciada corretamente",
            "Apenas origens explicitamente confiáveis são permitidas",
            "Requisição cross-origin de um domínio não listado é bloqueada (teste manual/automatizado)",
            "Frontend legítimo continua funcionando após a mudança",
        ],
    },
    {
        "titulo": "[Segurança] EnableSensitiveDataLogging() habilitado sem gate de ambiente",
        "labels": "security, medium",
        "descricao": (
            "O registro do DbContext ativa EnableSensitiveDataLogging() incondicionalmente, sem "
            "verificar se o ambiente é Development. Essa opção inclui valores de parâmetros de "
            "consulta em logs e mensagens de exceção."
        ),
        "evidencia": (
            "src/SuperHeroesApi.Infrastructure/Extensions/InfrastructureService.cs:19-20\n"
            "options.UseInMemoryDatabase(\"SuperHeroesDB\")\n"
            "      .EnableSensitiveDataLogging());"
        ),
        "impacto": (
            "Caso o provedor InMemory seja substituído por um banco real em produção sem remover "
            "essa configuração, dados sensíveis podem ser gravados em logs/telemetria, violando "
            "boas práticas de proteção de dados."
        ),
        "correcao": (
            "Condicionar EnableSensitiveDataLogging() a builder.Environment.IsDevelopment(), ou "
            "removê-lo e usar logging de diagnóstico apenas localmente via configuração dedicada."
        ),
        "criterios": [
            "EnableSensitiveDataLogging() não é chamado quando ASPNETCORE_ENVIRONMENT=Production",
            "Build de produção não expõe valores de parâmetros SQL em logs de exceção",
        ],
    },
]

# ---------------------------------------------------------------------------
# Gráficos
# ---------------------------------------------------------------------------

def gerar_grafico_rosca(path):
    labels = []
    sizes = []
    cores = []
    mapa_cor = {"CRÍTICA": COR_CRITICA, "ALTA": COR_ALTA, "MÉDIA": COR_MEDIA, "BAIXA": COR_BAIXA}
    for sev, qtd in TOTAL_POR_SEVERIDADE.items():
        if qtd > 0:
            labels.append(f"{sev} ({qtd})")
            sizes.append(qtd)
            cores.append(mapa_cor[sev])

    fig, ax = plt.subplots(figsize=(5, 4.2), dpi=200)
    wedges, texts, autotexts = ax.pie(
        sizes, labels=labels, colors=cores, autopct=lambda p: f"{int(round(p * sum(sizes) / 100))}",
        startangle=90, pctdistance=0.75,
        wedgeprops=dict(width=0.42, edgecolor="white", linewidth=2),
        textprops={"fontsize": 10}
    )
    for at in autotexts:
        at.set_color("white")
        at.set_fontweight("bold")
    ax.set_title("Achados por Severidade", fontsize=13, fontweight="bold")
    fig.tight_layout()
    fig.savefig(path, transparent=True)
    plt.close(fig)


def gerar_grafico_barras(path):
    categorias = list(TOTAL_POR_CATEGORIA.keys())
    valores = list(TOTAL_POR_CATEGORIA.values())
    cores_barras = [COR_INFO if v == 0 else COR_ALTA for v in valores]

    fig, ax = plt.subplots(figsize=(7, 4.2), dpi=200)
    bars = ax.barh(categorias, valores, color=cores_barras)
    ax.set_xlabel("Nº de achados")
    ax.set_title("Achados por Categoria", fontsize=13, fontweight="bold")
    ax.invert_yaxis()
    for bar, v in zip(bars, valores):
        ax.text(bar.get_width() + 0.05, bar.get_y() + bar.get_height() / 2, str(v),
                 va="center", fontsize=9, fontweight="bold")
    ax.spines["top"].set_visible(False)
    ax.spines["right"].set_visible(False)
    fig.tight_layout()
    fig.savefig(path, transparent=True)
    plt.close(fig)


# ---------------------------------------------------------------------------
# PDF
# ---------------------------------------------------------------------------

styles = getSampleStyleSheet()
styles.add(ParagraphStyle(name="TituloCapa", fontSize=24, leading=28, alignment=TA_CENTER,
                           fontName="Helvetica-Bold", textColor=colors.HexColor("#111827"),
                           spaceAfter=14))
styles.add(ParagraphStyle(name="SubtituloCapa", fontSize=13, leading=18, alignment=TA_CENTER,
                           textColor=colors.HexColor("#374151"), spaceAfter=6))
styles.add(ParagraphStyle(name="H1", fontSize=17, leading=21, fontName="Helvetica-Bold",
                           textColor=colors.HexColor("#111827"), spaceBefore=6, spaceAfter=10))
styles.add(ParagraphStyle(name="H2", fontSize=13, leading=16, fontName="Helvetica-Bold",
                           textColor=colors.HexColor("#1F2937"), spaceBefore=12, spaceAfter=6))
styles.add(ParagraphStyle(name="Corpo", fontSize=9.5, leading=13.5, alignment=TA_JUSTIFY,
                           textColor=colors.HexColor("#1F2937")))
styles.add(ParagraphStyle(name="CorpoPequeno", fontSize=8.5, leading=11.5, alignment=TA_JUSTIFY,
                           textColor=colors.HexColor("#374151")))
styles.add(ParagraphStyle(name="Codigo", fontSize=7.6, leading=10, fontName="Courier",
                           textColor=colors.HexColor("#111827"), backColor=colors.HexColor("#F3F4F6"),
                           borderPadding=6, leftIndent=2))
styles.add(ParagraphStyle(name="Mono", fontSize=7.6, leading=10, fontName="Courier",
                           textColor=colors.HexColor("#111827")))


class ChipSeveridade(Flowable):
    """Chip colorido com a severidade."""
    def __init__(self, texto, cor_hex, width=58, height=14):
        super().__init__()
        self.texto = texto
        self.cor = colors.HexColor(cor_hex)
        self.width = width
        self.height = height

    def wrap(self, availWidth, availHeight):
        return self.width, self.height

    def draw(self):
        c = self.canv
        c.setFillColor(self.cor)
        c.roundRect(0, 0, self.width, self.height, 4, fill=1, stroke=0)
        c.setFillColor(colors.white)
        c.setFont("Helvetica-Bold", 6.6)
        c.drawCentredString(self.width / 2, self.height / 2 - 2.3, self.texto)


def header_footer(canvas, doc):
    canvas.saveState()
    canvas.setFont("Helvetica", 7.5)
    canvas.setFillColor(colors.HexColor("#6B7280"))
    canvas.drawString(2 * cm, A4[1] - 1.3 * cm, "Relatório de Auditoria de Segurança — SuperHeroesApi")
    canvas.drawRightString(A4[0] - 2 * cm, A4[1] - 1.3 * cm, "Confidencial")
    canvas.setStrokeColor(colors.HexColor("#E5E7EB"))
    canvas.line(2 * cm, A4[1] - 1.45 * cm, A4[0] - 2 * cm, A4[1] - 1.45 * cm)

    canvas.line(2 * cm, 1.6 * cm, A4[0] - 2 * cm, 1.6 * cm)
    canvas.drawString(2 * cm, 1.2 * cm, "SuperHeroesApi — Auditoria de Segurança")
    canvas.drawRightString(A4[0] - 2 * cm, 1.2 * cm, f"Página {doc.page}")
    canvas.restoreState()


def build_pdf():
    doc = BaseDocTemplate(
        OUT_PDF, pagesize=A4,
        leftMargin=2 * cm, rightMargin=2 * cm, topMargin=2 * cm, bottomMargin=2 * cm,
        title="Relatório de Auditoria de Segurança - SuperHeroesApi",
    )
    frame = Frame(doc.leftMargin, doc.bottomMargin, doc.width, doc.height, id="normal")
    doc.addPageTemplates([PageTemplate(id="all", frames=frame, onPage=header_footer)])

    story = []

    # --- Capa ---
    story.append(Spacer(1, 4 * cm))
    story.append(Paragraph("Relatório de Auditoria de Segurança", styles["TituloCapa"]))
    story.append(Paragraph("SuperHeroesApi", styles["TituloCapa"]))
    story.append(Spacer(1, 0.6 * cm))
    story.append(Paragraph("Data: 23 de setembro de 2026", styles["SubtituloCapa"]))
    story.append(Paragraph("Repositório: MarcosMuriloPJ/SuperHeroesApi", styles["SubtituloCapa"]))
    story.append(Spacer(1, 1 * cm))
    story.append(HRFlowable(width="60%", thickness=1, color=colors.HexColor("#D1D5DB"), hAlign="CENTER"))
    story.append(Spacer(1, 1 * cm))

    escopo = (
        "Escopo auditado: solução .NET 8 (ASP.NET Core Web API) composta por quatro projetos — "
        "SuperHeroesApi.Domain, SuperHeroesApi.Application, SuperHeroesApi.Infrastructure e "
        "SuperHeroesApi.WebAPI — implementando um CRUD de super-heróis e superpoderes sobre "
        "Entity Framework Core com provedor InMemory. Não há projeto de frontend, Dockerfile, "
        "pipeline de CI/CD, Helm chart ou Terraform neste repositório."
    )
    story.append(Paragraph(escopo, styles["Corpo"]))
    story.append(Spacer(1, 0.5 * cm))

    metodologia = (
        "<b>Nota metodológica — mapeamento das categorias para a stack detectada:</b><br/>"
        "(1) Isolamento de tenant/dono: não há Supabase/RLS nem middleware de tenant nesta stack; "
        "buscou-se qualquer coluna de propriedade (owner_id/user_id/tenant_id) e filtro "
        "correspondente nas queries. Nenhum foi encontrado porque o domínio não modela usuários. "
        "(2) Permissão definida no navegador: como não há frontend no repositório, adaptou-se a "
        "verificação para o equivalente de backend — presença de [Authorize]/políticas de "
        "autorização do ASP.NET Core nos controllers. (3) IDOR: percorridos todos os handlers de "
        "HeroesController e SuperpowersController verificando o tratamento do parâmetro {id} "
        "frente a qualquer modelo de posse. (4) Chaves expostas: inspecionados appsettings*.json, "
        "código de configuração de infraestrutura (DbContext) e ausência de arquivos de deploy. "
        "(5) XSS: verificada a ausência de geração de HTML/e-mails no backend, dado que a API "
        "expõe somente JSON."
    )
    story.append(Paragraph(metodologia, styles["CorpoPequeno"]))
    story.append(PageBreak())

    # --- Resumo executivo ---
    story.append(Paragraph("Resumo Executivo", styles["H1"]))

    total = len(ACHADOS)
    resumo_txt = (
        f"Foram identificados <b>{total} achados</b> classificados como: "
        f"<font color='{COR_CRITICA}'><b>{TOTAL_POR_SEVERIDADE['CRÍTICA']} crítico(s)</b></font>, "
        f"<font color='{COR_ALTA}'><b>{TOTAL_POR_SEVERIDADE['ALTA']} alto(s)</b></font>, "
        f"<font color='{COR_MEDIA}'><b>{TOTAL_POR_SEVERIDADE['MÉDIA']} médio(s)</b></font> e "
        f"<font color='{COR_BAIXA}'><b>{TOTAL_POR_SEVERIDADE['BAIXA']} baixo(s)</b></font>. "
        "Quatro das cinco categorias solicitadas (isolamento de tenant, IDOR, chaves hardcoded e "
        "XSS) não se aplicam a este repositório pela natureza da stack (API sem conceito de "
        "usuário/tenant, sem frontend e sem arquivos de deploy) — ver seção 'Categorias não "
        "aplicáveis'. O achado central e mais grave é a <b>ausência completa de autenticação e "
        "autorização</b> em toda a API."
    )
    story.append(Paragraph(resumo_txt, styles["Corpo"]))
    story.append(Spacer(1, 0.3 * cm))

    grafico_rosca = os.path.join(HERE, "_grafico_rosca.png")
    grafico_barras = os.path.join(HERE, "_grafico_barras.png")
    gerar_grafico_rosca(grafico_rosca)
    gerar_grafico_barras(grafico_barras)

    tbl_graficos = Table(
        [[Image(grafico_rosca, width=8 * cm, height=6.4 * cm),
          Image(grafico_barras, width=9.2 * cm, height=6.4 * cm)]],
        colWidths=[8.2 * cm, 9.4 * cm]
    )
    story.append(tbl_graficos)
    story.append(Spacer(1, 0.4 * cm))

    # --- Pontos fortes ---
    story.append(Paragraph("Pontos Fortes (verificados e corretos)", styles["H2"]))
    dados_fortes = [["Item", "Evidência"]]
    for titulo, evid in PONTOS_FORTES:
        dados_fortes.append([Paragraph(f"<b>{titulo}</b>", styles["CorpoPequeno"]),
                              Paragraph(evid, styles["CorpoPequeno"])])
    t_fortes = Table(dados_fortes, colWidths=[6.2 * cm, 10.8 * cm], repeatRows=1)
    t_fortes.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor(COR_FORTE)),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("FONTSIZE", (0, 0), (-1, 0), 9),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#D1D5DB")),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#F0FDF4")]),
        ("TOPPADDING", (0, 0), (-1, -1), 5),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 5),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
    ]))
    story.append(t_fortes)
    story.append(Spacer(1, 0.4 * cm))

    # --- Pontos fracos ---
    story.append(Paragraph("Pontos Fracos (riscos centrais)", styles["H2"]))
    for a in ACHADOS:
        linha = (f"<b>[{a['severidade']}] {a['titulo']}</b> — {a['arquivo']}")
        story.append(Paragraph(linha, styles["CorpoPequeno"]))
        story.append(Spacer(1, 0.15 * cm))
    story.append(PageBreak())

    # --- Categorias não aplicáveis ---
    story.append(Paragraph("Categorias Não Aplicáveis a Esta Stack", styles["H1"]))
    for cat, motivo in NAO_APLICAVEIS:
        story.append(Paragraph(f"<b>{cat}</b>", styles["H2"]))
        story.append(Paragraph(motivo, styles["Corpo"]))
    story.append(PageBreak())

    # --- Achados detalhados ---
    story.append(Paragraph("Achados Detalhados", styles["H1"]))
    for a in ACHADOS:
        bloco = []
        cab = Table(
            [[ChipSeveridade(a["severidade"], a["cor"]),
              Paragraph(f"<b>{a['id']} — {a['titulo']}</b>", styles["Corpo"])]],
            colWidths=[2.2 * cm, 14.8 * cm]
        )
        cab.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "MIDDLE")]))
        bloco.append(cab)
        bloco.append(Spacer(1, 0.15 * cm))
        bloco.append(Paragraph(f"<b>Categoria:</b> {a['categoria']}", styles["CorpoPequeno"]))
        bloco.append(Paragraph(f"<b>Arquivo:linha:</b> {a['arquivo']}", styles["CorpoPequeno"]))
        bloco.append(Spacer(1, 0.1 * cm))
        bloco.append(Paragraph(a["descricao"], styles["Corpo"]))
        bloco.append(Spacer(1, 0.1 * cm))
        bloco.append(Paragraph(a["codigo"].replace("\n", "<br/>").replace(" ", "&nbsp;"), styles["Codigo"]))
        bloco.append(Spacer(1, 0.1 * cm))
        bloco.append(Paragraph(f"<b>Condição de explorabilidade:</b> {a['explorabilidade']}", styles["CorpoPequeno"]))
        bloco.append(Spacer(1, 0.35 * cm))
        bloco.append(HRFlowable(width="100%", thickness=0.6, color=colors.HexColor("#E5E7EB")))
        bloco.append(Spacer(1, 0.25 * cm))
        story.append(KeepTogether(bloco))

    story.append(PageBreak())

    # --- Recomendações ---
    story.append(Paragraph("Recomendações Priorizadas", styles["H1"]))
    dados_rec = [["Prioridade", "Recomendação"]]
    for prio, rec in RECOMENDACOES:
        dados_rec.append([Paragraph(f"<b>{prio}</b>", styles["CorpoPequeno"]),
                           Paragraph(rec, styles["CorpoPequeno"])])
    t_rec = Table(dados_rec, colWidths=[2 * cm, 15 * cm], repeatRows=1)
    t_rec.setStyle(TableStyle([
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#111827")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 0), (-1, 0), "Helvetica-Bold"),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#D1D5DB")),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#F9FAFB")]),
        ("TOPPADDING", (0, 0), (-1, -1), 5),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 5),
    ]))
    story.append(t_rec)
    story.append(PageBreak())

    # --- Issues para o GitHub ---
    story.append(Paragraph("Issues para o GitHub", styles["H1"]))
    story.append(Paragraph(
        "As issues abaixo estão prontas para copiar e colar no GitHub, uma por bloco delimitado.",
        styles["CorpoPequeno"]
    ))
    story.append(Spacer(1, 0.2 * cm))

    for i, issue in enumerate(ISSUES, start=1):
        bloco = []
        bloco.append(Paragraph(f"--- ISSUE {i} ---", styles["Mono"]))
        bloco.append(Spacer(1, 0.1 * cm))
        md = []
        md.append(f"## {issue['titulo']}")
        md.append("")
        md.append(f"**Labels sugeridas:** {issue['labels']}")
        md.append("")
        md.append("### Descrição do problema")
        md.append(issue["descricao"])
        md.append("")
        md.append("### Evidência")
        md.append("```csharp")
        md.append(issue["evidencia"])
        md.append("```")
        md.append("")
        md.append("### Impacto")
        md.append(issue["impacto"])
        md.append("")
        md.append("### Sugestão de correção")
        md.append(issue["correcao"])
        md.append("")
        md.append("### Critérios de aceite")
        for c in issue["criterios"]:
            md.append(f"- [ ] {c}")
        texto_md = "\n".join(md)
        for linha in texto_md.split("\n"):
            linha_html = linha.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
            linha_html = linha_html.replace(" ", "&nbsp;") if linha_html.startswith("```") else linha_html
            bloco.append(Paragraph(linha_html if linha_html else "&nbsp;", styles["Mono"]))
        bloco.append(Spacer(1, 0.1 * cm))
        bloco.append(Paragraph(f"--- FIM ISSUE {i} ---", styles["Mono"]))
        bloco.append(Spacer(1, 0.3 * cm))
        story.append(KeepTogether(bloco))

    doc.build(story)

    for tmp in (grafico_rosca, grafico_barras):
        try:
            os.remove(tmp)
        except OSError:
            pass

    print(f"PDF gerado em: {OUT_PDF}")


if __name__ == "__main__":
    build_pdf()
