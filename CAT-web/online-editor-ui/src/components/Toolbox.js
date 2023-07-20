import React from 'react';

const Toolbox = ({ expanded }) => {
    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>tool box</div>;
};

export default Toolbox;