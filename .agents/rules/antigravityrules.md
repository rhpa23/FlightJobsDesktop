---
trigger: always_on
---

# 🛫 FLIGHTJOBS DESKTOP WORKSPACE RULES

## 📌 Visão Geral do Projeto
O **FlightJobs Desktop** é uma aplicação desktop desenvolvida em **C# / WPF (.NET Framework 4.8)** que se conecta ao Microsoft Flight Simulator (MSFS) via SimConnect SDK para rastrear e registrar voos de pilotos em uma plataforma de empregos virtuais (FlightJobs).

- **Projeto Principal:** `FlightJobs.Presentation` (Desktop App)
- **Visual e Design:** Tema Escuro Premium baseado no ModernWpf (`ui` namespace) e em um design system unificado em `App.xaml`.

---

## 🏗️ Arquitetura do Workspace (MVVM)
O projeto é estruturado estritamente sob o padrão **MVVM**. As diretrizes de desenvolvimento para cada camada são:

### 1. Views (`FlightJobs.Presentation/Views/`)
- Definidas em XAML. O Code-behind (`.xaml.cs`) deve conter apenas código estritamente relacionado à interface do usuário (ex: animações, manipulação direta de controles, eventos de carregamento de janela).
- **Sem Lógica de Negócios:** Toda lógica de negócios e estado de dados devem residir nos ViewModels.
- **Estilos Globais:** Utilize os recursos de estilo do `App.xaml` para garantir conformidade visual (ver seção de Design System abaixo).

### 2. ViewModels (`FlightJobs.Presentation/ViewModels/`)
- Devem herdar da classe base `ObservableObject` para obter suporte nativo a `INotifyPropertyChanged`.
- Use o padrão de propriedades auto-notificáveis com backing fields:
  ```csharp
  private string _username;
  public string Username
  {
      get => _username;
      set => OnPropertyChanged(ref _username, value);
  }
  ```
- Comandos devem ser implementados para expor ações para a View (via bindings).
- Lógica de comunicação assíncrona deve ser chamada a partir dos ViewModels.

### 3. Services & Infrastructure (`FlightJobs.Infrastructure/` & `ConnectorClientAPI/`)
- Acesso a banco de dados local, arquivos de propriedades, persistência e chamadas de API.
- Devem estender `ServiceBase` ou implementar as interfaces correspondentes em `Interfaces/`.
- Devem ser consumidos preferencialmente via injeção/instanciação nos ViewModels ou Controllers centrais.

### 4. Domain & Model (`FlightJobs.Domain/` & `FlightJobs.Model/`)
- Contém entidades do domínio, enums de voo (status, tipos), DTOs (Data Transfer Objects), e exceções customizadas.

---

## 🎨 Sistema de Design (Design System)
Baseado no design da aplicação web FlightJobs (React/Tailwind) e adaptado para a tecnologia WPF. O visual é de um **Cockpit/Dashboard de Aviação de Alta Tecnologia**.

### 1. Paleta de Cores (Definida no `App.xaml`)
Use **apenas** recursos nomeados da paleta de cores para manter a consistência visual. **Nunca escreva cores hexadecimais (hardcoded) nas Views.**

#### ◼️ Escala de Cinza / Backgrounds
- `{StaticResource BackgroundPrimary}`: `#111827` (Cinza escuro profundo de fundo)
- `{StaticResource BackgroundSecondary}`: `#1f2937` (Cinza médio para cards, painéis e menus)
- `{StaticResource BackgroundTertiary}`: `#374151` (Cinza claro para hover/elementos secundários)

#### 📝 Textos
- `{StaticResource TextPrimary}`: `#f9fafb` (Quase branco, texto principal de alta legibilidade)
- `{StaticResource TextSecondary}`: `#9ca3af` (Cinza médio para descrições, subtítulos e labels)
- `{StaticResource TextTertiary}`: `#6b7280` (Cinza escuro para detalhes e estados desabilitados)

#### 🔵 Cores de Destaque (Accent)
- `{StaticResource AccentPrimary}`: `#3b82f6` (Azul principal de destaque)
- `{StaticResource AccentPrimaryHover}`: `#2563eb` (Azul escuro para botões em foco)
- `{StaticResource AccentPrimaryBg}`: `#1a3b82f6` (Fundo azul translúcido - Opacidade 10%)
- `{StaticResource AccentPrimaryBgHover}`: `#333b82f6` (Fundo azul translúcido para hover - Opacidade 20%)

