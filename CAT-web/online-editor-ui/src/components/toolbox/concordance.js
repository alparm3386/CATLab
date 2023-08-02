// concordance.js
import 'styles/concordance.scss';
import 'styles/spinner-small.css';
import React, { useState, useEffect } from 'react';
import editorApi from 'services/editorApi';
import { useSelector, useDispatch } from 'react-redux';
import { setStatusBarMessage } from 'store/jobDataSlice';

var renderCntr = 0;
const Concordance = () => {
    const [tmMatches, setTmMatches] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const dispatch = useDispatch();

    console.log("Concordance rendered: " + renderCntr++);

    useEffect(() => {
        setIsLoading(true);
        //setTmMatches([]);
        editorApi.getConcordance().then(response => {
            setTmMatches(response.data);
        }).catch(error => {
            dispatch(setStatusBarMessage('Error:' + error.toString()));
        }).finally(() => setIsLoading(false));
        return () => {
        };
    }, [dispatch]);

    return <div className="concordance-results mb-2">
        {isLoading && (
            <div className="spinner-small-loading-overlay">
                <div className="spinner-small"></div>
            </div>
        )}
        {tmMatches ? tmMatches.map((tmMatch, index) => (
            <div key={index} className="result-row">
                <div className="result-row-num">{index + 1}</div>
                <div className="result-row-content">
                    <div className="result-source">{tmMatch.source}</div>
                    <div className="result-target">{tmMatch.target}</div>
                </div>
            </div>
        )) : (<div className="result-row"><div className="result-row-content">No matches.</div></div>)}
    </div>;
};

export default Concordance;
