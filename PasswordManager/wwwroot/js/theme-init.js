(function () {
    try {
        var saved = localStorage.getItem('password_vault_theme_v1');
        var theme = saved === 'light' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', theme);
    } catch (e) {
        document.documentElement.setAttribute('data-theme', 'dark');
    }
})();
