# Shotcut - Documento Master de Features

Documento de referencia com todas as funcionalidades do aplicativo. Descreve regras de negocio, edge cases, caminho feliz e resultado esperado de cada feature.

---

## F01 - Gerenciamento de Grupos (Abas)

### Criar grupo
- **Caminho feliz:** Usuario clica "Grupo", digita o nome no prompt e confirma. Uma nova aba aparece no TabControl e o foco muda para ela.
- **Resultado esperado:** Grupo adicionado a lista interna `_groups`, persistido no arquivo de configuracao, aba criada com ListView vazio.
- **Regras de negocio:**
  - O nome nao pode ser vazio nem conter apenas espacos.
  - Se o usuario cancelar o prompt ou deixar vazio, nenhum grupo e criado.
  - O novo grupo e selecionado automaticamente apos criacao.
- **Edge cases:**
  - Nomes duplicados sao permitidos (nao ha validacao de unicidade).
  - Nao ha limite maximo de grupos.

### Renomear grupo
- **Caminho feliz:** Usuario seleciona a aba do grupo, clica "Renomear", digita o novo nome e confirma. O texto da aba atualiza.
- **Resultado esperado:** Propriedade `Name` do grupo atualizada, aba reflete o novo nome, configuracao salva.
- **Regras de negocio:**
  - So funciona se houver um grupo selecionado.
  - Nome vazio cancela a operacao.
- **Edge cases:**
  - Renomear para o mesmo nome e permitido (salva normalmente).

### Remover grupo
- **Caminho feliz:** Usuario seleciona a aba e clica "- Grupo". Um dialogo de confirmacao aparece. Ao confirmar, o grupo e todos os seus atalhos sao removidos.
- **Resultado esperado:** Grupo removido da lista, aba removida do TabControl, hotkeys do grupo desregistrados se estavam ativos, configuracao salva.
- **Regras de negocio:**
  - Exige confirmacao via MessageBox (Sim/Nao).
  - Se os atalhos estavam ativos, cada hotkey do grupo e desregistrado antes da remocao.
- **Edge cases:**
  - Remover o unico grupo existente deixa o TabControl vazio. A aplicacao continua funcional.
  - Se nenhum grupo esta selecionado, o clique nao faz nada.

---

## F02 - Gerenciamento de Atalhos

### Criar atalho
- **Caminho feliz:** Usuario clica "Adicionar", preenche nome, pressiona a combinacao de teclas no campo de atalho, seleciona a pasta destino pelo browser e confirma.
- **Resultado esperado:** Atalho adicionado ao grupo atual com ID unico incremental, exibido na ListView com nome, tecla e caminho. Configuracao salva.
- **Regras de negocio:**
  - Obrigatorio informar: nome (nao vazio), tecla de atalho (diferente de `Keys.None`), pasta destino (deve existir no sistema de arquivos).
  - Se qualquer campo obrigatorio estiver invalido, o modal exibe aviso e nao fecha.
  - O ID e gerado automaticamente como max(IDs existentes) + 1.
  - O atalho e criado com `Enabled = true` por padrao.
  - Se os hotkeys estavam ativos, sao desregistrados antes de abrir o modal e re-registrados ao fechar.
- **Edge cases:**
  - Se nao existe nenhum grupo, exibe aviso "Crie um grupo primeiro" e nao abre o modal.
  - Mesma tecla pode existir em atalhos de grupos diferentes (sem conflito, pois so um grupo fica ativo por vez).
  - Mesma tecla no mesmo grupo: o sistema tenta registrar ambos, mas o segundo registro falha no Windows (RegisterHotKey retorna false) e exibe erro.

### Editar atalho
- **Caminho feliz:** Usuario seleciona um atalho na lista, clica "Editar", modifica os campos desejados e confirma.
- **Resultado esperado:** O atalho no grupo e substituido pelo novo objeto com os dados atualizados. ListView atualizado, configuracao salva.
- **Regras de negocio:**
  - Exige selecao de um item na ListView.
  - O ID e o estado `Enabled` sao preservados do atalho original.
  - Mesmas validacoes de criacao se aplicam (nome, tecla, pasta).
  - Hotkeys desregistrados durante edicao e re-registrados apos.
- **Edge cases:**
  - Editar sem alterar nada e confirmar: salva normalmente (sem verificacao de mudanca).
  - Se nenhum item esta selecionado, o clique nao faz nada.

### Remover atalho
- **Caminho feliz:** Usuario seleciona o atalho na lista e clica "Remover". Dialogo de confirmacao aparece com o nome do atalho. Ao confirmar, o atalho e removido.
- **Resultado esperado:** Atalho removido da lista do grupo, ListView atualizado, hotkey desregistrado se estava ativo, configuracao salva.
- **Regras de negocio:**
  - Exige confirmacao via MessageBox (Sim/Nao).
  - Se os atalhos estavam ativos, o hotkey individual e desregistrado.
