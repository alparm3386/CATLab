import React, { useState, useContext } from 'react';
import StatusBarContext from './../contexts/StatusBarContext';

var renderCntr = 0;
const EditorGrid = React.memo(function EditorGrid () {
    //const { message } = useContext(StatusBarContext);

    console.log("EditorGrid rendered: " + renderCntr++);

    return (
        <div className="grid-area">
            grid
            {/*message*/} {/* Grid content goes here */}
        </div>
    );
});

export default EditorGrid;
