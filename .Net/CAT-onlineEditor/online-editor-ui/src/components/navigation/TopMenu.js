//TopMenu.js
import 'styles/navbar.scss';
import React from 'react';
import { Navbar, Nav, NavDropdown } from 'react-bootstrap';
import { showAlert } from 'store/appUiSlice';
import { useSelector, useDispatch } from 'react-redux';
import editorApi from 'services/editorApi';
import utils from 'utils/utils';
import modalService from 'services/modalService';
import { showLoading } from 'store/appUiSlice';

const TopMenu = () => {
    const dispatch = useDispatch();
    const jobData = useSelector((state) => state.appData.jobData);

    const handleDownloadJob = async () => {
        try {
            dispatch(showLoading(true));
            const response = await editorApi.downloadJob();
            dispatch(showLoading(false));

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
            dispatch(showLoading(false));
            dispatch(showAlert({ title: 'Error', message: error.message }));
            console.error('There was a problem with the fetch operation:', error);
        }
    };

    const handleSubmitJob = async () => {
        try {
            dispatch(showLoading(true));
            editorApi.submitJob().then((response) => {
                dispatch(showLoading(false));
                modalService.showAlert("Success", "Job submitted").then((result) => {
                    window.location.href = "/";
                });
            }).catch((error) => {
                dispatch(showLoading(false));
                modalService.showAlert("Error", "Unable to submit job")
            });

        } catch (error) {
            dispatch(showAlert({ title: 'Error', message: error.message }));
            console.error('There was a problem with the fetch operation:', error);
        }
    };

    const isJobSubmit = () => {
        if (jobData.task === 2 || jobData.task === 3 || jobData.task === 4 || jobData.task === 5)
            return true;
        else
            return false;
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