- **Edge cases:**
  - Se nenhum item esta selecionado, o clique nao faz nada.

### Habilitar/Desabilitar atalho individual
- **Caminho feliz:** Usuario marca ou desmarca a checkbox do atalho na ListView.
- **Resultado esperado:** Propriedade `Enabled` do atalho atualizada. Se o sistema esta ativo, todos os hotkeys sao desregistrados e re-registrados (somente os habilitados).
- **Regras de negocio:**
  - A mudanca e persistida imediatamente.
  - Atalhos desabilitados continuam na lista mas nao sao registrados como hotkeys globais.
- **Edge cases:**
  - Desabilitar todos os atalhos de um grupo ativo: nenhum hotkey fica registrado, mas o estado "Ativo" permanece.

---

## F03 - Sistema de Hotkeys Globais

### Ativacao e desativacao
- **Caminho feliz:** Usuario clica "Ativar". O botao muda para "Desativar" (vermelho), status muda para "Ativo" (verde). Os atalhos habilitados do grupo selecionado sao registrados como hotkeys globais do Windows.
- **Resultado esperado:** Hotkeys funcionam em qualquer janela do sistema. Pressionar a tecla registrada aciona a movimentacao de arquivos.
- **Regras de negocio:**
  - Somente atalhos com `Enabled = true` do grupo atual sao registrados.
  - Ao desativar, todos os hotkeys de todos os grupos sao desregistrados.
  - Ao trocar de aba com o sistema ativo, os hotkeys sao desregistrados e os do novo grupo sao registrados automaticamente.
- **Edge cases:**
  - Ativar sem nenhum grupo ou sem atalhos: nada e registrado, mas o estado visual muda para "Ativo".
  - Se outro programa ja usa a mesma combinacao de teclas, `RegisterHotKey` retorna false e exibe mensagem de erro com o nome do atalho conflitante.
  - Flag `MOD_NOREPEAT` ativa: manter a tecla pressionada nao gera eventos repetidos.

### Troca de aba com hotkeys ativos
- **Caminho feliz:** Usuario troca de aba enquanto o sistema esta ativo.
- **Resultado esperado:** Os hotkeys do grupo anterior sao desregistrados e os do novo grupo sao registrados.
- **Regras de negocio:**
  - O metodo `UnregisterAll` percorre todos os grupos (nao so o anterior) para garantir limpeza completa.
  - `RegisterAll` registra apenas os atalhos habilitados do grupo agora selecionado.

---

## F04 - Movimentacao de Imagens

### Mover imagens selecionadas
- **Caminho feliz:** Usuario seleciona uma ou mais imagens no Windows Explorer e pressiona o hotkey configurado. As imagens sao movidas para a pasta destino. Uma notificacao balloon aparece: "X imagem(ns) movida(s) para 'NomeDoAtalho'".
- **Resultado esperado:** Arquivos fisicamente movidos da origem para o destino. Balloon tip no tray exibe contagem de movidos.
- **Regras de negocio:**
  - Deteccao de arquivos via COM interop (`Shell.Application`): percorre todas as janelas do Explorer abertas e coleta os itens selecionados.
  - Apenas arquivos com extensao de imagem sao processados: `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.webp`, `.tiff`, `.tif`, `.svg`, `.ico`, `.heic`, `.avif`.
  - Comparacao de extensao e case-insensitive (convertida para lowercase).
  - Usa `File.Move` para transferir o arquivo.
- **Edge cases:**
  - Nenhuma imagem selecionada (ou somente nao-imagens): exibe balloon "Nenhuma imagem selecionada no Explorer".
  - Arquivo com mesmo nome ja existe no destino: renomeia com sufixo `_yyyyMMdd_HHmmss` (timestamp do momento da movimentacao). Exemplo: `foto_20260712_143022.jpg`.
  - Falha ao mover um arquivo individual (permissao, arquivo em uso, etc.): o catch vazio pula o arquivo silenciosamente. Demais arquivos continuam sendo processados.
  - Multiplas janelas do Explorer abertas: coleta selecao de todas elas.
  - Janela que nao e Explorer (ex: Internet Explorer COM window): filtrada pelo nome do executavel (`explorer`).

---

## F05 - Persistencia de Configuracao

### Salvar e carregar configuracao
- **Caminho feliz:** Toda alteracao (criar/editar/remover grupo ou atalho, marcar checkbox) salva automaticamente. Ao abrir o app, a configuracao e carregada.
- **Resultado esperado:** Arquivo `%APPDATA%\Shotcut\shortcuts.txt` contem todos os grupos e atalhos. Ao reabrir o app, o estado e restaurado.
- **Regras de negocio:**
  - Formato de arquivo texto estruturado (nao JSON, nao XML).
  - Grupos delimitados por `[Group: NomeDoGrupo]`.
  - Atalhos delimitados por `---`, seguidos por pares `Chave = Valor`.
  - Campos persistidos por atalho: Id, Label, Key, Modifiers, DestinationPath, Enabled.
  - Diretorio de configuracao e criado automaticamente se nao existir (`Directory.CreateDirectory`).
  - O `_nextId` e calculado como max de todos os IDs existentes + 1 na carga.
