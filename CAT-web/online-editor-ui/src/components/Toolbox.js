import React, { useContext, useState } from 'react';
import { SetStatusBarMessageContext } from './../contexts/StatusBarContext';

var renderCntr = 0;
const Toolbox = ({ expanded }) => {
    const { setMessage } = useContext(SetStatusBarMessageContext);
    const [counter, setCounter] = useState(1);
    console.log("Toolbox rendered: " + renderCntr++);

    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>
        <div>tool box</div>
        <button onClick={() => { setMessage(`New message from SomeOtherComponent ${counter}`); setCounter(counter + 1) }}>
            Click me to update the message
        </button>
    </div>;
};

export default Toolbox;