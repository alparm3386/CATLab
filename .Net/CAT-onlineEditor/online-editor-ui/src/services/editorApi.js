// editorApi.js
import axios from 'axios';

const editorApi = (function () {
    let _apiClient = null;
    let _jwt = '';
    let _urlParams = '';
    let _baseUrl = '/onlineeditor/api';

    return {
        initializeApiClient: (urlParams, jwt, errorHandler) => {
            _jwt = jwt;
            _urlParams = urlParams;

            _apiClient = axios.create({
                baseURL: '/',
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
            return _apiClient.post(_baseUrl + '/auth/login', { Username: username, Password: password });
        },

        getJobData: () => {
            // Ensure _jwt is a string (or handle other cases as needed)
            return _apiClient.get(_baseUrl + '/editorApi/GetEditorData?urlParams=' + encodeURIComponent(_urlParams), {
                headers: {
                    'Authorization': `Bearer ${_jwt}`
                }
            });
        },

        getTMMatches: async (tuid) => {
            try {
                const response = await _apiClient.post(_baseUrl + '/editorApi/getTMMatches', { urlParams: _urlParams, tuid }, {
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

        getConcordance: async (searchText, caseSensitive, searchInTarget) => {
            try {
                const response = await _apiClient.post(_baseUrl + '/editorApi/getConcordance', { urlParams: _urlParams, searchText, caseSensitive, searchInTarget }, {
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

        saveSegment: async (tuid, target, confirmed, propagate) => {
            try {
                const response = await _apiClient.post(_baseUrl + '/editorApi/saveSegment', { urlParams: _urlParams, tuid, target, confirmed, propagate }, {
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

        downloadJob: () => {
            // Ensure _jwt is a string (or handle other cases as needed)
            return _apiClient.get(_baseUrl + '/EditorApi/DownloadJob?urlParams=' + encodeURIComponent(_urlParams), {
                headers: {
                    'Authorization': `Bearer ${_jwt}`
                },
                responseType: 'arraybuffer'
            });
        },

        submitJob: () => {
            // Ensure _jwt is a string (or handle other cases as needed)
            return _apiClient.get(_baseUrl + '/EditorApi/SubmitJob?urlParams=' + encodeURIComponent(_urlParams), {
                headers: {
                    'Authorization': `Bearer ${_jwt}`
                },
                responseType: 'arraybuffer'
            });
        },
    }
})();

export default editorApi;
