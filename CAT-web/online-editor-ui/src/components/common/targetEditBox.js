import React from 'react';
import sanitizeHtml from "sanitize-html"
import { useSelector, useDispatch } from 'react-redux';
import { setTargetEditbBoxContent } from 'store/appDataSlice';

let cntr = 0;
const TargetEditbBox = ({ className, tuid }) => {
    const editRef = React.useRef(null); 
    const dispatch = useDispatch();
    const content = useSelector((state) => state.appData.targetEditbBoxContent);
    //const jobData = useSelector((state) => state.jobData.jobData);

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

        dispatch(setTargetEditbBoxContent(sanitizeHtml(evt.currentTarget.innerHTML, sanitizeConf)));
    }, [dispatch]);

    return (
        <div className={className}
            ref={editRef}
            contentEditable
            onBlur={onContentBlur}
            dangerouslySetInnerHTML={{ __html: content }} />
    )
};

export default TargetEditbBox;