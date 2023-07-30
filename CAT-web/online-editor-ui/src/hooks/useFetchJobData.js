// useFetchJobData.js
import { useEffect, useCallback } from 'react';
import { useDispatch } from 'react-redux';
import editorApi from 'services/editorApi';
import { setJobData } from 'store/editorDataSlice';
import { showLoading } from 'store/appUiSlice';

const useFetchJobData = () => {
    const dispatch = useDispatch();

    const fetchData = useCallback(async () => {
        dispatch(showLoading(true));
        try {
            const result = await editorApi.getJobData();
            dispatch(setJobData(result.data));
        } catch (error) {
            console.log(error);
        } finally {
            dispatch(showLoading(false));
        }
    }, [dispatch]);

    useEffect(() => {
        fetchData();
    }, [fetchData]);

    return fetchData; // Return fetchData as an object property
};

export default useFetchJobData;
