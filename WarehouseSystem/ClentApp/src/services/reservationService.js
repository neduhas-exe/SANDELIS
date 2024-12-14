import api from './api';

const reservationService = {
    // Gauti visas objekto rezervacijas
    getByObject: async (objectId) => {
        return await api.get(`/reservations/object/${objectId}`);
    },

    // Gauti rezervaciją pagal ID
    getById: async (id) => {
        return await api.get(`/reservations/${id}`);
    },

    // Sukurti naują rezervaciją
    create: async (reservation) => {
        return await api.post('/reservations', reservation);
    },

    // Atnaujinti rezervaciją
    update: async (id, reservation) => {
        return await api.put(`/reservations/${id}`, reservation);
    },

    // Atšaukti rezervaciją
    cancel: async (id, reason) => {
        return await api.put(`/reservations/${id}/cancel`, { reason });
    },

    // Gauti aktyvias rezervacijas
    getActive: async () => {
        return await api.get('/reservations/active');
    },

    // Gauti kliento rezervacijas
    getByCustomer: async (customerId) => {
        return await api.get(`/reservations/customer/${customerId}`);
    },

    // Gauti rezervacijas pagal datą
    getByDate: async (date) => {
        return await api.get(`/reservations/date/${date}`);
    },

    // Gauti rezervacijas pagal laikotarpį
    getByDateRange: async (startDate, endDate) => {
        return await api.get(`/reservations/range?start=${startDate}&end=${endDate}`);
    },

    // Eksportuoti rezervacijas į CSV
    exportToCsv: async (objectId) => {
        const response = await fetch(`${api.API_URL}/reservations/export-csv/${objectId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            throw new Error('Nepavyko eksportuoti rezervacijų');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `rezervacijos_${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    },

    // Gauti statistiką pagal laikotarpį
    getStatistics: async (startDate, endDate) => {
        return await api.get(`/reservations/statistics?start=${startDate}&end=${endDate}`);
    },

    // Patikrinti ar prekė galima rezervuoti
    checkProductAvailability: async (productId, quantity, date) => {
        return await api.get(`/reservations/check-availability/${productId}?quantity=${quantity}&date=${date}`);
    },

    // Gauti artėjančias rezervacijas
    getUpcoming: async () => {
        return await api.get('/reservations/upcoming');
    },

    // Pridėti pastabą prie rezervacijos
    addNote: async (id, note) => {
        return await api.post(`/reservations/${id}/notes`, { note });
    },

    // Gauti rezervacijų istoriją
    getHistory: async (objectId) => {
        return await api.get(`/reservations/history/${objectId}`);
    }
};

export default reservationService;
