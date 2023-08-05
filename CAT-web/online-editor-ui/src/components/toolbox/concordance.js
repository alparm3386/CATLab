import 'styles/concordance.scss';
import 'styles/spinner-small.css';
import React, { useState } from 'react';
import editorApi from 'services/editorApi';
import { useSelector, useDispatch } from 'react-redux';
import { showStatusBarMessage } from 'store/appUiSlice';
import appDataService from 'services/appDataService';

var renderCntr = 0;
const Concordance = () => {
    const [tmMatches, setTmMatches] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [searchText, setSearchText] = useState('');
    const [caseSensitive, setCaseSensitive] = useState(false);
    const [searchInTarget, setSearchInTarget] = useState(false);
    const dispatch = useDispatch();

    console.log("Concordance rendered: " + renderCntr++);

    const handleEnterKeyPress = (event) => {
        if (event.key === 'Enter') {
            getConcordance();
        }
    };

    const getConcordance = () => {
        setTmMatches([]);
        setIsLoading(true);
        editorApi.getConcordance(searchText, caseSensitive, searchInTarget)
            .then(response => {
                setTmMatches(response.data);
            })
            .catch(error => {
                dispatch(showStatusBarMessage('Error:' + error.toString()));
            })
            .finally(() => setIsLoading(false));
    };

    const onTestButtonClick = () => {
        //const jobData = appDataService.jobData;
        //jobData.translationUnits[0].target = jobData.translationUnits[0].target + "_";
    }

    //useEffect(() => {
    //    return () => {
    //        // Clean up if needed
    //    };
    //}, []);

    return (
        <>
            <div className="form-group m-2">
                <div className="input-group mb-2">
                    <input type="text" className="form-control" placeholder="Search Text" value={searchText}
                        onChange={(e) => setSearchText(e.target.value)} onKeyDown={handleEnterKeyPress}
                    />
                    <div className="input-group-append">
                        <button
                            className="btn btn-primary" type="button" onClick={getConcordance}>
                            Search
                        </button>
                    </div>
                </div>

                <div className="form-check form-check-inline mb-2">
                    <input type="checkbox" className="form-check-input" id="caseSensitiveCheckbox"
                        checked={caseSensitive} onChange={() => setCaseSensitive(!caseSensitive)}
                    />
                    <label className="form-check-label" htmlFor="caseSensitiveCheckbox">
                        Case Sensitive
                    </label>
                </div>
                <div className="form-check form-check-inline mb-2">
                    <input type="checkbox" className="form-check-input" id="searchInTargetCheckbox"
                        checked={searchInTarget} onChange={() => setSearchInTarget(!searchInTarget)}
                    />
                    <label className="form-check-label" htmlFor="searchInTargetCheckbox">
                        Search in Target
                    </label>
                </div>
                <div className="form-check form-check-inline mb-2">
                <button
                    className="btn btn-primary" type="button" onClick={onTestButtonClick}>
                    Test button
                    </button>
                </div>
            </div>

            <div className="concordance-results mb-2">
                {/* Rest of the code remains the same */}
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
            </div>
        </>
    );
};

export default Concordance;
