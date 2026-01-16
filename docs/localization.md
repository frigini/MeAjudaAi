# Localização (i18n)

Guia completo de internacionalização e localização do MeAjudaAi Admin Portal.

## Visão Geral

O sistema suporta múltiplos idiomas através de arquivos `.resx` (Resource Files) e o framework de localização do .NET/Blazor.

**Idiomas Suportados:**
- 🇧🇷 Português (Brasil) - `pt-BR` (padrão)
- 🇺🇸 English (US) - `en-US`

## Arquitetura

```
src/Web/MeAjudaAi.Web.Admin/
├── Resources/
│   ├── Strings.resx          # Strings em inglês (fallback)
│   └── Strings.pt-BR.resx    # Strings em português
├── Services/
│   └── LocalizationService.cs # Serviço de gerenciamento de idioma
└── Components/
    └── Common/
        └── LanguageSwitcher.razor # Seletor de idioma
```

### Componentes Principais

#### 1. LocalizationService
Gerencia cultura atual e mudanças de idioma:

```csharp
public class LocalizationService
{
    public CultureInfo CurrentCulture { get; }
    public string CurrentLanguage { get; }
    public IReadOnlyList<CultureInfo> SupportedCultures { get; }
    
    public void SetCulture(string cultureName);
    public string GetString(string name);
    public string GetString(string name, params object[] arguments);
    
    public event Action? OnCultureChanged;
}
```

#### 2. Arquivos .resx
Armazenam strings localizadas com chave-valor:

**Strings.resx (inglês):**
```xml
<data name="Common.Save" xml:space="preserve">
  <value>Save</value>
</data>
```

**Strings.pt-BR.resx (português):**
```xml
<data name="Common.Save" xml:space="preserve">
  <value>Salvar</value>
</data>
```

#### 3. LanguageSwitcher Component
Menu dropdown para seleção de idioma na AppBar.

## Uso em Componentes Blazor

### Opção 1: IStringLocalizer (Recomendado)
Usa injeção de dependência do .NET:

```razor
@using Microsoft.Extensions.Localization
@inject IStringLocalizer<Resources.Strings> L

<MudButton>@L["Common.Save"]</MudButton>
<MudText>@L["Providers.Title"]</MudText>

<!-- Com parâmetros -->
<MudText>@L["Messages.ItemsFound", count]</MudText>
```

### Opção 2: LocalizationService
Para casos com lógica adicional:

```razor
@inject LocalizationService Localization

<MudButton>@Localization.GetString("Common.Save")</MudButton>

@code {
    protected override void OnInitialized()
    {
        // Escutar mudanças de idioma
        Localization.OnCultureChanged += StateHasChanged;
    }
}
```

## Categorias de Strings

### Common (Comum)
Textos usados em toda aplicação:

| Chave | pt-BR | en-US |
|-------|-------|-------|
| `Common.Save` | Salvar | Save |
| `Common.Cancel` | Cancelar | Cancel |
| `Common.Delete` | Excluir | Delete |
| `Common.Edit` | Editar | Edit |
| `Common.Search` | Pesquisar | Search |
| `Common.Loading` | Carregando... | Loading... |
| `Common.Actions` | Ações | Actions |
| `Common.Refresh` | Atualizar | Refresh |

### Navigation
Itens de menu e navegação:

| Chave | pt-BR | en-US |
|-------|-------|-------|
| `Nav.Dashboard` | Painel | Dashboard |
| `Nav.Providers` | Provedores | Providers |
| `Nav.Documents` | Documentos | Documents |
| `Nav.Profile` | Perfil | Profile |
| `Nav.Logout` | Sair | Logout |

### Providers
Tela de provedores:

| Chave | pt-BR | en-US |
|-------|-------|-------|
| `Providers.Title` | Provedores | Providers |
| `Providers.SearchPlaceholder` | Pesquisar por nome... | Search by name... |
| `Providers.Name` | Nome | Name |
| `Providers.Document` | Documento | Document |
| `Providers.Status` | Status | Status |
| `Providers.Active` | Ativo | Active |
| `Providers.Inactive` | Inativo | Inactive |

### Validation Messages
Mensagens de validação:

| Chave | pt-BR | en-US |
|-------|-------|-------|
| `Validation.Required` | Este campo é obrigatório | This field is required |
| `Validation.InvalidEmail` | E-mail inválido | Invalid email |
| `Validation.InvalidPhone` | Telefone inválido | Invalid phone |
| `Validation.InvalidDocument` | Documento inválido | Invalid document |

### Success/Error Messages

