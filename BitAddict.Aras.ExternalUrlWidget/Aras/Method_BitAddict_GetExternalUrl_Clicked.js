// MIT License, see COPYING.TXT
// ReSharper disable ThisInGlobalContext
// ReSharper disable PossiblyUnassignedProperty

const item = parent.item;

const body =
  `<id>${item.id}</id>
   <type>${item.attributes["type"].value}</type>
   <baseurl>${aras.getBaseURL()}</baseurl>`;

const resultAML = aras.applyMethod("BitAddict_GetExternalUrl", body);
console.log(`Result: ${resultAML}`);

const resultItem = new Item("result");
resultItem.loadAML(resultAML);

if (resultItem.isError()) {
    alert(resultItem.getErrorString());
    return;
}

const url = resultItem.getResult();
console.log(`URL: ${url}`);
    
var showSnackbar = () => { 
    console.log("Clipboard set, showing snackbar");
    const snackbar = document.getElementById("bitaddict_getexternalurl_snackbar");
    snackbar.classList.add("show");

    setTimeout(() => {
        console.log("Hiding snackbar");
        snackbar.classList = snackbar.classList.remove("show");
    }, 3000);
};

// Clipboard API
console.log("Trying Clipboard API");

const p = {name: "clipboard-write"};
const ps = navigator.permissions;

ps.query(p)
.then(r => {
    console.log(`clipboard-write permission: ${r.state}`);
    
    if (r.state === "granted")
        return r;
        
    if (!hasOwnProperty.call(ps, "request")) 
        throw "permission request not supported by browser";

    console.log("requesting clipboard-write permission");
    return ps.request(p).then(r2 => {
        console.log(`clipboard-write permission req: ${r2.state}`);
        return r2;
    });
})
.then(r => {
    if (r.state !== "granted")
        throw "Failed to get clipboard-write permission";
})
.then(() => navigator.clipboard.writeText(url))
.then(() => showSnackbar())
.catch(e => {
    console.log(e);
    
    // Copy, usually works
    try {
        console.log("Trying copy command");

        // create temp element, set text and select it
        const textArea = document.createElement("textarea");
        textArea.value = url;
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
        
        console.log(r);
    } catch (e3) {
        console.log(e3);
    }
        
    // old IE support
    try {
        console.log("Trying IE clipboardData");
        window.clipboardData.setData("Text", url);
        showSnackbar();
        return;
    } catch (e2) {
        console.log(e2);
    }
    
    alert(url);
});
