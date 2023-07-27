import React, { useState } from 'react';
import { Modal, Button, Form } from 'react-bootstrap'
import { useSelector } from 'react-redux';
import { getJobData } from 'api/editorApi';

export const LoginForm = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const isLoginModalOpen = useSelector((state) => state.editorData.isLoginModalOpen);

    const handleLogin = () => {
        //onLogin(username, password);
        setUsername('');
        setPassword('');
    };

    return (
        <Modal show={ isLoginModalOpen }>
            <Modal.Header>
                <Modal.Title>Login</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <div>
                    <Form.Control className="mb-2" type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} />
                    <Form.Control className="mb-2" type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} />
                    <div className="d-flex justify-content-end mt-4">
                        <Button onClick={handleLogin}>Login</Button>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
            </Modal.Footer>
        </Modal>
    );
};

export default LoginForm;