| Chave | pt-BR | en-US |
|-------|-------|-------|
| `Success.SavedSuccessfully` | Salvo com sucesso | Saved successfully |
| `Success.DeletedSuccessfully` | Excluído com sucesso | Deleted successfully |
| `Error.GenericError` | Ocorreu um erro | An error occurred |
| `Error.NetworkError` | Erro de conexão | Connection error |
| `Error.Unauthorized` | Sem permissão | Unauthorized |

## Adicionando Novas Strings

### 1. Adicionar em Strings.resx (inglês)
```xml
<data name="Providers.ConfirmDelete" xml:space="preserve">
  <value>Are you sure you want to delete this provider?</value>
</data>
```

### 2. Adicionar em Strings.pt-BR.resx (português)
```xml
<data name="Providers.ConfirmDelete" xml:space="preserve">
  <value>Tem certeza que deseja excluir este provedor?</value>
</data>
```

### 3. Usar no componente
```razor
@inject IStringLocalizer<Resources.Strings> L

<MudDialog>
    <DialogContent>
        <MudText>@L["Providers.ConfirmDelete"]</MudText>
    </DialogContent>
</MudDialog>
```

## Convenções de Nomenclatura

### Estrutura de Chaves
```
{Categoria}.{Ação/Contexto}{Tipo}
```

**Exemplos:**
- `Common.Save` - Ação comum "Salvar"
- `Providers.Title` - Título da página de Provedores
- `Validation.Required` - Mensagem de validação "obrigatório"
- `Error.NetworkError` - Mensagem de erro de rede

### Categorias
- `Common.` - Textos compartilhados
- `Nav.` - Navegação e menus
- `{Entity}.` - Específico de entidade (Providers, Documents, etc.)
- `Validation.` - Mensagens de validação
- `Success.` - Mensagens de sucesso
- `Error.` - Mensagens de erro
- `Aria.` - Labels de acessibilidade

## Mudança de Idioma

### Mudança Programática
```csharp
@inject LocalizationService Localization

@code {
    private void SwitchToEnglish()
    {
        Localization.SetCulture("en-US");
        // UI será atualizada automaticamente
    }
    
    private void SwitchToPortuguese()
    {
        Localization.SetCulture("pt-BR");
    }
}
```

### Persistência de Preferência
Para salvar preferência do usuário:

```csharp
@inject LocalizationService Localization
@inject ILocalStorageService LocalStorage

@code {
    protected override async Task OnInitializedAsync()
    {
        // Carregar preferência salva
        var savedCulture = await LocalStorage.GetItemAsync<string>("user-culture");
        if (!string.IsNullOrEmpty(savedCulture))
        {
            Localization.SetCulture(savedCulture);
        }
    }
    
    private async Task ChangeCulture(string cultureName)
    {
        Localization.SetCulture(cultureName);
        
        // Salvar preferência
        await LocalStorage.SetItemAsync("user-culture", cultureName);
    }
}
```

## Formatação de Data/Hora

As datas são formatadas automaticamente conforme a cultura:

```razor
@using System.Globalization

@code {
    private DateTime now = DateTime.Now;
    
    // pt-BR: 15/12/2024 14:30:00
    // en-US: 12/15/2024 2:30:00 PM
}

<MudText>@now.ToString("f")</MudText>
```

### Formatação Customizada
```csharp
// Formato longo
date.ToString("D", CultureInfo.CurrentUICulture)
// pt-BR: domingo, 15 de dezembro de 2024
// en-US: Sunday, December 15, 2024

// Formato curto
date.ToString("d", CultureInfo.CurrentUICulture)
// pt-BR: 15/12/2024
// en-US: 12/15/2024
```

## Números e Moedas

```csharp
decimal value = 1234.56m;

// Moeda
value.ToString("C", CultureInfo.CurrentUICulture)
// pt-BR: R$ 1.234,56
// en-US: $1,234.56

// Número
value.ToString("N2", CultureInfo.CurrentUICulture)
// pt-BR: 1.234,56
// en-US: 1,234.56

// Porcentagem
(0.15).ToString("P", CultureInfo.CurrentUICulture)
// pt-BR: 15,00%
// en-US: 15.00%
```

## MudBlazor Localization

MudBlazor tem suporte nativo para localização:

```csharp
// Program.cs
builder.Services.AddMudServices(config =>
{
    // ... outras configurações
});

// MudBlazor automaticamente usa CultureInfo.CurrentUICulture
// para formatações internas (DatePicker, DataGrid, etc.)
```

## Pluralização

Para textos com plural:

```xml
<!-- Strings.resx -->
<data name="Providers.ItemsFound" xml:space="preserve">
  <value>{0} provider(s) found</value>
</data>

<!-- Strings.pt-BR.resx -->
<data name="Providers.ItemsFound" xml:space="preserve">
  <value>{0} provedor(es) encontrado(s)</value>
</data>
```

