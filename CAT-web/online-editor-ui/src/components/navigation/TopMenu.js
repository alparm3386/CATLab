//TopMenu.js
import 'styles/navbar.scss';
import React from 'react';
import { Navbar, Nav, NavDropdown } from 'react-bootstrap';
import { showAlert } from 'store/appUiSlice';
import { useDispatch } from 'react-redux';
import editorApi from 'services/editorApi';
import utils from 'utils/utils';

const TopMenu = () => {
    const dispatch = useDispatch();

    const handleDownloadJob = async () => {
        try {
            const response = await editorApi.downloadJob();

            if (response.status !== 200) {
                throw new Error('Network response was not ok');
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

    return (
        <Navbar bg="light" expand="lg">
            <Navbar.Brand>Logo</Navbar.Brand>
            <Navbar.Toggle aria-controls="navbarNav" />
            <Navbar.Collapse id="navbarNav">
                <Nav>
                    <NavDropdown title="Job" id="navbarDropdown">
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
