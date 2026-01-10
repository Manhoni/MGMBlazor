# 🏥 Sistema de Automação Fiscal - Medicina do Trabalho

> Sistema web para automação de geração de boletos bancários e notas fiscais de serviço (NFS-e) para empresa de medicina ocupacional.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Em%20Desenvolvimento-yellow)](https://github.com/seu-usuario/sistema-mgm)

---

## 📋 Sobre o Projeto

Sistema desenvolvido para automatizar processos manuais repetitivos em uma empresa de medicina do trabalho, reduzindo tempo e erros operacionais.

### Problema Resolvido

Anteriormente, o processo manual envolvia:
1. ❌ Gerar fatura em PDF no sistema legado
2. ❌ Acessar site do banco e preencher dados manualmente
3. ❌ Acessar site da prefeitura e emitir NFS-e manualmente
4. ⏱️ Tempo médio: **15-20 minutos por fatura**
5. 😰 Alto consumo de energia mental e risco de erros

### Solução Implementada

✅ Interface web unificada  
✅ Integração com API bancária para geração automática de boletos  
✅ Integração com sistema de NFS-e da prefeitura  
✅ Armazenamento de histórico e rastreabilidade  
✅ Tempo médio: **2-3 minutos por fatura**  
✅ **Redução de 85% no tempo de processamento**

---

## 🚀 Funcionalidades

- [x] Cadastro de empresas clientes
- [ ] Importação de dados de faturas
- [ ] Geração automática de boletos via API bancária
- [x] Emissão de NFS-e via webservice da prefeitura
- [x] Exportação de relatórios
- [ ] Notificações por email (em desenvolvimento)
- [ ] Dashboard analítico (planejado)

---

## 🛠️ Tecnologias Utilizadas

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **Dapper** - Queries otimizadas

### Frontend
- **Razor Pages** - Views
- **Bootstrap 5** - Interface responsiva
- **JavaScript/jQuery** - Interatividade

### Integrações
- **API REST** - Integração bancária
- **SOAP/XML** - Webservice NFS-e da prefeitura
- **PDF Sharp** - Manipulação de PDFs

### Infraestrutura
- **IIS** - Hospedagem em servidor local
- **Zero Tier** - Acesso remoto seguro

---

## 📦 Arquitetura

```
┌─────────────────────────────────────────────────┐
│           Interface Web (Razor Pages)           │
├─────────────────────────────────────────────────┤
│                  API Controllers                │
├────────────────┬────────────────┬───────────────┤
│  Business Logic│   Services     │  Repositories │
├────────────────┴────────────────┴───────────────┤
│            Entity Framework Core                │
├─────────────────────────────────────────────────┤
│                  SQL Server                     │
└─────────────────────────────────────────────────┘
         ↓                    ↓
   API Banco              API Prefeitura
```

---

## ⚙️ Configuração

### Pré-requisitos

- .NET SDK 8.0 ou superior
- SQL Server 2019 ou superior (Express funciona)
- Visual Studio 2022 ou VS Code com C# extension
- Git

---

## 🔐 Segurança

### ⚠️ IMPORTANTE - Nunca Versionar

Este projeto **NÃO** versiona:
- ❌ Strings de conexão com credenciais reais
- ❌ Certificados digitais
- ❌ Chaves de API
- ❌ Senhas ou tokens

### Configuração de Credenciais

**Para desenvolvimento local:**

1. Copie `appsettings.example.json` para `appsettings.Development.json`
2. Preencha com suas credenciais locais
3. O `.gitignore` garante que não será commitado

**Para produção:**

Use **User Secrets** ou **variáveis de ambiente**:

```bash
# Configurar secrets
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "sua-string-aqui"
dotnet user-secrets set "BancoAPI:ChaveAPI" "sua-chave-aqui"
```

### LGPD e Compliance

Este sistema lida com dados sensíveis (saúde ocupacional). Implementações:
- Criptografia de dados em repouso
- Logs de auditoria
- Controle de acesso baseado em roles
- Backup automático com retenção de 30 dias

---

## 🧪 Testes

```bash
# Rodar testes unitários
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

---

## 📊 Exemplo de Uso

```csharp
// Gerar boleto automaticamente
var boleto = await _boletoService.GerarBoletoAsync(new BoletoRequest
{
    EmpresaId = empresaId,
    Valor = 1500.00m,
    DataVencimento = DateTime.Now.AddDays(10)
});

// Emitir NFS-e
var nfse = await _nfseService.EmitirNotaAsync(new NfseRequest
{
    EmpresaId = empresaId,
    ServicoId = servicoId,
    Valor = 1500.00m
});
```

---

## 🤝 Contribuindo

Este é um projeto de portfólio pessoal, mas sugestões são bem-vindas!

1. Fork o projeto
2. Crie uma branch (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Add: nova feature X'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

## 📝 Licença

Este projeto está sob a licença CC BY-NC-SA 4.0.

### O que isso significa?

✅ **Você PODE:**
- Estudar e aprender com o código
- Usar para projetos pessoais e educacionais
- Modificar e adaptar

❌ **Você NÃO PODE:**
- Usar comercialmente sem permissão
- Vender ou lucrar diretamente com este código
- Incorporar em produtos/serviços comerciais

📧 **Uso Comercial:** Para licenciamento comercial, 
   entre em contato: joaogmanhoni@hotmail.com

Veja o arquivo [LICENSE](LICENSE) para detalhes completos.

[![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc-sa/4.0/)

---

## 📧 Contato

**João Gabriel Manhoni** - [Linkedin](https://www.linkedin.com/in/joao-gabriel-manhoni-2aa4a9259) - joaogmanhoni@hotmail.com

Link do Projeto: [https://github.com/Manhoni/MGMBlazor](https://github.com/Manhoni/MGMBlazor)

---

## 🙏 Agradecimentos

- Família pela paciência durante o desenvolvimento
- Comunidade .NET pelo suporte

---

## 🗺️ Roadmap

- [x] MVP - CRUD básico
- [x] Integração bancária
- [x] Integração prefeitura
- [ ] Notificações por email
- [ ] Dashboard analítico
- [ ] App mobile (planejado para 2026)
- [ ] IA para análise de padrões (futuro)

---


**⭐ Se este projeto te ajudou, considere dar uma estrela!**