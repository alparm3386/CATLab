// jobDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

const jobInitialState = {
    jobData: {},
    statusBar: { message: '' },
    jwt: '',
    isJobLoad: false
};

export const jobDataSlice = createSlice({
    name: 'jobData',
    initialState: jobInitialState,
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

export const { setJobData, setStatusBarMessage, showLoginModal } = jobDataSlice.actions;

export default jobDataSlice.reducer;
