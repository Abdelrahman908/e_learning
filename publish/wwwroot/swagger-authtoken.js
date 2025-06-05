window.addEventListener('load', function () {
    const token = localStorage.getItem('token'); // 🧠 بنقرأ التوكن من localStorage
    if (token) {
        const bearerToken = "Bearer " + token;
        const ui = window.ui;

        if (ui) {
            ui.preauthorizeApiKey("Bearer", bearerToken);
        }
    }
});
