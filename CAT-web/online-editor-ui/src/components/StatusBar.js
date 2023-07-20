import React from 'react';
import { useSelector } from 'react-redux';

const StatusBar = ({ onClick }) => {
    const message = useSelector((state) => state.statusBar.message);
    const date = useSelector((state) => state.statusBar.date);
    return <div onClick={onClick}>
        <span>Click on the translation to edit text...</span>  {message} { date }
    </div>;
};

export default StatusBar;