import React, { useContext } from 'react';
import { StatusBarMessageContext } from '../contexts/StatusBarContext';

const StatusBar = ({ onClick }) => {
    const { message } = useContext(StatusBarMessageContext);

    return <div onClick={onClick}>
        <span>Click on the translation to edit text...</span>  {message}
    </div>;
};

export default StatusBar;