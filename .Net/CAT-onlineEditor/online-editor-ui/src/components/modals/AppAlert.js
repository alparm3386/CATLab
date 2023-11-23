// AppAlert.js
import React from 'react';
import { Modal, Alert } from 'react-bootstrap';
import { useSelector, useDispatch } from 'react-redux';
import { showAlert } from 'store/appUiSlice';

export const AppAlert = () => {
    const alert = useSelector((state) => state.appUi.alert);
    const dispatch = useDispatch();

    const onClose = React.useCallback((event) => {
        if (alert.callback) {
            alert.callback("ok"); // Call the callback function when the modal is closed
        }
        dispatch(showAlert({ title: '', message: '', show: false }));
    }, [dispatch, alert]);

    return (
        <Modal show={alert.show} centered onHide={onClose}>
            <Modal.Header closeButton>
                <Modal.Title>{alert.title}</Modal.Title>
            </Modal.Header>
            <Alert variant="success">
                {alert.message}
            </Alert>
        </Modal>
    );
};

export default AppAlert;
