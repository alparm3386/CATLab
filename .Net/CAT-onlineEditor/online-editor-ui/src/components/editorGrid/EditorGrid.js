import 'styles/editorGrid.scss';
import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCoffee } from '@fortawesome/free-solid-svg-icons'
import { setCurrentTuid } from 'store/appDataSlice';
import TargetEditbBox from 'components/editorGrid/targetEditBox';
import { VariableSizeList as List } from 'react-window';
import utils from 'utils/utils';

var renderCntr = 0;
function highLightTags(text) {
    return text.replace(/({\/*\d*\/*})/g, '<span class="text-danger">$1</span>');
}

const ROW_HEIGHT = 60;

const BASE_HEIGHT = 25;   // assuming 20 pixels for a single line
const CHAR_THRESHOLD = 50;  // assuming 50 characters fit in one line

const estimateHeight = (text) => {
    const pureText = utils.extractTextFromHTML(text);
    const lineCount = Math.ceil(pureText.length / CHAR_THRESHOLD);
    return Math.max(lineCount * BASE_HEIGHT, ROW_HEIGHT);
}

const EditorGrid = //React.memo(
    function EditorGrid() {
        const [rowHeights, setRowHeights] = React.useState({});
        const [viewportHeight, setViewportHeight] = React.useState(0);
        const containerRef = React.useRef(null);
        const listRef = React.useRef(null);

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

        const getItemSize = (index) => {
            const tu = translationUnits[index];
            return estimateHeight(highLightTags(tu.source));
        };

        const setRef = (index) => (node) => {
            if (node) {
                const height = node.getBoundingClientRect().height;
                if (rowHeights[index] !== height) {
                    setRowHeights((prev) => {
                        const newHeights = { ...prev, [index]: height };
                        if (listRef.current) {
                            listRef.current.resetAfterIndex(index, true);
                        }
                        return newHeights;
                    });
                }
            }
        };


        const renderRow = ({ index, style }) => {
            const tu = translationUnits[index];
            return (
                <div ref={setRef(index)} key={index} style={style} className="tu-row">
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
            console.log("onTargetClick: " + tuid);
            dispatch(setCurrentTuid(tuid));
        }

        return (
            <div ref={containerRef} className="grid-area">
                {translationUnits &&
                    <List ref={listRef}
                        height={viewportHeight}  // replace with the height you want
                        itemCount={translationUnits.length}
                        itemSize={getItemSize}
                        width='100%'  // assume you want it to take up the full width of its container
                    >
                        {renderRow}
                    </List>
                }
            </div>
        );
    }//);

export default EditorGrid;
