// store.js
import { configureStore } from '@reduxjs/toolkit';
import editorDataReducer from './editorDataSlice';
import appUiReducer from './appUiSlice';

export default configureStore({
    reducer: {
        editorData: editorDataReducer,
        appUi: appUiReducer
    }
});
