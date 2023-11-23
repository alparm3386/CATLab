import React, { } from 'react';
import LoginForm from 'components/modals/LoginForm';
import Alert from 'components/modals/Alert';

export const ModalContainer = () => {
    //const message = useSelector((state) => state.appUi.statusBar.message);

    return (
        <>
            <LoginForm />
            <Alert />
        </>
    );
};

export default ModalContainer;
