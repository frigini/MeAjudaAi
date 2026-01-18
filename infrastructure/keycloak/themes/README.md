# Tema Customizado Keycloak - MeAjudaAi

Este diretório contém o tema customizado para o Keycloak que faz a tela de login parecer parte do Admin Portal.

## 📁 Estrutura

```text
meajudaai/
├── login/          # Tema de login (principal)
├── account/        # Tema da área de conta do usuário
└── email/          # Tema de emails enviados
```

## 🎨 Estilo

- **Cores:** Roxo #594AE2 (mesmo do MudBlazor)
- **Fonte:** Roboto (Google Fonts)
- **Estilo:** Material Design
- **Fundo:** Gradient roxo/lilás

## 🚀 Como Usar

O tema é aplicado automaticamente quando o Keycloak inicia via AppHost.

O realm `meajudaai` está configurado para usar este tema:
```json
{
  "loginTheme": "meajudaai",
  "accountTheme": "meajudaai",
  "emailTheme": "meajudaai"
}
```

## 🎨 Customizar

Edite: `login/resources/css/login.css`

Principais variáveis:
```css
:root {
    --primary: #594AE2;        /* Cor principal */
    --primary-dark: #4839B8;   /* Hover */
    --primary-light: #7965FF;  /* Active */
}
```

Textos de branding:
```css
#kc-content::before {
    content: "MeAjudaAi - Admin Portal";
}
```

## 📖 Documentação Completa

Ver: [Keycloak UI Customization - Themes](https://www.keycloak.org/ui-customization/themes)
