import Cookies from 'js-cookie';

const cookieHelper = (function () {
    return {
        setToken: () => {
            const token = 'YourJWTToken';
            Cookies.set('token', token, { secure: true, sameSite: 'Strict' });
        },

        getToken: () => {
            const token = Cookies.get('token');
            console.log(token);
        },

        removeToken: () => {
            Cookies.remove('token');
        }
    };
})();

export default cookieHelper;
