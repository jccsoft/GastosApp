(function (app) {
    ("use strict");

    app.setCulture = function (culture) {
        localStorage.setItem('blazorCulture', culture);
        // Establece la cookie de cultura para el servidor
        document.cookie = `.AspNetCore.Culture=c=${culture}|uic=${culture}; path=/`;
        location.reload();
    }

})((window.cultureHelper = window.cultureHelper || {}));