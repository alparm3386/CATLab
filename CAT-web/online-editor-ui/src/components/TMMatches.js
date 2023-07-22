﻿// TMMatches.js
import './../styles/tmMatches.scss';
import React, { useState, useEffect } from 'react';
import { getTMMatches } from '../api/editorApi';

var renderCntr = 0;
const TMMatches = ({ expanded, tuid }) => {
    const [tmMatches, setTmMatches] = useState([]);

    console.log("TMMatches rendered: " + renderCntr++);

    useEffect(() => {
        getTMMatches(tuid).then(tmMatches => setTmMatches(tmMatches));
        return () => {
        };
    }, [tuid]);

    return <div className="tm-matches mb-2">
            {tmMatches && tmMatches.map((tmMatch, index) => (
                <div key={index} className="tmm-row">
                    <div className="tmm-row-num">{index + 1}</div>
                    <div className="tmm-row-content">
                        <div className="tmm-source">{tmMatch.source}</div>
                        <div className="tmm-target">{tmMatch.target}</div>
                        <div className="tmm-quality">quality: {tmMatch.quality}%</div>
                    </div>
                </div>
            ))}
    </div>;
};

export default TMMatches;