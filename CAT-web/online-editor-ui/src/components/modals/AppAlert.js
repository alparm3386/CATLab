import React from 'react';
import { Modal, Alert } from 'react-bootstrap';
import { useSelector, useDispatch } from 'react-redux';
import { showAlert } from 'store/appUiSlice';

export const AppAlert = () => {
    const alert = useSelector((state) => state.appUi.alert);
    const dispatch = useDispatch();

    const onClose = React.useCallback(event => {
        dispatch(showAlert({ title: '', message: '', show: false }));

    }, [dispatch]);

    return (
        <Modal show={alert.show} centered>
            <Modal.Header closeButton onClick={onClose}>
                <Modal.Title>{alert.title}</Modal.Title>
            </Modal.Header>
            <Alert variant="success">
                {alert.message}
            </Alert>
        </Modal>);

};

export default AppAlert;