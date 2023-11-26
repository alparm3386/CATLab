import React from 'react';
import sanitizeHtml from "sanitize-html"
import { useSelector, useDispatch } from 'react-redux';
import { updateTranslationUnitTarget } from 'store/appDataSlice';
import utils from 'utils/utils';
import { setCurrentTuid } from 'store/appDataSlice';
import editorApi from 'services/editorApi';
import { showLoading } from 'store/appUiSlice';
import { showStatusBarMessage } from 'store/appUiSlice';


let cntr = 0;
const TargetEditbBox = ({ className }) => {
    const editRef = React.useRef(null);
    const dispatch = useDispatch();
    //const content = useSelector((state) => state.appData.targetEditbBoxContent);
    const currentTuid = useSelector((state) => state.appData.currentTuid);
    const translationUnits = useSelector((state) => state.appData.translationUnits);
    let content = translationUnits[currentTuid - 1].target;

    console.log("TargetEditbBox rendered: " + cntr++);


    React.useEffect(() => {
        // After the component mounts, move the cursor to the end of the contenteditable div
        const editboxNode = editRef.current;
        if (editboxNode) {
            const textLength = editboxNode.textContent.length;
            console.log("text length: " + textLength);
            const range = document.createRange();
            range.selectNodeContents(editboxNode);
            range.collapse(false); // Set cursor to the end (false: collapse to the end)
            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(range);
            editboxNode.focus();
        }
    }, []);

    const onContentBlur = React.useCallback(event => {
        console.log("onContentBlur: " + currentTuid);
        const sanitizeConf = {
            allowedTags: ["span"],
            allowedAttributes: {}
        };

        //update the stored content
        const sHtml = sanitizeHtml(event.currentTarget.innerHTML, sanitizeConf);
        if (content === sHtml)
            return;
        //update the translationUnits
        dispatch(updateTranslationUnitTarget({ index: currentTuid - 1, target: utils.extractTextFromHTML(sHtml) }));

        dispatch(showLoading(true));
        const target = utils.extractTextFromHTML(sHtml);
        editorApi.saveSegment(currentTuid, target, false, 0).then(() => {
            dispatch(showStatusBarMessage('Segment saved ...'));
        }).catch((error) => {
            dispatch(showStatusBarMessage('Error:' + error.toString()));
        }).finally(() => {
            dispatch(showLoading(false));
        });

    }, [dispatch, currentTuid]);

    const handleKeyDown = React.useCallback((event) => {
        if (event.ctrlKey && event.key === 'Enter') {
            //update the translationUnits
            const sanitizeConf = {
                allowedTags: ["span"],
                allowedAttributes: {}
            };
            const sHtml = sanitizeHtml(event.currentTarget.innerHTML, sanitizeConf);
            const target = utils.extractTextFromHTML(sHtml);
            dispatch(updateTranslationUnitTarget({ index: currentTuid - 1, target: target }));

            console.log('Ctrl+Enter pressed');

            dispatch(showLoading(true));
            editorApi.saveSegment(currentTuid, target, true, 0).then(() => {
                dispatch(showStatusBarMessage('Segment saved ...'));
            }).catch((error) => {
                dispatch(showStatusBarMessage('Error:' + error.toString()));
            }).finally(() => {
                dispatch(showLoading(false));
            });

            //jump to next segment
            dispatch(setCurrentTuid(currentTuid + 1));
        }
    }, [dispatch, currentTuid]);

    return (
        <div className={className} ref={editRef} contentEditable onBlur={onContentBlur} dangerouslySetInnerHTML={{ __html: content }}
            onKeyDown={handleKeyDown} />
    )
};

export default TargetEditbBox;