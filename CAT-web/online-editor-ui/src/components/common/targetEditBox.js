import React from 'react';
import sanitizeHtml from "sanitize-html"
import { useSelector, useDispatch } from 'react-redux';
import { setTargetEditbBoxTuid, setTargetEditbBoxContent } from 'store/appUiSlice';

let cntr = 0;
const TargetEditbBox = ({ className, tuid }) => {
    const editRef = React.useRef(null); 
    const dispatch = useDispatch();
    const targetEditbBox = useSelector((state) => state.appUi.targetEditbBox);
    const jobData = useSelector((state) => state.jobData.jobData);

    React.useEffect(() => {
        editRef.current && editRef.current.focus();
        dispatch(setTargetEditbBoxTuid(tuid));
        dispatch(setTargetEditbBoxContent(jobData.translationUnits[tuid].target));
    }, [dispatch, tuid, jobData]);

    const onContentBlur = React.useCallback(evt => {
        console.log("onContentBlur: " + cntr++);
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
            dangerouslySetInnerHTML={{ __html: targetEditbBox.content }} />
    )
};

export default TargetEditbBox;