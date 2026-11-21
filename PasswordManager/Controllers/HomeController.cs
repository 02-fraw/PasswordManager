using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Services.Pdf;
using PasswordManager.ViewModels;

namespace PasswordManager.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IVaultPdfService _pdfService;

    public HomeController(ApplicationDbContext context, IVaultPdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accounts = await _context.AccountCredentials
            .AsNoTracking()
            .OrderBy(a => a.AccountName)
            .ThenBy(a => a.Id)
            .Select(a => new AccountItemViewModel
            {
                Id = a.Id,
                AccountName = a.AccountName,
                LoginType = a.LoginType,
                LoginIdentifier = a.LoginIdentifier,
                Password = a.Password,
                CreatedAtUtc = a.CreatedAtUtc,
                UpdatedAtUtc = a.UpdatedAtUtc
            })
            .ToListAsync();

        var recoveryBlocks = await _context.RecoveryCodeEntries
            .AsNoTracking()
            .OrderBy(r => r.ServiceName)
            .ThenBy(r => r.Id)
            .Select(r => new RecoveryBlockViewModel
            {
                Id = r.Id,
                ServiceName = r.ServiceName,
                Codes = r.Codes,
                Notes = r.Notes,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc
            })
            .ToListAsync();

        var model = new VaultPageViewModel
        {
            Accounts = accounts,
            RecoveryBlocks = recoveryBlocks,
            TotalAccountCount = accounts.Count,
            TotalRecoveryBlockCount = recoveryBlocks.Count,
            IsAdminView = false
        };

        ViewData["Title"] = "Password Vault";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? theme)
    {
        bool darkTheme = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase);

        var accounts = await _context.AccountCredentials
            .AsNoTracking()
            .OrderBy(a => a.AccountName)
            .ThenBy(a => a.Id)
            .Select(a => new AccountItemViewModel
            {
                Id = a.Id,
                AccountName = a.AccountName,
                LoginType = a.LoginType,
                LoginIdentifier = a.LoginIdentifier,
                Password = a.Password,
                CreatedAtUtc = a.CreatedAtUtc,
                UpdatedAtUtc = a.UpdatedAtUtc
            })
            .ToListAsync();

        var recoveryBlocks = await _context.RecoveryCodeEntries
            .AsNoTracking()
            .OrderBy(r => r.ServiceName)
            .ThenBy(r => r.Id)
            .Select(r => new RecoveryBlockViewModel
            {
                Id = r.Id,
                ServiceName = r.ServiceName,
                Codes = r.Codes,
                Notes = r.Notes,
                CreatedAtUtc = r.CreatedAtUtc,
                UpdatedAtUtc = r.UpdatedAtUtc
            })
            .ToListAsync();

        byte[] pdfBytes = _pdfService.GenerateVaultPdf(accounts, recoveryBlocks, darkTheme);
        string themeSuffix = darkTheme ? "dark" : "light";
        string fileName = $"password-vault-{themeSuffix}-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
