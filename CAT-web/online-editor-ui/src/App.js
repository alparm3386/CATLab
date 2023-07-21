import React, { useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import './App.scss';
import ContentArea from './components/ContentArea';
import Navbar from './components/Navbar';
import StatusBar from './components/StatusBar';
import { useDispatch, Provider } from 'react-redux';
import store from './store/store';
//import { getTMMatches } from './api/apiService';
import { loadJobData } from './api/apiService';
import { setJobData } from './store/editorDataSlice';
import { fetchInitialData } from './store/actions/dataActions';

function AppInit() {
    const dispatch = useDispatch();
    let ignore = false;

    useEffect(() => {
        if (!ignore) {
            console.log("App start ...")
            //load the data and store it in the global store
            //dispatch(setJobData(loadJobData()));
            loadJobData().then(jobData => dispatch(setJobData(jobData)));
        }

        return () => { ignore = true; }
    }, []);

    return null; // this component doesn't need to render anything
}

function App() {
    return (
        <Provider store={store}>
            <div className="app">
                <Navbar />
                <ContentArea className="ContentArea" />
                <StatusBar />
                <AppInit />
            </div>
        </Provider>
    );
}

export default App;
