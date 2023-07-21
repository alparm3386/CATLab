// Toolbox.js
import './../styles/toolboxArea.scss';
import React, { useState, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import { setStatusBarMessage } from '../store/editorDataSlice';
import { getTMMatches } from './../api/apiService';


var renderCntr = 0;
const Toolbox = ({ expanded, currentIdx }) => {
    const [counter, setCounter] = useState(1);
    const [tmMatches, setTmMatches] = useState([]);
    const dispatch = useDispatch();

    console.log("Toolbox rendered: " + renderCntr++);

    useEffect(() => {
        const tmMatches = getTMMatches(currentIdx);
        setTmMatches(tmMatches);
        return () => {
        };
    }, [currentIdx]);

    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>
        <div>
            {tmMatches && tmMatches.map((tmMatch, index) => (
                <div key={index} className="tmm-row">
                    <div className="tmm-row-num">{index + 1}</div>
                    <div className="tmm-source">{tmMatch.source}</div>
                    <div className="tmm-target">{tmMatch.target}</div>
                    <div className="tmm-quality">{tmMatch.quality}</div>
                </div>
            ))}
        </div>
        <button onClick={() => { dispatch(setStatusBarMessage(`New message from toolbox ${counter}`)); setCounter(counter + 1) }}>
            Click me to update the message
        </button>
    </div>;
};

export default Toolbox;