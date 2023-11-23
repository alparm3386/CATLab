//css
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import 'styles/app.scss';

//react
import React, { useEffect } from 'react';
import { Provider, useDispatch } from 'react-redux';
import store from 'store/store';
import editorApi from 'services/editorApi';
import errorHandler from 'services/errorHandler';
import appDataService from 'services/appDataService';

//components
import ContentArea from 'components/ContentArea';
import TopMenu from 'components/navigation/TopMenu';
import StatusBar from 'components/statusBar/StatusBar';
import ModalContainer from 'components/modals/ModalContainer';
import Spinner from 'components/common/Spinner';
import modalService from 'services/modalService';


//misc.
import cookieHelper from 'utils/cookieHelper';

function AppInit() {
    const dispatch = useDispatch();
    modalService.initialize(dispatch);
    let ignore = false;
    console.log("App init ...");

    useEffect(() => {
        if (!ignore) {
            //initialize the api service
            //jwt
            const jwt = cookieHelper.getToken();
            //the url params
            const urlParams = decodeURIComponent(window.location.search.substring(1));
            editorApi.initializeApiClient(urlParams, jwt, errorHandler);

            //load the job data
            appDataService.loadJobData(dispatch);
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
                <TopMenu />
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
