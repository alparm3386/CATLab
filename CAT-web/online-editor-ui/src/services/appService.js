// appService.js
import editorApi from 'services/editorApi';
import { setJobData } from 'store/jobDataSlice';
import { showLoading } from 'store/appUiSlice';

const appService = {
    loadJobData: async (dispatch) => {
        dispatch(showLoading(true));
        try {
            const result = await editorApi.getJobData();
            dispatch(setJobData(result.data));
        } catch (error) {
            console.log(error);
        } finally {
            dispatch(showLoading(false));
        }
    },

    // Add more application-wide functions here
};

export default appService;
