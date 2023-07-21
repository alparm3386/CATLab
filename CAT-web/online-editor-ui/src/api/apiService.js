// apiService.js
import axios from 'axios';

export const loadJobData = async () => {
    let promise = new Promise(resolve => setTimeout(resolve, 2000));
    await promise;
    let loadJobData = {
        translationUnits: [
            { source: "Celestial Print Velour Sleepsuit and Hat Set", target: "Lot de combinaison et chapeau en velours à imprimé céleste" },
            { source: "This velour set may be the star of their cosy collection!", target: "Cet ensemble en velours est peut-être la star de leur collection cosy !" },
            { source: "Harry Potter™ Gryffindor Phone Case", target: "Coque de téléphone Harry Potter™ Gryffondor" },
            { source: "Put your house pride on full display with this case", target: "Mettez la fierté de votre maison à l'honneur avec cet étui" },
            { source: "Disney’s Minnie Mouse Rain Jacket", target: "Veste de pluie Disney Minnie Mouse" },
            { source: "Their rainy day adventures just got a lot more exciting with this hooded Disney raincoat!", target: "Their rainy day adventures just got a lot more exciting with this hooded Disney raincoat!" },
            { source: "Coats & Jackets", target: "Coats & Jackets" },
            { source: "Mickey, Minnie & Friends", target: "Mickey, Minnie & Friends" },
            { source: "SHELL-100% Polyester with Polyurethane Coating, LINING-100% Polyester", target: "EXTÉRIEUR : 100 % polyester enduit de polyuréthane, DOUBLURE : 100 % polyester" },
            { source: "Disney’s Mickey Mouse and Minnie Mouse Tweezer Set", target: "Disney’s Mickey Mouse and Minnie Mouse Tweezer Set" },
            { source: "Mickey Mouse, Minnie Mouse", target: "Mickey Mouse, Minnie Mouse" },
            { source: "A baby tee that makes a statement!", target: "A baby tee that makes a statement!" },
            { source: "Harry Potter, Ron Weasley, Hermione Granger", target: "Harry Potter, Ron Weasley, Hermione Granger" },
            { source: "80% Polyester, 17% Viscose, 3% Elastane", target: "80% Polyester, 17% Viscose, 3% Elastane" },
            { source: "Disney's Winnie The Pooh Phone Case", target: "Étui pour téléphone Winnie l'ourson de Disney" },
            { source: "Wrap your device in whimsy with this case", target: "Enveloppez votre appareil de fantaisie avec cet étui" },
    ]};
    return loadJobData;
};

export const getTMMatches = async (currentIdx) => {
    let promise = new Promise(resolve => setTimeout(resolve, 2000));
    await promise;
    let getTMMatches = [
        { source: "Celestial Print Velour Sleepsuit and Hat Set", target: "Lot de combinaison et chapeau en velours à imprimé céleste", quality: 101, origin: "TM" },
        { source: "This velour set may be the star of their cosy collection!", target: "Cet ensemble en velours est peut-être la star de leur collection cosy !", quality: 85, origin: "TM" },
        { source: "Harry Potter™ Gryffindor Phone Case", target: "Coque de téléphone Harry Potter™ Gryffondor", quality: 75, origin: "TM" },
        { source: "Put your house pride on full display with this case", target: "Mettez la fierté de votre maison à l'honneur avec cet étui", quality: 50, origin: "TM" },
    ];

    return getTMMatches;
};

const apiClient = axios.create({
    baseURL: 'http://api.yourserver.com',
    // You can put any default headers here, e.g. for authorization
});

export const fetchJobData = async () => {
    try {
        const response = await apiClient.get('/jobData');
        return response.data;
    } catch (error) {
        throw error;
    }
};

export const postJobData = async (jobData) => {
    try {
        const response = await apiClient.post('/jobData', jobData);
        return response.data;
    } catch (error) {
        throw error;
    }
};

// Add other API functions as needed...
