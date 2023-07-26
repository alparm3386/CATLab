import React, { useState } from 'react';
import 'styles/contentArea.scss';
import EditorGrid from 'components/editorGrid/EditorGrid';
import Toolbox from 'components/toolbox/Toolbox';

var renderCntr = 0;

const ContentArea = ({ settings }) => {

    console.log("ContentArea rendered: " + renderCntr++);
    return (
        <div className="content-area">
            <EditorGrid />
            <Toolbox expanded="true" />
        </div>
    );
}

export default ContentArea;
