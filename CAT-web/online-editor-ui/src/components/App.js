import React, { useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.css'; // If you haven't linked Bootstrap in your HTML
import 'styles/App.scss';
import ContentArea from 'components/ContentArea';
import Navbar from 'components/navigation/Navbar';
import StatusBar from 'components/statusBar/StatusBar';
import LoginForm from 'components/modals/LoginForm';
import { useDispatch, useSelector, Provider } from 'react-redux';
import store from 'store/store';
import { getJobData } from 'api/editorApi';
import { setJobData, setUrlParams, showLoginModal } from 'store/editorDataSlice';
import Modal from 'react-bootstrap/Modal';

function AppInit() {
    const dispatch = useDispatch();
    let ignore = false;

    useEffect(() => {
        if (!ignore) {
            console.log("App start ...")
            //load the data and store it in the global store
            //dispatch(setJobData(loadJobData()));
            const urlParams = window.location.search.substring(1);
            dispatch(setUrlParams(urlParams));
            // Async function inside your useEffect.
            const fetchData = async () => {
                const urlParams = window.location.search.substring(1);
                dispatch(setUrlParams(urlParams));
                try {
                    const result = await getJobData(urlParams);
                    dispatch(setJobData(result.data));
                } catch (error) {
                    // Handle error
                    if (error?.response?.status === 401) //unauthorized
                        dispatch(showLoginModal(true));
                    else
                        console.log(error);
                }
            };

            // Call the async function
            fetchData();
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
                <AppInit />

                <LoginForm />
            {/*    {isLoginModalOpen && (*/}
            {/*        <Modal onClose={() => dispatch(hideLoginModal())}>*/}
            {/*            <LoginForm />*/}
            {/*        </Modal>*/}
            {/*    )}*/}
            </div>
        </Provider>
    );
}

export default App;
