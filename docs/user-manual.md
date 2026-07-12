# Manual do usuario

## Abrindo o sistema

1. Abra o executavel do TeamOps.
2. Aguarde a tela de login.
3. Informe seu usuario/senha.
4. Apos o login, o dashboard principal sera exibido.

Se o sistema nao abrir ou apresentar erro de banco/pasta, acione o suporte e informe a mensagem exibida.

## Tela principal

A tela principal funciona como menu de acesso aos modulos. Os botoes disponiveis podem variar conforme seu nivel de acesso.

Informacoes exibidas no dashboard:

- Usuario logado.
- Operador associado.
- Turno.
- Data/hora.
- Indicadores resumidos, como tarefas abertas e Master Cards em andamento.

## Modulos

### Operadores

Consulta e manutencao de informacoes de operadores. Use para localizar dados basicos e apoiar rotinas de equipe.

### Atribuicoes

Modulo administrativo para atribuir operadores a liderancas/grupos. Disponivel apenas para usuarios autorizados.

### Relatorios

Consulta dados consolidados e registros operacionais. Requer perfil de lideranca.

No hub de relatorios, os principais atalhos sao:

- **Operadores:** relatorio gerencial por operador. Use para analisar presenca, producao, Master Card, follow-up e historico diario. O percentual de producao compara o tempo rodando das maquinas do local com o tempo programado para essas maquinas no periodo filtrado.
- **Presenca:** relatorio focado somente em comparecimento. Use para acompanhar dias escalados, presenca conforme, Yukyu, faltas, atrasos, saidas antecipadas e Todoke pendente. Possui filtros por periodo, turno, setor, grupo, status e busca por FJ/nome.
- **Follow, Tasks e MasterCard:** consultas especificas dos respectivos modulos.

Para usar o relatorio de presenca:

1. Abra **Relatorios** no dashboard.
2. Clique em **Presenca**.
3. Selecione o periodo e, se necessario, filtre por turno, setor, grupo ou status.
4. Use a busca para localizar um operador pelo codigo FJ ou nome.
5. Confira os cards de resumo e a tabela de operadores.

### Presenca e layout

Exibe e organiza a presenca de operadores por setor/turno. Pode usar arquivos CSV de agenda importados pelo sistema.

### Haidai

Modulo para acompanhamento/exportacao de informacoes para visualizacao em monitor ou TV. **Comportamento inferido pelo codigo:** o HTML exportado e gravado em pasta configurada para consulta publica/local.

### Follow-up

Registro e acompanhamento de ocorrencias, motivos, tipos, equipamentos e status de tratativa.

### Tarefas

Controle de tarefas por turno, responsavel e status. O dashboard mostra a quantidade de tarefas abertas do turno atual.

### Master Card

Modulo para acompanhamento de itens em andamento e em follow-up. O dashboard mostra contagens por status.

### Monitor de producao

Importa e apresenta eventos de maquinas. Usa arquivos TXT/DAT e pode executar um BAT de sincronizacao antes da importacao.

Para detalhes de uso, validacao e comandos de suporte, consulte `docs/production-monitor-guide.md`.

### Hikitsugui

Registro e leitura de passagem de informacao. Existem telas para criacao, leitura por lider e leitura por operador.

### Sobra de peca

Registro e acompanhamento de sobras de peca. Requer perfil autorizado.

### PR e CL

Geracao/controle de documentos PR e CL com uso de templates e pastas configuradas.

### Yukyu/Paid leave

Acompanhamento de solicitacoes/controles relacionados a folga/licenca paga. Usa dados de operador, turno e motivos cadastrados.

### Administracao

Modulo restrito para manter os cadastros base do sistema. Use para revisar turnos, grupos, setores, locais, maquinas, equipamentos, status de maquina, codigos visuais da producao, tempos de procedimento, categorias, motivos/tipos de follow, shain, pendencias de maquina e log do sistema.

Operacoes principais:

- Selecione um cadastro na lateral esquerda.
- Use **Buscar** para filtrar os registros carregados.
- Clique em **Novo** para cadastrar um item editavel.
- Use o icone de lapis para editar uma linha existente.
- Use o icone de lixeira para excluir uma linha, apos confirmar.
- Em telas somente leitura, como **Pendencias de Maquina** e **Log do Sistema**, os botoes de novo, editar e excluir ficam indisponiveis.
- No **Log do Sistema**, preencha os filtros de data, usuario, modulo ou acao e clique em **Atualizar**.

Para a documentacao completa do painel e dos botoes, consulte `docs/admin-panel.md`.

### Controle de acesso

Modulo para usuarios, niveis de permissao e configuracoes de acesso. Uso restrito.

## Operacoes comuns

- Entrar no sistema no inicio do turno.
- Conferir se seu nome, turno e setor estao corretos.
- Abrir o modulo necessario pelo dashboard.
- Registrar informacoes com atencao a data, turno, setor, operador e anexos.
- Salvar antes de fechar a tela.
- Em relatorios, usar filtros antes de imprimir/exportar.
- Em modulos de monitor, aguardar a conclusao da importacao antes de atualizar a visualizacao.

## Boas praticas

- Nao abrir varias janelas do mesmo modulo sem necessidade.
- Evitar desligar o computador durante importacao ou salvamento.
- Confirmar se o drive/pasta de rede esta acessivel antes de iniciar o turno.
- Informar mensagens de erro completas ao suporte.
- Nao mover arquivos de templates, anexos, banco ou exportacoes sem alinhar com a equipe tecnica.
- Em monitores publicos, atualizar a tela quando houver suspeita de cache.

## Mensagens importantes

- **Acesso negado:** seu usuario nao possui permissao para o modulo.
- **Operador nao encontrado:** o usuario logado nao esta associado a um operador valido.
- **Turno nao encontrado:** o cadastro do operador aponta para um turno inexistente ou inconsistente.
- **Arquivo nao encontrado:** uma pasta ou template configurado esta ausente ou inacessivel.
- **Erro de banco:** o SQLite pode estar indisponivel, bloqueado ou inacessivel pela rede.

## Em caso de erro

1. Anote a mensagem.
2. Feche somente a tela com erro, se possivel.
3. Verifique se a rede e as pastas compartilhadas estao acessiveis.
4. Tente abrir novamente o modulo.
5. Se persistir, acione o suporte com usuario, modulo, horario e acao realizada.
