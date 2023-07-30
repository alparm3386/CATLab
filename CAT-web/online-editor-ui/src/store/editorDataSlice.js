// editorDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

const editorInitialState = {
    jobData: {},
    statusBar: { message: '' },
    jwt: '',
    isJobLoad: false
};

export const editorDataSlice = createSlice({
    name: 'editorData',
    initialState: editorInitialState,
    reducers: {
        setJobData: (state, action) => {
            state.jobData = action.payload;
        },
        setStatusBarMessage: (state, action) => {
            state.statusBar.message = action.payload;
        },
        onJobLoad: (state, action) => {
            state.isJobLoad = action.payload;
        },
    }
});

export const { setJobData, setStatusBarMessage, showLoginModal } = editorDataSlice.actions;

export default editorDataSlice.reducer;
