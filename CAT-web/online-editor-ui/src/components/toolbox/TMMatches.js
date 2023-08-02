// TMMatches.js
import 'styles/tmMatches.scss';
import React, { useState, useEffect } from 'react';
import editorApi from 'services/editorApi';
import { useSelector, useDispatch } from 'react-redux';
import { setStatusBarMessage } from 'store/jobDataSlice';

var renderCntr = 0;
const TMMatches = () => {
    const [tmMatches, setTmMatches] = useState([]);
    const urlParams = useSelector((state) => state.jobData.urlParams);
    const tuid = useSelector((state) => state.appUi.currentSegment);
    const dispatch = useDispatch();

    console.log("TMMatches rendered: " + renderCntr++);

    useEffect(() => {
        editorApi.getTMMatches(tuid || 0).then(response => {
            setTmMatches(response.data);
        }).catch(error => {
            dispatch(setStatusBarMessage('Error:' + error.toString()));
        });
        return () => {
        };
    }, [tuid, urlParams, dispatch]);

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