using System.Security.Cryptography;
using System.Text;
using JupiterDMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JupiterDMS.Infrastructure.Infra.Services;

/// <summary>
/// File storage service implementation using local file system.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<FileStorageService> _logger;

    /// <summary>
    /// Configuration key for storage path.
    /// </summary>
    public const string StoragePathKey = "FileStorage:Path";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _storagePath = configuration[StoragePathKey] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
            _logger.LogInformation("Created storage directory: {StoragePath}", _storagePath);
        }
    }

    /// <inheritdoc/>
    public async Task<string> SaveFileAsync(string fileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storagePath, uniqueFileName);

        using (var fileToWrite = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await fileStream.CopyToAsync(fileToWrite, cancellationToken);
        }

        return uniqueFileName;
    }

    /// <inheritdoc/>
    public async Task<string> SaveFileAsync(IFormFile file, Guid documentId, int versionNumber, CancellationToken cancellationToken = default)
    {
        // Create directory structure: documents/{documentId}/versions/{versionNumber}/
        var documentPath = Path.Combine(_storagePath, "documents", documentId.ToString());
        var versionPath = Path.Combine(documentPath, "versions", versionNumber.ToString());

        if (!Directory.Exists(versionPath))
        {
            Directory.CreateDirectory(versionPath);
        }

        var fileName = file.FileName;
        var filePath = Path.Combine(versionPath, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        // Return relative path from storage root
        return Path.Combine("documents", documentId.ToString(), "versions", versionNumber.ToString(), fileName);
    }

    /// <inheritdoc/>
    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return await Task.FromResult(stream);
    }

    /// <inheritdoc/>
    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, filePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_storagePath, filePath);
        return await Task.FromResult(File.Exists(fullPath));
    }

    /// <inheritdoc/>
    public async Task<string> CalculateFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = await Task.Run(() => sha256.ComputeHash(fileStream), cancellationToken);
            return Convert.ToBase64String(hash);
        }
    }

    /// <inheritdoc/>
    public async Task<string> SaveFileAsync(string libraryName, string folderPath, string fileName, Stream fileStream, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(libraryName))
            throw new ArgumentException("Library name cannot be null or empty.", nameof(libraryName));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        // Sanitize inputs
        libraryName = SanitizePathComponent(libraryName);
        folderPath = SanitizeFolderPath(folderPath);
        fileName = SanitizeFileName(fileName);

        // Create directory structure
        var directoryPath = Path.Combine(_storagePath, libraryName);
        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            directoryPath = Path.Combine(directoryPath, folderPath);
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            _logger.LogDebug("Created directory: {DirectoryPath}", directoryPath);
        }

        // Generate unique file name if needed
        var uniqueFileName = await GenerateUniqueFileNameAsync(libraryName, folderPath, fileName, cancellationToken);

        var fullFilePath = Path.Combine(directoryPath, uniqueFileName);
        var relativePath = Path.GetRelativePath(_storagePath, fullFilePath).Replace('\\', '/');

        try
        {
            using var fileStreamOutput = new FileStream(fullFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
            await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            await fileStreamOutput.FlushAsync(cancellationToken);

            _logger.LogInformation("File saved successfully: {FilePath}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file: {FilePath}", relativePath);

            // Clean up partial file if it exists
            if (File.Exists(fullFilePath))
            {
                try
                {
                    File.Delete(fullFilePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx, "Failed to delete partial file: {FilePath}", fullFilePath);
                }
            }

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var fullPath = Path.Combine(_storagePath, filePath.TrimStart('/', '\\'));

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var fileInfo = new FileInfo(fullPath);
        return await Task.FromResult(fileInfo.Length);
    }

    /// <inheritdoc/>
    public async Task<string> GenerateUniqueFileNameAsync(string libraryName, string folderPath, string fileName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        var directoryPath = Path.Combine(_storagePath, SanitizePathComponent(libraryName));
        if (!string.IsNullOrWhiteSpace(folderPath))
        {
            directoryPath = Path.Combine(directoryPath, SanitizeFolderPath(folderPath));
        }

        var originalFileName = fileName;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var counter = 1;

        while (File.Exists(Path.Combine(directoryPath, fileName)))
        {
            fileName = $"{fileNameWithoutExtension}_{counter}{extension}";
            counter++;
        }

        if (fileName != originalFileName)
        {
            _logger.LogDebug("Generated unique file name: {OriginalName} -> {UniqueName}", originalFileName, fileName);
        }

        return await Task.FromResult(fileName);
    }

    /// <summary>
    /// Sanitizes a path component (library name, folder name).
    /// </summary>
    /// <param name="component">The component to sanitize.</param>
    /// <returns>The sanitized component.</returns>
    private static string SanitizePathComponent(string component)
    {
        if (string.IsNullOrWhiteSpace(component))
            return string.Empty;

        var invalidChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).ToArray();
        var sanitized = new StringBuilder();

        foreach (var c in component)
        {
            if (!invalidChars.Contains(c) && c != '.' && c != ' ')
            {
                sanitized.Append(c);
            }
            else if (c == ' ')
            {
                sanitized.Append('_');
            }
        }

        return sanitized.ToString().Trim('_');
    }

    /// <summary>
    /// Sanitizes a folder path.
    /// </summary>
    /// <param name="folderPath">The folder path to sanitize.</param>
    /// <returns>The sanitized folder path.</returns>
    private static string SanitizeFolderPath(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return string.Empty;

        var parts = folderPath.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries);
        var sanitizedParts = parts.Select(SanitizePathComponent).Where(p => !string.IsNullOrEmpty(p));
        return string.Join("/", sanitizedParts);
    }

    /// <summary>
    /// Sanitizes a file name.
    /// </summary>
    /// <param name="fileName">The file name to sanitize.</param>
    /// <returns>The sanitized file name.</returns>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new StringBuilder();

        foreach (var c in fileName)
        {
            if (!invalidChars.Contains(c))
            {
                sanitized.Append(c);
            }
            else if (c == ' ')
            {
                sanitized.Append('_');
            }
        }

        return sanitized.ToString();
    }

    /// <inheritdoc/>
    public async Task CreateLibraryDirectoryAsync(string libraryName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(libraryName))
            throw new ArgumentException("Library name cannot be null or empty.", nameof(libraryName));

        var sanitizedLibraryName = SanitizePathComponent(libraryName);
        var libraryPath = Path.Combine(_storagePath, sanitizedLibraryName);

        if (!Directory.Exists(libraryPath))
        {
            Directory.CreateDirectory(libraryPath);
            _logger.LogInformation("Created library directory: {LibraryPath}", libraryPath);
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task CreateFolderDirectoryAsync(string libraryName, string folderPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(libraryName))
            throw new ArgumentException("Library name cannot be null or empty.", nameof(libraryName));

        if (string.IsNullOrWhiteSpace(folderPath))
            throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));

        var sanitizedLibraryName = SanitizePathComponent(libraryName);
        var sanitizedFolderPath = SanitizeFolderPath(folderPath);
        var fullFolderPath = Path.Combine(_storagePath, sanitizedLibraryName, sanitizedFolderPath);

        if (!Directory.Exists(fullFolderPath))
        {
            Directory.CreateDirectory(fullFolderPath);
            _logger.LogInformation("Created folder directory: {FolderPath}", fullFolderPath);
        }

        await Task.CompletedTask;
    }
}

