// editorDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

const initialState = {
    jobData: {},
    statusBar: { message: '' }
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
        }
    }
});

export const { setJobData, setStatusBarMessage } = editorDataSlice.actions;

export default editorDataSlice.reducer;
