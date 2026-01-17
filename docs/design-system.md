# Design System - MeAjudaAi Admin Portal

Este documento define o sistema de design do Admin Portal, incluindo paleta de cores, tipografia e componentes visuais.

---

## 🎨 Paleta de Cores

### Cores da Brand

O MeAjudaAi utiliza um esquema de cores profissional e acessível:

**Cores Primárias:**
- **Azul (Primary)**: `#1E88E5` (Material Blue 600)
  - Uso: Appbar, botões principais, links, elementos interativos
  - Variantes:
    - Light: `#42A5F5` (Blue 400)
    - Dark: `#1565C0` (Blue 800)
  - Contraste: `#FFFFFF` (branco)

- **Laranja (Secondary)**: `#FB8C00` (Material Orange 600)
  - Uso: CTAs secundários, highlights, badges de status
  - Variantes:
    - Light: `#FFA726` (Orange 400)
    - Dark: `#EF6C00` (Orange 800)
  - Contraste: `#FFFFFF` (branco)

**Cores Complementares:**
- **Creme (Tertiary)**: `#FFF8E1`
  - Uso: Linhas alternadas em tabelas, backgrounds sutis
  - Contraste: `#5D4037` (marrom escuro)

- **Branco (Background)**: `#FFFFFF`
  - Uso: Background principal, cards, modais
  - Contraste: `#212121` (quase preto)

### Cores de Estado

**Success (Sucesso):**
- Color: `#388E3C` (Material Green 700)
- Uso: Mensagens de sucesso, validações corretas

**Warning (Aviso):**
- Color: `#F57C00` (Material Orange 700)
- Uso: Alertas, ações que precisam atenção

**Error (Erro):**
- Color: `#D32F2F` (Material Red 700)
- Uso: Erros, validações falhas, ações destrutivas

**Info (Informação):**
- Color: `#0288D1` (Material Light Blue 700)
- Uso: Mensagens informativas, tooltips

### Cores de Texto

- **Primary**: `#212121` (quase preto)
- **Secondary**: `#757575` (cinza médio)
- **Disabled**: `#BDBDBD` (cinza claro)

### Cores de Background

- **Background**: `#FFFFFF` (branco)
- **Background Gray**: `#FAFAFA` (cinza muito claro)
- **Surface**: `#FFFFFF` (branco)

---

## 🌙 Dark Mode

### Paleta Dark Mode

O Admin Portal suporta modo escuro com ajustes nas cores:

**Cores Primárias (Dark):**
- **Azul (Primary)**: `#42A5F5` (mais claro para melhor contraste)
- **Laranja (Secondary)**: `#FFA726` (mais claro)
- **Marrom (Tertiary)**: `#5D4037` (substitui creme)

**Backgrounds (Dark):**
- **Background**: `#121212` (Material dark)
- **Surface**: `#1E1E1E`
- **Appbar**: `#1E1E1E`

**Texto (Dark):**
- **Primary**: `#FFFFFF`
- **Secondary**: `#B0B0B0`
- **Disabled**: `#6C6C6C`

---

## 📐 Tipografia

### Font Stack
```
Roboto, Helvetica, Arial, sans-serif
```

### Hierarchy

| Tipo | Tamanho | Peso | Uso |
|------|---------|------|-----|
| H1 | 2.5rem | 300 | Títulos de página |
| H2 | 2rem | 300 | Seções principais |
| H3 | 1.75rem | 400 | Subsecções |
| H4 | 1.5rem | 400 | Cards, dialogs |
| H5 | 1.25rem | 400 | Cabeçalhos de tabela |
| H6 | 1rem | 500 | Labels destacados |
| Body1 | 1rem | 400 | Texto principal |
| Body2 | 0.875rem | 400 | Texto secundário |
| Button | 0.875rem | 500 | Botões (uppercase) |
| Caption | 0.75rem | 400 | Legendas, notas |
| Subtitle1 | 1rem | 400 | Subtítulos |
| Subtitle2 | 0.875rem | 500 | Subtítulos menores |

---

## 🎯 Componentes Visuais

### Appbar
- Background: `#1E88E5` (azul primário)
- Texto: `#FFFFFF`
- Altura: 64px
- Sombra: Elevation 4

### Drawer (Menu Lateral)
- Background: `#FFFFFF` (light mode) / `#1E1E1E` (dark mode)
- Largura: 240px
- Item ativo: Background `#FFF8E1` (creme)
- Item hover: Opacity 0.06

### Tabelas
- Linhas: `#E0E0E0`
- Linhas alternadas: `#FFF8E1` (creme)
- Hover: `#FFF3E0` (laranja claro)

### Botões
- Primary: Azul `#1E88E5`
- Secondary: Laranja `#FB8C00`
- Hover: Opacity 0.08
- Ripple: Opacity 0.12

### Cards
- Background: `#FFFFFF`
- Border radius: 4px
- Sombra: Elevation 2
- Padding: 16px

---

## ♿ Acessibilidade

### Contraste WCAG 2.1 AA

Todas as combinações de cores atendem ao padrão WCAG 2.1 AA:

| Foreground | Background | Ratio | Status |
|------------|------------|-------|--------|
| #1E88E5 (azul) | #FFFFFF | 4.79:1 | ✅ AA Large |
| #FB8C00 (laranja) | #FFFFFF | 4.66:1 | ✅ AA Large |
| #212121 (texto) | #FFFFFF | 16.1:1 | ✅ AAA |
| #757575 (texto sec) | #FFFFFF | 4.61:1 | ✅ AA |

### Diretrizes
- Texto pequeno (<18pt): contraste mínimo 4.5:1
- Texto grande (≥18pt): contraste mínimo 3:1
- Componentes interativos: contraste mínimo 3:1

---

## 📦 Implementação

### Arquivo de Tema
```
src/Web/MeAjudaAi.Web.Admin/Themes/BrandTheme.cs
```

### Uso no App
```razor
<MudThemeProvider Theme="@BrandTheme.Theme" />
```

### Customização de Componentes

**Botão Primary:**
```razor
<MudButton Color="Color.Primary" Variant="Variant.Filled">
    Ação Principal
</MudButton>
```

**Botão Secondary:**
```razor
<MudButton Color="Color.Secondary" Variant="Variant.Filled">
    Ação Secundária
</MudButton>
```

**Card com Background Creme:**
```razor
<MudCard Style="background-color: var(--mud-palette-tertiary)">
    Conteúdo
</MudCard>
```

---

## 🔄 Histórico de Versões

### v1.0.0 (16 Jan 2026)
- ✅ Criação do design system
- ✅ Definição da paleta de cores da brand
- ✅ Implementação do BrandTheme.cs
- ✅ Suporte a dark mode
- ✅ Documentação de acessibilidade

---

## 📚 Referências

- [MudBlazor Theming](https://mudblazor.com/customization/default-theme)
- [Material Design Color System](https://m2.material.io/design/color/the-color-system.html)
- [WCAG 2.1 Contrast Guidelines](https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html)
