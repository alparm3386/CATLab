// TMMatches.js
import 'styles/tmMatches.scss';
import 'styles/spinner-small.css';
import React, { useState, useEffect } from 'react';
import editorApi from 'services/editorApi';
import { useSelector, useDispatch } from 'react-redux';
import { setStatusBarMessage } from 'store/appUiSlice';

var renderCntr = 0;
const TMMatches = () => {
    const [tmMatches, setTmMatches] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const tuid = useSelector((state) => state.appUi.currentSegment);
    const dispatch = useDispatch();

    //console.log("TMMatches rendered: " + renderCntr++);

    useEffect(() => {
        setIsLoading(true);
        //setTmMatches([]);
        editorApi.getTMMatches(tuid || 0).then(response => {
            setTmMatches(response.data);
        }).catch(error => {
            dispatch(setStatusBarMessage('Error:' + error.toString()));
        }).finally(() => setIsLoading(false));
        return () => {
        };
    }, [tuid, dispatch]);

    return <div className="tm-matches mb-2">
        {isLoading && (
            <div className="spinner-small-loading-overlay">
                <div className="spinner-small"></div>
            </div>
        )}
        {tmMatches ? tmMatches.map((tmMatch, index) => (
            <div key={index} className="tmm-row">
                <div className="tmm-row-num">{index + 1}</div>
                <div className="tmm-row-content">
                    <div className="tmm-source">{tmMatch.source}</div>
                    <div className="tmm-target">{tmMatch.target}</div>
                    <div className="tmm-quality">quality: {tmMatch.quality}%</div>
                </div>
            </div>
        )) : (<div className="tmm-row"><div className="tmm-row-content">No matches.</div></div>)}
    </div>;
};

export default TMMatches;
