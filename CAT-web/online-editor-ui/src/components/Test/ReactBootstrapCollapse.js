import React, { useState } from 'react';
import Collapse from 'react-bootstrap/Collapse';
import Button from 'react-bootstrap/Button';

function ReactBootstrapCollapse() {
    const [open, setOpen] = useState(false);

    return (
        <>
            <Button onClick={() => setOpen(!open)}>
                {open ? 'Hide' : 'Show'} Toolbox
            </Button>
            <Collapse in={open}>
                <div>
                aaaaaaaaaaaaaaaaaa
                    {/* Your toolbox content here */}
                </div>
            </Collapse>
        </>
    );
}

export default ReactBootstrapCollapse;
