// store.js
import { configureStore } from '@reduxjs/toolkit';
import jobDataReducer from 'store/jobDataSlice';
import appUiReducer from 'store/appUiSlice';
import appDataReducer from 'store/appDataSlice';

export default configureStore({
    reducer: {
        jobData: jobDataReducer,
        appUi: appUiReducer,
        appData: appDataReducer
    }
});
