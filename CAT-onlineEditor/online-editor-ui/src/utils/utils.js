const utils = {
    extractTextFromHTML: (htmlString) => {
        const tempElement = document.createElement('div');
        tempElement.innerHTML = htmlString;
        return tempElement.textContent;
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