import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import EditorGrid from './EditorGrid';
import StatusBar from './StatusBar';

const ContentArea = () => {
    const [isToolboxExpanded, setToolboxExpanded] = useState(true);

    const toggleToolbox = () => {
        setToolboxExpanded(!isToolboxExpanded);
    };

    return (
        <>
            <div className="content-area">
                <EditorGrid />
                <div className={`toolbox-area${isToolboxExpanded ? ' expanded' : ''}`}> fsdfs
                    toolbox {/* Toolbox content goes here */}
                </div>
            </div>
            <StatusBar onClick={toggleToolbox} />
        </>
    );
}

export default ContentArea;
