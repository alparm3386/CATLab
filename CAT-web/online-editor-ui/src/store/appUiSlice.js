// appUiSlice.js
import { createSlice } from '@reduxjs/toolkit';

export const appUiSlice = createSlice({
    name: 'appUi',
    initialState: {
        currentTuid: -1,
        targetEditbBox: {
            tuid: -1,
            content: ''
        },
        login: { isOpen: false },
        alert: { title: "Alert", message: "" },
        isLoading: false
    },
    reducers: {
        showAlert: (state, action) => {
            state = action.payload;
        },
        showLoginModal: (state, action) => {
            state.login.isOpen = action.payload;
        },
        showLoading: (state, action) => {
            state.isLoading = action.payload
        },
        setCurrentSegment: (state, action) => {
            state.currentSegment = action.payload
        },
        setTargetEditbBoxTuid: (state, action) => {
            state.targetEditbBox.tuid = action.payload
        },
        setTargetEditbBoxContent: (state, action) => {
            state.targetEditbBox.content = action.payload
        }
    }
});

export const { showAlert, showLoginModal, showLoading, setCurrentSegment, setTargetEditbBoxTuid, setTargetEditbBoxContent } = appUiSlice.actions;

export default appUiSlice.reducer;
