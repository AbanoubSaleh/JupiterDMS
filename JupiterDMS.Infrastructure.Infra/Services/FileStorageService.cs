using System.Security.Cryptography;
using JupiterDMS.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace JupiterDMS.Infrastructure.Infra.Services;

/// <summary>
/// File storage service implementation using local file system.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _storagePath;

    /// <summary>
    /// Configuration key for storage path.
    /// </summary>
    public const string StoragePathKey = "FileStorage:Path";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileStorageService"/> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public FileStorageService(IConfiguration configuration)
    {
        _storagePath = configuration[StoragePathKey] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
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
}

