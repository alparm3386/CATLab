// appUiSlice.js
import { createSlice } from '@reduxjs/toolkit';

export const appUiSlice = createSlice({
    name: 'appUi',
    initialState: {
        login: { isOpen: false },
        alert: { title: "Alert", message: ""}
    },
    reducers: {
        showAlert: (state, action) => {
            state = action.payload;
        },
        showLoginModal: (state, action) => {
            state.login.isOpen = action.payload;
        }
    }
});

export const { showAlert, showLoginModal } = appUiSlice.actions;

export default appUiSlice.reducer;
