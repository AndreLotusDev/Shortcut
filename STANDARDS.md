# Padroes de Codigo e Arquitetura — WinForms (.NET 8)

Regras que todo codigo novo ou refatorado deve seguir neste projeto.

---

## SOLID

### Single Responsibility (SRP)
- Cada classe tem uma unica razao para mudar.
- Forms nao fazem logica de negocio. MainForm orquestra a UI, delega para helpers e services.
- Se um metodo faz mais de uma coisa (ex: valida + persiste + notifica), quebre em metodos privados ou extraia para outra classe.

### Open/Closed (OCP)
- Prefira adicionar classes/metodos novos a modificar os existentes quando o comportamento muda.
- Use interfaces quando houver variacao previsivel (ex: diferentes estrategias de persistencia).

### Liskov Substitution (LSP)
- Subclasses de controles (como HotkeyTextBox) devem funcionar onde o controle base funcionaria.
- Nao quebre contratos: se override OnKeyDown, mantenha o comportamento esperado do TextBox.

### Interface Segregation (ISP)
- Interfaces pequenas e focadas. Nenhuma classe deve ser obrigada a implementar metodos que nao usa.

### Dependency Inversion (DIP)
- Camadas altas (Forms) dependem de abstracoes, nao de implementacoes concretas.
- ConfigStore, ExplorerHelper e GlobalHotkey sao candidatos a interface quando o projeto crescer.

---

## Arquitetura do Projeto

### Camadas

```
UI (Forms)  -->  Services/Helpers  -->  Models
    |                 |
    v                 v
 Win32 API        Persistencia
(GlobalHotkey)   (ConfigStore)
```

- **UI**: Forms e controles customizados. Responsavel por capturar input e exibir feedback.
- **Services/Helpers**: Logica de negocio e integracao com o sistema (ExplorerHelper, GlobalHotkey).
- **Models**: Dados puros sem logica de UI (ShortcutConfig, ShortcutGroup).

### Regras de Dependencia
- Models nao referenciam Forms nem Services.
- Services nao referenciam Forms.
- Forms podem usar Services e Models.

---

## Convencoes de Codigo

### Nomenclatura
- Classes: `PascalCase`
- Metodos publicos: `PascalCase`
- Metodos privados: `PascalCase`
- Campos privados: `_camelCase` com underscore
- Variaveis locais e parametros: `camelCase`
- Constantes: `PascalCase`
- Eventos: `PascalCase`, prefixo `On` no handler

### Organizacao de Arquivos
- Uma classe por arquivo. Nome do arquivo = nome da classe.
- Forms com Designer: `NomeForm.cs` + `NomeForm.Designer.cs`.
- Arquivos na raiz enquanto o projeto for pequeno. Quando passar de ~15 arquivos .cs, organizar em pastas (`Models/`, `Services/`, `Controls/`).

### Formatacao
- Chaves na linha seguinte (Allman style).
- Maximo ~120 caracteres por linha.
- Um `using` por linha, ordenados: System primeiro, depois terceiros, depois do projeto.

---

## Padrao para Forms

### Estrutura de um Form
1. Campos privados
2. Construtor
3. Metodos de inicializacao de UI (se houver algo alem do Designer)
4. Event handlers
5. Metodos privados auxiliares
6. Override de WndProc / OnFormClosing / etc

### Regras
- Event handlers devem ser curtos: extraia logica para metodos privados ou services.
- Nao coloque SQL, HTTP, ou file I/O direto no handler. Delegue.
- Dispose de recursos COM e handles nativos no FormClosing ou Dispose.
- Use `Invoke`/`BeginInvoke` para atualizar UI de threads secundarias.

---

## P/Invoke e Interop

- Wrapper classes dedicadas para cada API nativa (ex: GlobalHotkey para user32).
- Nunca chame DllImport direto de um Form.
- Documente o handle/recurso e garanta cleanup (UnregisterHotKey, Release de objetos COM).
- Use `Marshal.ReleaseComObject` ou `dynamic` com cuidado no COM interop.

---

## Persistencia

- Toda leitura/escrita de arquivo passa pelo ConfigStore.
- Nao espalhe `File.ReadAllText` / `File.WriteAllText` pelo projeto.
- Path do arquivo de config definido em um unico lugar.
- Trate erros de I/O (arquivo corrompido, permissao negada) com fallback razoavel.

---

## Tratamento de Erros

- Catch generico (`catch (Exception)`) so no nivel mais alto (WndProc, Main).
- Em todo o resto, capture excecoes especificas.
- Log ou notifique o usuario. Nunca engula excecoes silenciosamente.
- Use `try/finally` para garantir cleanup de recursos nativos.

---

## O que NAO fazer

- Nao adicione frameworks de DI, logging ou ORM para um projeto WinForms deste tamanho. Mantenha simples.
- Nao crie interfaces "por precaucao" — so quando houver necessidade real de substituicao.
- Nao use async/await em event handlers de UI a menos que a operacao seja genuinamente lenta (>100ms).
- Nao misture logica de UI com logica de negocio no mesmo metodo.
- Nao hardcode strings magicas — use constantes ou enums.
