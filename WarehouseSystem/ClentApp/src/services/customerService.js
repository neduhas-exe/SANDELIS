import api from './api';

const customerService = {
    // Gauti visus klientus
    getAll: async () => {
        return await api.get('/customers');
    },

    // Gauti klientą pagal ID
    getById: async (id) => {
        return await api.get(`/customers/${id}`);
    },

    // Sukurti naują klientą
    create: async (customer) => {
        return await api.post('/customers', customer);
    },

    // Atnaujinti klientą
    update: async (id, customer) => {
        return await api.put(`/customers/${id}`, customer);
    },

    // Ištrinti klientą
    delete: async (id) => {
        return await api.delete(`/customers/${id}`);
    },

    // Gauti tik įmones
    getCompanies: async () => {
        return await api.get('/customers/companies');
    },

    // Gauti privačius klientus
    getPrivateCustomers: async () => {
        return await api.get('/customers/private');
    },

    // Gauti aktyvius klientus
    getActiveCustomers: async () => {
        return await api.get('/customers/active');
    },

    // Gauti klientus pagal vadybininką
    getByManager: async (managerId) => {
        return await api.get(`/customers/manager/${managerId}`);
    },

    // Gauti klientus, kuriems reikia susisiekti
    getNeedingContact: async () => {
        return await api.get('/customers/needs-contact');
    },

    // Atnaujinti kliento kontaktinę informaciją
    updateContact: async (id, contactData) => {
        return await api.put(`/customers/${id}/contact`, contactData);
    },

    // Priskirti vadybininką
    assignManager: async (customerId, managerId, managerName) => {
        return await api.put(`/customers/${customerId}/manager`, {
            managerId,
            managerName
        });
    },

    // Eksportuoti klientus į CSV
    exportToCsv: async () => {
        const response = await fetch(`${api.API_URL}/customers/export-csv`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            throw new Error('Nepavyko eksportuoti klientų');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `klientai_${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    },

    // Paieška pagal pavadinimą/vardą
    search: async (term) => {
        return await api.get(`/customers/search?term=${encodeURIComponent(term)}`);
    },

    // Patikrinti ar įmonės kodas unikalus
    checkCompanyCodeUnique: async (companyCode) => {
        return await api.get(`/customers/check-company-code/${companyCode}`);
    },

    // Patikrinti ar el. paštas unikalus
    checkEmailUnique: async (email) => {
        return await api.get(`/customers/check-email/${encodeURIComponent(email)}`);
    }
};

export default customerService;
