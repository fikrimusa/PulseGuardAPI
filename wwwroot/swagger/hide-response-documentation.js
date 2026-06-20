(() => {
    const hideResponseDocumentation = () => {
        document.querySelectorAll(".responses-wrapper").forEach((wrapper) => {
            const liveResponse = wrapper.querySelector(":scope > .responses-inner > div");
            const responseHeader = wrapper.querySelector(":scope > .opblock-section-header");
            const responseTable = wrapper.querySelector(":scope > .responses-inner > .responses-table");
            const duplicateResponseHeader = wrapper.querySelector(":scope > .responses-inner > h4");

            wrapper.hidden = !liveResponse;

            if (responseHeader) {
                responseHeader.hidden = true;
            }

            if (responseTable) {
                responseTable.hidden = true;
            }

            if (duplicateResponseHeader) {
                duplicateResponseHeader.hidden = true;
            }
        });
    };

    const observer = new MutationObserver(hideResponseDocumentation);
    observer.observe(document.body, { childList: true, subtree: true });

    hideResponseDocumentation();
})();
