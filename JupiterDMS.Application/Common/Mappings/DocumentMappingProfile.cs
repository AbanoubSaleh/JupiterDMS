using AutoMapper;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Domain.Entities;

namespace JupiterDMS.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile for document-related mappings.
/// </summary>
public class DocumentMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentMappingProfile"/> class.
    /// </summary>
    public DocumentMappingProfile()
    {
        CreateMap<Document, DocumentDto>()
            .ForMember(dest => dest.FolderName, opt => opt.Ignore())
            .ForMember(dest => dest.FolderPath, opt => opt.Ignore())
            .ForMember(dest => dest.LibraryName, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore());

        CreateMap<UploadDocumentDto, Document>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentVersion, opt => opt.Ignore())
            .ForMember(dest => dest.FileSizeBytes, opt => opt.Ignore())
            .ForMember(dest => dest.ContentType, opt => opt.Ignore())
            .ForMember(dest => dest.FileExtension, opt => opt.Ignore())
            .ForMember(dest => dest.MetadataJson, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.FileHash, opt => opt.Ignore())
            .ForMember(dest => dest.FileType, opt => opt.Ignore())
            .ForMember(dest => dest.CheckoutStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutBy, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutOn, opt => opt.Ignore())
            .ForMember(dest => dest.CheckoutExpiry, opt => opt.Ignore())
            .ForMember(dest => dest.Folder, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutByUser, opt => opt.Ignore())
            .ForMember(dest => dest.Versions, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<UpdateDocumentDto, Document>()
            .ForMember(dest => dest.FolderId, opt => opt.Ignore())
            .ForMember(dest => dest.FilePath, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentVersion, opt => opt.Ignore())
            .ForMember(dest => dest.FileSizeBytes, opt => opt.Ignore())
            .ForMember(dest => dest.ContentType, opt => opt.Ignore())
            .ForMember(dest => dest.FileExtension, opt => opt.Ignore())
            .ForMember(dest => dest.MetadataJson, opt => opt.Ignore())
            .ForMember(dest => dest.FileHash, opt => opt.Ignore())
            .ForMember(dest => dest.FileType, opt => opt.Ignore())
            .ForMember(dest => dest.CheckoutStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutBy, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutOn, opt => opt.Ignore())
            .ForMember(dest => dest.CheckoutExpiry, opt => opt.Ignore())
            .ForMember(dest => dest.Folder, opt => opt.Ignore())
            .ForMember(dest => dest.CheckedOutByUser, opt => opt.Ignore())
            .ForMember(dest => dest.Versions, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<DocumentVersion, DocumentVersionDto>()
            .ForMember(dest => dest.VersionNumber, opt => opt.MapFrom(src => src.VersionNumber))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
    }
}
