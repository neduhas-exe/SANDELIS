const API_URL = 'http://localhost:5199/api';

// Bazinės užklausų funkcijos
const api = {
    get: async (endpoint) => {
        try {
            const response = await fetch(`${API_URL}${endpoint}`);
            if (!response.ok) {
                throw new Error(`HTTP klaida! statusas: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error('API klaida:', error);
            throw error;
        }
    },

    post: async (endpoint, data) => {
        try {
            const response = await fetch(`${API_URL}${endpoint}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data)
            });
            if (!response.ok) {
                throw new Error(`HTTP klaida! statusas: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error('API klaida:', error);
            throw error;
        }
    },

    put: async (endpoint, data) => {
        try {
            const response = await fetch(`${API_URL}${endpoint}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(data)
            });
            if (!response.ok) {
                throw new Error(`HTTP klaida! statusas: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            console.error('API klaida:', error);
            throw error;
        }
    },

    delete: async (endpoint) => {
        try {
            const response = await fetch(`${API_URL}${endpoint}`, {
                method: 'DELETE'
            });
            if (!response.ok) {
                throw new Error(`HTTP klaida! statusas: ${response.status}`);
            }
            return true;
        } catch (error) {
            console.error('API klaida:', error);
            throw error;
        }
    }
};

export default api;
