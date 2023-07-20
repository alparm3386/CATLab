import React, { useState, useContext } from 'react';
import StatusBarContext from './../contexts/StatusBarContext';

var renderCntr = 0;
const EditorGrid = React.memo(function EditorGrid () {

    console.log("EditorGrid rendered: " + renderCntr++);

    return (
        <div className="grid-area">
            grid
        </div>
    );
});

export default EditorGrid;
