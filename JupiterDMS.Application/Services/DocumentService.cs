using AutoMapper;
using JupiterDMS.Application.Common.DTOs;
using JupiterDMS.Application.Common.Interfaces;
using JupiterDMS.Domain.Entities;
using JupiterDMS.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JupiterDMS.Application.Services;

/// <summary>
/// Service implementation for document management operations.
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly ILogger<DocumentService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work.</param>
    /// <param name="fileStorageService">The file storage service.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <param name="logger">The logger.</param>
    public DocumentService(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IMapper mapper,
        ILogger<DocumentService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DocumentDto>> GetDocumentsByFolderAsync(Guid folderId, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
        
        var filteredDocuments = documents
            .Where(d => d.FolderId == folderId && (includeDeleted || !d.IsDeleted))
            .OrderBy(d => d.Name);

        var documentDtos = new List<DocumentDto>();
        
        foreach (var document in filteredDocuments)
        {
            var dto = await MapDocumentToDto(document, cancellationToken);
            documentDtos.Add(dto);
        }

        return documentDtos;
    }

    /// <inheritdoc/>
    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken);
        if (document == null || document.IsDeleted)
            return null;

        return await MapDocumentToDto(document, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> UploadDocumentAsync(UploadDocumentDto request, IFormFile file, Guid uploadedBy, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be null or empty.", nameof(file));

        // Get folder information
        var folder = await _unitOfWork.Folders.GetByIdAsync(request.FolderId, cancellationToken);
        if (folder == null)
            throw new InvalidOperationException($"Folder with ID '{request.FolderId}' not found.");

        // Get library information
        var library = await _unitOfWork.Libraries.GetByIdAsync(folder.LibraryId, cancellationToken);
        if (library == null)
            throw new InvalidOperationException($"Library with ID '{folder.LibraryId}' not found.");

        // Calculate file hash
        string fileHash;
        using (var stream = file.OpenReadStream())
        {
            fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);
        }

        // Save file to storage
        string filePath;
        using (var stream = file.OpenReadStream())
        {
            filePath = await _fileStorageService.SaveFileAsync(
                library.Name,
                folder.Path?.TrimStart('/') ?? string.Empty,
                file.FileName,
                stream,
                cancellationToken);
        }

        // Create document entity
        var document = new Document
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            FolderId = request.FolderId,
            FilePath = filePath,
            CurrentVersion = 1,
            FileSizeBytes = file.Length,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileExtension = Path.GetExtension(file.FileName),
            Title = request.Title,
            Description = request.Description,
            Tags = request.Tags,
            Status = DocumentStatus.Published,
            FileHash = fileHash,
            FileType = GetFileType(file.FileName),
            CheckoutStatus = CheckoutStatus.Available,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = uploadedBy,
            IsDeleted = false
        };

        // Create initial document version
        var documentVersion = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = 1,
            FilePath = filePath,
            FileSizeBytes = file.Length,
            Comment = "Initial version",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = uploadedBy
        };

        // Save to database
        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.DocumentVersions.AddAsync(documentVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document uploaded successfully: {DocumentName} (ID: {DocumentId})", document.Name, document.Id);

        return await MapDocumentToDto(document, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> UploadDocumentWithOptionsAsync(UploadDocumentDto request, IFormFile file, Guid uploadedBy, string duplicateAction = "rename", CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be null or empty.", nameof(file));

        // Check if document with same name exists in folder
        var existingDocument = await GetDocumentByNameAndFolderAsync(request.Name, request.FolderId, cancellationToken);

        if (existingDocument != null)
        {
            switch (duplicateAction.ToLowerInvariant())
            {
                case "replace":
                    return await ReplaceDocumentAsync(existingDocument.Id, file, uploadedBy, cancellationToken);

                case "version":
                    return await CreateDocumentVersionAsync(existingDocument.Id, file, "Uploaded from Office Add-in", uploadedBy, cancellationToken);

                case "rename":
                default:
                    // Generate unique name and proceed with normal upload
                    request.Name = await GenerateUniqueDocumentNameAsync(request.Name, request.FolderId, cancellationToken);
                    break;
            }
        }

        // Proceed with normal upload
        return await UploadDocumentAsync(request, file, uploadedBy, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> DocumentExistsInFolderAsync(string name, Guid folderId, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
        return documents.Any(d => !d.IsDeleted &&
                                 d.FolderId == folderId &&
                                 string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    public async Task<DocumentDto?> GetDocumentByNameAndFolderAsync(string name, Guid folderId, CancellationToken cancellationToken = default)
    {
        var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
        var document = documents.FirstOrDefault(d => !d.IsDeleted &&
                                                    d.FolderId == folderId &&
                                                    string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

        return document != null ? await MapDocumentToDto(document, cancellationToken) : null;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateUniqueDocumentNameAsync(string baseName, Guid folderId, CancellationToken cancellationToken = default)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(baseName);
        var extension = Path.GetExtension(baseName);
        var counter = 1;
        var uniqueName = baseName;

        while (await DocumentExistsInFolderAsync(uniqueName, folderId, cancellationToken))
        {
            uniqueName = $"{nameWithoutExtension} ({counter}){extension}";
            counter++;
        }

        return uniqueName;
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> UpdateDocumentAsync(UpdateDocumentDto request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{request.Id}' not found.");

        // Check if document is checked out by another user
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut && document.CheckedOutBy != updatedBy)
            throw new InvalidOperationException("Document is checked out by another user.");

        // Update document properties
        document.Name = request.Name;
        document.Title = request.Title;
        document.Description = request.Description;
        document.Tags = request.Tags;
        document.Status = request.Status;
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = updatedBy;

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document updated successfully: {DocumentName} (ID: {DocumentId})", document.Name, document.Id);

        return await MapDocumentToDto(document, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DocumentDownloadDto> DownloadDocumentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{id}' not found.");

        if (!await _fileStorageService.FileExistsAsync(document.FilePath, cancellationToken))
            throw new InvalidOperationException($"Document file not found: {document.FilePath}");

        var fileStream = await _fileStorageService.GetFileAsync(document.FilePath, cancellationToken);

        return new DocumentDownloadDto
        {
            Name = document.Name,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            FileStream = fileStream
        };
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteDocumentAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id, cancellationToken);
        if (document == null || document.IsDeleted)
            return false;

        // Check if document is checked out
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut)
            throw new InvalidOperationException("Cannot delete a checked out document.");

        // Soft delete
        document.IsDeleted = true;
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = deletedBy;

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document deleted successfully: {DocumentName} (ID: {DocumentId})", document.Name, document.Id);

        return true;
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> MoveDocumentAsync(Guid documentId, Guid targetFolderId, Guid movedBy, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{documentId}' not found.");

        var targetFolder = await _unitOfWork.Folders.GetByIdAsync(targetFolderId, cancellationToken);
        if (targetFolder == null)
            throw new InvalidOperationException($"Target folder with ID '{targetFolderId}' not found.");

        // Check if document is checked out
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut && document.CheckedOutBy != movedBy)
            throw new InvalidOperationException("Cannot move a document that is checked out by another user.");

        // Update folder
        document.FolderId = targetFolderId;
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = movedBy;

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document moved successfully: {DocumentName} (ID: {DocumentId}) to folder {FolderId}", 
            document.Name, document.Id, targetFolderId);

        return await MapDocumentToDto(document, cancellationToken);
    }

    /// <summary>
    /// Maps a Document entity to a DocumentDto.
    /// </summary>
    /// <param name="document">The document entity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The document DTO.</returns>
    private async Task<DocumentDto> MapDocumentToDto(Document document, CancellationToken cancellationToken)
    {
        // Get folder and library information
        var folder = await _unitOfWork.Folders.GetByIdAsync(document.FolderId, cancellationToken);
        var library = folder != null ? await _unitOfWork.Libraries.GetByIdAsync(folder.LibraryId, cancellationToken) : null;

        // Get user information for created by and checked out by
        var createdByUser = await _unitOfWork.Users.GetByIdAsync(document.CreatedBy, cancellationToken);
        var checkedOutByUser = document.CheckedOutBy.HasValue 
            ? await _unitOfWork.Users.GetByIdAsync(document.CheckedOutBy.Value, cancellationToken) 
            : null;
        var modifiedByUser = document.ModifiedBy.HasValue 
            ? await _unitOfWork.Users.GetByIdAsync(document.ModifiedBy.Value, cancellationToken) 
            : null;

        return new DocumentDto
        {
            Id = document.Id,
            Name = document.Name,
            FolderId = document.FolderId,
            FolderName = folder?.Name ?? string.Empty,
            FolderPath = folder?.Path ?? string.Empty,
            LibraryName = library?.Name ?? string.Empty,
            FilePath = document.FilePath,
            CurrentVersion = document.CurrentVersion,
            FileSizeBytes = document.FileSizeBytes,
            ContentType = document.ContentType,
            FileExtension = document.FileExtension,
            Title = document.Title,
            Description = document.Description,
            Tags = document.Tags,
            Status = document.Status,
            CheckoutStatus = document.CheckoutStatus,
            CheckedOutBy = checkedOutByUser?.Username,
            CheckedOutOn = document.CheckedOutOn,
            CheckoutExpiry = document.CheckoutExpiry,
            CreatedOn = document.CreatedOn,
            CreatedBy = createdByUser?.Username ?? string.Empty,
            ModifiedOn = document.ModifiedOn,
            ModifiedBy = modifiedByUser?.Username
        };
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> CreateDocumentVersionAsync(Guid documentId, IFormFile file, string? versionComment, Guid uploadedBy, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be null or empty.", nameof(file));

        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{documentId}' not found.");

        // Check if document is checked out by another user
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut && document.CheckedOutBy != uploadedBy)
            throw new InvalidOperationException("Document is checked out by another user.");

        // Get folder and library information
        var folder = await _unitOfWork.Folders.GetByIdAsync(document.FolderId, cancellationToken);
        var library = await _unitOfWork.Libraries.GetByIdAsync(folder!.LibraryId, cancellationToken);

        // Calculate file hash
        string fileHash;
        using (var stream = file.OpenReadStream())
        {
            fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);
        }

        // Save new version file
        string filePath;
        using (var stream = file.OpenReadStream())
        {
            var versionFileName = $"v{document.CurrentVersion + 1}_{file.FileName}";
            filePath = await _fileStorageService.SaveFileAsync(
                library!.Name,
                folder.Path?.TrimStart('/') ?? string.Empty,
                versionFileName,
                stream,
                cancellationToken);
        }

        // Create new document version
        var newVersion = document.CurrentVersion + 1;
        var documentVersion = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            VersionNumber = newVersion,
            FilePath = filePath,
            FileSizeBytes = file.Length,
            Comment = versionComment ?? $"Version {newVersion}",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = uploadedBy
        };

        // Update document
        document.CurrentVersion = newVersion;
        document.FilePath = filePath;
        document.FileSizeBytes = file.Length;
        document.ContentType = file.ContentType ?? "application/octet-stream";
        document.FileHash = fileHash;
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = uploadedBy;

        // If document was checked out, check it back in
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut)
        {
            document.CheckoutStatus = CheckoutStatus.Available;
            document.CheckedOutBy = null;
            document.CheckedOutOn = null;
            document.CheckoutExpiry = null;
        }

        await _unitOfWork.DocumentVersions.AddAsync(documentVersion, cancellationToken);
        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New document version created: {DocumentName} v{Version} (ID: {DocumentId})",
            document.Name, newVersion, document.Id);

        return await MapDocumentToDto(document, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> ReplaceDocumentAsync(Guid existingDocumentId, IFormFile newFile, Guid userId, CancellationToken cancellationToken = default)
    {
        if (newFile == null || newFile.Length == 0)
            throw new ArgumentException("File cannot be null or empty.", nameof(newFile));

        var document = await _unitOfWork.Documents.GetByIdAsync(existingDocumentId, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{existingDocumentId}' not found.");

        // Check if document is checked out by another user
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut && document.CheckedOutBy != userId)
            throw new InvalidOperationException("Document is checked out by another user.");

        // Get folder and library information
        var folder = await _unitOfWork.Folders.GetByIdAsync(document.FolderId, cancellationToken);
        var library = await _unitOfWork.Libraries.GetByIdAsync(folder!.LibraryId, cancellationToken);

        // Calculate file hash
        string fileHash;
        using (var stream = newFile.OpenReadStream())
        {
            fileHash = await _fileStorageService.CalculateFileHashAsync(stream, cancellationToken);
        }

        // Save replacement file (overwrite existing)
        string filePath;
        using (var stream = newFile.OpenReadStream())
        {
            filePath = await _fileStorageService.SaveFileAsync(
                library!.Name,
                folder.Path?.TrimStart('/') ?? string.Empty,
                newFile.FileName,
                stream,
                cancellationToken);
        }

        // Update document with new file information
        document.FilePath = filePath;
        document.FileSizeBytes = newFile.Length;
        document.ContentType = newFile.ContentType ?? "application/octet-stream";
        document.FileExtension = Path.GetExtension(newFile.FileName);
        document.FileHash = fileHash;
        document.FileType = GetFileType(newFile.FileName);
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = userId;

        // If document was checked out, check it back in
        if (document.CheckoutStatus == CheckoutStatus.CheckedOut)
        {
            document.CheckoutStatus = CheckoutStatus.Available;
            document.CheckedOutBy = null;
            document.CheckedOutOn = null;
            document.CheckoutExpiry = null;
        }

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document replaced successfully: {DocumentName} (ID: {DocumentId}) by user {UserId}",
            document.Name, document.Id, userId);

        return await MapDocumentToDto(document, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DocumentVersionDto>> GetDocumentVersionsAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var versions = await _unitOfWork.DocumentVersions.GetAllAsync(cancellationToken);
        var documentVersions = versions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber);

        var versionDtos = new List<DocumentVersionDto>();

        foreach (var version in documentVersions)
        {
            var createdByUser = await _unitOfWork.Users.GetByIdAsync(version.CreatedBy, cancellationToken);

            versionDtos.Add(new DocumentVersionDto
            {
                Id = version.Id,
                DocumentId = version.DocumentId,
                VersionNumber = version.VersionNumber,
                FilePath = version.FilePath,
                FileSizeBytes = version.FileSizeBytes,
                Comment = version.Comment,
                CreatedOn = version.CreatedOn,
                CreatedBy = createdByUser?.Username ?? string.Empty
            });
        }

        return versionDtos;
    }

    /// <inheritdoc/>
    public async Task<DocumentDownloadDto> DownloadDocumentVersionAsync(Guid documentId, int version, CancellationToken cancellationToken = default)
    {
        var documentVersion = (await _unitOfWork.DocumentVersions.GetAllAsync(cancellationToken))
            .FirstOrDefault(v => v.DocumentId == documentId && v.VersionNumber == version);

        if (documentVersion == null)
            throw new InvalidOperationException($"Document version {version} not found for document ID '{documentId}'.");

        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{documentId}' not found.");

        if (!await _fileStorageService.FileExistsAsync(documentVersion.FilePath, cancellationToken))
            throw new InvalidOperationException($"Document version file not found: {documentVersion.FilePath}");

        var fileStream = await _fileStorageService.GetFileAsync(documentVersion.FilePath, cancellationToken);

        return new DocumentDownloadDto
        {
            Name = $"{document.Name}_v{version}{document.FileExtension}",
            ContentType = document.ContentType,
            FileSizeBytes = documentVersion.FileSizeBytes,
            FileStream = fileStream
        };
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DocumentDto>> SearchDocumentsAsync(string searchTerm, Guid? libraryId = null, Guid? folderId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<DocumentDto>();

        var documents = await _unitOfWork.Documents.GetAllAsync(cancellationToken);
        var searchTermLower = searchTerm.ToLowerInvariant();

        var filteredDocuments = documents.Where(d => !d.IsDeleted);

        // Filter by library if specified
        if (libraryId.HasValue)
        {
            var libraryFolders = (await _unitOfWork.Folders.GetAllAsync(cancellationToken))
                .Where(f => f.LibraryId == libraryId.Value)
                .Select(f => f.Id)
                .ToHashSet();

            filteredDocuments = filteredDocuments.Where(d => libraryFolders.Contains(d.FolderId));
        }

        // Filter by folder if specified
        if (folderId.HasValue)
        {
            filteredDocuments = filteredDocuments.Where(d => d.FolderId == folderId.Value);
        }

        // Search in name, title, description, and tags
        var matchingDocuments = filteredDocuments.Where(d =>
            d.Name.ToLowerInvariant().Contains(searchTermLower) ||
            (d.Title?.ToLowerInvariant().Contains(searchTermLower) ?? false) ||
            (d.Description?.ToLowerInvariant().Contains(searchTermLower) ?? false) ||
            (d.Tags?.ToLowerInvariant().Contains(searchTermLower) ?? false))
            .OrderBy(d => d.Name);

        var documentDtos = new List<DocumentDto>();

        foreach (var document in matchingDocuments)
        {
            var dto = await MapDocumentToDto(document, cancellationToken);
            documentDtos.Add(dto);
        }

        return documentDtos;
    }

    /// <inheritdoc/>
    public async Task<bool> CheckOutDocumentAsync(Guid documentId, Guid checkedOutBy, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.IsDeleted)
            return false;

        if (document.CheckoutStatus == CheckoutStatus.CheckedOut)
            throw new InvalidOperationException("Document is already checked out.");

        document.CheckoutStatus = CheckoutStatus.CheckedOut;
        document.CheckedOutBy = checkedOutBy;
        document.CheckedOutOn = DateTime.UtcNow;
        document.CheckoutExpiry = DateTime.UtcNow.AddHours(24); // 24-hour checkout period
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = checkedOutBy;

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document checked out: {DocumentName} (ID: {DocumentId}) by user {UserId}",
            document.Name, document.Id, checkedOutBy);

        return true;
    }

    /// <inheritdoc/>
    public async Task<DocumentDto> CheckInDocumentAsync(Guid documentId, IFormFile? file, string? versionComment, Guid checkedInBy, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.IsDeleted)
            throw new InvalidOperationException($"Document with ID '{documentId}' not found.");

        if (document.CheckoutStatus != CheckoutStatus.CheckedOut)
            throw new InvalidOperationException("Document is not checked out.");

        if (document.CheckedOutBy != checkedInBy)
            throw new InvalidOperationException("Document is checked out by another user.");

        // If a new file is provided, create a new version
        if (file != null && file.Length > 0)
        {
            return await CreateDocumentVersionAsync(documentId, file, versionComment, checkedInBy, cancellationToken);
        }
        else
        {
            // Just check in without creating a new version
            document.CheckoutStatus = CheckoutStatus.Available;
            document.CheckedOutBy = null;
            document.CheckedOutOn = null;
            document.CheckoutExpiry = null;
            document.ModifiedOn = DateTime.UtcNow;
            document.ModifiedBy = checkedInBy;

            _unitOfWork.Documents.Update(document);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Document checked in: {DocumentName} (ID: {DocumentId}) by user {UserId}",
                document.Name, document.Id, checkedInBy);

            return await MapDocumentToDto(document, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> CancelCheckOutAsync(Guid documentId, Guid cancelledBy, CancellationToken cancellationToken = default)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
        if (document == null || document.IsDeleted)
            return false;

        if (document.CheckoutStatus != CheckoutStatus.CheckedOut)
            return false;

        if (document.CheckedOutBy != cancelledBy)
            throw new InvalidOperationException("Document is checked out by another user.");

        document.CheckoutStatus = CheckoutStatus.Available;
        document.CheckedOutBy = null;
        document.CheckedOutOn = null;
        document.CheckoutExpiry = null;
        document.ModifiedOn = DateTime.UtcNow;
        document.ModifiedBy = cancelledBy;

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Document checkout cancelled: {DocumentName} (ID: {DocumentId}) by user {UserId}",
            document.Name, document.Id, cancelledBy);

        return true;
    }

    /// <summary>
    /// Determines the file type based on the file extension.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The file type.</returns>
    private static FileType GetFileType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".doc" or ".docx" => FileType.Word,
            ".xls" or ".xlsx" => FileType.Excel,
            ".ppt" or ".pptx" => FileType.PowerPoint,
            ".pdf" => FileType.Pdf,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => FileType.Image,
            ".mp4" or ".avi" or ".mov" or ".wmv" => FileType.Video,
            ".mp3" or ".wav" or ".wma" => FileType.Audio,
            ".zip" or ".rar" or ".7z" => FileType.Archive,
            _ => FileType.Other
        };
    }
}
