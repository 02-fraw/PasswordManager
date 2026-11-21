using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Security;
using PasswordManager.ViewModels;

namespace PasswordManager.Controllers;

[TypeFilter(typeof(LocalOnlyAdminFilter))]
[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
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
            IsAdminView = true
        };

        ViewData["Title"] = "Admin Panel";
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAccount(AccountFormInputModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide a valid account name.";
            return RedirectToAction(nameof(Index));
        }

        string identifier = (input.LoginIdentifier ?? string.Empty).Trim();
        var loginType = string.IsNullOrWhiteSpace(identifier)
            ? LoginIdentifierType.None
            : (Enum.IsDefined(typeof(LoginIdentifierType), input.LoginType) && input.LoginType != LoginIdentifierType.None
                ? input.LoginType
                : LoginIdentifierType.Email);

        var entity = new AccountCredential
        {
            AccountName = input.AccountName.Trim(),
            LoginType = loginType,
            LoginIdentifier = identifier,
            Password = (input.Password ?? string.Empty).Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.AccountCredentials.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Added '{entity.AccountName}' to your vault.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> EditAccount(AccountFormInputModel input)
    {
        if (!ModelState.IsValid || input.Id <= 0)
        {
            TempData["ErrorMessage"] = "Please provide valid account details.";
            return RedirectToAction(nameof(Index));
        }

        var entity = await _context.AccountCredentials.FindAsync(input.Id);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "The selected account could not be found.";
            return RedirectToAction(nameof(Index));
        }

        string identifier = (input.LoginIdentifier ?? string.Empty).Trim();
        var loginType = string.IsNullOrWhiteSpace(identifier)
            ? LoginIdentifierType.None
            : (Enum.IsDefined(typeof(LoginIdentifierType), input.LoginType) && input.LoginType != LoginIdentifierType.None
                ? input.LoginType
                : LoginIdentifierType.Email);

        entity.AccountName = input.AccountName.Trim();
        entity.LoginType = loginType;
        entity.LoginIdentifier = identifier;
        entity.Password = (input.Password ?? string.Empty).Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Updated '{entity.AccountName}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var entity = await _context.AccountCredentials.FindAsync(id);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Account not found.";
            return RedirectToAction(nameof(Index));
        }

        string name = entity.AccountName;
        _context.AccountCredentials.Remove(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Removed '{name}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecoveryCode(RecoveryCodeFormInputModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please enter a service name and recovery codes.";
            return RedirectToAction(nameof(Index));
        }

        var entity = new RecoveryCodeEntry
        {
            ServiceName = input.ServiceName.Trim(),
            Codes = input.Codes.Trim(),
            Notes = (input.Notes ?? string.Empty).Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.RecoveryCodeEntries.Add(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Added recovery codes for '{entity.ServiceName}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> EditRecoveryCode(RecoveryCodeFormInputModel input)
    {
        if (!ModelState.IsValid || input.Id <= 0)
        {
            TempData["ErrorMessage"] = "Please provide valid recovery code details.";
            return RedirectToAction(nameof(Index));
        }

        var entity = await _context.RecoveryCodeEntries.FindAsync(input.Id);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "The selected recovery code block could not be found.";
            return RedirectToAction(nameof(Index));
        }

        entity.ServiceName = input.ServiceName.Trim();
        entity.Codes = input.Codes.Trim();
        entity.Notes = (input.Notes ?? string.Empty).Trim();
        entity.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Updated recovery codes for '{entity.ServiceName}'.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRecoveryCode(int id)
    {
        var entity = await _context.RecoveryCodeEntries.FindAsync(id);
        if (entity is null)
        {
            TempData["ErrorMessage"] = "Recovery code block not found.";
            return RedirectToAction(nameof(Index));
        }

        string name = entity.ServiceName;
        _context.RecoveryCodeEntries.Remove(entity);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Removed recovery codes for '{name}'.";
        return RedirectToAction(nameof(Index));
    }
}