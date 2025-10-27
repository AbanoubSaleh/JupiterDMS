using AutoMapper;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for mapping Create DTOs to entities.
/// These mappings exclude auto-generated fields that should not be set by users.
/// </summary>
public class CreateDtoMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDtoMappingProfile"/> class.
    /// </summary>
    public CreateDtoMappingProfile()
    {
        // CreateLibraryDto -> Library
        CreateMap<CreateLibraryDto, Library>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Set from JWT token
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore()) // Set from JWT token
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.Folders, opt => opt.Ignore()); // Navigation property

        // CreateFolderDto -> Folder
        CreateMap<CreateFolderDto, Folder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.Path, opt => opt.Ignore()) // Calculated in controller
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Set from JWT token
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore()) // Set from JWT token
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore()) // Auto-generated
            .ForMember(dest => dest.Library, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.ParentFolder, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.ChildFolders, opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Documents, opt => opt.Ignore()); // Navigation property
    }
}
