import Cookies from 'js-cookie';

const cookieHelper = (function () {
    return {
        setToken: (token) => {
            Cookies.set('token', token, { secure: true, sameSite: 'Strict' });
        },

        getToken: () => {
            const token = Cookies.get('token');
            console.log(token);
            return token;
        },

        removeToken: () => {
            Cookies.remove('token');
        }
    };
})();

export default cookieHelper;
