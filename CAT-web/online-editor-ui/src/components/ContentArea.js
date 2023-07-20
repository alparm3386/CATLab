import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import EditorGrid from './EditorGrid';
import StatusBar from './StatusBar';
import Toolbox from './Toolbox';

const ContentArea = () => {
    const [isToolboxExpanded, setToolboxExpanded] = useState(true);

    const toggleToolbox = () => {
        setToolboxExpanded(!isToolboxExpanded);
    };

    return (
        <>
            <div className="content-area">
                <EditorGrid />
                <Toolbox expanded={isToolboxExpanded} />
            </div>
            <StatusBar onClick={toggleToolbox} />
        </>
    );
}

export default ContentArea;
