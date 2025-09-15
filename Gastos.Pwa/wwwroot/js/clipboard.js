(function (app) {
    ("use strict");

    app.copyText = async function (dotNetObject, dotNetCallback, textToCopy) {
        try {
            navigator.clipboard.writeText(textToCopy)
                .then(function () {
                    dotNetObject.invokeMethodAsync(dotNetCallback, true);
                })
                .catch(function (error) {
                    dotNetObject.invokeMethodAsync(dotNetCallback, false);
                });
        } catch (ex) {
            dotNetObject.invokeMethodAsync(dotNetCallback, false);
        }
    }

})((window.clipboardCopy = window.clipboardCopy || {}));
