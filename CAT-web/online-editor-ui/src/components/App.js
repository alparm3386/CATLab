//css
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import 'styles/App.scss';

//react
import React, { useEffect } from 'react';
import { Provider, useDispatch, useSelector } from 'react-redux';
import store from 'store/store';
import editorApi from 'services/editorApi';
import errorHandler from 'services/errorHandler';
import appService from 'services/appService';

//components
import ContentArea from 'components/ContentArea';
import Navbar from 'components/navigation/Navbar';
import StatusBar from 'components/statusBar/StatusBar';
import ModalContainer from 'components/modals/ModalContainer';
import Spinner from 'components/common/Spinner';

//misc.
import cookieHelper from 'utils/cookieHelper';

function AppInit() {
    const dispatch = useDispatch();
    let ignore = false;
    console.log("App init ...");

    useEffect(() => {
        if (!ignore) {
            //initialize the api service
            //jwt
            const jwt = cookieHelper.getToken();
            //the url params
            const urlParams = window.location.search.substring(1);
            editorApi.initializeApiClient(urlParams, jwt, errorHandler);

            //load the job data
            appService.loadJobData(dispatch);
        }

        return () => { ignore = true; }
    }, []);

    return null; // this component doesn't need to render anything
}

function App() {
    console.log("App ...");

    return (
        <Provider store={store}>
            {/*Init the app*/}
            <AppInit />
            <div className="app">
                <Navbar />
                <ContentArea className="ContentArea" />
                <StatusBar />

                {/*The modals*/}
                <ModalContainer />

                {/* the main spinner */}
                <Spinner fullScreen={true} />
            </div>
        </Provider>
    );
}

export default App;
