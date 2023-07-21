// AuxiliaryTools.js
import './../styles/auxiliaryTools.scss';


var renderCntr = 0;
const AuxiliaryTools = () => {
    console.log("AuxiliaryTools rendered: " + renderCntr++);

    return <div className="auxiliary-tools">
        Auxiliary tools ...
    </div>;
};

export default AuxiliaryTools;