// errorHandler.js
import { showLoginModal } from 'store/appUiSlice';
import  store  from 'store/store';

const ErrorHandler = {
    handle401: function () {
        store.dispatch(showLoginModal(true));
    },
    handle404: function () {
    },
    // ...other handlers
};

export default ErrorHandler;