- **Edge cases:**
  - Arquivo de configuracao nao existe (primeira execucao): retorna lista vazia, app abre sem grupos.
  - Arquivo corrompido ou com linhas invalidas: linhas sem `=` ou fora de contexto sao ignoradas silenciosamente.
  - Valores de enum invalidos (`Key`, `Modifiers`): `Enum.Parse` lanca excecao nao tratada (crash potencial).

---

## F06 - System Tray e Ciclo de Vida

### Minimizar para tray
- **Caminho feliz:** Usuario clica no X da janela. A janela esconde (Hide), icone aparece no tray com balloon: "Aplicacao minimizada. Atalhos continuam ativos."
- **Resultado esperado:** Janela some da barra de tarefas. Atalhos continuam funcionando. Icone no tray permanece visivel.
- **Regras de negocio:**
  - `CloseReason.UserClosing` cancela o fechamento e esconde a janela.
  - Outros motivos de fechamento (Windows shutdown, TaskManager kill) permitem fechamento real.
  - No fechamento real: todos os hotkeys sao desregistrados e o icone do tray e escondido.

### Restaurar do tray
- **Caminho feliz:** Usuario da duplo-clique no icone do tray ou clica "Abrir" no menu de contexto.
- **Resultado esperado:** Janela reaparece no estado normal (nao minimizada) e vai para frente.
- **Regras de negocio:**
  - `Show()` + `WindowState = Normal` + `BringToFront()`.

### Sair da aplicacao
- **Caminho feliz:** Usuario clica "Sair" no menu de contexto do tray.
- **Resultado esperado:** Icone do tray escondido, aplicacao encerra via `Application.Exit()`.
- **Regras de negocio:**
  - Nao passa pelo evento `OnFormClosing` com `UserClosing` (usa `Application.Exit()` direto).

---

## F07 - Modal de Edicao de Atalho

### Captura de hotkey
- **Caminho feliz:** Usuario clica no campo de atalho e pressiona uma combinacao de teclas (ex: Ctrl+Shift+A). O campo exibe "Ctrl + Shift + A".
- **Resultado esperado:** `HotKey` e `HotKeyModifiers` do `HotkeyTextBox` armazenam a tecla e os modificadores.
- **Regras de negocio:**
  - O campo e ReadOnly (nao aceita digitacao normal).
  - Teclas modificadoras sozinhas (Ctrl, Alt, Shift) sao ignoradas ate que uma tecla principal seja pressionada.
  - A ultima combinacao pressionada e a que vale (sobrescreve a anterior).
  - O texto placeholder antes de qualquer captura: "Pressione um atalho...".
  - Modificadores sao opcionais. Uma tecla sozinha (ex: F5, C) e valida.

### Selecao de pasta destino
- **Caminho feliz:** Usuario clica "..." ao lado do campo de pasta. FolderBrowserDialog abre. Ao selecionar e confirmar, o caminho aparece no campo.
- **Resultado esperado:** Campo de texto preenchido com o caminho absoluto da pasta.
- **Regras de negocio:**
  - Se ja havia um caminho no campo, o dialogo abre nesse diretorio como ponto de partida.
  - Cancelar o dialogo nao altera o campo.

### Validacao ao confirmar
- **Regras de negocio:**
  - Nome vazio: MessageBox de aviso, modal permanece aberto.
  - Hotkey nao definido (`Keys.None`): MessageBox de aviso, modal permanece aberto.
  - Pasta vazia ou inexistente (`Directory.Exists` retorna false): MessageBox de aviso, modal permanece aberto.
  - `DialogResult` e resetado para `None` quando a validacao falha, impedindo o fechamento.

---

## F08 - Interface Visual

### Layout principal
- Janela com tamanho inicial 660x460, minimo 560x400.
- TabControl a esquerda (520x350, ancora em todas as direcoes).
- Botoes a direita em coluna fixa (posicao X = 544, largura 96).
- Botao "Ativar/Desativar" com altura maior (40px), negrito, cores distintas (verde para ativar, vermelho para desativar).
- Label de status abaixo do botao toggle (verde "Ativo" ou cinza "Inativo").

### ListView de atalhos
- Colunas: "Nome" (120px), "Atalho" (120px), "Pasta Destino" (260px).
- CheckBoxes habilitados para toggle individual.
- FullRowSelect e GridLines ativados.
- Dock Fill dentro da TabPage.
