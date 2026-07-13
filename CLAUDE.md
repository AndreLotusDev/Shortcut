# Shotcut - Mover Imagens via Atalhos Globais

## Sobre o Projeto
Aplicação WinForms (.NET 8) que permite configurar atalhos de teclado globais para mover imagens selecionadas no Windows Explorer para pastas específicas. Suporta agrupamentos de atalhos navegáveis por abas.

## Como Funciona
1. Usuário cria grupos (abas) e dentro de cada grupo configura atalhos (hotkey + pasta destino)
2. Ao clicar "Ativar", os atalhos do grupo/aba selecionado são registrados globalmente via Win32 API (RegisterHotKey)
3. Trocar de aba re-registra os atalhos do novo grupo (permite mesmas teclas em grupos diferentes com destinos diferentes)
4. Quando o atalho é pressionado, a app detecta os arquivos selecionados no Explorer via COM (Shell.Application)
5. Filtra apenas imagens e move para a pasta destino configurada

## Arquitetura

| Arquivo | Responsabilidade |
|---------|-----------------|
| `MainForm.cs` | Formulário principal com TabControl para grupos, lista de atalhos por grupo, toggle ativar/desativar, WndProc para WM_HOTKEY |
| `EditShortcutForm.cs` | Modal para criar/editar um atalho (nome, hotkey, pasta) |
| `HotkeyTextBox.cs` | TextBox customizado que captura combinações de teclas |
| `GlobalHotkey.cs` | Wrapper P/Invoke para RegisterHotKey/UnregisterHotKey do user32.dll |
| `ExplorerHelper.cs` | Detecta arquivos selecionados no Explorer via COM interop dinâmico |
| `ConfigStore.cs` | Persistência em arquivo txt estruturado no AppData |
| `ShortcutConfig.cs` | Modelos de dados: ShortcutConfig e ShortcutGroup |

## Persistência
Arquivo salvo em `%APPDATA%\Shotcut\shortcuts.txt` (sobrevive a rebuilds).

Formato:
```
[Group: Imagens]
---
Id = 1
Label = Comida
Key = C
Modifiers = None
DestinationPath = F:\imagens\comida
Enabled = True
```

## Extensões de Imagem Suportadas
.jpg, .jpeg, .png, .gif, .bmp, .webp, .tiff, .tif, .svg, .ico, .heic, .avif

## Comportamentos Importantes
- Grupos funcionam como abas: só o grupo ativo tem hotkeys registrados
- Trocar de aba automaticamente re-registra hotkeys do novo grupo
- Mesma tecla pode existir em grupos diferentes (com destinos diferentes)
- Hotkeys são desregistrados temporariamente quando a modal de edição está aberta
- Fechar a janela minimiza para o tray (atalhos continuam ativos)
- Arquivos duplicados ganham sufixo com timestamp
- Checkbox na lista habilita/desabilita atalhos individuais sem remover

## Build & Run
```
dotnet build
dotnet run
```

## Versionamento
Pasta `releases/` contém builds publicados de cada versão.

**REGRA: Toda vez que uma nova versão do app for concluída (feature nova, fix importante, etc.), OBRIGATORIAMENTE gerar o build e copiar para `releases/`:**
```
dotnet publish -c Release -o releases/vX.Y.Z
```
Exemplo: `dotnet publish -c Release -o releases/v1.0.0`

Incrementar a versão seguindo semver (major.minor.patch). Versão atual: **v1.1.0**.

## Documento Master de Features
O arquivo [FEATURES.md](FEATURES.md) contem a especificacao funcional completa de todas as features do app: regras de negocio, edge cases, caminho feliz e resultado esperado.

**REGRA: Toda feature nova ou atualizacao de feature existente OBRIGATORIAMENTE deve atualizar o FEATURES.md:**
- Feature nova: adicionar secao completa com caminho feliz, resultado esperado, regras de negocio e edge cases.
- Atualizacao de feature: documentar apenas o diferencial (o que mudou em relacao ao que ja esta documentado).
- Ao escrever ou reescrever trechos do FEATURES.md, aplicar a skill `ai-writting` (modo `edit`) para remover padroes de escrita artificial.

## Padroes de Codigo
Seguir as regras definidas em [STANDARDS.md](STANDARDS.md) (SOLID, arquitetura, convencoes WinForms).

## Manutencao do README
Quando uma alteracao modificar funcionalidades, estrutura de arquivos, fluxo de uso ou requisitos do projeto de forma significativa, atualizar o [README.md](README.md) para refletir o estado atual. Ao escrever ou reescrever trechos do README, aplicar a skill `ai-writting` (modo `edit`) para remover padroes de escrita artificial.

## Idioma
Interface em Português (PT-BR).
