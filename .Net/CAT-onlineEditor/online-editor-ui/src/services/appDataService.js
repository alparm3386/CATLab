// appDataService.js
import editorApi from 'services/editorApi';
import { showLoading, showAlert } from 'store/appUiSlice';
import { setTranslationUnits } from 'store/appDataSlice';

const appDataService = (() => {
    const service = {
        jobData: {},

        loadJobData: async (dispatch) => {
            dispatch(showLoading(true));
            try {
                const result = await editorApi.getJobData();
                service.jobData = result.data;
                dispatch(setTranslationUnits(service.jobData.translationUnits));
            } catch (error) {
                if (error?.response?.status === 401) //Unauthorized
                    return;
                let msg = error.message;
                if (error?.response?.data?.detail)
                    msg = error?.response?.data?.detail;
                dispatch(showAlert({ title: 'Error', message: msg }));
                console.log(error);
            } finally {
                dispatch(showLoading(false));
            }
        }

        // Add more application-wide functions here
    };

    return service;
})();

export default appDataService;

