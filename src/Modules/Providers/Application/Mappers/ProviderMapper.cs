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
            Id: provider.Id.Value,
            UserId: provider.UserId,
            Name: provider.Name,
            Slug: provider.Slug,
            Type: provider.Type,
            provider.BusinessProfile.ToDto(),
            provider.Status,
            provider.VerificationStatus,
            provider.Tier,
            provider.Documents.Select(d => d.ToDto()).ToList(),
            provider.Qualifications.Select(q => q.ToDto()).ToList(),
            provider.Services.Select(s => s.ToDto()).ToList(),
            provider.CreatedAt,
            provider.UpdatedAt,
            provider.IsDeleted,
            provider.DeletedAt,
            provider.IsActive,
            provider.SuspensionReason,
            provider.RejectionReason
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
    /// O Endereço primário é omitido quando ShowAddressToClient é false.
    /// </summary>
    public static BusinessProfileDto ToDto(this BusinessProfile businessProfile)
    {
        return new BusinessProfileDto(
            businessProfile.LegalName,
            businessProfile.FantasyName,
            businessProfile.Description,
            businessProfile.ContactInfo.ToDto(),
            businessProfile.ShowAddressToClient ? businessProfile.PrimaryAddress.ToDto() : null,
            businessProfile.ShowAddressToClient
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
            contactInfo.Website,
            contactInfo.AdditionalPhoneNumbers
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
            document.DocumentType,
            document.FileName,
            document.FileUrl,
            document.IsPrimary
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
    /// Converte ProviderService para ProviderServiceDto.
    /// </summary>
    public static ProviderServiceDto ToDto(this ProviderService service)
    {
        return new ProviderServiceDto(
            service.ServiceId,
            service.ServiceName
        );
    }

    /// <summary>
    /// Converte BusinessProfileDto para BusinessProfile.
    /// </summary>
    public static BusinessProfile ToDomain(this BusinessProfileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto.ContactInfo);

        var primaryAddress = dto.ShowAddressToClient && dto.PrimaryAddress != null
            ? new Address(
                dto.PrimaryAddress.Street,
                dto.PrimaryAddress.Number,
                dto.PrimaryAddress.Neighborhood,
                dto.PrimaryAddress.City,
                dto.PrimaryAddress.State,
                dto.PrimaryAddress.ZipCode,
                dto.PrimaryAddress.Country,
                dto.PrimaryAddress.Complement)
            : new Address("N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", null);

        return new BusinessProfile(
            dto.LegalName,
            new ContactInfo(dto.ContactInfo.Email, dto.ContactInfo.PhoneNumber, dto.ContactInfo.Website, dto.ContactInfo.AdditionalPhones),
            primaryAddress,
            dto.FantasyName,
            dto.Description,
            dto.ShowAddressToClient
        );
    }

    /// <summary>
    /// Converte DocumentDto para Document.
    /// </summary>
    public static Document ToDomain(this DocumentDto dto)
    {
        return new Document(dto.Number, dto.DocumentType, dto.FileName, dto.FileUrl, dto.IsPrimary);
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
            dto.DocumentNumber);
    }


}
