using PasswordManager.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PasswordManager.Services.Pdf;

public sealed class VaultPdfService : IVaultPdfService
{
    public byte[] GenerateVaultPdf(
        IReadOnlyList<AccountItemViewModel> accounts,
        IReadOnlyList<RecoveryBlockViewModel> recoveryBlocks,
        bool darkTheme)
    {
        string pageBg = darkTheme ? "#0F172A" : "#FFFFFF";
        string textPrimary = darkTheme ? "#F8FAFC" : "#0F172A";
        string textMuted = darkTheme ? "#94A3B8" : "#475569";
        string headerBg = darkTheme ? "#312E81" : "#1E293B";
        string headerText = "#FFFFFF";
        string rowEven = darkTheme ? "#162033" : "#FFFFFF";
        string rowOdd = darkTheme ? "#1E293B" : "#F8FAFC";
        string border = darkTheme ? "#334155" : "#CBD5E1";
        string accent = darkTheme ? "#818CF8" : "#4F46E5";
        string blockBg = darkTheme ? "#162033" : "#F8FAFC";
        string codeBg = darkTheme ? "#0B1120" : "#FFFFFF";

        var generatedUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm 'UTC'");

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.PageColor(pageBg);
                page.DefaultTextStyle(x => x.FontSize(9.5f).FontColor(textPrimary));

                page.Header().PaddingBottom(12).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Password Vault Archive")
                            .FontSize(18)
                            .Bold()
                            .FontColor(textPrimary);

                        col.Item().PaddingTop(2)
                            .Text($"Generated {generatedUtc}  |  {accounts.Count} Accounts  |  {recoveryBlocks.Count} Recovery Blocks")
                            .FontSize(8.5f)
                            .FontColor(textMuted);
                    });
                });

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Text("Accounts")
                        .FontSize(13)
                        .Bold()
                        .FontColor(accent);

                    if (accounts.Count == 0)
                    {
                        col.Item().Text("No saved accounts.").FontColor(textMuted);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2.4f);
                                columns.RelativeColumn(3.2f);
                                columns.RelativeColumn(2.8f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(headerBg).Border(0.6f).BorderColor(border).Padding(7)
                                    .Text("Account / Application").Bold().FontColor(headerText);
                                header.Cell().Background(headerBg).Border(0.6f).BorderColor(border).Padding(7)
                                    .Text("Login (Email / Username / Phone)").Bold().FontColor(headerText);
                                header.Cell().Background(headerBg).Border(0.6f).BorderColor(border).Padding(7)
                                    .Text("Password").Bold().FontColor(headerText);
                            });

                            for (int i = 0; i < accounts.Count; i++)
                            {
                                var item = accounts[i];
                                string bg = i % 2 == 0 ? rowEven : rowOdd;

                                string loginDisplay = string.IsNullOrWhiteSpace(item.LoginIdentifier)
                                    ? "-"
                                    : string.IsNullOrWhiteSpace(item.LoginTypeLabel)
                                        ? item.LoginIdentifier
                                        : $"[{item.LoginTypeLabel}] {item.LoginIdentifier}";

                                string passDisplay = string.IsNullOrWhiteSpace(item.Password)
                                    ? "-"
                                    : item.Password;

                                table.Cell().Background(bg).Border(0.5f).BorderColor(border).Padding(6)
                                    .Text(item.AccountName).SemiBold();

                                table.Cell().Background(bg).Border(0.5f).BorderColor(border).Padding(6)
                                    .Text(loginDisplay);

                                table.Cell().Background(bg).Border(0.5f).BorderColor(border).Padding(6)
                                    .Text(passDisplay).FontFamily("Consolas");
                            }
                        });
                    }

                    if (recoveryBlocks.Count > 0)
                    {
                        col.Item().PaddingTop(8).Text("Recovery Codes")
                            .FontSize(13)
                            .Bold()
                            .FontColor(accent);

                        foreach (var block in recoveryBlocks)
                        {
                            col.Item().ShowEntire().Background(blockBg).Border(0.8f).BorderColor(border).Padding(10).Column(box =>
                            {
                                box.Spacing(5);
                                box.Item().Text(block.ServiceName)
                                    .FontSize(11)
                                    .Bold()
                                    .FontColor(textPrimary);

                                if (!string.IsNullOrWhiteSpace(block.Notes))
                                {
                                    box.Item().Text(block.Notes)
                                        .FontSize(8.5f)
                                        .FontColor(textMuted);
                                }

                                box.Item().Background(codeBg).Border(0.5f).BorderColor(border).Padding(7)
                                    .Text(block.Codes)
                                    .FontFamily("Consolas")
                                    .FontSize(9f)
                                    .FontColor(textPrimary);
                            });
                        }
                    }
                });

                page.Footer().AlignRight().Text(txt =>
                {
                    txt.Span("Page ").FontSize(8.5f).FontColor(textMuted);
                    txt.CurrentPageNumber().FontSize(8.5f).FontColor(textMuted);
                    txt.Span(" / ").FontSize(8.5f).FontColor(textMuted);
                    txt.TotalPages().FontSize(8.5f).FontColor(textMuted);
                });
            });
        });

        return document.GeneratePdf();
    }
}
