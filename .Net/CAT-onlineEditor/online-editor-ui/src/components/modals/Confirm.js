// Confirm.js
import React from 'react';
import { Modal } from 'react-bootstrap';
import Button from 'react-bootstrap/Button';
import { useSelector, useDispatch } from 'react-redux';
import { showConfirm } from 'store/appUiSlice';

export const Confirm = () => {
    const confirm = useSelector((state) => state.appUi.confirm);
    const dispatch = useDispatch();

    const onCancel = React.useCallback((event) => {
        dispatch(showConfirm({ title: '', message: '', show: false }));
    }, [dispatch]);

    const onConfirm = React.useCallback((event) => {
        if (confirm.callback) {
            confirm.callback("ok"); // Call the callback function when the modal is closed
        }
        dispatch(showConfirm({ title: '', message: '', show: false }));
    }, [dispatch, confirm]);

    return (
        <Modal show={confirm.show} centered onHide={onCancel}>
            <Modal.Header closeButton>
                <Modal.Title>{confirm.title}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                {confirm.message}
            </Modal.Body>
            <Modal.Footer>
                <Button variant="warning" onClick={onConfirm}>
                    Confirm
                </Button>
                <Button variant="light" onClick={onCancel}>
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
    );
};

export default Confirm;
