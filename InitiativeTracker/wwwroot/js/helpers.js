window.openHtmlInNewTab = function (htmlContent) {
    const newTab = window.open("", "_blank");
    newTab.document.open();
    newTab.document.write(htmlContent);
    newTab.document.close();
};
