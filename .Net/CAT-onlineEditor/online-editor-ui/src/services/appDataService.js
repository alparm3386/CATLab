// appDataService.js
import editorApi from 'services/editorApi';
import { showLoading } from 'store/appUiSlice';
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

