import { useState, useEffect } from 'react';
import { Table, Button, Alert } from '@/components/ui/table';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';

const ReservationHistory = ({ objectId }) => {
    const [reservations, setReservations] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [selectedReservation, setSelectedReservation] = useState(null);
    const [showDetails, setShowDetails] = useState(false);

    useEffect(() => {
        if (objectId) {
            fetchReservationHistory();
        }
    }, [objectId]);

    // Rezervacijų istorijos užkrovimas
    const fetchReservationHistory = async () => {
        try {
            const response = await fetch(`http://localhost:5199/api/reservations/object/${objectId}`);
            if (!response.ok) {
                throw new Error('Nepavyko gauti rezervacijų istorijos');
            }
            const data = await response.json();
            setReservations(data);
            setLoading(false);
        } catch (err) {
            setError(err.message);
            setLoading(false);
        }
    };

    // Rezervacijos būsenos spalvos
    const getStatusBadgeColor = (status) => {
        switch (status.toLowerCase()) {
            case 'active': return 'bg-green-500';
            case 'completed': return 'bg-blue-500';
            case 'cancelled': return 'bg-red-500';
            default: return 'bg-gray-500';
        }
    };

    // Datos formatavimas
    const formatDate = (dateString) => {
        return new Date(dateString).toLocaleString('lt-LT', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
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
                <CardTitle>Rezervacijų Istorija</CardTitle>
            </CardHeader>
            <CardContent>
                <div className="overflow-x-auto">
                    <Table>
                        <thead>
                            <tr className="bg-gray-100">
                                <th className="p-2">ID</th>
                                <th className="p-2">Data</th>
                                <th className="p-2">Prekių Kiekis</th>
                                <th className="p-2">Bendra Suma</th>
                                <th className="p-2">Būsena</th>
                                <th className="p-2">Sukūrė</th>
                                <th className="p-2">Veiksmai</th>
                            </tr>
                        </thead>
                        <tbody>
                            {reservations.map(reservation => (
                                <tr key={reservation.id} className="border-b">
                                    <td className="p-2">{reservation.id}</td>
                                    <td className="p-2">{formatDate(reservation.createdDate)}</td>
                                    <td className="p-2">{reservation.totalItems}</td>
                                    <td className="p-2">{reservation.totalAmount.toFixed(2)} €</td>
                                    <td className="p-2">
                                        <Badge className={getStatusBadgeColor(reservation.status)}>
                                            {reservation.status}
                                        </Badge>
                                    </td>
                                    <td className="p-2">{reservation.createdBy}</td>
                                    <td className="p-2">
                                        <Button 
                                            variant="outline" 
                                            size="sm"
                                            onClick={() => {
                                                setSelectedReservation(reservation);
                                                setShowDetails(true);
                                            }}
                                        >
                                            Detalės
                                        </Button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </div>

                {/* Rezervacijos detalių modalinis langas */}
                <Dialog open={showDetails} onOpenChange={setShowDetails}>
                    <DialogContent className="max-w-3xl">
                        <DialogTitle>Rezervacijos Detalės</DialogTitle>
                        {selectedReservation && (
                            <div>
                                <div className="grid grid-cols-2 gap-4 mb-4">
                                    <div>
                                        <p className="text-sm font-semibold">Rezervacijos ID</p>
                                        <p>{selectedReservation.id}</p>
                                    </div>
                                    <div>
                                        <p className="text-sm font-semibold">Data</p>
                                        <p>{formatDate(selectedReservation.createdDate)}</p>
                                    </div>
                                    <div>
                                        <p className="text-sm font-semibold">Būsena</p>
                                        <Badge className={getStatusBadgeColor(selectedReservation.status)}>
                                            {selectedReservation.status}
                                        </Badge>
                                    </div>
                                    <div>
                                        <p className="text-sm font-semibold">Sukūrė</p>
                                        <p>{selectedReservation.createdBy}</p>
                                    </div>
                                </div>

                                <Table>
                                    <thead>
                                        <tr className="bg-gray-100">
                                            <th className="p-2">Prekės Kodas</th>
                                            <th className="p-2">Pavadinimas</th>
                                            <th className="p-2">Kiekis</th>
                                            <th className="p-2">Kaina</th>
                                            <th className="p-2">Suma</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {selectedReservation.items?.map(item => (
                                            <tr key={item.productId} className="border-b">
                                                <td className="p-2">{item.productCode}</td>
                                                <td className="p-2">{item.productName}</td>
                                                <td className="p-2">{item.quantity}</td>
                                                <td className="p-2">{item.price.toFixed(2)} €</td>
                                                <td className="p-2">{(item.quantity * item.price).toFixed(2)} €</td>
                                            </tr>
                                        ))}
                                        <tr className="font-bold bg-gray-50">
                                            <td colSpan="4" className="p-2 text-right">Bendra suma:</td>
                                            <td className="p-2">{selectedReservation.totalAmount.toFixed(2)} €</td>
                                        </tr>
                                    </tbody>
                                </Table>

                                {selectedReservation.notes && (
                                    <div className="mt-4">
                                        <p className="text-sm font-semibold">Pastabos</p>
                                        <p className="mt-1">{selectedReservation.notes}</p>
                                    </div>
                                )}
                            </div>
                        )}
                    </DialogContent>
                </Dialog>
            </CardContent>
        </Card>
    );
};

export default ReservationHistory;
