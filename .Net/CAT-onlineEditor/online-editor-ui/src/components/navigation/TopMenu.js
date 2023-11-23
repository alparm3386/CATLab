//TopMenu.js
import 'styles/navbar.scss';
import React from 'react';
import { Navbar, Nav, NavDropdown } from 'react-bootstrap';
import { showAlert } from 'store/appUiSlice';
import { useDispatch } from 'react-redux';
import editorApi from 'services/editorApi';
import utils from 'utils/utils';
import modalService from 'services/modalService';

const TopMenu = () => {
    const dispatch = useDispatch();

    const handleDownloadJob = async () => {
        try {
            const response = await editorApi.downloadJob();

            if (response.status !== 200) {
                dispatch(showAlert({ title: 'Error', message: "Unable to download document." }));
                return;
            }

            const contentDisposition = response.headers['content-disposition'];
            const filename = utils.extractFilenameFromContentDisposition(contentDisposition) || 'default_name.txt'; // Use a default name if not found

            const blob = new Blob([response.data], { type: 'application/octet-stream' });
            const url = window.URL.createObjectURL(blob);

            const downloadAnchor = document.createElement('a');
            downloadAnchor.style.display = 'none';
            downloadAnchor.href = url;
            downloadAnchor.download = filename;
            document.body.appendChild(downloadAnchor);
            downloadAnchor.click();
            document.body.removeChild(downloadAnchor);
            window.URL.revokeObjectURL(url);
        } catch (error) {
            dispatch(showAlert({ title: 'Error', message: error.message }));
            console.error('There was a problem with the fetch operation:', error);
        }
    };

    const handleSubmitJob = async () => {
        try {
            //dispatch(showAlert({ title: 'Error', message: "Job submitted" }));
            modalService.showAlert("Success", "Job submitted");
            return;
            const response = await editorApi.submitJob();

            if (response.status !== 200) {
                throw new Error('Unable to submit job');
            }

        } catch (error) {
            dispatch(showAlert({ title: 'Error', message: error.message }));
            console.error('There was a problem with the fetch operation:', error);
        }
    };

    const isJobSubmit = () => {
        return true;
    };


    return (
        <Navbar bg="light" expand="lg">
            <Navbar.Brand>Logo</Navbar.Brand>
            <Navbar.Toggle aria-controls="navbarNav" />
            <Navbar.Collapse id="navbarNav">
                <Nav>
                    <NavDropdown title="Job" id="navbarDropdown">
                        {isJobSubmit() ? (<NavDropdown.Item onClick={handleSubmitJob}>Submit job</NavDropdown.Item>) : null}
                        <NavDropdown.Item onClick={handleDownloadJob}>Download job</NavDropdown.Item>
                    </NavDropdown>
                    <Nav.Link href="#">Features</Nav.Link>
                    <Nav.Link href="#">Pricing</Nav.Link>
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
}

export default TopMenu;
