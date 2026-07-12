# TeamOps - Documentacao

## Visao geral

TeamOps e um sistema interno para gestao operacional de fabrica. A solution combina WinForms, WebView2 e modulos HTML/CSS/JavaScript para apoiar rotinas de lideranca, acompanhamento de operadores, producao, registros operacionais e consultas.

Esta documentacao foi gerada por analise da solution. Quando uma regra de negocio nao esta explicitamente descrita no codigo, ela aparece marcada como **comportamento inferido pelo codigo**.

## Objetivo do sistema

Centralizar informacoes operacionais usadas no ambiente industrial, reduzindo dependencias de controles dispersos e permitindo que lideres, operadores e administradores acompanhem dados de turno, operadores, maquinas, pendencias, comunicados e documentos operacionais.

## Publico alvo

- Operadores de fabrica que consultam comunicados e informacoes do turno.
- Lideres de grupo e liderancas intermediarias que acompanham presenca, tarefas, follow-up, PR, CL e monitoramento.
- Administradores responsaveis por usuarios, acessos e cadastros.
- Equipe tecnica responsavel por publicacao, banco SQLite, pastas compartilhadas e suporte.

## Principais modulos identificados

- Dashboard principal.
- Operadores e atribuicoes.
- Relatorios, incluindo relatorio gerencial de operadores e relatorio dedicado de presenca.
- Layout/presenca de operadores.
- Haidai, com exportacao HTML para TV ou monitor publico.
- Follow-up e relatorios de follow-up.
- Tarefas e relatorios de tarefas.
- Master Card e relatorios.
- Monitor de producao.
- Hikitsugui: criacao, leitura por lider e leitura por operador.
- Sobra de peca.
- PR e CL.
- Yukyu/paid leave tracking.
- Administracao e controle de acesso.

## Guias operacionais especificos

- `docs/production-monitor-guide.md`: uso do Monitor de Producao, comandos do `ProductionMonitorProbe.exe`, validacao de banco, status por setor, EC2 Administrator, dashboard e orientacao para prints.
- `docs/admin-panel.md`: documentacao do painel Administracao, com botoes, cadastros, validacoes e prints.

## Tecnologias utilizadas

- C# e .NET 9.
- WinForms.
- Microsoft Edge WebView2.
- Dapper.
- Microsoft.Data.Sqlite.
- SQLite.
- HTML/CSS/JavaScript nos modulos embarcados em WebView2.
- ClosedXML para rotinas com Excel.
- FluentValidation e BCrypt.Net-Next na camada Core.

## Estrutura do projeto

- `TeamOps.UI`: aplicacao principal WinForms e modulos HTML.
- `TeamOps.OperatorApp`: aplicacao separada para leitura de Hikitsugui por operador.
- `TeamOps.Core`: entidades, validadores e tipos comuns.
- `TeamOps.Data`: acesso a dados, repositorios, scripts SQL e migrations.
- `TeamOps.Config`: resolucao de caminhos, configuracao do banco e app settings.

## Como executar

1. Abrir `TeamOps.sln` no Visual Studio 2022 ou superior.
2. Restaurar pacotes NuGet.
3. Conferir os caminhos em `TeamOps.UI/App.config` e `TeamOps.OperatorApp/App.config`.
4. Garantir acesso de leitura/escrita ao banco SQLite configurado em `DatabasePath`.
5. Definir `TeamOps.UI` como projeto de inicializacao para o sistema principal.
6. Executar em Debug ou publicar como self-contained para distribuicao.

O app inicializa a cultura `pt-BR`, cria/atualiza o banco na inicializacao e abre a tela de login. Apos login valido, o dashboard HTML e carregado dentro do WinForms via WebView2.

## Versao HTML da documentacao

Alem dos arquivos Markdown, existe uma versao HTML em `docs/index.html`. Ela usa a logo `TeamOps.UI/Logo.png` por referencia relativa para apresentar a documentacao em formato mais visual.

## Manutencao da documentacao

Toda nova documentacao criada dentro de `docs/` deve ser integrada ao `docs/index.html`, com conteudo consultavel diretamente na pagina HTML. O arquivo Markdown continua como fonte de edicao, mas o usuario nao deve precisar abrir o `.md` para acessar as informacoes principais. Inclua tambem o link na lista de arquivos Markdown.
