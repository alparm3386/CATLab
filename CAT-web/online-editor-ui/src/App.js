import React, { useState } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import './App.scss';
import ContentArea from './components/ContentArea';
import Navbar from './components/Navbar';
import StatusBar from './components/StatusBar';
import { StatusBarMessageContext, SetStatusBarMessageContext } from './contexts/StatusBarContext';

function App() {
    const [message, setMessage] = useState('');

    return (
        <SetStatusBarMessageContext.Provider value={{ setMessage }}>
            <div className="app">
                <Navbar />
                <ContentArea className="ContentArea" />
                <StatusBarMessageContext.Provider value={message}>
                    <StatusBar />
                </StatusBarMessageContext.Provider>
            </div>
        </SetStatusBarMessageContext.Provider>
    );
}

export default App;
