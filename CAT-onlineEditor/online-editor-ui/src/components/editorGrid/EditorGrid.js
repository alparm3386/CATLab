import 'styles/editorGrid.scss';
import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCoffee } from '@fortawesome/free-solid-svg-icons'
import { setCurrentTuid } from 'store/appDataSlice';
import TargetEditbBox from 'components/editorGrid/targetEditBox';
import { FixedSizeList as List } from 'react-window';

var renderCntr = 0;
function highLightTags(text) {
    return text.replace(/({\/*\d*\/*})/g, '<span class="text-danger">$1</span>');
}

const ROW_HEIGHT = 60;

const EditorGrid = //React.memo(
    function EditorGrid() {
        const [viewportHeight, setViewportHeight] = React.useState(0);
        const containerRef = React.useRef(null);

        const dispatch = useDispatch();
        const translationUnits = useSelector((state) => state.appData.translationUnits);
        //const jobData = appDataService.jobData;
        const currentTuid = useSelector((state) => state.appData.currentTuid);

        console.log("EditorGrid rendered: " + renderCntr++);

        React.useEffect(() => {
            if (containerRef.current) {
                setViewportHeight(containerRef.current.getBoundingClientRect().height);
            }

            const handleResize = () => {
                if (containerRef.current) {
                    setViewportHeight(containerRef.current.getBoundingClientRect().height);
                }
            };

            window.addEventListener('resize', handleResize);
            return () => window.removeEventListener('resize', handleResize);
        }, []);

        const renderRow = ({ index, style }) => {
            const tu = translationUnits[index];
            return (
                <div key={index} style={style} className="tu-row">
                    <div className="row-num">{index + 1}</div>
                    <div className="source" dangerouslySetInnerHTML={{ __html: highLightTags(tu.source) }} />
                    {currentTuid === index + 1 ?
                        <TargetEditbBox key={`target_${index}`} className="target" /> :
                        <div className="target" onClick={() => onTargetClick(index + 1)} dangerouslySetInnerHTML={{ __html: highLightTags(tu.target) }} />
                    }
                    <div className="status"><FontAwesomeIcon icon={faCoffee} /></div>
                </div>
            );
        };

        function onTargetClick(tuid) {
            dispatch(setCurrentTuid(tuid));
        }

        return (
            <div ref={containerRef} className="grid-area">
                {translationUnits &&
                    <List
                        height={viewportHeight}  // replace with the height you want
                        itemCount={translationUnits.length}
                        itemSize={ROW_HEIGHT}
                        width='100%'  // assume you want it to take up the full width of its container
                    >
                        {renderRow}
                    </List>
                }
            </div>
        );
    }//);

export default EditorGrid;
