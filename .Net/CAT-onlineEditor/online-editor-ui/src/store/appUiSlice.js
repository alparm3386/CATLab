// appUiSlice.js
import { createSlice } from '@reduxjs/toolkit';

export const appUiSlice = createSlice({
    name: 'appUi',
    initialState: {
        login: { isOpen: false },
        alert: { title: 'Alert', message: '', show: false, callback: null },
        isLoading: false,
        statusBar: { message: '' },

    },
    reducers: {
        showAlert: (state, action) => {
            state.alert = action.payload;
            state.alert.show = 'show' in action.payload ? action.payload.show : true;
        },
        showLoginModal: (state, action) => {
            state.login.isOpen = action.payload;
        },
        showLoading: (state, action) => {
            state.isLoading = action.payload
        },
        showStatusBarMessage: (state, action) => {
            state.statusBar.message = action.payload;
        }
    }
});

export const { showAlert, showLoginModal, showLoading, showStatusBarMessage } = appUiSlice.actions;

export default appUiSlice.reducer;
