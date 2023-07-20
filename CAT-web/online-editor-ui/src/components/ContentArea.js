import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import EditorGrid from './EditorGrid';
import Toolbox from './Toolbox';

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
