// appDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

export const appDataSlice = createSlice({
    name: 'appData',
    initialState: {
        currentTuid: -1,
        targetEditbBoxContent: '',
    },
    reducers: {
        setCurrentTuid: (state, action) => {
            state.currentTuid = action.payload
        },
        setTargetEditbBoxContent: (state, action) => {
            state.targetEditbBoxContent = action.payload
        }
    }
});

export const { setCurrentTuid, setTargetEditbBoxContent } = appDataSlice.actions;

export default appDataSlice.reducer;
