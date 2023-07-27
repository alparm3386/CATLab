// appUiSlice.js
import { createSlice } from '@reduxjs/toolkit';

export const appUiSlice = createSlice({
    name: 'appUi',
    initialState: {},
    reducers: {
        showAlert: (state, action) => {
            state = action.payload;
        }
    }
});

export const { showAlert } = appUiSlice.actions;

export default appUiSlice.reducer;
