import React, { useState } from 'react';
import { Modal, Button, Form, Alert } from 'react-bootstrap';
import { useSelector, useDispatch } from 'react-redux';
import editorApi from 'services/editorApi';
import { setJobData } from 'store/editorDataSlice';
import { showLoginModal } from 'store/appUiSlice';

//misc.
import cookieHelper from 'utils/cookieHelper';
import appService from 'services/appService';


export const LoginForm = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState(null);
    const isLoginModalOpen = useSelector((state) => state.appUi.login.isOpen);
    const dispatch = useDispatch();
    //const fetchJobData = useFetchJobData();
    console.log("LoginForm ...")

    // Get stored credentials from localStorage
    React.useEffect(() => {
    }, []);

    const handleLogin = async (event) => {
        event.preventDefault(); // To prevent form submission

        try {
            //get the jwt
            const result = await editorApi.login(username, password);
            editorApi.setJWT(result.data.token);
            cookieHelper.setToken(result.data.token);

            //load the job data
            appService.loadJobData(dispatch);


            //close the login modal
            dispatch(showLoginModal(false));

            //reset the input fields
            setUsername('');
            setPassword('');

            // Clear the error state if login was successful
            setError(null);

        } catch (error) {
            setError("Authentication Failed. Please try again."); // Set the error state if authentication fails
            console.log(error);
        }
    };

    return (
        <Modal show={isLoginModalOpen}>
            <Modal.Header>
                <Modal.Title>Login</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form onSubmit={handleLogin}>
                    {error && <Alert variant="danger">{error}</Alert>}
                    <Form.Control className="mb-2" type="text" id="username" name="username" autocomplete="username"
                        placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} required />
                    <Form.Control className="mb-2" type="password" id="password" name="password" autocomplete="current-password"
                        placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} required />
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
