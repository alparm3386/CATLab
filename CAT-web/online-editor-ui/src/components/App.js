//css
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import 'styles/App.scss';

//react
import React, { useEffect } from 'react';
import { Provider, useDispatch, useSelector } from 'react-redux';
import store from 'store/store';
import editorApi from 'services/editorApi';
import errorHandler from 'services/errorHandler';
import { setJobData } from 'store/editorDataSlice';

//components
import ContentArea from 'components/ContentArea';
import Navbar from 'components/navigation/Navbar';
import StatusBar from 'components/statusBar/StatusBar';
import ModalContainer from './modals/ModalContainer';

//misc.
import cookieHelper from 'utils/cookieHelper';

function AppInit() {
    const dispatch = useDispatch();
    let ignore = false;

    useEffect(() => {
        if (!ignore) {
            console.log("App start ...")
            //initialize the api service
            //jwt
            const jwt = cookieHelper.getToken();
            //the url params
            const urlParams = window.location.search.substring(1);
            editorApi.initializeApiClient(urlParams, jwt, errorHandler);

            //load the data and store it in the global store. Async function inside useEffect.
            const fetchjobData = async () => {
                try {
                    const result = await editorApi.getJobData();
                    dispatch(setJobData(result.data));
                } catch (error) {
                    console.log(error);
                }
            };

            // Call the async function
            fetchjobData();
        }

        return () => { ignore = true; }
    }, []);

    return null; // this component doesn't need to render anything
}

function App() {
    //const dispatch = useDispatch();
    //const isLoginModalOpen = useSelector(state => state.isLoginModalOpen);
    return (
        <Provider store={store}>
            <div className="app">
                <Navbar />
                <ContentArea className="ContentArea" />
                <StatusBar />

                {/*Init the app*/}
                <AppInit />

                {/*The modals*/}
                <ModalContainer/>
            </div>
        </Provider>
    );
}

export default App;
