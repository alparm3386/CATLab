import 'styles/editorGrid.scss';
import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCoffee } from '@fortawesome/free-solid-svg-icons'
import { setCurrentTuid, setTargetEditbBoxContent } from 'store/appDataSlice';
import TargetEditbBox from 'components/common/targetEditBox';
import appDataService from 'services/appDataService';

var renderCntr = 0;
const EditorGrid = //React.memo(
    function EditorGrid() {
        const dispatch = useDispatch();
        const jobData = appDataService.jobData;
        const currentTuid = useSelector((state) => state.appData.currentTuid);

        console.log("EditorGrid rendered: " + renderCntr++);

        function onTargetClick(tuid) {
            dispatch(setCurrentTuid(tuid));
            dispatch(setTargetEditbBoxContent(jobData.translationUnits[tuid - 1].target));
        }

        return (
            <div className="grid-area">
                {
                    jobData.translationUnits &&
                    jobData.translationUnits.map((tu, index) => (
                        <div key={index} className="tu-row">
                            <div className="row-num">{index + 1}</div>
                            <div className="source">{tu.source}</div>
                            {currentTuid === index + 1 ? <TargetEditbBox key={`target_${index}`} className="target" tuid={index + 1} /> :
                                <div className="target" onClick={() => onTargetClick(index + 1)}>{tu.target}</div>}
                            <div className="status"><FontAwesomeIcon icon={faCoffee} /></div>
                        </div>
                    ))
                }
            </div >
        );
    }//);

export default EditorGrid;
