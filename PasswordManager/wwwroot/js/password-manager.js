(function () {
    'use strict';

    const THEME_KEY = 'password_vault_theme_v1';
    let toastTimer = null;

    function showToast(message) {
        const toast = document.getElementById('toastMsg');
        if (!toast) return;
        toast.textContent = message;
        toast.classList.add('show');
        if (toastTimer) window.clearTimeout(toastTimer);
        toastTimer = window.setTimeout(() => {
            toast.classList.remove('show');
        }, 2200);
    }

    function applyTheme(theme) {
        const selected = theme === 'light' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', selected);
        const label = document.getElementById('themeLabel');
        if (label) {
            label.textContent = selected === 'dark' ? 'Light Mode' : 'Dark Mode';
        }
        try {
            localStorage.setItem(THEME_KEY, selected);
        } catch (e) {
            // Ignore storage restrictions
        }
    }

    function initThemeToggle() {
        const current = document.documentElement.getAttribute('data-theme') || 'dark';
        applyTheme(current);

        const btn = document.getElementById('themeToggleBtn');
        if (!btn) return;
        btn.addEventListener('click', () => {
            const next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            applyTheme(next);
            showToast(next === 'dark' ? 'Switched to Dark Mode.' : 'Switched to Light Mode.');
        });
    }

    function initDialogs() {
        const exportBtn = document.getElementById('exportPdfBtn');
        const pdfDialog = document.getElementById('pdfThemeDialog');

        if (exportBtn && pdfDialog && typeof pdfDialog.showModal === 'function') {
            exportBtn.addEventListener('click', () => pdfDialog.showModal());
        }

        document.querySelectorAll('[data-close-dialog]').forEach((btn) => {
            btn.addEventListener('click', () => {
                const id = btn.getAttribute('data-close-dialog');
                const dlg = id ? document.getElementById(id) : null;
                if (dlg && dlg.open) dlg.close();
            });
        });

        document.querySelectorAll('dialog.vault-dialog').forEach((dlg) => {
            dlg.addEventListener('click', (event) => {
                if (event.target === dlg) dlg.close();
            });
        });
    }

    function initLiveSearch() {
        const input = document.getElementById('vaultSearchInput');
        const clearBtn = document.getElementById('vaultSearchClearBtn');
        if (!input) return;

        const accountRows = Array.from(document.querySelectorAll('#accountsTable tbody tr.vault-row'));
        const recoveryCards = Array.from(document.querySelectorAll('#recoveryGrid article.recovery-block'));
        const visibleAccountsEl = document.getElementById('visibleAccountsCount');
        const visibleRecoveryEl = document.getElementById('visibleRecoveryCount');
        const noAccountsMatch = document.getElementById('noAccountsMatch');
        const noRecoveryMatch = document.getElementById('noRecoveryMatch');

        function applyFilter() {
            const q = (input.value || '').trim().toLowerCase();
            if (clearBtn) {
                clearBtn.hidden = q.length === 0;
            }

            let visibleAccounts = 0;
            accountRows.forEach((row) => {
                const text = row.getAttribute('data-search') || '';
                const match = !q || text.includes(q);
                row.hidden = !match;
                if (match) visibleAccounts++;
            });

            let visibleRecovery = 0;
            recoveryCards.forEach((card) => {
                const text = card.getAttribute('data-search') || '';
                const match = !q || text.includes(q);
                card.hidden = !match;
                if (match) visibleRecovery++;
            });

            if (visibleAccountsEl) visibleAccountsEl.textContent = String(visibleAccounts);
            if (visibleRecoveryEl) visibleRecoveryEl.textContent = String(visibleRecovery);
            if (noAccountsMatch && accountRows.length > 0) noAccountsMatch.hidden = visibleAccounts > 0;
            if (noRecoveryMatch && recoveryCards.length > 0) noRecoveryMatch.hidden = visibleRecovery > 0;
        }

        input.addEventListener('input', applyFilter);

        if (clearBtn) {
            clearBtn.addEventListener('click', () => {
                input.value = '';
                applyFilter();
                input.focus();
            });
        }

        document.addEventListener('keydown', (event) => {
            if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
                event.preventDefault();
                input.focus();
                input.select();
            }
        });

        applyFilter();
    }

    function initPasswordVisibilityAndCopy() {
        const toggleAllBtn = document.getElementById('toggleAllPasswordsBtn');
        let allVisible = false;

        function setCodeVisibility(codeEl, btnEl, visible) {
            const secret = codeEl.getAttribute('data-secret') || '';
            if (visible) {
                codeEl.textContent = secret;
                codeEl.classList.remove('masked');
                if (btnEl) btnEl.textContent = 'Hide';
            } else {
                codeEl.textContent = '••••••••••';
                codeEl.classList.add('masked');
                if (btnEl) btnEl.textContent = 'Show';
            }
        }

        document.addEventListener('click', (event) => {
            const revealBtn = event.target.closest('.reveal-btn');
            if (revealBtn) {
                const wrap = revealBtn.closest('.password-cell-wrap');
                const codeEl = wrap ? wrap.querySelector('.password-value') : null;
                if (codeEl) {
                    const isMasked = codeEl.classList.contains('masked');
                    setCodeVisibility(codeEl, revealBtn, isMasked);
                }
                return;
            }

            const copyBtn = event.target.closest('.copy-btn');
            if (copyBtn) {
                const text = copyBtn.getAttribute('data-copy') || '';
                if (!text) return;
                if (navigator.clipboard && typeof navigator.clipboard.writeText === 'function') {
                    navigator.clipboard.writeText(text)
                        .then(() => showToast('Copied to clipboard.'))
                        .catch(() => showToast('Could not copy to clipboard.'));
                }
            }
        });

        if (toggleAllBtn) {
            toggleAllBtn.addEventListener('click', () => {
                allVisible = !allVisible;
                document.querySelectorAll('.password-cell-wrap').forEach((wrap) => {
                    const codeEl = wrap.querySelector('.password-value');
                    const btnEl = wrap.querySelector('.reveal-btn');
                    if (codeEl) setCodeVisibility(codeEl, btnEl, allVisible);
                });
                toggleAllBtn.textContent = allVisible ? 'Hide All Passwords' : 'Show All Passwords';
            });
        }
    }

    function initAdminModalsAndDeletes() {
        const editAccountDialog = document.getElementById('editAccountDialog');
        const editRecoveryDialog = document.getElementById('editRecoveryDialog');

        document.querySelectorAll('.js-edit-account').forEach((btn) => {
            btn.addEventListener('click', () => {
                if (!editAccountDialog) return;
                document.getElementById('editAccountId').value = btn.getAttribute('data-id') || '';
                document.getElementById('editAccountName').value = btn.getAttribute('data-account-name') || '';
                document.getElementById('editLoginType').value = btn.getAttribute('data-login-type') || '1';
                document.getElementById('editLoginIdentifier').value = btn.getAttribute('data-login-identifier') || '';
                document.getElementById('editPassword').value = btn.getAttribute('data-password') || '';
                editAccountDialog.showModal();
            });
        });

        document.querySelectorAll('.js-edit-recovery').forEach((btn) => {
            btn.addEventListener('click', () => {
                if (!editRecoveryDialog) return;
                document.getElementById('editRecoveryId').value = btn.getAttribute('data-id') || '';
                document.getElementById('editServiceName').value = btn.getAttribute('data-service-name') || '';
                document.getElementById('editRecoveryNotes').value = btn.getAttribute('data-notes') || '';
                document.getElementById('editRecoveryCodes').value = btn.getAttribute('data-codes') || '';
                editRecoveryDialog.showModal();
            });
        });

        document.querySelectorAll('form[data-confirm-delete]').forEach((form) => {
            form.addEventListener('submit', (event) => {
                const targetName = form.getAttribute('data-confirm-delete') || 'this item';
                if (!window.confirm(`Delete "${targetName}"? This action cannot be undone.`)) {
                    event.preventDefault();
                }
            });
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        initThemeToggle();
        initDialogs();
        initLiveSearch();
        initPasswordVisibilityAndCopy();
        initAdminModalsAndDeletes();
    });
})();
