namespace JupiterDMS.Domain.Enums;

/// <summary>
/// Defines the types of files supported by the system.
/// </summary>
public enum FileType
{
    /// <summary>
    /// Unknown or unsupported file type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Microsoft Word document (.doc, .docx).
    /// </summary>
    Word = 1,

    /// <summary>
    /// Microsoft Excel spreadsheet (.xls, .xlsx).
    /// </summary>
    Excel = 2,

    /// <summary>
    /// Microsoft PowerPoint presentation (.ppt, .pptx).
    /// </summary>
    PowerPoint = 3,

    /// <summary>
    /// Portable Document Format (.pdf).
    /// </summary>
    Pdf = 4,

    /// <summary>
    /// Plain text file (.txt).
    /// </summary>
    Text = 5,

    /// <summary>
    /// Image file (.jpg, .jpeg, .png, .gif, .bmp).
    /// </summary>
    Image = 6,

    /// <summary>
    /// Video file (.mp4, .avi, .mov, .wmv).
    /// </summary>
    Video = 7,

    /// <summary>
    /// Audio file (.mp3, .wav, .wma).
    /// </summary>
    Audio = 8,

    /// <summary>
    /// Archive file (.zip, .rar, .7z).
    /// </summary>
    Archive = 9,

    /// <summary>
    /// XML file (.xml).
    /// </summary>
    Xml = 10,

    /// <summary>
    /// JSON file (.json).
    /// </summary>
    Json = 11,

    /// <summary>
    /// CSV file (.csv).
    /// </summary>
    Csv = 12,

    /// <summary>
    /// HTML file (.html, .htm).
    /// </summary>
    Html = 13,

    /// <summary>
    /// Other file types not specifically categorized.
    /// </summary>
    Other = 99
}
