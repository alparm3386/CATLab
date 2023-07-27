import React, { } from 'react';
import { Modal, Alert } from 'react-bootstrap'
import LoginForm from 'components/modals/LoginForm';

export const ModalContainer = () => {
    return (
        <>
            <LoginForm />
            <Modal show={false} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Alert</Modal.Title>
                </Modal.Header>
                <Alert variant="success">
                    This is a message
                </Alert>
            </Modal>
        </>
    );
};

export default ModalContainer;
