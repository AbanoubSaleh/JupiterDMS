namespace JupiterDMS.Domain.Constants;

/// <summary>
/// Contains domain-wide constants.
/// </summary>
public static class DomainConstants
{
    /// <summary>
    /// Constants related to Library entity.
    /// </summary>
    public static class Library
    {
        /// <summary>
        /// Maximum length for library name.
        /// </summary>
        public const int NameMaxLength = 200;

        /// <summary>
        /// Maximum length for library description.
        /// </summary>
        public const int DescriptionMaxLength = 1000;
    }

    /// <summary>
    /// Constants related to Folder entity.
    /// </summary>
    public static class Folder
    {
        /// <summary>
        /// Maximum length for folder name.
        /// </summary>
        public const int NameMaxLength = 200;

        /// <summary>
        /// Maximum length for folder path.
        /// </summary>
        public const int PathMaxLength = 2000;
    }

    /// <summary>
    /// Constants related to Document entity.
    /// </summary>
    public static class Document
    {
        /// <summary>
        /// Maximum length for document name.
        /// </summary>
        public const int NameMaxLength = 255;

        /// <summary>
        /// Maximum length for file path.
        /// </summary>
        public const int FilePathMaxLength = 2000;

        /// <summary>
        /// Maximum length for content type.
        /// </summary>
        public const int ContentTypeMaxLength = 100;

        /// <summary>
        /// Maximum file size in bytes (100 MB).
        /// </summary>
        public const long MaxFileSizeBytes = 104857600;

        /// <summary>
        /// Maximum length for document title.
        /// </summary>
        public const int TitleMaxLength = 500;

        /// <summary>
        /// Maximum length for document description.
        /// </summary>
        public const int DescriptionMaxLength = 2000;

        /// <summary>
        /// Maximum length for document tags.
        /// </summary>
        public const int TagsMaxLength = 1000;

        /// <summary>
        /// Default checkout duration in hours.
        /// </summary>
        public const int DefaultCheckoutHours = 24;

        /// <summary>
        /// Maximum checkout duration in hours.
        /// </summary>
        public const int MaxCheckoutHours = 168; // 7 days
    }

    /// <summary>
    /// Constants related to User entity.
    /// </summary>
    public static class User
    {
        /// <summary>
        /// Maximum length for username.
        /// </summary>
        public const int UsernameMaxLength = 100;

        /// <summary>
        /// Maximum length for email.
        /// </summary>
        public const int EmailMaxLength = 255;

        /// <summary>
        /// Maximum length for password hash.
        /// </summary>
        public const int PasswordHashMaxLength = 500;

        /// <summary>
        /// Maximum length for first name.
        /// </summary>
        public const int FirstNameMaxLength = 100;

        /// <summary>
        /// Maximum length for last name.
        /// </summary>
        public const int LastNameMaxLength = 100;
    }

    /// <summary>
    /// Constants related to AuditLog entity.
    /// </summary>
    public static class AuditLog
    {
        /// <summary>
        /// Maximum length for entity type.
        /// </summary>
        public const int EntityTypeMaxLength = 100;

        /// <summary>
        /// Maximum length for description.
        /// </summary>
        public const int DescriptionMaxLength = 2000;
    }

    /// <summary>
    /// Constants related to JWT authentication.
    /// </summary>
    public static class Jwt
    {
        /// <summary>
        /// Configuration key for JWT settings section.
        /// </summary>
        public const string ConfigurationSection = "JwtSettings";

        /// <summary>
        /// Configuration key for JWT secret key.
        /// </summary>
        public const string KeyConfigurationKey = "JwtSettings:Key";

        /// <summary>
        /// Configuration key for JWT issuer.
        /// </summary>
        public const string IssuerConfigurationKey = "JwtSettings:Issuer";

        /// <summary>
        /// Configuration key for JWT audience.
        /// </summary>
        public const string AudienceConfigurationKey = "JwtSettings:Audience";

        /// <summary>
        /// Configuration key for JWT expiry minutes.
        /// </summary>
        public const string ExpiryMinutesConfigurationKey = "JwtSettings:ExpiryMinutes";

        /// <summary>
        /// Claim type for user ID.
        /// </summary>
        public const string UserIdClaimType = "userId";

        /// <summary>
        /// Claim type for username.
        /// </summary>
        public const string UsernameClaimType = "username";

        /// <summary>
        /// Claim type for user role.
        /// </summary>
        public const string RoleClaimType = "role";

        /// <summary>
        /// Default token expiry in minutes.
        /// </summary>
        public const int DefaultExpiryMinutes = 60;
    }

    /// <summary>
    /// Constants related to authentication and authorization.
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Minimum password length.
        /// </summary>
        public const int MinPasswordLength = 6;

        /// <summary>
        /// Minimum username length.
        /// </summary>
        public const int MinUsernameLength = 3;

        /// <summary>
        /// Admin role name.
        /// </summary>
        public const string AdminRole = "Admin";

        /// <summary>
        /// Editor role name.
        /// </summary>
        public const string EditorRole = "Editor";

        /// <summary>
        /// Viewer role name.
        /// </summary>
        public const string ViewerRole = "Viewer";

        /// <summary>
        /// Authorization policy for Admin role.
        /// </summary>
        public const string AdminPolicy = "AdminPolicy";

        /// <summary>
        /// Authorization policy for Editor role (Admin + Editor).
        /// </summary>
        public const string EditorPolicy = "EditorPolicy";

        /// <summary>
        /// Authorization policy for Viewer role (Admin + Editor + Viewer).
        /// </summary>
        public const string ViewerPolicy = "ViewerPolicy";
    }

    /// <summary>
    /// Constants related to search functionality.
    /// </summary>
    public static class Search
    {
        /// <summary>
        /// Minimum search query length.
        /// </summary>
        public const int MinQueryLength = 2;

        /// <summary>
        /// Maximum search query length.
        /// </summary>
        public const int MaxQueryLength = 500;

        /// <summary>
        /// Default page size for search results.
        /// </summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// Maximum page size for search results.
        /// </summary>
        public const int MaxPageSize = 100;
    }

    /// <summary>
    /// Constants related to file operations.
    /// </summary>
    public static class FileOperations
    {
        /// <summary>
        /// Supported Word document extensions.
        /// </summary>
        public static readonly string[] WordExtensions = { ".doc", ".docx" };

        /// <summary>
        /// Supported Excel document extensions.
        /// </summary>
        public static readonly string[] ExcelExtensions = { ".xls", ".xlsx" };

        /// <summary>
        /// Supported PowerPoint document extensions.
        /// </summary>
        public static readonly string[] PowerPointExtensions = { ".ppt", ".pptx" };

        /// <summary>
        /// Supported PDF document extensions.
        /// </summary>
        public static readonly string[] PdfExtensions = { ".pdf" };

        /// <summary>
        /// Supported image extensions.
        /// </summary>
        public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

        /// <summary>
        /// All supported document extensions.
        /// </summary>
        public static readonly string[] AllSupportedExtensions =
            WordExtensions.Concat(ExcelExtensions)
                         .Concat(PowerPointExtensions)
                         .Concat(PdfExtensions)
                         .Concat(ImageExtensions)
                         .Concat(new[] { ".txt", ".xml", ".json", ".csv", ".html", ".htm" })
                         .ToArray();
    }
}

