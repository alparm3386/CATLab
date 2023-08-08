import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { showAlert } from 'store/appUiSlice';

const Alert = () => {
    // Access the appUi state from the Redux store
    const alertState = useSelector(state => state.appUi.alert);

    // We'll assume that the alert will be shown when the message is not empty
    const isAlertVisible = !!alertState.message;

    if (!isAlertVisible) {
        return null; // Do not render anything if the alert shouldn't be visible
    }

    return (
        <div style={{ position: 'absolute', top: '10%', left: '50%', transform: 'translateX(-50%)', padding: '20px', border: '1px solid black', backgroundColor: 'white' }}>
            <h2>{alertState.title}</h2>
            <p>{alertState.message}</p>
            <button onClick={() => {
                // Clear the alert by dispatching showAlert with an empty message and title
                // This assumes that an empty message will hide the alert; you can adjust this behavior as needed
                useDispatch(showAlert({ title: "", message: "" }));
            }}>
                Close
            </button>
        </div>
    );
};

export default Alert;
