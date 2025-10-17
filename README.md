
# 🏦 Case Itaú — Classificação Automática de Reclamações

## 📘 Visão Geral

Este projeto apresenta uma solução completa para **classificação automática de reclamações bancárias**, desde o recebimento até a exibição dos resultados em um painel web interativo.  
A aplicação identifica automaticamente as categorias de uma reclamação com base em **palavras-chave** e simula a integração com um **DataMesh interno** do banco para exibir informações do reclamante.

---

## 🧩 Arquitetura e Camadas

O sistema segue uma **Clean Architecture simplificada**, separando responsabilidades em camadas independentes:

```
Front-end (Bootstrap + JS)
        ↓
API (ASP.NET Core)
        ↓
Application (orquestra integrações)
        ↓
Infrastructure (classificação e dados simulados)
        ↓
Domain (interfaces e contratos)
```

---

## ⚙️ Tecnologias Utilizadas

| Camada | Tecnologia / Biblioteca | Descrição |
|--------|-------------------------|------------|
| **Front-end** | Bootstrap 5.3 | Interface moderna e responsiva |
|  | JavaScript Fetch API | Comunicação direta com o endpoint `/api/Classification` |
| **Back-end** | ASP.NET Core 9.0 | API REST de classificação |
|  | Swagger / OpenAPI | Documentação e teste da API |
|  | xUnit | Testes unitários automatizados |
| **Infraestrutura** | Regex + Normalização | Lógica de classificação por palavras-chave |
| **Mock DataMesh** | JSON local | Simula dados bancários do cliente |

---

## 🧠 Lógica de Classificação

A classe `KeywordClassifier`:

1. Normaliza o texto (remove acentos e padroniza)  
2. Mapeia palavras-chave por categoria  
3. Calcula o nível de confiança por peso e frequência  
4. Retorna uma lista ordenada de categorias relevantes  

### Exemplo de entrada

```json
{
  "reclamacao": "Meu aplicativo está travando e não consigo acessar minha conta."
}
```

### Exemplo de saída

```json
[
  { "category": "aplicativo", "confidence": 0.75, "matchedKeywords": ["aplicativo", "travando"] },
  { "category": "acesso", "confidence": 0.60, "matchedKeywords": ["acessar"] }
]
```

---

## 🧪 Testes Unitários

Os testes utilizam **xUnit** para garantir a confiabilidade da classificação:

```csharp
[Fact]
public void Deve_Classificar_Reclamacao_Corretamente()
{
    var categorias = new Dictionary<string, IEnumerable<string>> {
        ["acesso"] = new[] { "acessar", "login", "senha" },
        ["aplicativo"] = new[] { "app", "aplicativo", "travando" }
    };

    ICategoryClassifier classifier = new KeywordClassifier(categorias);
    var resultado = classifier.Classify("Não consigo acessar o app da conta.");

    Assert.NotEmpty(resultado);
    Assert.Contains(resultado, r => r.Category == "acesso");
    Assert.Contains(resultado, r => r.Category == "aplicativo");
}
```

**Saída esperada:**

```
Resumo do teste: total: 2; falhou: 0; bem-sucedido: 2
```

---

## 🔐 Segurança e Boas Práticas

- HTTPS habilitado (em `launchSettings.json`)  
- CORS configurado apenas para o front local  
- Validação de entrada no Controller  
- Try/Catch global para erros  
- Código documentado e testado  

---

## 💻 Front-end Interativo

O front usa **Bootstrap 5.3** e **Fetch API**, permitindo:

- Registro de reclamações com:
  - Nome do reclamante  
  - CPF  
  - Contato  
  - Descrição  
  - Upload de documentos (opcional)
- Exibição de histórico de reclamações registradas  
- Classificação automática por tipo de demanda  

---

## 📊 Integração com DataMesh (Simulação)

Ao informar um CPF válido, o sistema simula uma consulta ao **DataMesh interno**, retornando:

```json
{
  "cpf": "12345678900",
  "contasAtivas": 2,
  "produtosContratados": ["Cartão Black", "Empréstimo Pessoal"],
  "ultimaInteracao": "2025-09-12"
}
```

---

## 🧱 Estrutura de Pastas

```
CaseItau/
 ├── src/
 │   ├── CaseItau.Domain/          # Interfaces e contratos de negócio
 │   ├── CaseItau.Application/     # Regras de orquestração e integração
 │   ├── CaseItau.Infrastructure/  # Implementações concretas (KeywordClassifier, DataMeshMock)
 │   └── CaseItau.Api/             # API ASP.NET Core (Controllers e Configuração)
 ├── tests/
 │   └── CaseItau.Tests/           # Testes unitários (xUnit)
 ├── front/
 │   └── CaseItau.Front/           # Interface Web (HTML, Bootstrap e JS)
 │       ├── index.html            # Página principal do dashboard
 │       ├── README.md             # Documentação do front-end
 │       ├── /assets/
 │       │   ├── css/
 │       │   │   └── style.css     # Estilos customizados
 │       │   └── js/
 │       │       └── main.js       # Lógica de interação com a API
 ├── CaseItau.sln                  # Solução principal .NET
 └── README.md                     # Documentação do projeto completo
```

---

## 🚀 Como Executar Localmente

### 1️⃣ Clonar o repositório

```bash
git clone https://github.com/seu-usuario/CaseItau.git
cd CaseItau
```

### 2️⃣ Restaurar e compilar

```bash
dotnet restore
dotnet build
```

### 3️⃣ Rodar a API

```bash
dotnet run --project src/CaseItau.Api
```

### 4️⃣ Abrir o front-end

Abra o arquivo abaixo no navegador

```
front/CaseItau.Front/index.html
```

---

📅 **Apresentação:** Outubro / 2025  
💼 **Autor:** Lucas Queiroz  
💻 **Instituição:** Itaú Unibanco — Case Técnico
