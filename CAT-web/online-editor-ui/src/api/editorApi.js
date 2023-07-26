// editorApi.js
import axios from 'axios';

const apiClient = axios.create({
    baseURL: 'http://localhost:3000/api/EditorApi',
    // You can put any default headers here, e.g. for authorization
});

export const getJobData = (urlParams) => {
        return apiClient.get('/GetEditorData?urlParams=' + urlParams );
};

export const getTMMatches = async (urlParams, tuid) => {
    try {
        const response = await apiClient.post('/FetchTMMatches', { urlParams, tuid });
        if (response.status === 401) {
            console.log("401 error ...");
        }

        return { data: response.data, status: response.status };
    } catch (error) {
        throw error;
    }
};

export const postJobData = async (jobData) => {
    try {
        const response = await apiClient.post('/jobData', jobData);
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Add other API functions as needed...
