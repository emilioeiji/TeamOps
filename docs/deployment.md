# Deployment

## Pre-requisitos

- Windows com suporte a .NET 9 desktop.
- Microsoft Edge WebView2 Runtime instalado.
- Acesso ao caminho configurado do SQLite.
- Permissao de leitura e escrita nas pastas operacionais.
- Templates Excel necessarios para PR/CL.
- Compartilhamentos de rede estaveis, quando usados.

## Projetos publicaveis

- `TeamOps.UI`: aplicacao principal.
- `TeamOps.OperatorApp`: app especifico para leitura de Hikitsugui por operador.

Ambos usam `net9.0-windows`, WinForms e dependem de `TeamOps.Core`, `TeamOps.Data` e `TeamOps.Config`.

## Publicacao self-contained

O projeto foi desenhado para deploy simples. A publicacao self-contained reduz necessidade de instalar SDK/runtime .NET na estacao, mas o WebView2 Runtime ainda deve estar presente ou ser distribuido pela politica interna de TI.

Pontos de atencao:

- Publicar todos os arquivos do output, incluindo `ui`, `Assets`, `App.config` e DLLs.
- Validar que os arquivos HTML/JS/CSS foram copiados.
- Manter icones e logos no output.
- Publicar `TeamOps.OperatorApp` separadamente quando usado em estacoes de operador.

## Estrutura esperada de pastas

Configuracao padrao em `TeamOps.UI/App.config`:

```text
C:\TeamOps\
  DB\teamops.db
  PR\
  CL\
  Modelos\ModeloPR.xlsx
  Modelos\ModeloCL.xlsx
  Anexo\Hikitsugui\
  Schedule\
  Haidai\
  Production\
    Events\
    Source\Events\
    Source\Dat\
    import-production.bat
    import.done
```

Esses caminhos podem ser adaptados para compartilhamento de rede. Se usar drive mapeado, garantir que o mapeamento exista no contexto do usuario que executa o app.

## Banco SQLite

`DatabasePath` define onde o SQLite sera aberto. A connection string usa `Cache=Shared` e `Mode=ReadWriteCreate`, portanto o app pode criar o arquivo se ele nao existir e houver permissao.

Recomendacoes:

- Usar pasta com backup regular.
- Evitar armazenamento em rede instavel.
- Garantir permissao de escrita no arquivo e na pasta.
- Evitar multiplas copias divergentes do banco.
- Definir rotina de restauracao em caso de corrupcao.

## WebView2

Todos os modulos HTML dependem do Edge WebView2 Runtime. Sem ele, telas podem ficar brancas ou falhar ao abrir.

Validacao pos-deploy:

1. Abrir dashboard.
2. Abrir um modulo HTML simples.
3. Abrir um modulo com ida/volta de dados, como operadores ou tarefas.
4. Verificar se nao ha tela branca.

## Permissoes

Conceder aos usuarios/grupos:

- Leitura no diretorio da aplicacao.
- Leitura/escrita no banco SQLite.
- Leitura/escrita em anexos, PR, CL, Schedule, Haidai e Production, conforme uso.
- Execucao do BAT de importacao de producao quando configurado.

## Dependencias externas por modulo

- PR/CL: templates Excel e pastas de saida.
- Hikitsugui: pasta de anexos.
- Presenca: arquivos CSV em `OperatorScheduleDirectory`.
- Haidai: pasta de exportacao HTML.
- Producao: pastas de eventos, DAT, BAT e arquivo de conclusao.
- WebView: runtime WebView2 e arquivos `ui`.

## Checklist de deploy

- Build Release concluido.
- Pasta publicada copiada integralmente.
- `App.config` revisado para ambiente alvo.
- `DatabasePath` acessivel.
- WebView2 instalado.
- Templates PR/CL presentes.
- Pastas de anexos/exportacao/importacao presentes.
- Teste de login realizado.
- Teste de abertura dos modulos principais realizado.
- Backup inicial do banco configurado.
