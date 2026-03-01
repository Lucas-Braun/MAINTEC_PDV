# API PDV Desktop v1

API REST para o aplicativo PDV Desktop (C# / WPF / .NET 8).

**Prefixo**: `/api/v1/pdv`

---

## Autenticacao

Autenticacao via Bearer Token. O login com email/senha identifica o usuario e a empresa automaticamente.

### Fluxo

```
1. POST /auth/login  { login, senha }  → retorna Bearer token
2. Todas as demais rotas: Authorization: Bearer <token>
```

**Nao precisa de API Key.** O PDV e app interno do ERP — o email do operador ja identifica a empresa.

### Bearer Token

Header `Authorization` obrigatorio em todas as rotas **exceto** `/auth/login`.

```
Authorization: Bearer <token>
```

- Gerado pelo `POST /auth/login` apos autenticacao com email/senha
- Assinado com `itsdangerous.URLSafeTimedSerializer` (salt: `pdv-api-v1`)
- TTL: 24 horas — renovar via `POST /auth/refresh` antes de expirar
- Contem: `emp_in_codigo`, `org_in_codigo`, `fil_in_codigo`, `user_in_codigo`

### Contexto no Servidor

O decorator `@require_pdv_auth` seta:
- `g.pdv` (dict): `emp_in_codigo`, `org_in_codigo`, `fil_in_codigo`, `user_in_codigo`, `user_st_nome`
- `g.ctx` (SimpleNamespace): compatibilidade com helpers existentes

### Validacoes Automaticas

Em cada requisicao autenticada:
1. Token nao expirado (< 24h)
2. Usuario existe e esta ativo (`user_bo_ativo = 'A'`)

### Codigos de Erro de Autenticacao

| HTTP | `code` | Significado |
|------|--------|-------------|
| 401 | — | Token ausente ou vazio |
| 401 | `TOKEN_EXPIRED` | Token expirado — fazer login novamente |
| 401 | — | Token invalido ou usuario nao encontrado |
| 403 | — | Usuario inativo |

---

## Respostas Padrao

Todas as respostas sao JSON.

**Sucesso:**
```json
{
    "success": true,
    "message": "Descricao da acao",
    "dados": { ... }
}
```

**Erro:**
```json
{
    "success": false,
    "error": "Mensagem de erro legivel"
}
```

**Codigos HTTP:**
| Codigo | Uso |
|--------|-----|
| 200 | Sucesso |
| 201 | Criado (cadastro de cliente) |
| 400 | Validacao falhou (ValueError) |
| 401 | Nao autenticado |
| 403 | Sem permissao / usuario inativo |
| 404 | Recurso nao encontrado |
| 409 | Conflito (duplicidade / idempotencia em andamento) |
| 500 | Erro interno |

---

## Rate Limiting

Todos os endpoints possuem rate limit por IP.

| Tipo de operacao | Limite |
|------------------|--------|
| Leitura (GET) | 30-60 req/min |
| Escrita (POST) | 10-30 req/min |
| Login | 10 req/min |

---

## Endpoints

### Auth (`routes_auth.py`)

#### POST /auth/login

Login do operador. Unico endpoint que nao exige Bearer Token.
Aceita email ou username. Mesma logica do login web (bloqueio por tentativas, argon2).

**Body:**
```json
{
    "login": "operador@empresa.com",
    "senha": "..."
}
```

O campo `login` aceita email ou username do operador.
O login identifica automaticamente a empresa — nao precisa de API Key.

**Response (sucesso):**
```json
{
    "success": true,
    "token": "<bearer_token>",
    "usuario": { "id": 1, "nome": "Joao", "email": "joao@empresa.com" },
    "empresa": { "id": 1, "nome": "Empresa LTDA", "cnpj": "12.345.678/0001-00" },
    "filial":  { "id": 1, "nome": "Matriz", "org_id": 1, "org_nome": "Org Principal" },
    "filiais": [
        { "id": 1, "nome": "Matriz", "cnpj": "12.345.678/0001-00", "org_id": 1, "org_nome": "Org Principal" },
        { "id": 2, "nome": "Filial 2", "cnpj": "12.345.678/0002-00", "org_id": 1, "org_nome": "Org Principal" }
    ]
}
```

**Filiais:** Vem da tabela `glo_user_filial` (mesma logica da `vw_sessao_usuario`).
Retorna TODAS as filiais que o usuario tem acesso, podendo ser de organizacoes diferentes.
A filial inicial e a default do cadastro do usuario (`glo_users.fil_in_codigo`), ou a primeira da lista se a default nao estiver nas permitidas.

**Response (senha incorreta):**
```json
{ "success": false, "error": "Senha incorreta. 3 tentativa(s) restante(s).", "tentativas_restantes": 3 }
```

**Response (conta bloqueada):**
```json
{ "success": false, "error": "Conta bloqueada. Tente novamente em 8 minuto(s).", "code": "ACCOUNT_LOCKED", "minutos_restantes": 8 }
```

**Erros:**
- 400: Login e senha obrigatorios
- 401: Credenciais invalidas / senha incorreta (com tentativas restantes)
- 403: Usuario inativo
- 429: Conta bloqueada por tentativas (com minutos restantes)

**Seguranca (mesma logica do login web):**
- Senha validada com `argon2.PasswordHasher`
- Bloqueio por tentativas: padrao 5 tentativas, bloqueio 10 minutos (configuravel por usuario)
- Reset de tentativas apos login bem-sucedido
- Atualiza `user_dt_ultimo_login` e `user_ip_ultimo_login`

---

#### GET /auth/sessao

Dados completos da sessao atual. Usado no startup do app para popular o estado.

**Response:**
```json
{
    "success": true,
    "empresa": { "emp_in_codigo": 1, "emp_st_nome": "...", "emp_st_cnpj": "...", "emp_st_ie": "..." },
    "filial": { "fil_in_codigo": 1, "fil_st_nome": "...", "fil_st_cnpj": "...", "fil_st_endereco": "...", "fil_st_logradouro": "...", "fil_st_numero": "...", "fil_st_bairro": "...", "fil_st_cep": "...", "fil_st_telefone": "..." },
    "usuario": { "user_in_codigo": 1, "user_st_nome": "...", "user_st_email": "..." }
}
```

---

#### GET /auth/filiais

Lista todas as filiais que o usuario tem acesso (mesma logica do `filial_switcher.py` / `vw_sessao_usuario`).
Consulta `glo_user_filial` → `glo_filial` → `glo_organizacao`.

**Response:**
```json
{
    "success": true,
    "filiais": [
        { "id": 1, "nome": "Matriz", "cnpj": "12.345.678/0001-00", "org_id": 1, "org_nome": "Org Principal", "atual": true },
        { "id": 2, "nome": "Filial SP", "cnpj": "12.345.678/0002-00", "org_id": 1, "org_nome": "Org Principal", "atual": false },
        { "id": 1, "nome": "Filial RJ", "cnpj": "98.765.432/0001-00", "org_id": 2, "org_nome": "Org Sudeste", "atual": false }
    ],
    "filial_atual": { "org_id": 1, "id": 1 }
}
```

**Notas:**
- `atual: true` marca a filial ativa na sessao (do token)
- Um usuario pode ter filiais em organizacoes diferentes
- Filtros: `userfil_bo_ativo = 'S'`, `fil_bo_ativo = 'A'`, `org_bo_ativo = 'A'`

---

#### POST /auth/trocar-filial

Troca a filial+organizacao do operador. Valida acesso via `glo_user_filial`.
Retorna novo token com a nova org+fil.

**Body:**
```json
{ "org_in_codigo": 2, "fil_in_codigo": 1 }
```

**IMPORTANTE:** Enviar AMBOS `org_in_codigo` e `fil_in_codigo` (a mesma filial pode existir em orgs diferentes).

**Response:**
```json
{
    "success": true,
    "token": "<novo_bearer_token>",
    "filial": { "id": 1, "nome": "Filial RJ", "cnpj": "98.765.432/0001-00", "org_id": 2, "org_nome": "Org Sudeste" }
}
```

**Erros:**
- 400: `org_in_codigo` e `fil_in_codigo` obrigatorios
- 403: Voce nao tem acesso a esta filial ou ela esta inativa

---

#### POST /auth/refresh

Renova o token antes de expirar. Retorna novo token com mesmo contexto e novo TTL de 24h.

**Response:**
```json
{
    "success": true,
    "token": "<novo_bearer_token>"
}
```

**Recomendacao:** Chamar a cada ~12h ou quando `TOKEN_EXPIRED` for retornado.

---

### Caixa (`routes_caixa.py`)

#### GET /caixa/config-terminal

Configuracao de terminal para abertura de caixa.

**Response:**
```json
{
    "success": true,
    "usar_terminal_fixo": false,
    "terminais": [
        { "ter_in_codigo": 1, "ter_st_nome": "CAIXA-01", "setor_nome": "Loja" }
    ],
    "terminal_operador": null
}
```

Se `usar_terminal_fixo = true`, retorna `terminal_operador` com o terminal vinculado ao operador (nao precisa selecionar).

---

#### GET /caixa/status-turno

Status do turno atual para o operador. Usado para alertas de fim de turno.

**Response (turno ativo):**
```json
{
    "success": true,
    "turno_ativo": true,
    "turno_atual": { "tur_in_codigo": 1, "tur_st_nome": "Manha" },
    "autorizado": true,
    "minutos_restantes": 45,
    "em_tolerancia": false,
    "alerta": null
}
```

**Response (turno desativado):**
```json
{
    "success": true,
    "turno_ativo": false,
    "mensagem": "Sistema de turnos desativado"
}
```

**Alertas possiveis:**
- `"Turno termina em X minutos"` — dentro do aviso configurado
- `"Turno encerrado! Tolerancia: X min restantes"` — apos fim, dentro da tolerancia

---

#### GET /caixa/status

Status do caixa (aberto/fechado).

**Response (aberto):**
```json
{
    "success": true,
    "caixa_aberto": true,
    "caixa": { "cai_in_codigo": 5, "cai_dt_abertura": "...", "cai_re_vl_abertura": 200.00, "..." }
}
```

**Response (fechado):**
```json
{ "success": true, "caixa_aberto": false }
```

---

#### POST /caixa/abrir

Abre um novo caixa.

**Body:**
```json
{
    "valor_abertura": 200.00,
    "ter_in_codigo": 1
}
```

- `ter_in_codigo`: obrigatorio se `usar_terminal_fixo = false`, ignorado se `true` (auto-detect)
- Valida turno automaticamente se sistema de turnos estiver ativo

**Response:**
```json
{
    "success": true,
    "message": "Caixa #5 aberto",
    "caixa": { "cai_in_codigo": 5, "..." }
}
```

**Erros:**
- 400: Operador nao vinculado a terminal (modo fixo)
- 400: Caixa ja aberto para este operador
- 403: Nao autorizado para o turno atual

---

#### POST /caixa/fechar

Fecha o caixa aberto.

**Body:**
```json
{ "valor_fechamento": 350.00 }
```

**Response:**
```json
{
    "success": true,
    "message": "Caixa #5 fechado",
    "resultado": {
        "cai_in_codigo": 5,
        "valor_esperado": 345.00,
        "valor_fechamento": 350.00,
        "diferenca": 5.00,
        "dt_fechamento": "2026-02-28T18:00:00"
    }
}
```

---

#### POST /caixa/sangria

Registra sangria (retirada de dinheiro do caixa).

**Body:**
```json
{ "valor": 100.00, "observacao": "Deposito banco" }
```

**Response:**
```json
{
    "success": true,
    "message": "Sangria de R$ 100.00 registrada",
    "movimento": { "mov_in_codigo": 12, "..." }
}
```

---

#### POST /caixa/suprimento

Registra suprimento (entrada de dinheiro no caixa).

**Body:**
```json
{ "valor": 50.00, "observacao": "Troco extra" }
```

**Response:**
```json
{
    "success": true,
    "message": "Suprimento de R$ 50.00 registrado",
    "movimento": { "mov_in_codigo": 13, "..." }
}
```

---

#### GET /caixa/resumo

Resumo detalhado do caixa aberto (totais por tipo de movimento).

**Response:**
```json
{
    "success": true,
    "caixa_aberto": true,
    "caixa": { "cai_in_codigo": 5, "..." },
    "saldo_atual": 345.00,
    "totais": { "vendas": 250.00, "sangrias": 100.00, "suprimentos": 50.00, "estornos": -5.00, "..." }
}
```

---

#### GET /caixa/movimentos

Lista movimentos do caixa aberto.

**Query:** `?limit=50&offset=0`

**Response:**
```json
{
    "success": true,
    "movimentos": [
        { "mov_in_codigo": 1, "mov_st_tipo": "VE", "mov_re_valor": 91.80, "ped_in_codigo": 123, "mov_dt_criado": "...", "..." }
    ]
}
```

**Tipos de movimento:**
| Tipo | Descricao | Valor |
|------|-----------|-------|
| `VE` | Venda | Positivo |
| `SA` | Sangria | Positivo |
| `SU` | Suprimento | Positivo |
| `AJ` | Ajuste | +/- |
| `ES` | Estorno | Negativo |

---

### Venda (`routes_venda.py`)

#### POST /venda/finalizar-direto

**Endpoint principal de venda.** Cria pedido + itens + finaliza em um passo.
O carrinho e mantido em memoria no app desktop — nao existe rascunho no servidor.

**Headers:**
```
X-Idempotency-Key: 550e8400-e29b-41d4-a716-446655440000
```

**IMPORTANTE:** Sempre enviar `X-Idempotency-Key` (UUID v4). Previne vendas duplicadas em caso de retry, double-click ou queda de conexao.

**Body:**
```json
{
    "itens": [
        { "pro_in_codigo": 123, "quantidade": 2, "preco_unitario": 45.90, "desconto_perc": 0 },
        { "pro_in_codigo": 456, "quantidade": 1, "preco_unitario": 8.50, "desconto_perc": 5 }
    ],
    "parcelas": [
        { "fcb_in_codigo": 1, "valor": 100.30, "vencimento": "2026-02-28" }
    ],
    "cpf_nota": "12345678900",
    "troco": 8.20
}
```

**Campos do item:**
| Campo | Obrigatorio | Descricao |
|-------|-------------|-----------|
| `pro_in_codigo` | Sim | Codigo do produto |
| `quantidade` | Sim | Quantidade |
| `preco_unitario` | Nao | **IGNORADO** — servidor busca preco real da tabela PDV |
| `desconto_perc` | Nao | Desconto %, validado contra maximo configurado |

**Campos da parcela:**
| Campo | Obrigatorio | Descricao |
|-------|-------------|-----------|
| `fcb_in_codigo` | Sim | Forma de pagamento (FK `fin_forma_cobranca`) |
| `valor` | Sim | Valor da parcela |
| `vencimento` | Nao | Data de vencimento (YYYY-MM-DD) |

**Outros campos:**
| Campo | Obrigatorio | Descricao |
|-------|-------------|-----------|
| `cpf_nota` | Nao | CPF/CNPJ para a NFC-e (somente digitos) |
| `troco` | Nao | Valor do troco (calculado: recebido - total) |

**Response (sucesso):**
```json
{
    "success": true,
    "message": "Venda #12345 finalizada",
    "resultado": {
        "ped_in_codigo": 12345,
        "nf_in_codigo": 678,
        "valor_venda": 100.30,
        "troco": 8.20,
        "nfce_status": "NFE_AUT",
        "nfce_chave": "3526..."
    }
}
```

**Response (idempotencia — venda ja processada):**
```json
{
    "success": true,
    "message": "Venda ja processada anteriormente",
    "from_cache": true,
    "resultado": { "ped_in_codigo": 12345 }
}
```

**Erros:**
- 400: Nenhum item / nenhuma parcela / desconto acima do permitido / estoque insuficiente
- 400: Caixa nao aberto
- 409: Requisicao em andamento (retry muito rapido)

**Invariantes:**
1. **Servidor recalcula tudo** — precos vem da tabela PDV (`tab_bo_pdv = 'S'`), nunca do payload
2. **Idempotencia** — mesma `X-Idempotency-Key` = mesmo resultado (tabela `pdv_idempotency`, expira em 24h)
3. **Caixa obrigatorio** — nao existe venda sem caixa aberto
4. **Baixa de estoque automatica** — via `EstoqueHelper` ao finalizar
5. **Grava direto em `ven_notafiscal`** — NAO usa `ven_pedidovenda` intermediario. O `nf_in_codigo` retornado e o ID principal da venda

---

#### GET /vendas

Lista vendas PDV com filtros. Consulta direto de `ven_notafiscal` (modelo 65 / NFC-e).

**Query:**
| Parametro | Default | Descricao |
|-----------|---------|-----------|
| `data_inicio` | hoje | Data inicio (YYYY-MM-DD) |
| `data_fim` | hoje | Data fim (YYYY-MM-DD) |
| `status` | todos | `NFE_AUT`, `NFE_REJ`, `NFE_CAN`, `VEN_CAN`, `todos` |
| `nf` | — | Numero da NF (`nf_in_codigo`) |
| `caixa` | — | Numero do caixa |
| `limit` | 50 | Max 200 |

**Response:**
```json
{
    "success": true,
    "vendas": [
        {
            "nf_in_codigo": 678,
            "nf_numero": 123,
            "data": "2026-02-28T14:30:00",
            "hora": "14:30",
            "status": "NFE_AUT",
            "status_nome": "Autorizada",
            "status_cor": "#28a745",
            "total": 91.80,
            "qtd_itens": 2,
            "cliente_nome": "Consumidor Final",
            "cpf_nota": null,
            "forma_pagamento": "Dinheiro",
            "caixa": 5,
            "chave": "3526...",
            "motivo_rejeicao": null
        }
    ],
    "total_registros": 1,
    "filtros": { "data_inicio": "2026-02-28", "data_fim": "2026-02-28", "status": "", "caixa": "" }
}
```

**NOTA:** As vendas PDV sao gravadas diretamente em `ven_notafiscal` (NFC-e modelo 65), nao usam `ven_pedidovenda`.

---

#### GET /venda/{nf_in_codigo}

Dados da venda (ven_notafiscal) com itens (ven_itemnotafiscal).

**Response:**
```json
{
    "success": true,
    "venda": {
        "nf_in_codigo": 678,
        "valor_total": 91.80,
        "valor_recebido": 100.00,
        "troco": 0,
        "status": "NFE_AUT",
        "status_nome": "Autorizada",
        "data_emissao": "2026-02-28T14:30:00",
        "cpf_nota": "***456789**",
        "tipo_documento": "NFC-e",
        "forma_pagamento": "Dinheiro",
        "qtd_itens": 2,
        "nfce": {
            "numero": 678,
            "status": "NFE_AUT",
            "chave": "3526...",
            "mensagem": null,
            "autorizada": true,
            "rejeitada": false,
            "processando": false
        }
    },
    "itens": [
        {
            "sequencia": 1,
            "pro_in_codigo": 123,
            "codigo": "ARR001",
            "descricao": "Arroz Tipo 1 5kg",
            "unidade": "UN",
            "quantidade": 2.0,
            "preco_unitario": 45.90,
            "total": 91.80,
            "ean": "7891234567890",
            "ncm": "10063021",
            "cfop": "5102"
        }
    ]
}
```

---

#### POST /venda/{nf_in_codigo}/estornar

Estorna venda PDV. Cancela a NF, cria movimento ES negativo no caixa, devolve estoque.
So funciona se NFC-e **nao** estiver autorizada (cancelar NFC-e via SEFAZ primeiro).

**Body:**
```json
{ "motivo": "Cliente desistiu" }
```

**Response:**
```json
{
    "success": true,
    "message": "Venda #678 estornada",
    "resultado": {
        "nf_in_codigo": 678,
        "mov_estorno": 15,
        "valor_estornado": 91.80
    }
}
```

**Erros:**
- 400: Motivo obrigatorio
- 400: NFC-e autorizada (cancelar NFC-e antes via `/nfce/<id>/cancelar`)
- 400: NF ja cancelada
- 404: NF nao encontrada

---

### Produto (`routes_produto.py`)

#### GET /produto/buscar

Busca produto por nome/descricao. Usa tabela de preco PDV (`tab_bo_pdv = 'S'`).

**Query:** `?q=arroz&limit=20`

- `q`: termo de busca (minimo 2 caracteres)
- `limit`: max 50 (default 20)

**Response:**
```json
{
    "success": true,
    "produtos": [
        {
            "pro_in_codigo": 123,
            "pro_st_descricao": "Arroz Tipo 1 5kg",
            "pro_st_referencia": "ARR001",
            "pro_st_codigo_barras": "7891234567890",
            "unidade": "UN",
            "preco": 45.90,
            "estoque": 100.0,
            "ncm": "10063021",
            "..."
        }
    ]
}
```

---

#### GET /produto/buscar-codigo

Busca produto por codigo exato ou EAN (leitor de codigo de barras).

**Query:** `?codigo=7891234567890`

**Response (encontrado):**
```json
{
    "success": true,
    "produto": { "pro_in_codigo": 123, "pro_st_descricao": "...", "preco": 45.90, "..." }
}
```

**Response (nao encontrado):**
```json
{ "success": false, "error": "Produto nao encontrado" }
```

---

#### POST /calcular-impostos

Preview de impostos para um produto (ICMS, PIS, COFINS, IPI).

**Body:**
```json
{
    "produto_id": 123,
    "quantidade": 2,
    "valor_unitario": 45.90
}
```

**Response:**
```json
{
    "success": true,
    "impostos": {
        "icms": { "base": 91.80, "aliquota": 18.0, "valor": 16.52 },
        "pis": { "base": 91.80, "aliquota": 1.65, "valor": 1.51 },
        "cofins": { "base": 91.80, "aliquota": 7.6, "valor": 6.98 },
        "ipi": { "base": 91.80, "aliquota": 0, "valor": 0 },
        "total_impostos": 25.01,
        "valor_total": 116.81
    }
}
```

---

### NFC-e (`routes_nfce.py`)

#### POST /nfce/{nf_in_codigo}/emitir

Emite NFC-e para o pedido. Envia para o PlugNotas e faz polling ate autorizacao.

**Response:**
```json
{
    "success": true,
    "message": "NFC-e emitida com sucesso",
    "nfce": {
        "nf_in_codigo": 678,
        "nf_in_codigo_nf": 123,
        "nf_st_chave": "35260212345678000190650010000001231234567890",
        "sta_st_codigo": "NFE_AUT",
        "..."
    }
}
```

---

#### GET /nfce/{nf_in_codigo}/consultar

Consulta status da NFC-e (leitura do banco, sem chamar PlugNotas).

**Response:**
```json
{
    "success": true,
    "nfce": { "nf_in_codigo": 678, "sta_st_codigo": "NFE_AUT", "nf_st_chave": "...", "..." }
}
```

---

#### POST /nfce/{nf_in_codigo}/cancelar

Cancela NFC-e autorizada. Limite: 30 minutos apos autorizacao (regra SEFAZ).

**Body:**
```json
{ "motivo": "Erro na venda" }
```

**Response:**
```json
{
    "success": true,
    "message": "NFC-e #678 cancelada",
    "resultado": { "..." }
}
```

---

#### GET /nfce/{nf_in_codigo}/pdf

Download do PDF (DANFCE) da NFC-e.

**Response:** `application/pdf` (binario)
**Content-Disposition:** `inline; filename=nfce_678.pdf`

---

#### GET /nfce/{nf_in_codigo}/xml

Download do XML da NFC-e.

**Response:** `application/xml` (binario)
**Content-Disposition:** `attachment; filename=nfce_678.xml`

---

#### POST /nfce/{nf_in_codigo}/atualizar-status

Atualiza status da NFC-e consultando no PlugNotas. Usar quando NFC-e esta em processamento.

**Response:**
```json
{
    "success": true,
    "nfce": { "nf_in_codigo": 678, "sta_st_codigo": "NFE_AUT", "..." }
}
```

---

#### POST /nfce/{nf_in_codigo}/reemitir

Reemite NFC-e rejeitada ou pendente. Somente para status `NFE_PEN`, `NFE_RAS`, `NFE_REJ`.

**Response:**
```json
{
    "success": true,
    "message": "NFC-e reemitida com sucesso",
    "nfce": { "..." }
}
```

---

#### GET /nfce

Lista NFC-e com filtros.

**Query:**
| Parametro | Descricao |
|-----------|-----------|
| `caixa` | Numero do caixa |
| `data_inicio` | YYYY-MM-DD |
| `data_fim` | YYYY-MM-DD |
| `status` | `NFE_AUT`, `NFE_CAN`, `NFE_REJ`, `NFE_PRO` |
| `numero` | Numero da NF |

**Response:**
```json
{
    "success": true,
    "notas": [ { "nf_in_codigo": 678, "sta_st_codigo": "NFE_AUT", "..." } ]
}
```

---

#### GET /cupom/{nf_in_codigo}/dados

Dados estruturados do cupom para impressao local (ESC/POS no desktop).
O app desktop monta os bytes ESC/POS localmente com estes dados.

**Response:**
```json
{
    "success": true,
    "cupom": {
        "empresa": "Razao Social LTDA",
        "cnpj": "12.345.678/0001-00",
        "endereco": "Rua X, 100, Bairro, Cidade, UF",
        "numero_venda": 12345,
        "data": "28/02/2026 14:30",
        "operador": "Joao",
        "cliente": "Maria da Silva",
        "cpf": "123.456.789-00",
        "itens": [
            { "descricao": "Arroz 5kg", "qtd": 2, "preco_unit": 45.90, "total": 91.80 }
        ],
        "subtotal": 91.80,
        "desconto": 0,
        "total": 91.80,
        "pagamentos": [
            { "forma": "Dinheiro", "valor": 100.00 }
        ],
        "troco": 8.20,
        "chave_nfce": "35260212345678000190650010000001231234567890"
    }
}
```

---

### Cliente (`routes_cliente.py`)

#### GET /cliente/buscar

Busca cliente por nome ou CPF/CNPJ.

**Query:**
- `?q=Joao` — busca por nome (LIKE, minimo 2 chars)
- `?doc=12345678900` — busca por CPF/CNPJ exato (somente digitos)

**Response (busca por nome):**
```json
{
    "success": true,
    "clientes": [
        { "agn_in_codigo": 42, "agn_st_nome": "Joao Silva", "agn_st_cnpj_cpf": "123.456.789-00", "agn_st_email": "joao@email.com", "agn_st_telefone": "11999990000" }
    ]
}
```

**NOTA:** `agn_st_email` vem de `glo_agente_email` (principal) e `agn_st_telefone` vem de `glo_agente_contatos` (principal). Podem ser `null` se nao cadastrados.
```

**Response (busca por doc — unico ou null):**
```json
{
    "success": true,
    "cliente": { "agn_in_codigo": 42, "agn_st_nome": "Joao Silva", "..." }
}
```

---

#### GET /cliente/ultimos

Ultimos clientes cadastrados.

**Query:** `?limit=10` (max 50)

**Response:**
```json
{
    "success": true,
    "clientes": [ { "agn_in_codigo": 42, "agn_st_nome": "...", "..." } ]
}
```

---

#### POST /cliente/cadastrar

Cadastro rapido de cliente (nome + CPF + telefone + email).

**Body:**
```json
{
    "nome": "Joao Silva",
    "cpf_cnpj": "123.456.789-00",
    "telefone": "(11) 99999-0000",
    "email": "joao@email.com"
}
```

- `nome`: obrigatorio
- Demais: opcionais

**Response:**
```json
{
    "success": true,
    "message": "Cliente cadastrado",
    "cliente": { "agn_in_codigo": 42, "agn_st_nome": "Joao Silva", "agn_st_cnpj_cpf": "123.456.789-00", "..." }
}
```

**Erros:**
- 400: Nome obrigatorio
- 409: CPF/CNPJ ja cadastrado (retorna `duplicado: true` + `cliente_existente`)

---

#### POST /cliente/cadastrar-completo

Cadastro completo com endereco, contatos e validacao de CPF/CNPJ.

**Body:**
```json
{
    "nome": "Joao Silva",
    "cpf_cnpj": "123.456.789-00",
    "tipo_pessoa": "F",
    "telefone": "(11) 99999-0000",
    "whatsapp": "(11) 99999-0000",
    "email": "joao@email.com",
    "cep": "01310-100",
    "logradouro": "Av. Paulista",
    "numero": "1000",
    "complemento": "Sala 1",
    "bairro": "Bela Vista",
    "cidade": "Sao Paulo",
    "cidade_numero": 3550308,
    "uf": "SP",
    "estado_numero": 35
}
```

**Validacoes:**
- CPF: 11 digitos, validado com algoritmo
- CNPJ: 14 digitos, validado com algoritmo
- `tipo_pessoa`: auto-detectado pelo tamanho do documento (`F` ou `J`)
- Duplicidade por CPF/CNPJ

**Response (201):**
```json
{
    "success": true,
    "message": "Cliente Joao Silva cadastrado com sucesso!",
    "cliente": { "agn_in_codigo": 42, "agn_st_nome": "Joao Silva", "agn_st_cnpj_cpf": "12345678900" }
}
```

**Erros:**
- 400: Nome obrigatorio / CPF invalido / CNPJ invalido
- 409: CPF/CNPJ ja cadastrado

---

### Impressao (`routes_impressao.py`)

#### POST /imprimir/{nf_in_codigo}

Reimprime cupom via impressora de rede do terminal. Usa a impressora configurada no terminal vinculado ao caixa aberto do operador.

**Sem body.** Toda informacao vem do contexto (caixa aberto → terminal → impressora IP:porta).

**Response:**
```json
{ "success": true, "message": "Cupom enviado para impressora" }
```

**Erros:**
- 400: Nenhuma impressora configurada para este terminal
- 404: Venda nao encontrada
- 500: Erro de conexao com impressora

---

#### POST /impressora/testar

Testa conexao com impressora de rede (TCP socket, timeout 5s).

**Body:**
```json
{ "ip": "192.168.1.100", "porta": 9100 }
```

**Response (sucesso):**
```json
{ "success": true, "message": "Conexao OK com 192.168.1.100:9100" }
```

**Response (falha):**
```json
{ "success": false, "error": "Timeout ao conectar em 192.168.1.100:9100" }
```

---

### Configuracao (`routes_config.py`)

#### GET /configuracao

Configuracao completa do PDV para a filial.

**Response:**
```json
{
    "success": true,
    "configuracao": {
        "pdv_bo_emitir_nfce_auto": "S",
        "pdv_bo_exigir_cpf": "N",
        "pdv_in_casas_decimais_qtd": 3,
        "pdv_in_casas_decimais_preco": 2,
        "pdv_bo_exigir_abertura": "S",
        "pdv_bo_imprimir_cupom": "S",
        "pdv_bo_usar_terminal_fixo": "N",
        "pdv_st_modo_entrada": "A",
        "pdv_bo_usar_turno": "N",
        "pdv_bo_bloquear_fora_turno": "N",
        "pdv_in_aviso_fim_turno": 15,
        "pdv_in_limite_horas_aberto": 12,
        "..."
    },
    "tipo_documento": { "tpd_in_codigo": 1, "tpd_st_descricao": "PDV" }
}
```

**Campos relevantes para o desktop:**
| Campo | Descricao |
|-------|-----------|
| `pdv_bo_emitir_nfce_auto` | Emitir NFC-e automaticamente ao finalizar |
| `pdv_bo_exigir_cpf` | Exigir CPF/CNPJ para finalizar |
| `pdv_in_casas_decimais_qtd` | Casas decimais para quantidade |
| `pdv_in_casas_decimais_preco` | Casas decimais para preco |
| `pdv_bo_exigir_abertura` | Exigir abertura de caixa para vender |
| `pdv_bo_imprimir_cupom` | Imprimir cupom apos finalizacao |
| `pdv_st_modo_entrada` | `A` (automatico/leitor) ou `M` (manual) |
| `pdv_bo_usar_turno` | Habilitar sistema de turnos |
| `pdv_in_aviso_fim_turno` | Minutos antes do fim para avisar |

---

#### GET /formas-pagamento

Formas de pagamento habilitadas no PDV.

**Response:**
```json
{
    "success": true,
    "formas": [
        { "fcb_in_codigo": 1, "nome": "Dinheiro", "descricao": "Dinheiro", "padrao": "S", "permite_troco": "S" },
        { "fcb_in_codigo": 2, "nome": "PIX", "descricao": "PIX QR Code", "padrao": "N", "permite_troco": "N" },
        { "fcb_in_codigo": 3, "nome": "Cartao Credito", "descricao": "Cartao de Credito", "padrao": "N", "permite_troco": "N" }
    ]
}
```

- `padrao = 'S'`: forma pre-selecionada no pagamento
- `permite_troco = 'S'`: mostra campo "Valor Recebido" e calcula troco

---

#### GET /ping

Health check / keep-alive. Valida que o token ainda e valido.

**Response:**
```json
{
    "success": true,
    "operador": "Joao",
    "emp": 1
}
```

**Recomendacao:** Chamar a cada 10 minutos para manter indicador de conexao.

---

## Fluxo Completo de Venda

```
1. Login
   POST /auth/login
   → Armazenar token + dados empresa/filial/filiais

2. [Opcional] Trocar filial (se usuario tem mais de uma)
   GET /auth/filiais   → listar filiais disponiveis
   POST /auth/trocar-filial { org_in_codigo, fil_in_codigo }
   → Armazenar novo token

3. Carregar configuracao
   GET /configuracao
   GET /formas-pagamento
   GET /caixa/config-terminal

4. Abrir caixa
   POST /caixa/abrir { valor_abertura, ter_in_codigo }

5. Loop de venda:
   a. Escanear/buscar produto
      GET /produto/buscar-codigo?codigo=<ean>  (leitor)
      GET /produto/buscar?q=<texto>            (manual)
   b. Adicionar ao carrinho (MEMORIA LOCAL)
   c. [Opcional] Calcular impostos
      POST /calcular-impostos

6. Pagamento (F2):
   a. Informar CPF (se exigido)
   b. Selecionar forma de pagamento
   c. Informar valor recebido (dinheiro)
   d. [Se TEF] Processar cartao localmente

7. Confirmar venda:
   POST /venda/finalizar-direto
   Header: X-Idempotency-Key: <uuid>
   Body: { itens, parcelas, cpf_nota, troco }

8. Pos-venda:
   a. Se pdv_bo_emitir_nfce_auto = 'S' → NFC-e ja emitida no response
   b. Senao → POST /nfce/<id>/emitir
   c. Consultar NFC-e: GET /nfce/<id>/consultar
   d. Imprimir cupom:
      - Local: GET /cupom/<id>/dados → montar ESC/POS no desktop
      - Rede: POST /imprimir/<id> → servidor envia para impressora
   e. Nova venda (limpar carrinho)

9. Operacoes de caixa:
   POST /caixa/sangria
   POST /caixa/suprimento
   GET /caixa/resumo

10. Consulta:
    GET /vendas?data_inicio=...&status=...      (ven_notafiscal)
    GET /venda/<nf_in_codigo>                    (detalhe + itens)
    GET /nfce?data_inicio=...&status=...

11. Fechar caixa:
    POST /caixa/fechar { valor_fechamento }

12. Keep-alive (cada 10 min):
    GET /ping

13. Renovar token (cada ~12h):
    POST /auth/refresh
```

---

## Estrutura de Arquivos

```
lib/backend/routes/api/v1/pdv/
├── README.md                 # Este arquivo
├── __init__.py               # Blueprint (pdv_api_v1_bp, prefixo /api/v1/pdv)
├── routes_auth.py            # Login, sessao, filiais, trocar-filial, refresh (5 endpoints)
├── routes_caixa.py           # Caixa: abrir, fechar, sangria, suprimento, etc (9 endpoints)
├── routes_venda.py           # Finalizar-direto, listar, detalhe, estornar (4 endpoints)
├── routes_produto.py         # Buscar, buscar-codigo, calcular-impostos (3 endpoints)
├── routes_nfce.py            # NFC-e completa + cupom dados (9 endpoints)
├── routes_cliente.py         # Buscar, cadastrar, cadastrar-completo (4 endpoints)
├── routes_impressao.py       # Imprimir cupom, testar impressora (2 endpoints)
└── routes_config.py          # Configuracao, formas-pagamento, ping (3 endpoints)

lib/backend/decorators/
└── pdv_auth_decorators.py    # @require_pdv_auth, gerar/validar token
```

**Total: 39 endpoints**

---

## Tabela Completa de Endpoints

| # | Metodo | Rota | Descricao | Rate Limit |
|---|--------|------|-----------|------------|
| 1 | POST | `/auth/login` | Login operador | 10/min |
| 2 | GET | `/auth/sessao` | Dados sessao | — |
| 3 | GET | `/auth/filiais` | Listar filiais do usuario | — |
| 4 | POST | `/auth/trocar-filial` | Trocar filial+org | — |
| 5 | POST | `/auth/refresh` | Renovar token | — |
| 6 | GET | `/caixa/config-terminal` | Config terminal | 30/min |
| 7 | GET | `/caixa/status-turno` | Status turno | 60/min |
| 8 | GET | `/caixa/status` | Caixa aberto? | 60/min |
| 9 | POST | `/caixa/abrir` | Abrir caixa | 10/min |
| 10 | POST | `/caixa/fechar` | Fechar caixa | 10/min |
| 11 | POST | `/caixa/sangria` | Registrar sangria | 20/min |
| 12 | POST | `/caixa/suprimento` | Registrar suprimento | 20/min |
| 13 | GET | `/caixa/resumo` | Resumo detalhado | 30/min |
| 14 | GET | `/caixa/movimentos` | Listar movimentos | 30/min |
| 15 | POST | `/venda/finalizar-direto` | Criar + finalizar venda | 30/min |
| 16 | GET | `/vendas` | Listar vendas (ven_notafiscal) | 30/min |
| 17 | GET | `/venda/<nf_in_codigo>` | Detalhe venda + itens | 30/min |
| 18 | POST | `/venda/<nf_in_codigo>/estornar` | Estornar venda | 10/min |
| 19 | GET | `/produto/buscar` | Buscar por nome | 60/min |
| 20 | GET | `/produto/buscar-codigo` | Buscar por EAN | 60/min |
| 21 | POST | `/calcular-impostos` | Preview impostos | 60/min |
| 22 | POST | `/nfce/<id>/emitir` | Emitir NFC-e | 20/min |
| 23 | GET | `/nfce/<id>/consultar` | Status NFC-e (DB) | 60/min |
| 24 | POST | `/nfce/<id>/cancelar` | Cancelar NFC-e | 10/min |
| 25 | GET | `/nfce/<id>/pdf` | Download DANFCE | 30/min |
| 26 | GET | `/nfce/<id>/xml` | Download XML | 30/min |
| 27 | POST | `/nfce/<id>/atualizar-status` | Refresh via PlugNotas | 30/min |
| 28 | POST | `/nfce/<id>/reemitir` | Reemitir rejeitada | 10/min |
| 29 | GET | `/nfce` | Listar NFC-e | 30/min |
| 30 | GET | `/cupom/<id>/dados` | Dados cupom (ESC/POS) | 30/min |
| 31 | GET | `/cliente/buscar` | Buscar por nome/CPF | 60/min |
| 32 | GET | `/cliente/ultimos` | Ultimos cadastrados | 30/min |
| 33 | POST | `/cliente/cadastrar` | Cadastro rapido | 20/min |
| 34 | POST | `/cliente/cadastrar-completo` | Cadastro com endereco | 20/min |
| 35 | POST | `/imprimir/<id>` | Reimprimir cupom (rede) | 30/min |
| 36 | POST | `/impressora/testar` | Testar impressora | 10/min |
| 37 | GET | `/configuracao` | Config PDV | 30/min |
| 38 | GET | `/formas-pagamento` | Formas habilitadas | 30/min |
| 39 | GET | `/ping` | Keep-alive | — |

---

## Diferenca entre API v1 e Rotas Web

O PDV Web possui ~55 rotas (telas HTML + APIs). A API v1 cobre as 39 operacoes necessarias para o desktop.

**Excluidas por design (nao necessarias no desktop):**

| Rota Web | Motivo |
|----------|--------|
| `venda/nova`, `venda/<id>/item`, `venda/<id>/finalizar` | Carrinho server-side — desktop usa `finalizar-direto` com carrinho local |
| `venda/<id>/cancelar` | Cancelar rascunho — nao existe no fluxo desktop |
| `configuracao POST` (salvar) | Admin — feito pela interface web |
| `configuracao/tipos-documento`, `configuracao/clientes` | Admin — feito pela interface web |
| Setores CRUD, Terminais CRUD, Turnos CRUD | Admin — feito pela interface web |
| Telas HTML (caixa, venda, consulta, pagamento) | Desktop renderiza telas WPF nativas |

---

## Exemplo: Chamada cURL

```bash
# 1. Login (sem token, so email + senha)
curl -X POST https://meintec.com.br/api/v1/pdv/auth/login \
  -H "Content-Type: application/json" \
  -d '{"login":"operador@empresa.com","senha":"123456"}'

# 2. Buscar produto (com Bearer)
curl https://meintec.com.br/api/v1/pdv/produto/buscar-codigo?codigo=7891234567890 \
  -H "Authorization: Bearer eyJ..."

# 3. Finalizar venda
curl -X POST https://meintec.com.br/api/v1/pdv/venda/finalizar-direto \
  -H "Authorization: Bearer eyJ..." \
  -H "Content-Type: application/json" \
  -H "X-Idempotency-Key: $(uuidgen)" \
  -d '{
    "itens": [{"pro_in_codigo": 123, "quantidade": 2}],
    "parcelas": [{"fcb_in_codigo": 1, "valor": 91.80}],
    "troco": 8.20
  }'
