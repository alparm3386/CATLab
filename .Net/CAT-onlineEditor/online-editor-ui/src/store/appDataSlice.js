﻿// appDataSlice.js
import { createSlice } from '@reduxjs/toolkit';

export const appDataSlice = createSlice({
    name: 'appData',
    initialState: {
        jobData: {},
        currentTuid: -1,
        targetEditbBoxContent: '',
        translationUnits: []
    },
    reducers: {
        setCurrentTuid: (state, action) => {
            state.currentTuid = action.payload
        },
        //setTargetEditbBoxContent: (state, action) => {
        //    state.targetEditbBoxContent = action.payload
        //},
        setJobData: (state, action) => {
            state.jobData = action.payload
        },
        setTranslationUnits: (state, action) => {
            state.translationUnits = action.payload
        },
        updateTranslationUnitTarget: (state, action) => {
            const { index, target } = action.payload;
            state.translationUnits[index].target = target;
        }
    }
});

export const { setCurrentTuid, setJobData, /*setTargetEditbBoxContent,*/ setTranslationUnits, updateTranslationUnitTarget } = appDataSlice.actions;

export default appDataSlice.reducer;
