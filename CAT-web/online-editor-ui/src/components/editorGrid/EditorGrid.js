import 'styles/editorGrid.scss';
import React, { useState } from 'react';
import { useSelector } from 'react-redux';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCoffee } from '@fortawesome/free-solid-svg-icons'

var renderCntr = 0;
const EditorGrid = React.memo(function EditorGrid() {
    const jobData = useSelector((state) => state.editorData.jobData);
    console.log("EditorGrid rendered: " + renderCntr++);

    function onTargetClick(index) {
        alert("segment " + index + " clicked.");
    }

    return (
        <div className="grid-area">
            {
                jobData.translationUnits &&
                jobData.translationUnits.map((tu, index) => (
                    <div key={index} className="tu-row">
                        <div className="row-num">{ index + 1 }</div>
                        <div className="source">{tu.source}</div>
                        <div className="target" contentEditable="true" onClick={ () => onTargetClick(index) }>{tu.target}</div>
                        <div className="status"><FontAwesomeIcon icon={faCoffee} /></div>
                    </div>
                ))
            }
        </div >
    );
});

export default EditorGrid;
