const utils = {
    extractTextFromHTML: (htmlString) => {
        const parser = new DOMParser();
        const doc = parser.parseFromString(htmlString, 'text/html');
        return doc.body.textContent || "";
    },

    extractFilenameFromContentDisposition: (headerValue) => {
        if (!headerValue) return null;

        const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(headerValue);
        if (matches != null && matches[1]) {
            let filename = matches[1].replace(/['"]/g, '');
            return decodeURIComponent(filename);
        }
        return null;
    }
};

export default utils;