Uso:
```razor
<MudText>@L["Providers.ItemsFound", count]</MudText>
```

## Testando Localização

### Teste Manual
1. Iniciar aplicação
2. Clicar no ícone de idioma (🌐) na AppBar
3. Selecionar idioma desejado
4. Verificar se textos mudaram

### Teste Programático
```csharp
[Fact]
public void LocalizationService_SwitchesToPortuguese()
{
    // Arrange
    var service = new LocalizationService(localizer);
    
    // Act
    service.SetCulture("pt-BR");
    
    // Assert
    Assert.Equal("pt-BR", service.CurrentCulture.Name);
    Assert.Equal("pt", service.CurrentLanguage);
}
```

## Boas Práticas

### ✅ DO (Faça)
- Use chaves descritivas e hierárquicas
- Mantenha strings.resx e strings.pt-BR.resx sincronizados
- Use `IStringLocalizer` quando possível
- Forneça valores padrão sensatos
- Teste em ambos os idiomas
- Use formatação de cultura para datas/números
- Documente strings complexas

### ❌ DON'T (Não Faça)
- Não hardcode textos em componentes
- Não use chaves genéricas (`Text1`, `Label2`)
- Não misture idiomas em uma chave
- Não esqueça de adicionar em ambos arquivos .resx
- Não use interpolação de string complexa (use parâmetros)

## Adicionando Novo Idioma

Para adicionar espanhol (es-ES):

### 1. Criar arquivo de recursos
```
Resources/Strings.es-ES.resx
```

### 2. Adicionar cultura suportada
```csharp
// LocalizationService.cs
public IReadOnlyList<CultureInfo> SupportedCultures { get; } = new List<CultureInfo>
{
    new CultureInfo("pt-BR"),
    new CultureInfo("en-US"),
    new CultureInfo("es-ES")  // ADICIONAR AQUI
};
```

### 3. Atualizar LanguageSwitcher
```csharp
private string GetLanguageName(CultureInfo culture)
{
    return culture.Name switch
    {
        "pt-BR" => "Português (Brasil)",
        "en-US" => "English (US)",
        "es-ES" => "Español (España)",  // ADICIONAR AQUI
        _ => culture.DisplayName
    };
}
```

### 4. Traduzir todas as strings
Copiar conteúdo de `Strings.resx` para `Strings.es-ES.resx` e traduzir valores.

## Troubleshooting

### Strings não mudam ao trocar idioma

**Problema:** Componente não atualiza após `SetCulture()`

**Solução:** Escutar evento `OnCultureChanged`:
```csharp
protected override void OnInitialized()
{
    Localization.OnCultureChanged += StateHasChanged;
}

public void Dispose()
{
    Localization.OnCultureChanged -= StateHasChanged;
}
```

### String aparece como chave

**Problema:** `@L["Common.Save"]` renderiza "Common.Save"

**Causas possíveis:**
1. Chave não existe em `.resx`
2. Namespace incorreto
3. Arquivo `.resx` não compilado

**Solução:**
1. Verificar chave existe em ambos arquivos
2. Rebuild projeto
3. Verificar `Build Action = Embedded Resource` no arquivo

### Cultura não muda

**Problema:** `SetCulture()` não tem efeito

**Solução:**
```csharp
// Definir cultura e UI culture
CultureInfo.CurrentCulture = culture;
CultureInfo.CurrentUICulture = culture;
```

## Referências

- [.NET Globalization and Localization](https://learn.microsoft.com/dotnet/core/extensions/globalization-and-localization)
- [Blazor Localization](https://learn.microsoft.com/aspnet/core/blazor/globalization-localization)
- [MudBlazor Internationalization](https://mudblazor.com/features/internationalization)
- [Resource Files (.resx)](https://learn.microsoft.com/dotnet/framework/resources/creating-resource-files-for-desktop-apps)

## Roadmap de Localização

### ✅ Implementado (Sprint 7.14)
- [x] Arquivos .resx para pt-BR e en-US
- [x] LocalizationService
- [x] LanguageSwitcher component
- [x] Integração com MudBlazor
- [x] Documentação completa

### 🚧 Futuro
- [ ] Persistência de preferência no backend
- [ ] Auto-detecção de idioma do navegador
- [ ] Strings para todas as páginas (Dashboard, Documents, etc.)
- [ ] Mensagens de validação FluentValidation localizadas
- [ ] Pluralização avançada
- [ ] Adicionar idiomas: es-ES, fr-FR
- [ ] Testes automatizados de localização
