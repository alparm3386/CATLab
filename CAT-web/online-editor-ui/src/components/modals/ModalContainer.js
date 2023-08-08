import React, { } from 'react';
import { Modal, Alert } from 'react-bootstrap'
import LoginForm from 'components/modals/LoginForm';
import AppAlert from 'components/modals/AppAlert';

export const ModalContainer = () => {
    //const message = useSelector((state) => state.appUi.statusBar.message);

    return (
        <>
            <LoginForm />
            <AppAlert />
        </>
    );
};

export default ModalContainer;
