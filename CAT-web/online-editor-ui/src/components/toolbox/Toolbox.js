// Toolbox.js
import 'styles/toolboxArea.scss';
import React, { useState } from 'react';
import { useDispatch } from 'react-redux';
import { setStatusBarMessage } from '../../store/jobDataSlice';
import TMMatches from './TMMatches';
import AuxiliaryTools from './AuxiliaryTools';


var renderCntr = 0;
const Toolbox = ({ expanded, currentIdx }) => {
    //const [counter, setCounter] = useState(1);
    //const dispatch = useDispatch();

    console.log("Toolbox rendered: " + renderCntr++);

    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>
        <TMMatches />
        <AuxiliaryTools/>
    </div>;
};

export default Toolbox;