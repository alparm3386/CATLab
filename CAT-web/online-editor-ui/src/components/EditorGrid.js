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
                jobData.translationUnits.map((tu, index) => (
                    <div key={index} className="tu-row">
                        <div className="row-num">{ index + 1 }</div>
                        <div className="source">{tu.source}</div>
                        <div className="target" contentEditable="true">{tu.target}</div>
                        <div className="status"><i className="fas fa-tint"></i></div>
                    </div>
                ))
            }
        </div >
    );
});

export default EditorGrid;
