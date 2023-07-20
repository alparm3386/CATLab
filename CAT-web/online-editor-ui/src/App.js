import React, { useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import './App.scss';
import ContentArea from './components/ContentArea';
import Navbar from './components/Navbar';
import StatusBar from './components/StatusBar';
import { Provider } from 'react-redux';
import store from './store/store';
import { loadEditorData } from './api/apiService';

function App() {
    let ignore = false;
    useEffect(() => {
        if (!ignore) {
            console.log("App start ...")
            //load the data and store it in the global store
            let editorData = loadEditorData();
        }

        return () => { ignore = true; }
    }, []);

    return (
        <Provider store={store}>
            <div className="app">
                <Navbar />
                <ContentArea className="ContentArea" />
                <StatusBar />
            </div>
        </Provider>
    );
}

export default App;
