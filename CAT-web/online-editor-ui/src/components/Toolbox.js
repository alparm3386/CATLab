// Toolbox.js
import './../styles/toolboxArea.scss';
import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { setStatusBarMessage } from '../store/editorDataSlice';

var renderCntr = 0;
const Toolbox = ({ expanded }) => {
    const [counter, setCounter] = useState(1);
    const dispatch = useDispatch();

    console.log("Toolbox rendered: " + renderCntr++);

    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>
        <div>tool box</div>
        <button onClick={() => { dispatch(setStatusBarMessage(`New message from toolbox ${counter}`)); setCounter(counter + 1) }}>
            Click me to update the message
        </button>
    </div>;
};

export default Toolbox;