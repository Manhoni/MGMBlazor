# 🏥 Sistema de Automação Fiscal - Medicina do Trabalho

> Sistema web para automação de geração de boletos bancários e notas fiscais de serviço (NFS-e) para empresa de medicina ocupacional.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Produção-success)](https://github.com/Manhoni/MGMBlazor)

---

## 📋 Sobre o Projeto

Sistema desenvolvido para automatizar processos manuais repetitivos em uma empresa de medicina do trabalho, reduzindo tempo e erros operacionais através de integrações diretas com órgãos municipais e bancários.

### Problema Resolvido

Anteriormente, o processo manual envolvia:
1. ❌ Gerar fatura em PDF no sistema legado
2. ❌ Acessar site do banco e preencher dados manualmente
3. ❌ Acessar site da prefeitura e emitir NFS-e manualmente
4. ⏱️ Tempo médio: **15-20 minutos por fatura**
5. 😰 Alto consumo de energia mental e risco de erros

### Solução Implementada

✅ Interface web reativa e unificada  
✅ Integração com API Sicoob V3 para geração automática de boletos. 
✅ Integração com Webservices Municipais (Abrasf 2.01) via SOAP/XML com assinatura digital A1.
✅ Armazenamento de histórico e rastreabilidade  
✅ Tempo médio: **2-3 minutos por fatura**  
✅ **Redução de 85% no tempo de processamento**

---

## 🚀 Funcionalidades

- [x] Cadastro de empresas clientes com busca automática via CEP (ViaCEP).
- [x] Importação de dados de faturas via CSV.
- [x] Geração automática de boletos via API Bancária.
- [x] Emissão de NFS-e com validação XSD e Assinatura Digital (Certificado A1).
- [x] Trilha de Auditoria (Logs detalhados de operações por usuário).
- [x] Notificações por e-mail consolidadas (Nota + Boletos em anexo).
- [ ] Dashboard analítico (planejado).

---

## 🛠️ Tecnologias Utilizadas

### Backend
- **ASP.NET Core 8.0** - Framework principal.
- **Entity Framework Core** - ORM com banco PostgreSQL.
- **MailKit** - Motor de envio de e-mails SMTP.
- **System.Security.Cryptography.Xml** - Assinatura digital de documentos fiscais.

### Frontend
- **Blazor Web App (Interactive Server)** - UI reativa.
- **Bootstrap 5** - Interface responsiva.
- **JavaScript Interop** - Manipulação de PDFs e recursos do navegador.

### Infraestrutura
- **Google Cloud Platform (GCP)** - Hospedagem em VM Ubuntu Minimal.
- **Nginx** - Proxy reverso e gerenciamento de tráfego.
- **PostgreSQL** - Persistência de dados robusta.

---

## 🔐 Segurança

### ⚠️ IMPORTANTE - Nunca Versionar

Este projeto **NÃO** versiona:
- ❌ Strings de conexão com credenciais reais
- ❌ Certificados digitais
- ❌ Chaves de API
- ❌ Senhas ou tokens

### LGPD e Compliance

- **Audit Logs**: Registro de "Quem, Quando e Onde" para todas as ações críticas.
- **mTLS**: Comunicação segura com o banco via troca de certificados públicos/privados.
- **Isolation**: Banco de dados fechado para acesso externo, operando apenas em localhost.

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
- [x] Notificações por email
- [ ] Dashboard analítico
- [ ] App mobile (planejado para 2026)
- [ ] IA para análise de padrões (futuro)

---


**⭐ Se este projeto te ajudou, considere dar uma estrela!**