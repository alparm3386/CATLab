// Toolbox.js
import 'styles/toolboxArea.scss';
import TMMatches from './TMMatches';
import AuxiliaryTools from './AuxiliaryTools';


var renderCntr = 0;
const Toolbox = ({ expanded, currentIdx }) => {
    console.log("Toolbox rendered: " + renderCntr++);

    return <div className={`toolbox-area${expanded ? ' expanded' : ''}`}>
        <TMMatches />
        <AuxiliaryTools/>
    </div>;
};

export default Toolbox;