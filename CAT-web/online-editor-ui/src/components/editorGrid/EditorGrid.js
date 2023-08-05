import 'styles/editorGrid.scss';
import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCoffee } from '@fortawesome/free-solid-svg-icons'
import { setCurrentTuid } from 'store/appDataSlice';
import TargetEditbBox from 'components/editorGrid/targetEditBox';
import appDataService from 'services/appDataService';

var renderCntr = 0;
const EditorGrid = //React.memo(
    function EditorGrid() {
        const dispatch = useDispatch();
        const translationUnits = useSelector((state) => state.appData.translationUnits);
        //const jobData = appDataService.jobData;
        const currentTuid = useSelector((state) => state.appData.currentTuid);

        console.log("EditorGrid rendered: " + renderCntr++);

        function onTargetClick(tuid) {
            dispatch(setCurrentTuid(tuid));
        }

        return (
            <div className="grid-area">
                {
                    translationUnits &&
                    translationUnits.map((tu, index) => (
                        <div key={index} className="tu-row">
                            <div className="row-num">{index + 1}</div>
                            <div className="source">{tu.source}</div>
                            {currentTuid === index + 1 ? <TargetEditbBox key={`target_${index}`} className="target" /> :
                                <div className="target" onClick={() => onTargetClick(index + 1)}>{tu.target}</div>}
                            <div className="status"><FontAwesomeIcon icon={faCoffee} /></div>
                        </div>
                    ))
                }
            </div >
        );
    }//);

export default EditorGrid;
