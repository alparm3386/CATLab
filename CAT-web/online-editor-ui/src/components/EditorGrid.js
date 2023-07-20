import './../styles/editorGrid.scss';
import React, { useState } from 'react';
import { useSelector } from 'react-redux';

var renderCntr = 0;
const EditorGrid = React.memo(function EditorGrid() {
    const jobData = useSelector((state) => state.editorData.jobData);
    console.log("EditorGrid rendered: " + renderCntr++);
    return (
        <div className="grid-area">
            {
                jobData.translationUnits &&
                jobData.translationUnits.map((unit, index) => (
                    <div key={index}>
                        <div>Source: {unit.source}</div>
                        <div>Target: {unit.target}</div>
                    </div>
                ))
            }
        </div >
    );
});

export default EditorGrid;
