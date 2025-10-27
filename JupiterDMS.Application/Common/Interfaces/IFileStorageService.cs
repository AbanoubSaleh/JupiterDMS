using Microsoft.AspNetCore.Http;

namespace JupiterDMS.Application.Common.Interfaces;

/// <summary>
/// Interface for file storage operations.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file asynchronously.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="fileStream">The file stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path where the file was saved.</returns>
    Task<string> SaveFileAsync(string fileName, Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a file asynchronously with library and folder structure.
    /// </summary>
    /// <param name="libraryName">The library name.</param>
    /// <param name="folderPath">The folder path within the library.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="fileStream">The file stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path where the file was saved.</returns>
    Task<string> SaveFileAsync(string libraryName, string folderPath, string fileName, Stream fileStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an uploaded file asynchronously with versioning support.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="documentId">The document identifier.</param>
    /// <param name="versionNumber">The version number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path where the file was saved.</returns>
    Task<string> SaveFileAsync(IFormFile file, Guid documentId, int versionNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file stream.</returns>
    Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    /// <param name="filePath">The full file path.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file size in bytes.</returns>
    Task<long> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique file name if a file with the same name already exists.
    /// </summary>
    /// <param name="libraryName">The library name.</param>
    /// <param name="folderPath">The folder path within the library.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unique file name.</returns>
    Task<string> GenerateUniqueFileNameAsync(string libraryName, string folderPath, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists asynchronously.
    /// </summary>
    /// <param name="filePath">The path of the file to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists; otherwise, false.</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the hash of a file asynchronously.
    /// </summary>
    /// <param name="fileStream">The file stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file hash.</returns>
    Task<string> CalculateFileHashAsync(Stream fileStream, CancellationToken cancellationToken = default);
}

