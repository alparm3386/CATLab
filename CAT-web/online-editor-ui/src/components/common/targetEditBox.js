﻿import React from 'react';
import sanitizeHtml from "sanitize-html"
import { useSelector, useDispatch } from 'react-redux';
import { setTargetEditbBoxContent } from 'store/appDataSlice';
import utils from 'utils/utils';

let cntr = 0;
const TargetEditbBox = ({ className, tuid }) => {
    const editRef = React.useRef(null); 
    const dispatch = useDispatch();
    const content = useSelector((state) => state.appData.targetEditbBoxContent);
    const currentTuid = useSelector((state) => state.appData.currentTuid);
    const jobData = useSelector((state) => state.jobData.jobData);

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

    const onContentBlur = React.useCallback(evt => {
        //console.log("onContentBlur: " + cntr++);
        const sanitizeConf = {
            allowedTags: ["span"],
            allowedAttributes: {}
        };

        //update the stored content
        const sHtml = sanitizeHtml(evt.currentTarget.innerHTML);
        dispatch(setTargetEditbBoxContent(sHtml, sanitizeConf));
        //update the job data
        jobData.translationUnits[currentTuid - 1].target = utils.extractTextFromHTML(sHtml);

    }, [dispatch, currentTuid, jobData]);

    return (
        <div className={className}
            ref={editRef}
            contentEditable
            onBlur={onContentBlur}
            dangerouslySetInnerHTML={{ __html: content }} />
    )
};

export default TargetEditbBox;