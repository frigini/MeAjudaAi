using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Shared.Tests.Unit.Constants;

/// <summary>
/// Testes unitários para a classe ModuleNames.
/// Valida os métodos de validação e verificação de implementação de módulos.
/// </summary>
public class ModuleNamesTests
{
    #region IsValid Tests

    [Theory]
    [InlineData(ModuleNames.Users, true)]
    [InlineData(ModuleNames.Providers, true)]
    [InlineData(ModuleNames.Documents, true)]
    [InlineData(ModuleNames.ServiceCatalogs, true)]
    [InlineData(ModuleNames.SearchProviders, true)]
    [InlineData(ModuleNames.Locations, true)]
    [InlineData(ModuleNames.Bookings, true)]
    [InlineData(ModuleNames.Notifications, true)]
    [InlineData(ModuleNames.Payments, true)]
    [InlineData(ModuleNames.Reports, true)]
    [InlineData(ModuleNames.Reviews, true)]
    public void IsValid_WithValidModuleName_ShouldReturnTrue(string moduleName, bool expected)
    {
        // Act
        var result = ModuleNames.IsValid(moduleName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("InvalidModule")]
    [InlineData("")]
    [InlineData("users")] // Case-sensitive
    [InlineData("USERS")] // Case-sensitive
    [InlineData("Random")]
    [InlineData("Admin")] // Foi removido
    [InlineData("Services")] // Foi removido
    public void IsValid_WithInvalidModuleName_ShouldReturnFalse(string moduleName)
    {
        // Act
        var result = ModuleNames.IsValid(moduleName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNullModuleName_ShouldReturnFalse()
    {
        // Act
        var result = ModuleNames.IsValid(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithWhiteSpaceModuleName_ShouldReturnFalse()
    {
        // Act
        var result = ModuleNames.IsValid("   ");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region IsImplemented Tests

    [Theory]
    [InlineData(ModuleNames.Users, true)]
    [InlineData(ModuleNames.Providers, true)]
    [InlineData(ModuleNames.Documents, true)]
    [InlineData(ModuleNames.ServiceCatalogs, true)]
    [InlineData(ModuleNames.SearchProviders, true)]
    [InlineData(ModuleNames.Locations, true)]
    public void IsImplemented_WithImplementedModule_ShouldReturnTrue(string moduleName, bool expected)
    {
        // Act
        var result = ModuleNames.IsImplemented(moduleName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ModuleNames.Bookings)]
    [InlineData(ModuleNames.Notifications)]
    [InlineData(ModuleNames.Payments)]
    [InlineData(ModuleNames.Reports)]
    [InlineData(ModuleNames.Reviews)]
    public void IsImplemented_WithPlannedModule_ShouldReturnFalse(string moduleName)
    {
        // Act
        var result = ModuleNames.IsImplemented(moduleName);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("InvalidModule")]
    [InlineData("")]
    [InlineData("Random")]
    public void IsImplemented_WithInvalidModuleName_ShouldReturnFalse(string moduleName)
    {
        // Act
        var result = ModuleNames.IsImplemented(moduleName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsImplemented_WithNullModuleName_ShouldReturnFalse()
    {
        // Act
        var result = ModuleNames.IsImplemented(null!);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Collections Tests

    [Fact]
    public void ImplementedModules_ShouldContainExactly6Modules()
    {
        // Assert
        Assert.Equal(6, ModuleNames.ImplementedModules.Count);
    }

    [Fact]
    public void ImplementedModules_ShouldContainExpectedModules()
    {
        // Arrange
        var expectedModules = new[]
        {
            ModuleNames.Users,
            ModuleNames.Providers,
            ModuleNames.Documents,
            ModuleNames.ServiceCatalogs,
            ModuleNames.SearchProviders,
            ModuleNames.Locations
        };

        // Assert
        foreach (var module in expectedModules)
        {
            Assert.Contains(module, ModuleNames.ImplementedModules);
        }
    }

    [Fact]
    public void AllModules_ShouldContainExactly11Modules()
    {
        // Assert
        Assert.Equal(11, ModuleNames.AllModules.Count);
    }

    [Fact]
    public void AllModules_ShouldContainAllImplementedModules()
    {
        // Assert
        foreach (var implementedModule in ModuleNames.ImplementedModules)
        {
            Assert.Contains(implementedModule, ModuleNames.AllModules);
        }
    }

    [Fact]
    public void AllModules_ShouldContainPlannedModules()
    {
        // Arrange
        var plannedModules = new[]
        {
            ModuleNames.Bookings,
            ModuleNames.Notifications,
            ModuleNames.Payments,
            ModuleNames.Reports,
            ModuleNames.Reviews
        };

        // Assert
        foreach (var module in plannedModules)
        {
            Assert.Contains(module, ModuleNames.AllModules);
        }
    }

    [Fact]
    public void ImplementedModules_ShouldBeReadOnly()
    {
        // Assert
        Assert.IsAssignableFrom<IReadOnlySet<string>>(ModuleNames.ImplementedModules);
    }

    [Fact]
    public void AllModules_ShouldBeReadOnly()
    {
        // Assert
        Assert.IsAssignableFrom<IReadOnlySet<string>>(ModuleNames.AllModules);
    }

    #endregion

    #region Constants Validation Tests

    [Fact]
    public void ModuleNames_AllConstantsShouldBeNotNullOrEmpty()
    {
        // Arrange
        var constantFields = typeof(ModuleNames)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.IsLiteral && f.FieldType == typeof(string));

        // Act & Assert
        foreach (var field in constantFields)
        {
            var value = field.GetValue(null) as string;
            Assert.NotNull(value);
            Assert.NotEmpty(value);
        }
    }

    [Fact]
    public void ModuleNames_AllImplementedModulesShouldBeValid()
    {
        // Assert
        foreach (var module in ModuleNames.ImplementedModules)
        {
            Assert.True(ModuleNames.IsValid(module), $"Módulo implementado '{module}' deveria ser válido");
        }
    }

    [Fact]
    public void ModuleNames_AllPlannedModulesShouldBeValid()
    {
        // Arrange
        var plannedModules = ModuleNames.AllModules.Except(ModuleNames.ImplementedModules);

        // Assert
        foreach (var module in plannedModules)
        {
            Assert.True(ModuleNames.IsValid(module), $"Módulo planejado '{module}' deveria ser válido");
        }
    }

    #endregion

    #region Edge Cases Tests

    [Theory]
    [InlineData("Users ", false)] // Trailing space
    [InlineData(" Users", false)] // Leading space
    [InlineData("Use rs", false)] // Space in the middle
    public void IsValid_WithSpacesInModuleName_ShouldReturnFalse(string moduleName, bool expected)
    {
        // Act
        var result = ModuleNames.IsValid(moduleName);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsImplemented_ShouldBeSubsetOfIsValid()
    {
        // Arrange - Get all possible module names
        var allModules = ModuleNames.AllModules;

        // Assert - Every implemented module should be valid
        foreach (var module in ModuleNames.ImplementedModules)
        {
            Assert.True(ModuleNames.IsValid(module), 
                $"Módulo implementado '{module}' deve ser válido");
            Assert.True(ModuleNames.IsImplemented(module), 
                $"Módulo implementado '{module}' deve retornar true em IsImplemented");
        }
    }

    [Fact]
    public void PlannedModules_ShouldBeValidButNotImplemented()
    {
        // Arrange
        var plannedModules = new[]
        {
            ModuleNames.Bookings,
            ModuleNames.Notifications,
            ModuleNames.Payments,
            ModuleNames.Reports,
            ModuleNames.Reviews
        };

        // Assert
        foreach (var module in plannedModules)
        {
            Assert.True(ModuleNames.IsValid(module), 
                $"Módulo planejado '{module}' deve ser válido");
            Assert.False(ModuleNames.IsImplemented(module), 
                $"Módulo planejado '{module}' não deve estar implementado");
        }
    }

    #endregion
}
