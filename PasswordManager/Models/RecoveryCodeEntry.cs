using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models;

public sealed class RecoveryCodeEntry
{
    public int Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string ServiceName { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Codes { get; set; } = string.Empty;

    [MaxLength(400)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
