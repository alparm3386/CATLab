// editorApi.js
import axios from 'axios';

let apiClient;

export function initializeApiClient(errorHandler) {
    apiClient = axios.create({
        baseURL: 'http://localhost:3000/',
        // other configuration
    });

    apiClient.interceptors.response.use(
        response => response,
        error => {
            if (error?.response?.status === 401) {
                errorHandler.handle401();
            } else if (error?.response?.status === 404) {
                errorHandler.handle404();
            }
            // ...other error handlers
            return Promise.reject(error);
        }
    );
}

export const login = (username, password) => {
    return apiClient.post('/api/auth/login', { Email: username, Password: password });
};

export const getJobData = (urlParams) => {
    return apiClient.get('/api/EditorApi/GetEditorData?urlParams=' + urlParams);
};

export const getTMMatches = async (urlParams, tuid) => {
    try {
        const response = await apiClient.post('/api/EditorApi/FetchTMMatches', { urlParams, tuid });
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
        const response = await apiClient.post('/api/EditorApi/jobData', jobData);
        return response.data;
    } catch (error) {
        throw error;
    }
};
