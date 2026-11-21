using PasswordManager.ViewModels;

namespace PasswordManager.Services.Pdf;

public interface IVaultPdfService
{
    byte[] GenerateVaultPdf(
        IReadOnlyList<AccountItemViewModel> accounts,
        IReadOnlyList<RecoveryBlockViewModel> recoveryBlocks,
        bool darkTheme);
}
