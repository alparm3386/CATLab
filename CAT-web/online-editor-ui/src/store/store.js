// store.js
import { configureStore } from '@reduxjs/toolkit';
import editorDataReducer from './editorDataSlice';

export default configureStore({
    reducer: {
        editorData: editorDataReducer
    }
});
