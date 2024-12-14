import { useState, useEffect } from 'react';
import { Table, Input, Button, Alert } from '@/components/ui/table';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';

const CustomerObjectList = ({ customerId }) => {
    const [objects, setObjects] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [filter, setFilter] = useState('all'); // all, active, construction, finished

    useEffect(() => {
        if (customerId) {
            fetchCustomerObjects();
        }
    }, [customerId]);

    // Objektų užkrovimas iš API
    const fetchCustomerObjects = async () => {
        try {
            const response = await fetch(`http://localhost:5199/api/customerobjects/customer/${customerId}`);
            if (!response.ok) {
                throw new Error('Nepavyko gauti objektų sąrašo');
            }
            const data = await response.json();
            setObjects(data);
            setLoading(false);
        } catch (err) {
            setError(err.message);
            setLoading(false);
        }
    };

    // Objektų filtravimas pagal pavadinimą ir statusą
    const filteredObjects = objects.filter(object => {
        const matchesSearch = object.objectName.toLowerCase().includes(searchTerm.toLowerCase());
        const matchesFilter = filter === 'all' 
            || (filter === 'active' && object.status === 'Active')
            || (filter === 'construction' && object.projectPhase === 'Construction')
            || (filter === 'finished' && object.status === 'Finished');

        return matchesSearch && matchesFilter;
    });

    // Projekto fazės ženklinimo spalvos
    const getPhaseBadgeColor = (phase) => {
        switch (phase.toLowerCase()) {
            case 'planning': return 'bg-blue-500';
            case 'construction': return 'bg-yellow-500';
            case 'finished': return 'bg-green-500';
            case 'renovation': return 'bg-purple-500';
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
                <CardTitle>Kliento Objektai</CardTitle>
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
                            <option value="active">Aktyvūs</option>
                            <option value="construction">Statybos</option>
                            <option value="finished">Baigti</option>
                        </select>
                    </div>
                    <Button className="bg-blue-500 text-white">Naujas Objektas</Button>
                </div>
            </CardHeader>
            <CardContent>
                <div className="overflow-x-auto">
                    <Table>
                        <thead>
                            <tr className="bg-gray-100">
                                <th className="p-2">ID</th>
                                <th className="p-2">Pavadinimas</th>
                                <th className="p-2">Tipas</th>
                                <th className="p-2">Adresas</th>
                                <th className="p-2">Projekto Fazė</th>
                                <th className="p-2">Kontaktinis Asmuo</th>
                                <th className="p-2">Veiksmai</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredObjects.map(object => (
                                <tr key={object.id} className="border-b">
                                    <td className="p-2">{object.id}</td>
                                    <td className="p-2">{object.objectName}</td>
                                    <td className="p-2">{object.objectType}</td>
                                    <td className="p-2">{object.fullAddress}</td>
                                    <td className="p-2">
                                        <Badge className={getPhaseBadgeColor(object.projectPhase)}>
                                            {object.projectPhase}
                                        </Badge>
                                    </td>
                                    <td className="p-2">
                                        {object.contactPerson}
                                        <br />
                                        <span className="text-sm text-gray-500">
                                            {object.contactPhone}
                                        </span>
                                    </td>
                                    <td className="p-2">
                                        <Button variant="outline" size="sm" className="mr-2">
                                            Redaguoti
                                        </Button>
                                        <Button variant="outline" size="sm" className="mr-2">
                                            Rezervuoti Prekes
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

export default CustomerObjectList;
