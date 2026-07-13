# Shortcut

Aplicacao Windows para mover imagens selecionadas no Explorer para pastas especificas usando atalhos de teclado globais. Feita com WinForms e .NET 8.

## O que faz

Voce seleciona imagens no Windows Explorer, pressiona um atalho de teclado configurado, e elas sao movidas para a pasta destino. Funciona em background, mesmo com a janela minimizada no system tray.

## Funcionalidades

- Atalhos de teclado globais registrados via Win32 API (funcionam com qualquer app em foco)
- Grupos de atalhos organizados em abas (so o grupo ativo registra hotkeys)
- Mesma tecla pode ter destinos diferentes em grupos diferentes
- Detecta arquivos selecionados no Explorer via COM (Shell.Application)
- Filtra apenas imagens: .jpg, .jpeg, .png, .gif, .bmp, .webp, .tiff, .tif, .svg, .ico, .heic, .avif
- Arquivos duplicados ganham sufixo com timestamp ao inves de sobrescrever
- Checkbox para habilitar/desabilitar atalhos individuais
- Minimiza para o system tray ao fechar (atalhos continuam funcionando)
- Notificacoes balloon tip para feedback visual
- Configuracoes persistidas em `%APPDATA%\Shotcut\shortcuts.txt`

## Como usar

1. Crie um grupo (aba) clicando em "+ Grupo"
2. Adicione atalhos no grupo: defina um nome, tecla de atalho e pasta destino
3. Clique em "Ativar"
4. No Explorer, selecione imagens e pressione o atalho configurado

## Build

```
dotnet build
dotnet run
```

Requer .NET 8 SDK e Windows.

## Estrutura

| Arquivo | Funcao |
|---------|--------|
| MainForm.cs | Formulario principal, TabControl, WndProc para WM_HOTKEY |
| EditShortcutForm.cs | Modal de criacao/edicao de atalhos |
| HotkeyTextBox.cs | TextBox que captura combinacoes de teclas |
| GlobalHotkey.cs | P/Invoke para RegisterHotKey/UnregisterHotKey |
| ExplorerHelper.cs | Detecta selecao no Explorer via COM interop |
| ConfigStore.cs | Leitura/escrita do arquivo de configuracao |
| ShortcutConfig.cs | Modelos de dados (ShortcutConfig, ShortcutGroup) |
