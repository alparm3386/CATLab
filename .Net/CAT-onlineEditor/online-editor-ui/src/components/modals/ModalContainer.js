import React, { } from 'react';
import LoginForm from 'components/modals/LoginForm';
import Alert from 'components/modals/Alert';
import Confirm from 'components/modals/Confirm';

export const ModalContainer = () => {
    //const message = useSelector((state) => state.appUi.statusBar.message);

    return (
        <>
            <LoginForm />
            <Alert />
            <Confirm />
        </>
    );
};

export default ModalContainer;
