// AuxiliaryTools.js
import React, { useState } from 'react';
import './../styles/auxiliaryTools.scss';

var renderCntr = 0;
const AuxiliaryTools = () => {
    const [selectedTab, setSelectedTab] = useState('context');
    console.log("AuxiliaryTools rendered: " + renderCntr++);

    return (
        <div className="auxiliary-tools">
            <div className="tabs">
                <button
                    className={selectedTab === 'context' ? 'active' : ''}
                    onClick={() => setSelectedTab('context')}
                >
                    Context
                </button>
                <button
                    className={selectedTab === 'preview' ? 'active' : ''}
                    onClick={() => setSelectedTab('preview')}
                >
                    Preview
                </button>
                <button
                    className={selectedTab === 'concordance' ? 'active' : ''}
                    onClick={() => setSelectedTab('concordance')}
                >
                    Concordance
                </button>
            </div>

            {selectedTab === 'context' && <div>Context content ...</div>}
            {selectedTab === 'preview' && <div>Preview content ...</div>}
            {selectedTab === 'concordance' && <div>Concordance content ...</div>}
        </div>
    );
};

export default AuxiliaryTools;
