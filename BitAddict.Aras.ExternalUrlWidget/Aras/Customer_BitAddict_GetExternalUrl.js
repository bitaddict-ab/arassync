// MIT License - Copyright CPAC Systems AB 2019

// ReSharper disable PossiblyUnassignedProperty
// ReSharper disable UseOfImplicitGlobalInFunctionScope

Aras.prototype.bitAddictCopyToClipBoard = function (clipboardData, doc) {
    const showSnackbar = () => {
        if (!doc)
            doc = document;

        const snackbar = doc.getElementById("bitaddict_externalurl_snackbar");

        if (!snackbar) {
            alert(clipboardData + "\n\nCopied to clipboard");
            return;
        }

        snackbar.classList.add("show");
        setTimeout(() => snackbar.classList.remove("show"), 3000);
    };

    // Clipboard API
    const cpwp = { name: "clipboard-write" };
    const ps = navigator.permissions;

    ps.query(cpwp)
        .then(r => {
            if (r.state === "granted")
                return r;

            if (!hasOwnProperty.call(ps, "request"))
                throw "permission request not supported by browser";

            return ps.request(cpwp);
        })
        .then(r => {
            if (r.state !== "granted")
                throw "Failed to get clipboard-write permission";
        })
        .then(() => navigator.clipboard.writeText(clipboardData))
        .then(() => showSnackbar())
        .catch(e => {
            console.log("Failed to use Clipboard API: " + e);

            // Copy, usually works
            try {
                // create temp element, set text and select it
                const textArea = document.createElement("textarea");
                textArea.value = clipboardData;
                document.body.appendChild(textArea);
                textArea.select();

                // Now that we've selected the text, execute the copy command  
                const r = document.execCommand("copy");

                // cleanup
                document.body.removeChild(textArea);

                // ok?
                if (r) {
                    showSnackbar();
                    return;
                }

                throw r;
            } catch (e3) {
                console.log("Failed to run copy command: " + e3);
            }

            // old IE support
            try {
                console.log("Trying IE clipboardData");
                window.clipboardData.setData("Text", clipboardData);
                showSnackbar();
                return;
            } catch (e2) {
                console.log(e2);
            }

            alert(clipboardData);
        });
};

console.log("BitAddict_GetExternalUrl.js: Patching Aras.prototype.uiPopulateInfoTableWithItem()");
Aras.prototype._uiPopulateInfoTableWithItem = Aras.prototype.uiPopulateInfoTableWithItem;
Aras.prototype.uiPopulateInfoTableWithItem = function (sourceItm, doc) {
    Aras.prototype._uiPopulateInfoTableWithItem.apply(this, arguments);

    const span = doc.getElementById("label_span");

    if (!span || !sourceItm)
        return;

    const url = Aras.prototype.getBaseURL.apply(this) +
        `/default.aspx?StartItem=` +
        `${sourceItm.attributes["type"].value}:${sourceItm.id}`;
    const onclick = `event.preventDefault(); top.aras.bitAddictCopyToClipBoard(\"${url}\", document);`;

    const link = doc.getElementById("bitaddict_externalurl_link");

    if (!link) {
        span.innerHTML += ` <a title='Copy item URL to clipboard' id='bitaddict_externalurl_link' href='${url}' onclick='${onclick}'>🔗</a>`;

        fetch("../customer/BitAddict_GetExternalUrl_Snackbar.html", { cache: "reload" })
            .then(r => r.text())
            .then(html => span.innerHTML = html + span.innerHTML)
            .catch(error => console.warn(error));
    } else {
        link.attributes["href"].value = url;
        link.attributes["onclick"].value = onclick;
    }
};