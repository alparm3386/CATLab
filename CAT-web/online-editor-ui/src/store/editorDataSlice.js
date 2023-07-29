// editorDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

const editorInitialState = {
    jobData: {},
    statusBar: { message: '' },
    urlParams: '',
    jwt: ''
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
        }
    }
});

export const { setJobData, setStatusBarMessage, showLoginModal } = editorDataSlice.actions;

export default editorDataSlice.reducer;
