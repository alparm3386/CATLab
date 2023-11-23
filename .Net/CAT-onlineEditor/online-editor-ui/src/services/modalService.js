// modalService.js
import { showAlert, showConfirm } from 'store/appUiSlice';

const modalService = (() => {
    const service = {
        dispatchFunction: {}, // Store the dispatch function

        initialize: function (dispatch) {
            this.dispatchFunction = dispatch; // Store the dispatch function during initialization
        },

        showAlert: function (title, message) {
            return new Promise((resolve) => {
                this.dispatchFunction(
                    showAlert({ title: title, message: message, callback: resolve })
                );
            });
        },

        showConfirm: function (title, message) {
            return new Promise((resolve) => {
                this.dispatchFunction(
                    showConfirm({ title: title, message: message, callback: resolve })
                );
            });
        },

        // Add more application-wide functions here
    };

    return service;
})();

export default modalService;
