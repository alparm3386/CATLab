// store.js
import { configureStore } from '@reduxjs/toolkit';
import jobDataReducer from 'store/jobDataSlice';
import appUiReducer from 'store/appUiSlice';

export default configureStore({
    reducer: {
        jobData: jobDataReducer,
        appUi: appUiReducer
    }
});
