// AuxiliaryTools.js
import React from 'react';
import 'styles/auxiliaryTools.scss';
import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import 'react-tabs/style/react-tabs.css';

var renderCntr = 0;
const AuxiliaryTools = () => {
    console.log("AuxiliaryTools rendered: " + renderCntr++);

    return (
        <div className="auxiliary-tools">
            <Tabs>
                <TabList>
                    <Tab>Context</Tab>
                    <Tab>Preview</Tab>
                    <Tab>Concordance</Tab>
                </TabList>

                <TabPanel>
                    <h2>Context content</h2>
                </TabPanel>
                <TabPanel>
                    <h2>Preview content</h2>
                </TabPanel>
                <TabPanel>
                    <h2>Concordance content</h2>
                </TabPanel>
            </Tabs>
        </div>
    );
};

export default AuxiliaryTools;
