// Toolbox.js
import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { setMessage } from './../store/statusBarSlice';

var renderCntr = 0;
const Toolbox = ({ expanded }) => {
    const [counter, setCounter] = useState(1);
    const dispatch = useDispatch();

    console.log("Toolbox rendered: " + renderCntr++);

    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>
        <div>tool box</div>
        <button onClick={() => { dispatch(setMessage(`New message from SomeOtherComponent ${counter}`)); setCounter(counter + 1) }}>
            Click me to update the message
        </button>
    </div>;
};

export default Toolbox;