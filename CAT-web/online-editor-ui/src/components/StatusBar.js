import React from 'react';
import { useSelector } from 'react-redux';

const StatusBar = ({ onClick }) => {
    const message = useSelector((state) => state.editorData.statusBar.message);
    return <div onClick={onClick}>
        <span>Click on the translation to edit text...</span>  {message}
    </div>;
};

export default StatusBar;