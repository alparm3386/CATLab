import 'styles/navbar.scss';
import React from 'react';
import { Navbar, Nav, NavDropdown } from 'react-bootstrap';
import { showAlert } from 'store/appUiSlice';
import { useDispatch } from 'react-redux';

const TopMenu = () => {
    const dispatch = useDispatch();

    const handleDownloadJob = async () => {
        try {
            const response = await fetch('/api/file/downloadJob');
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const blob = await response.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.style.display = 'none';
            a.href = url;
            // the filename you want
            a.download = 'FileName.txt';  // replace with your desired download file name
            document.body.appendChild(a);
            a.click();
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
