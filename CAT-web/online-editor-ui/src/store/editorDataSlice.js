// editorDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
    jobData: {},
    statusBar: { message: '' },
    urlParams: ''
};

export const editorDataSlice = createSlice({
    name: 'editorData',
    initialState,
    reducers: {
        setJobData: (state, action) => {
            state.jobData = action.payload;
        },
        setStatusBarMessage: (state, action) => {
            state.statusBar.message = action.payload;
        },
        setUrlParams: (state, action) => {
            state.urlParams = action.payload;
        }
    }
});

export const { setJobData, setStatusBarMessage, setUrlParams } = editorDataSlice.actions;

export default editorDataSlice.reducer;
