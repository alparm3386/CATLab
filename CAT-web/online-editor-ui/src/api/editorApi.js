// editorApi.js
import axios from 'axios';
import store from 'store/store';
import { showLoginModal } from 'store/appUiSlice';

const apiClient = axios.create({
    baseURL: 'http://localhost:3000',
    // You can put any default headers here, e.g. for authorization
});

apiClient.interceptors.response.use(
    response => response,
    error => {
        if (error?.response?.status === 401) {
            //dispatch showLoginModal action when status is 401
            store.dispatch(showLoginModal(true));
        }
        // If you want to pass the error on (maybe there's additional, specific error handling), 
        // you should return a rejected Promise here
        return Promise.reject(error);
    }
);
export const login = (username, password) => {
    return apiClient.post('/api/auth/login', { Email: username, Password: password });
};

export const getJobData = (urlParams) => {
    return apiClient.get('/api/EditorApi/GetEditorData?urlParams=' + urlParams );
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

// Add other API functions as needed...
