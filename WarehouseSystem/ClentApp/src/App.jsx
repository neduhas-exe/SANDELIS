import { useState } from 'react';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card } from '@/components/ui/card';
import ProductList from './components/products/ProductList';
import CustomerList from './components/customers/CustomerList';
import CustomerObjectList from './components/customers/objects/CustomerObjectList';
import ProductReservation from './components/customers/objects/reservations/ProductReservation';
import ReservationHistory from './components/customers/objects/reservations/ReservationHistory';

const App = () => {
    const [selectedCustomerId, setSelectedCustomerId] = useState(null);
    const [selectedObjectId, setSelectedObjectId] = useState(null);
    const [showReservation, setShowReservation] = useState(false);
    const [showHistory, setShowHistory] = useState(false);

    // Kliento pasirinkimo handler
    const handleCustomerSelect = (customerId) => {
        setSelectedCustomerId(customerId);
        setSelectedObjectId(null);
        setShowReservation(false);
        setShowHistory(false);
    };

    // Objekto pasirinkimo handler
    const handleObjectSelect = (objectId) => {
        setSelectedObjectId(objectId);
    };

    return (
        <div className="container mx-auto p-4">
            <h1 className="text-2xl font-bold mb-6 text-center">
                Sandėlio Valdymo Sistema
            </h1>

            <Tabs defaultValue="products" className="w-full">
                <TabsList className="grid w-full grid-cols-2 lg:grid-cols-4">
                    <TabsTrigger value="products">Prekės</TabsTrigger>
                    <TabsTrigger value="customers">Klientai</TabsTrigger>
                    {selectedCustomerId && (
                        <TabsTrigger value="objects">Objektai</TabsTrigger>
                    )}
                    {selectedObjectId && (
                        <TabsTrigger value="reservations">Rezervacijos</TabsTrigger>
                    )}
                </TabsList>

                <TabsContent value="products">
                    <ProductList />
                </TabsContent>

                <TabsContent value="customers">
                    <CustomerList onCustomerSelect={handleCustomerSelect} />
                </TabsContent>

                {selectedCustomerId && (
                    <TabsContent value="objects">
                        <CustomerObjectList 
                            customerId={selectedCustomerId}
                            onObjectSelect={handleObjectSelect}
                        />
                    </TabsContent>
                )}

                {selectedObjectId && (
                    <TabsContent value="reservations">
                        <Card className="mb-4">
                            <div className="p-4 flex justify-end space-x-4">
                                <button
                                    onClick={() => setShowReservation(true)}
                                    className="bg-blue-500 text-white px-4 py-2 rounded"
                                >
                                    Nauja Rezervacija
                                </button>
                                <button
                                    onClick={() => setShowHistory(true)}
                                    className="bg-gray-500 text-white px-4 py-2 rounded"
                                >
                                    Rezervacijų Istorija
                                </button>
                            </div>
                        </Card>

                        {showReservation && (
                            <ProductReservation
                                customerId={selectedCustomerId}
                                objectId={selectedObjectId}
                                onClose={() => setShowReservation(false)}
                            />
                        )}

                        {showHistory && (
                            <ReservationHistory
                                objectId={selectedObjectId}
                            />
                        )}
                    </TabsContent>
                )}
            </Tabs>
        </div>
    );
};

export default App;
