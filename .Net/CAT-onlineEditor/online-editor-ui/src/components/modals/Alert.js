// Alert.js
import React from 'react';
import { Modal } from 'react-bootstrap';
import BSAlert from 'react-bootstrap/Alert'; // Rename the imported component

import { useSelector, useDispatch } from 'react-redux';
import { showAlert } from 'store/appUiSlice';

export const Alert = () => {
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
            <BSAlert variant="success">
                {alert.message}
            </BSAlert>
        </Modal>
    );
};

export default Alert;