#### 🟢🔴🟡 Estados
- **Sucesso:** `{StaticResource Success}` (`#4ade80`) | Fundo: `{StaticResource SuccessBg}`
- **Erro:** `{StaticResource Error}` (`#f87171`) | Fundo: `{StaticResource ErrorBg}`
- **Alerta:** `{StaticResource Warning}` (`#fbbf24`) | Fundo: `{StaticResource WarningBg}`
- **Informação:** `{StaticResource Info}` (`#818cf8`) | Fundo: `{StaticResource InfoBg}`

### 2. Gradientes Premium (Visual de Alta Costura)
- `CardGradient` (Linear de `#1f2937` para `#111827`): Fundo padrão para Borders e painéis simulando profundidade.
- `HeaderGradient` (Linear horizontal de `#111827` -> `#1f2937` -> `#111827`): Usado no cabeçalho superior para criar uma identidade visual premium.
- `BlueGradientBg` (Linear de `#331e3a8a` para `#1f2937`): Gradiente azul tecnológico de fundo para áreas específicas de conexão/status.

### 3. Estilos de Controles (Aplicação Prática)
Sempre declare os estilos corretos em seus elementos WPF:
- **Cards e Painéis:** Use `<Border Style="{StaticResource CardStyle}">` ou `CardSubtleStyle`. Para cards clicáveis, use `CardHoverStyle` (possui feedback de hover e cursor Hand).
- **Botões:**
  - Primário (Destaque Azul): `<Button Style="{StaticResource ButtonPrimaryStyle}">`
  - Secundário (Borda/Ghost): `<Button Style="{StaticResource ButtonGhostStyle}">`
  - Perigo (Vermelho): `<Button Style="{StaticResource ButtonDangerStyle}">`
- **Inputs (Caixas de Texto):** `<TextBox Style="{StaticResource InputStyle}">`
- **Badges de Status:** `<Border Style="{StaticResource BadgeStyle}">` (ou `BadgeSuccessStyle` / `BadgeWarningStyle` para feedbacks).
- **Listas e Linhas:** Use `<Border Style="{StaticResource ListItemStyle}">` para linhas de grid interativas.
- **Tipografia:** Use o controle `TextBlock` padrão ou com `Style="{StaticResource TextPrimaryStyle}"` / `TextSecondaryStyle`.

---

## ⚡ Regras de Código e Concorrência (WPF / C#)
1. **Evite Bloquear a UI Thread:**
   - Chamadas de rede, atualizações do SimConnect e leituras de arquivos devem ser executadas assincronamente usando `async`/`await` e `Task.Run()` se necessário.
2. **Atualização Segura de UI (Dispatcher):**
   - Eventos assíncronos que alteram propriedades vinculadas à interface (ex: dados recebidos do SimConnect ou timers de voo) devem atualizar a UI através do Dispatcher:
     ```csharp
     Application.Current.Dispatcher.Invoke(() => {
         // Atualize o ViewModel ou controle de interface aqui
     });
     ```
3. **Uso de Ícones Nativo:**
   - Dê preferência aos ícones integrados do ModernWpf através de `<ui:SymbolIcon Symbol="Setting"/>` ou usando glifos da fonte do Windows com `<ui:FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xE7E8;"/>`.

---

## 🧭 Diretrizes do Workspace de Design (Adaptado de .windsurf/skills)
Estas regras adaptam as diretrizes de frontend web (Tailwind/React) para o ambiente WPF Desktop:

- **Minimalismo e Restrição Visual:** Evite decorações desnecessárias. Use espaçamento (`Margin` e `Padding` consistentes, ex: múltiplos de 4px ou 8px como `Margin="15,0,5,0"`, `Padding="20"`) in vez de linhas separadoras pesadas.
- **Profundidade e Camadas:** Utilize `DropShadowEffect` (como o `{StaticResource CardShadow}`) para dar profundidade aos cards e painéis flutuantes sobre o fundo escuro `#111827`.
- **Micro-interações:** Toda ação clicável deve possuir feedback visual imediato (mudança suave de cor ou opacidade).
- **Consistência de Layout:** Respeite a barra de navegação lateral (`LeftCompact` NavigationView) da janela principal, mantendo a visualização de sub-telas dentro do `<ui:Frame x:Name="contentFrame" />`.
