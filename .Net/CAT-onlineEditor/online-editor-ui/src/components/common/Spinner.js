import React from 'react';
import 'styles/spinner.css'; // Import the CSS file for styling
import { useSelector } from 'react-redux';

const Spinner = ({ fullScreen }) => {
    const isLoading = useSelector((state) => state.appUi.isLoading);
    // Optionally, you can pass a "fullScreen" prop to make the spinner cover the entire screen
    const spinnerClassName = fullScreen ? 'spinner full-screen' : 'spinner';

    return (
        <>
        { isLoading && <div className={spinnerClassName}>
            <div className="loader"></div>
            </div>}
        </>
    );
};

export default Spinner;
