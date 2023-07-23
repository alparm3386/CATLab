// editorApi.js
import axios from 'axios';

const apiClient = axios.create({
    baseURL: 'http://localhost:3000/EditorApi',
    // You can put any default headers here, e.g. for authorization
});

export const getJobData = async (urlParams) => {
    try {
        const response = await apiClient.get('/GetEditorData?urlParams=' + urlParams );
        return response.data;
    } catch (error) {
        throw error;
    }
};

export const getTMMatches = async (urlParams, tuid) => {
    try {
        const response = await apiClient.post('/FetchTMMatches', { urlParams, tuid });
        return response.data;
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
