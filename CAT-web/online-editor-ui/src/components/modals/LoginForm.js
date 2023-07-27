import React, { useState } from 'react';
import { Modal, Button, Form, Alert } from 'react-bootstrap';
import { useSelector, useDispatch } from 'react-redux';
import { login } from 'api/editorApi';
import { setJWT } from 'store/editorDataSlice';

export const LoginForm = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState(null);
    const [rememberMe, setRememberMe] = useState(false);
    const isLoginModalOpen = useSelector((state) => state.appUi.login.isOpen);
    const dispatch = useDispatch();

    // Get stored credentials from localStorage
    React.useEffect(() => {
        const storedCredentials = JSON.parse(localStorage.getItem("credentials"));
        if (storedCredentials) {
            setUsername(storedCredentials.username);
            setPassword(storedCredentials.password);
        }
    }, []);

    const handleLogin = async (event) => {
        event.preventDefault(); // To prevent form submission

        try {
            const result = await login(username, password);
            dispatch(setJWT(result.data));

            if (rememberMe) {
                localStorage.setItem("credentials", JSON.stringify({ username, password }));
            } else {
                localStorage.removeItem("credentials");
            }

            setUsername('');
            setPassword('');
            setError(null); // Clear the error state if login was successful

        } catch (error) {
            setError("Authentication Failed. Please try again."); // Set the error state if authentication fails
            console.log(error);
        }
    };

    return (
        <Modal show={isLoginModalOpen}>
            <Modal.Header closeButton>
                <Modal.Title>Login</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form onSubmit={handleLogin}>
                    {error && <Alert variant="danger">{error}</Alert>}
                    <Form.Control className="mb-2" type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} required />
                    <Form.Control className="mb-2" type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} required />
                    <Form.Check type="checkbox" label="Remember me" checked={rememberMe} onChange={e => setRememberMe(e.target.checked)} />
                    <div className="d-flex justify-content-end mt-4">
                        <Button type="submit">Login</Button>
                    </div>
                </Form>
            </Modal.Body>
            <Modal.Footer>
            </Modal.Footer>
        </Modal>
    );
};

export default LoginForm;
