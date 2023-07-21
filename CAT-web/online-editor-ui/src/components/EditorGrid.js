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
                    <div key={index} className="tuRow">
                        <div className="row-num">{ index + 1 }</div>
                        <div className="source">{ unit.source }</div>
                        <div className="target">{ unit.target }</div>
                        <div className="startus"><i class="fas fa-tint"></i></div>
                    </div>
                ))
            }
        </div >
    );
});

export default EditorGrid;
