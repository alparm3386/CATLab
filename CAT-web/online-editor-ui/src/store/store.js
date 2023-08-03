// store.js
import { configureStore } from '@reduxjs/toolkit';
import appUiReducer from 'store/appUiSlice';
import appDataReducer from 'store/appDataSlice';

export default configureStore({
    reducer: {
        appUi: appUiReducer,
        appData: appDataReducer
    },
    devTools: process.env.NODE_ENV !== 'production', // Enable Redux DevTools only in development mode
});

