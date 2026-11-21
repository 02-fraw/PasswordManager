using System.ComponentModel.DataAnnotations;
using PasswordManager.Models;

namespace PasswordManager.ViewModels;

public sealed class AccountItemViewModel
{
    public int Id { get; init; }
    public string AccountName { get; init; } = string.Empty;
    public LoginIdentifierType LoginType { get; init; }
    public string LoginIdentifier { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }

    public string LoginTypeLabel => LoginType switch
    {
        LoginIdentifierType.Email => "Email",
        LoginIdentifierType.Username => "Username",
        LoginIdentifierType.Phone => "Phone",
        _ => string.Empty
    };
}

public sealed class RecoveryBlockViewModel
{
    public int Id { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string Codes { get; init; } = string.Empty;
    public string Notes { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

public sealed class VaultPageViewModel
{
    public IReadOnlyList<AccountItemViewModel> Accounts { get; init; } = [];
    public IReadOnlyList<RecoveryBlockViewModel> RecoveryBlocks { get; init; } = [];
    public int TotalAccountCount { get; init; }
    public int TotalRecoveryBlockCount { get; init; }
    public bool IsAdminView { get; init; }
}

public sealed class AccountFormInputModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Account or application name is required.")]
    [MaxLength(160, ErrorMessage = "Account name cannot exceed 160 characters.")]
    public string AccountName { get; set; } = string.Empty;

    public LoginIdentifierType LoginType { get; set; } = LoginIdentifierType.Email;

    [MaxLength(200, ErrorMessage = "Login identifier cannot exceed 200 characters.")]
    public string? LoginIdentifier { get; set; }

    [MaxLength(300, ErrorMessage = "Password cannot exceed 300 characters.")]
    public string? Password { get; set; }
}

public sealed class RecoveryCodeFormInputModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Service or account name is required.")]
    [MaxLength(160, ErrorMessage = "Service name cannot exceed 160 characters.")]
    public string ServiceName { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one recovery code is required.")]
    [MaxLength(2000, ErrorMessage = "Recovery codes cannot exceed 2000 characters.")]
    public string Codes { get; set; } = string.Empty;

    [MaxLength(400, ErrorMessage = "Notes cannot exceed 400 characters.")]
    public string? Notes { get; set; }
}
