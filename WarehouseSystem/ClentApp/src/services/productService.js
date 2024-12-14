import api from './api';

const productService = {
    // Gauti visas prekes
    getAll: async () => {
        return await api.get('/products');
    },

    // Gauti prekę pagal ID
    getById: async (id) => {
        return await api.get(`/products/${id}`);
    },

    // Sukurti naują prekę
    create: async (product) => {
        return await api.post('/products', product);
    },

    // Atnaujinti prekę
    update: async (id, product) => {
        return await api.put(`/products/${id}`, product);
    },

    // Ištrinti prekę
    delete: async (id) => {
        return await api.delete(`/products/${id}`);
    },

    // Gauti prekes kurioms reikia papildymo
    getNeedsRestock: async () => {
        return await api.get('/products/needs-restock');
    },

    // Eksportuoti prekes į CSV
    exportToCsv: async () => {
        const response = await fetch(`${api.API_URL}/products/export-csv`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
            },
        });

        if (!response.ok) {
            throw new Error('Nepavyko eksportuoti prekių');
        }

        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `prekes_${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        window.URL.revokeObjectURL(url);
    },

    // Paieška pagal pavadinimą
    search: async (term) => {
        return await api.get(`/products/search?term=${encodeURIComponent(term)}`);
    },

    // Filtruoti pagal kategoriją
    getByCategory: async (category) => {
        return await api.get(`/products/category/${encodeURIComponent(category)}`);
    },

    // Gauti prekių kategorijų sąrašą
    getCategories: async () => {
        return await api.get('/products/categories');
    }
};

export default productService;
