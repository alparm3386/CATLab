import 'styles/editorGrid.scss';
import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCoffee } from '@fortawesome/free-solid-svg-icons'
import { setCurrentSegment } from 'store/appUiSlice';
import TargetEditbBox from 'components/common/targetEditBox';

var renderCntr = 0;
const EditorGrid = React.memo(function EditorGrid() {
    const dispatch = useDispatch();
    const jobData = useSelector((state) => state.jobData.jobData);
    const currentSegment = useSelector((state) => state.appUi.currentSegment);

    console.log("EditorGrid rendered: " + renderCntr++);

    function onTargetClick(index) {
        dispatch(setCurrentSegment(index));
    }

    return (
        <div className="grid-area">
            {
                jobData.translationUnits &&
                jobData.translationUnits.map((tu, index) => (
                    <div key={index} className="tu-row">
                        <div className="row-num">{ index + 1 }</div>
                        <div className="source">{tu.source}</div>
                        {currentSegment === index ? <TargetEditbBox className="target" onClick={() => onTargetClick(index)} tuid={index + 1} /> : 
                        <div className="target" onClick={() => onTargetClick(index)}>{tu.target}</div> }
                        <div className="status"><FontAwesomeIcon icon={faCoffee} /></div>
                    </div>
                ))
            }
        </div >
    );
});

export default EditorGrid;
