const utils = {
    extractTextFromHTML: (htmlString) => {
        const tempElement = document.createElement('div');
        tempElement.innerHTML = htmlString;
        return tempElement.textContent;
    }
    // Add more application-wide functions here
};

export default utils;