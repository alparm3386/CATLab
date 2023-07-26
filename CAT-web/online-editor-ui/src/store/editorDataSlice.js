// editorDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

const editorInitialState = {
    jobData: {},
    statusBar: { message: '' },
    urlParams: '',
    isLoginModalOpen: false, // add this to handle login modal state
};

export const editorDataSlice = createSlice({
    name: 'editorData',
    editorInitialState,
    reducers: {
        setJobData: (state, action) => {
            state.jobData = action.payload;
        },
        setStatusBarMessage: (state, action) => {
            state.statusBar.message = action.payload;
        },
        setUrlParams: (state, action) => {
            state.urlParams = action.payload;
        },
        // Add these two reducers to handle showing/hiding the login modal
        showLoginModal: (state, action) => {
            state.isLoginModalOpen = action.payload;
        }
    }
});

export const { setJobData, setStatusBarMessage, setUrlParams, showLoginModal } = editorDataSlice.actions;

export default editorDataSlice.reducer;
