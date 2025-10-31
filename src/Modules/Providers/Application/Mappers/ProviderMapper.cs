using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

namespace MeAjudaAi.Modules.Providers.Application.Mappers;

/// <summary>
/// Mapeador para conversão entre entidades de domínio e DTOs.
/// </summary>
public static class ProviderMapper
{
    /// <summary>
    /// Converte uma entidade Provider para ProviderDto.
    /// </summary>
    public static ProviderDto ToDto(this Provider provider)
    {
        return new ProviderDto(
            provider.Id.Value,
            provider.UserId,
            provider.Name,
            provider.Type,
            provider.BusinessProfile.ToDto(),
            provider.VerificationStatus,
            provider.Documents.Select(d => d.ToDto()).ToList(),
            provider.Qualifications.Select(q => q.ToDto()).ToList(),
            provider.CreatedAt,
            provider.UpdatedAt,
            provider.IsDeleted,
            provider.DeletedAt
        );
    }

    /// <summary>
    /// Converte uma coleção de entidades Provider para coleção de ProviderDto.
    /// </summary>
    public static IReadOnlyList<ProviderDto> ToDto(this IEnumerable<Provider> providers)
    {
        return providers.Select(p => p.ToDto()).ToList();
    }

    /// <summary>
    /// Converte BusinessProfile para BusinessProfileDto.
    /// </summary>
    public static BusinessProfileDto ToDto(this BusinessProfile businessProfile)
    {
        return new BusinessProfileDto(
            businessProfile.LegalName,
            businessProfile.FantasyName,
            businessProfile.Description,
            businessProfile.ContactInfo.ToDto(),
            businessProfile.PrimaryAddress.ToDto()
        );
    }

    /// <summary>
    /// Converte ContactInfo para ContactInfoDto.
    /// </summary>
    public static ContactInfoDto ToDto(this ContactInfo contactInfo)
    {
        return new ContactInfoDto(
            contactInfo.Email,
            contactInfo.PhoneNumber,
            contactInfo.Website
        );
    }

    /// <summary>
    /// Converte Address para AddressDto.
    /// </summary>
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.State,
            address.ZipCode,
            address.Country
        );
    }

    /// <summary>
    /// Converte Document para DocumentDto.
    /// </summary>
    public static DocumentDto ToDto(this Document document)
    {
        return new DocumentDto(
            document.Number,
            document.DocumentType
        );
    }

    /// <summary>
    /// Converte Qualification para QualificationDto.
    /// </summary>
    public static QualificationDto ToDto(this Qualification qualification)
    {
        return new QualificationDto(
            qualification.Name,
            qualification.Description,
            qualification.IssuingOrganization,
            qualification.IssueDate,
            qualification.ExpirationDate,
            qualification.DocumentNumber
        );
    }

    /// <summary>
    /// Converte BusinessProfileDto para BusinessProfile.
    /// </summary>
    public static BusinessProfile ToDomain(this BusinessProfileDto dto)
    {
        return new BusinessProfile(
            dto.LegalName,
            new ContactInfo(dto.ContactInfo.Email, dto.ContactInfo.PhoneNumber, dto.ContactInfo.Website),
            new Address(
                dto.PrimaryAddress.Street,
                dto.PrimaryAddress.Number,
                dto.PrimaryAddress.Neighborhood,
                dto.PrimaryAddress.City,
                dto.PrimaryAddress.State,
                dto.PrimaryAddress.ZipCode,
                dto.PrimaryAddress.Country,
                dto.PrimaryAddress.Complement
            ),
            dto.FantasyName,
            dto.Description
        );
    }

    /// <summary>
    /// Converte DocumentDto para Document.
    /// </summary>
    public static Document ToDomain(this DocumentDto dto)
    {
        return new Document(dto.Number, dto.DocumentType);
    }

    /// <summary>
    /// Converte QualificationDto para Qualification.
    /// </summary>
    public static Qualification ToDomain(this QualificationDto dto)
    {
        return new Qualification(
            dto.Name,
            dto.Description,
            dto.IssuingOrganization,
            dto.IssueDate,
            dto.ExpirationDate,
            dto.DocumentNumber
        );
    }
}