```

---

## Dependencias do Backend

| Componente | Arquivo |
|------------|---------|
| Decorator auth | `lib/backend/decorators/pdv_auth_decorators.py` |
| Rate limit | `lib/backend/decorators/rate_limit_decorators.py` |
| Error handler | `lib/backend/helpers/sistema/error_handlers.py` |
| DB connection | `lib/backend/database/connection.py` |
| Helpers caixa | `lib/backend/routes/pages/vendas/PDV/operacao/caixa/helpers.py` |
| Helpers venda | `lib/backend/routes/pages/vendas/PDV/operacao/venda/helpers.py` |
| Helpers NFC-e | `lib/backend/routes/pages/vendas/PDV/operacao/nfce/helpers.py` |
| Helpers turno | `lib/backend/routes/pages/vendas/PDV/configuracao/turno/helpers.py` |
| Helpers impressao | `lib/backend/routes/pages/vendas/PDV/configuracao/impressao/helpers.py` |
| ESC/POS | `lib/backend/routes/pages/vendas/PDV/configuracao/impressao/escpos.py` |
| Helpers terminal | `lib/backend/routes/pages/vendas/PDV/configuracao/terminal/helpers.py` |
| Impostos | `lib/backend/helpers/fiscal/impostos_helper.py` |
| Validacao fiscal | `lib/backend/helpers/fiscal/validacao_fiscal.py` |
| Agente utils | `lib/backend/helpers/cadastros/agente_utils.py` |
| Filial switcher | `lib/backend/routes/utils/filial_switcher.py` (referencia para troca de filial) |
| View sessao | `lib/backend/database/views/vw_sessao_usuario.sql` (query de filiais_usuario_json) |
| Blueprint | `lib/backend/routes/blueprints.py` (registrado em `get_api_blueprints()`) |

Toda logica de negocio esta nos helpers existentes — as rotas da API v1 sao wrappers finos que traduzem HTTP para chamadas de helper.
