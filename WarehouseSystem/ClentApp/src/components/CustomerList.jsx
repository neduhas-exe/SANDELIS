import { useState, useEffect } from 'react';
import { Table, Input, Button, Alert } from '@/components/ui/table';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

const CustomerList = () => {
    const [customers, setCustomers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filter, setFilter] = useState('all'); // all, company, private

    useEffect(() => {
        fetchCustomers();
    }, []);

    // Klientų užkrovimas iš API
    const fetchCustomers = async () => {
        try {
            const response = await fetch('http://localhost:5199/api/customers');
            if (!response.ok) {
                throw new Error('Nepavyko gauti klientų sąrašo');
            }
            const data = await response.json();
            setCustomers(data);
            setLoading(false);
        } catch (err) {
            setError(err.message);
            setLoading(false);
        }
    };

    // Klientų filtravimas pagal tipą ir pavadinimą/vardą
    const filteredCustomers = customers.filter(customer => {
        const matchesSearch = customer.isCompany
            ? customer.companyName.toLowerCase().includes(searchTerm.toLowerCase())
            : `${customer.firstName} ${customer.lastName}`.toLowerCase().includes(searchTerm.toLowerCase());

        const matchesFilter = filter === 'all' 
            || (filter === 'company' && customer.customerType === 'Company')
            || (filter === 'private' && customer.customerType === 'Private');

        return matchesSearch && matchesFilter;
    });

    // Kliento statuso ženklinimo spalvos
    const getStatusBadgeColor = (status) => {
        switch (status.toLowerCase()) {
            case 'active': return 'bg-green-500';
            case 'inactive': return 'bg-red-500';
            case 'pending': return 'bg-yellow-500';
            default: return 'bg-gray-500';
        }
    };

    if (loading) {
        return <div className="flex justify-center p-4">Kraunama...</div>;
    }

    if (error) {
        return (
            <Alert variant="destructive" className="m-4">
                <p>{error}</p>
            </Alert>
        );
    }

    return (
        <Card className="m-4">
            <CardHeader>
                <CardTitle>Klientų Sąrašas</CardTitle>
                <div className="flex flex-col sm:flex-row justify-between gap-4">
                    <div className="flex gap-2">
                        <Input
                            className="max-w-sm"
                            placeholder="Ieškoti pagal pavadinimą..."
                            value={searchTerm}
                            onChange={(e) => setSearchTerm(e.target.value)}
                        />
                        <select
                            className="border rounded p-2"
                            value={filter}
                            onChange={(e) => setFilter(e.target.value)}
                        >
                            <option value="all">Visi</option>
                            <option value="company">Įmonės</option>
                            <option value="private">Privatūs</option>
                        </select>
                    </div>
                    <Button className="bg-blue-500 text-white">Naujas Klientas</Button>
                </div>
            </CardHeader>
            <CardContent>
                <div className="overflow-x-auto">
                    <Table>
                        <thead>
                            <tr className="bg-gray-100">
                                <th className="p-2">ID</th>
                                <th className="p-2">Pavadinimas/Vardas</th>
                                <th className="p-2">Tipas</th>
                                <th className="p-2">El. paštas</th>
                                <th className="p-2">Telefonas</th>
                                <th className="p-2">Statusas</th>
                                <th className="p-2">Vadybininkas</th>
                                <th className="p-2">Veiksmai</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredCustomers.map(customer => (
                                <tr key={customer.id} className="border-b">
                                    <td className="p-2">{customer.id}</td>
                                    <td className="p-2">
                                        {customer.customerType === 'Company' 
                                            ? customer.companyName 
                                            : `${customer.firstName} ${customer.lastName}`}
                                    </td>
                                    <td className="p-2">
                                        {customer.customerType === 'Company' ? 'Įmonė' : 'Privatus'}
                                    </td>
                                    <td className="p-2">{customer.email}</td>
                                    <td className="p-2">{customer.phone}</td>
                                    <td className="p-2">
                                        <Badge className={getStatusBadgeColor(customer.customerStatus)}>
                                            {customer.customerStatus}
                                        </Badge>
                                    </td>
                                    <td className="p-2">{customer.assignedManagerName || '-'}</td>
                                    <td className="p-2">
                                        <Button variant="outline" size="sm" className="mr-2">
                                            Redaguoti
                                        </Button>
                                        <Button variant="outline" size="sm" className="mr-2">
                                            Objektai
                                        </Button>
                                        <Button variant="destructive" size="sm">
                                            Ištrinti
                                        </Button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </div>
            </CardContent>
        </Card>
    );
};

export default CustomerList;
