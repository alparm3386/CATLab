// src/store/actions/dataActions.js
import { loadJobData } from '../../services/apiService';

export const fetchJobData = () => async (dispatch, getState) => {
    try {
        const response = await loadJobData();
        dispatch({ type: 'DATA_LOADED', payload: response.data });
    } catch (error) {
        console.error("Failed to load data", error);
    }
};
