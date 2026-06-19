window.openHtmlInNewTab = function (htmlContent) {
    const newTab = window.open("", "_blank");
    newTab.document.open();
    newTab.document.write(htmlContent);
    newTab.document.close();
};

let _pasteDotNetHelper = null;

window.registerPasteListener = function (elementId, dotNetHelper) {
    const element = document.getElementById(elementId);
    if (!element) return;

    _pasteDotNetHelper = dotNetHelper;

    element.addEventListener("paste", async (e) => {
        try {
            const items = e.clipboardData?.items ?? e.originalEvent?.clipboardData?.items;
            if (!items) return;

            for (const item of items) {
                if (item.type.startsWith("image/")) {
                    e.preventDefault();
                    const file = item.getAsFile();
                    if (!file) continue;

                    const buffer = await file.arrayBuffer();
                    const bytes = new Uint8Array(buffer);
                    let binaryString = "";
                    const chunkSize = 8192;
                    for (let i = 0; i < bytes.length; i += chunkSize) {
                        const chunk = bytes.subarray(i, i + chunkSize);
                        binaryString += String.fromCharCode.apply(null, chunk);
                    }
                    const base64 = btoa(binaryString);

                    await dotNetHelper.invokeMethodAsync("HandleClipboardImage", item.type, base64);
                    return;
                }
            }
        } catch (ex) {
            console.warn("Clipboard read failed:", ex);
        }
    });
};
