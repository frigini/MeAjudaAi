namespace MeAjudaAi.Contracts.Utilities.Constants;

/// <summary>
/// Mensagens de validação padronizadas utilizadas no sistema
/// </summary>
/// <remarks>
/// Baseadas nas mensagens realmente utilizadas no projeto.
/// </remarks>
public static class ValidationMessages
{
    /// <summary>
    /// Mensagens para campos obrigatórios (baseadas no uso atual)
    /// </summary>
    public static class Required
    {
        public const string Email = "O email é obrigatório.";
        public const string Username = "O nome de usuário é obrigatório.";
        public const string FirstName = "O nome é obrigatório.";
        public const string LastName = "O sobrenome é obrigatório.";
        public const string Id = "O identificador é obrigatório.";
        public const string Password = "A senha é obrigatória.";
        public const string Role = "O papel não pode estar vazio.";
    }

    /// <summary>
    /// Mensagens para formatos inválidos
    /// </summary>
    public static class InvalidFormat
    {
        public const string Email = "Formato de email inválido.";
        public const string Guid = "Formato de identificador inválido.";
        public const string Username = "Nome de usuário deve conter apenas letras, números, _, - e ..";
        public const string FirstName = "Nome deve conter apenas letras e espaços.";
        public const string LastName = "Sobrenome deve conter apenas letras e espaços.";
        public const string Password = "A senha deve conter pelo menos uma letra minúscula, uma letra maiúscula e um número.";
        public const string Role = "Papel inválido. Papéis válidos: {0}.";
        public const string PhoneNumber = "O número de telefone deve estar no formato internacional (ex.: +5511999999999).";
    }

    /// <summary>
    /// Mensagens para limites de tamanho (baseadas nas constraints reais)
    /// </summary>
    public static class Length
    {
        public const string UsernameTooShort = "Nome de usuário deve ter pelo menos 3 caracteres.";
        public const string UsernameTooLong = "Nome de usuário deve ter no máximo 30 caracteres.";
        public const string EmailTooLong = "Email deve ter no máximo 254 caracteres.";
        public const string FirstNameTooShort = "Nome deve ter pelo menos 2 caracteres.";
        public const string FirstNameTooLong = "Nome deve ter no máximo 100 caracteres.";
        public const string LastNameTooShort = "Sobrenome deve ter pelo menos 2 caracteres.";
        public const string LastNameTooLong = "Sobrenome deve ter no máximo 100 caracteres.";
        public const string PasswordTooShort = "A senha deve ter pelo menos 8 caracteres.";
    }

    /// <summary>
    /// Mensagens para recursos não encontrados
    /// </summary>
    public static class NotFound
    {
        public const string User = "Usuário não encontrado.";
        public const string UserByEmail = "Usuário com este email não encontrado.";
        public const string Resource = "Recurso não encontrado.";
    }

    /// <summary>
    /// Mensagens para conflitos de dados
    /// </summary>
    public static class Conflict
    {
        public const string EmailAlreadyExists = "Este email já está sendo utilizado.";
        public const string UsernameAlreadyExists = "Este nome de usuário já está sendo utilizado.";
    }

    /// <summary>
    /// Mensagens de erro genéricas
    /// </summary>
    public static class Generic
    {
        public const string InvalidData = "Um ou mais campos contêm dados inválidos.";
        public const string InternalError = "Erro interno do servidor.";
        public const string Unauthorized = "Token de autenticação ausente, inválido ou expirado.";
        public const string Forbidden = "Acesso negado. Permissões insuficientes.";
        public const string RateLimitExceeded = "Muitas tentativas. Tente novamente em alguns minutos.";
    }

    /// <summary>
    /// Mensagens e valores padrão para o módulo ServiceCatalogs
    /// </summary>
    public static class Catalogs
    {
        /// <summary>
        /// Valor exibido quando o nome da categoria não está disponível (navegação não carregada)
        /// </summary>
        public const string UnknownCategoryName = "Desconhecida";
    }

    /// <summary>
    /// Mensagens para o módulo Providers
    /// </summary>
    public static class Providers
    {
        public const string ErrorRetrievingProviders = "Ocorreu um erro ao recuperar os prestadores.";
        public const string StateParameterRequired = "O parâmetro de estado é obrigatório.";
    }
}
