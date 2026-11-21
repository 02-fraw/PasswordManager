using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models;

public sealed class AccountCredential
{
    public int Id { get; set; }

    [Required]
    [MaxLength(160)]
    public string AccountName { get; set; } = string.Empty;

    public LoginIdentifierType LoginType { get; set; } = LoginIdentifierType.None;

    [MaxLength(200)]
    public string LoginIdentifier { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Password { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
