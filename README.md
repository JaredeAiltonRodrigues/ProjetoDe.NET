# ONG Alimento para Todos

Aplicacao ASP.NET Core MVC para organizar estoque, solicitacoes de apoio, equipe, escalas e livro-caixa.

## Requisitos

- .NET SDK 10.0.x
- Acesso ao NuGet para restaurar dependencias

## Executar localmente

```sh
dotnet tool restore
dotnet restore
dotnet run
```

O perfil local inicia em `http://localhost:5181` no ambiente `Development`. Nesse ambiente, as migrations do EF Core sao aplicadas na inicializacao. O SQLite usa `banco_ong.db` por padrao; o arquivo e seus auxiliares WAL/SHM ficam fora do Git.

Para validar o projeto:

```sh
dotnet build --configuration Release --no-restore -warnaserror
dotnet ef migrations has-pending-model-changes
dotnet list package --vulnerable --include-transitive
```

## Configuracao

Use variaveis de ambiente para valores privados. O provedor de configuracao .NET converte `__` em `:`.

- `ConnectionStrings__DefaultConnection`: conexao SQLite ou outro provider configurado no projeto. Padrao: `Data Source=banco_ong.db`.
- `Database__ApplyMigrationsOnStartup=true`: aplica migrations na inicializacao fora de Development. Em producao, coordene essa operacao com o processo de deploy.
- `BootstrapAdmin__Name` e `BootstrapAdmin__Password`: criam o primeiro administrador quando ainda nao existe um ativo. A senha deve ter pelo menos 12 caracteres. Configure ambos juntos e remova-os do ambiente apos a inicializacao.
- `GEMINI_API_KEY`: habilita a sugestao opcional do painel. Sem ela, a distribuicao continua manual.
- `Email__SmtpHost`, `Email__SmtpPort`, `Email__Username`, `Email__Password`, `Email__FromAddress` e `Email__FromName`: habilitam avisos por e-mail.
- `Privacy__ControllerName` e `Privacy__ContactEmail`: identificacao e canal oficial exibidos na politica de privacidade.

Nao existe credencial administrativa padrao. Contas antigas com senha em texto sao convertidas para hash no primeiro login valido; proteja e faca backup do banco durante essa transicao.

## Distribuicao de alimentos

O painel reserva uma cesta por familia, separando cinco unidades por reserva e priorizando os lotes com vencimento mais proximo. Cada reserva grava um historico permanente com os produtos, quantidades, validade, beneficiario, retirada prevista, entrega e estado da notificacao.

Uma reserva cancelada devolve os itens ao estoque. A entrega so e marcada quando a equipe confirma o recebimento. Notificacoes podem ser enviadas por e-mail quando SMTP estiver configurado; sem SMTP ou sem e-mail, o painel permite registrar contato telefonico manual. PIX e SEFAZ nao estao integrados.

## Integracoes

O sistema nao esta conectado a banco para receber PIX nem a SEFAZ. O webhook PIX retorna indisponivel ate que a assinatura do provedor seja validada. A acao de NF-e registra apenas uma simulacao sem validade fiscal. Nao use esses estados como comprovante ou conciliacao real.

## Producao

Configure HTTPS no host ou proxy, defina um caminho persistente e com backup para o SQLite e mantenha as credenciais somente no ambiente seguro do deploy. Revise a politica de privacidade com a entidade responsavel antes de receber dados reais.
