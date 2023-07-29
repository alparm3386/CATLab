// editorApi.js
import axios from 'axios';

const editorApi = (function () {
    let _apiClient = null;
    let _jwt = '';
    let _urlParams = '';

    return {
        initializeApiClient: (urlParams, jwt, errorHandler) => {
            _jwt = jwt;
            _urlParams = urlParams;

            _apiClient = axios.create({
                baseURL: 'http://localhost:3000/',
                // other configuration
            });

            _apiClient.interceptors.response.use(
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
        },

        setJWT: (jwt) => {
            _jwt = jwt;
        },

        login: (username, password) => {
            return _apiClient.post('/api/auth/login', { Email: username, Password: password });
        },

        getJobData: () => {
            // Ensure _jwt is a string (or handle other cases as needed)
            return _apiClient.get('/api/EditorApi/GetEditorData?urlParams=' + _urlParams, {
                headers: {
                    'Authorization': `Bearer ${_jwt}`
                }
            });
        },

        getTMMatches: async (tuid) => {
            try {
                const response = await _apiClient.post('/api/EditorApi/FetchTMMatches', { urlParams: _urlParams, tuid }, {
                    headers: {
                        'Authorization': `Bearer ${_jwt}`
                    }
                });
                if (response.status === 401) {
                    console.log("401 error ...");
                }

                return { data: response.data, status: response.status };
            } catch (error) {
                throw error;
            }
        },

        postJobData: async (jobData) => {
            try {
                const response = await _apiClient.post('/api/EditorApi/jobData', jobData);
                return response.data;
            } catch (error) {
                throw error;
            }
        }
    }
})();

export default editorApi;
