import { useState, useEffect } from 'react';
import { Table, Input, Button, Alert } from '@/components/ui/table';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogTitle, DialogContent } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';

const ProductReservation = ({ customerId, objectId, onClose }) => {
    const [products, setProducts] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchTerm, setSearchTerm] = useState('');
    const [selectedProducts, setSelectedProducts] = useState(new Map());
    const [showConfirmation, setShowConfirmation] = useState(false);

    useEffect(() => {
        fetchAvailableProducts();
    }, []);

    // Galimų produktų užkrovimas iš API
    const fetchAvailableProducts = async () => {
        try {
            const response = await fetch('http://localhost:5199/api/products');
            if (!response.ok) {
                throw new Error('Nepavyko gauti produktų sąrašo');
            }
            const data = await response.json();
            // Filtruojame tik produktus su pakankamu kiekiu
            setProducts(data.filter(p => p.stockQuantity > 0));
            setLoading(false);
        } catch (err) {
            setError(err.message);
            setLoading(false);
        }
    };

    // Produktų filtravimas pagal pavadinimą
    const filteredProducts = products.filter(product =>
        product.name.toLowerCase().includes(searchTerm.toLowerCase())
    );

    // Produkto kiekio atnaujinimas
    const handleQuantityChange = (productId, quantity) => {
        const product = products.find(p => p.id === productId);
        if (!product) return;

        const newQuantity = Math.min(Math.max(0, quantity), product.stockQuantity);
        const updatedSelected = new Map(selectedProducts);
        
        if (newQuantity === 0) {
            updatedSelected.delete(productId);
        } else {
            updatedSelected.set(productId, newQuantity);
        }
        
        setSelectedProducts(updatedSelected);
    };

    // Rezervacijos išsaugojimas
    const handleSaveReservation = async () => {
        try {
            const reservationData = {
                customerId,
                objectId,
                products: Array.from(selectedProducts, ([productId, quantity]) => ({
                    productId,
                    quantity
                })),
                reservationDate: new Date().toISOString()
            };

            const response = await fetch('http://localhost:5199/api/reservations', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(reservationData)
            });

            if (!response.ok) {
                throw new Error('Nepavyko išsaugoti rezervacijos');
            }

            setShowConfirmation(true);
            setTimeout(() => {
                onClose();
            }, 2000);
        } catch (err) {
            setError(err.message);
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
                <CardTitle>Prekių Rezervavimas Objektui</CardTitle>
                <div className="flex justify-between items-center">
                    <Input
                        className="max-w-sm"
                        placeholder="Ieškoti prekių..."
                        value={searchTerm}
                        onChange={(e) => setSearchTerm(e.target.value)}
                    />
                </div>
            </CardHeader>
            <CardContent>
                <div className="overflow-x-auto">
                    <Table>
                        <thead>
                            <tr className="bg-gray-100">
                                <th className="p-2">Kodas</th>
                                <th className="p-2">Pavadinimas</th>
                                <th className="p-2">Kaina</th>
                                <th className="p-2">Sandėlyje</th>
                                <th className="p-2">Rezervuoti</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredProducts.map(product => (
                                <tr key={product.id} className="border-b">
                                    <td className="p-2">{product.barcode}</td>
                                    <td className="p-2">{product.name}</td>
                                    <td className="p-2">{product.price.toFixed(2)} €</td>
                                    <td className="p-2">
                                        <Badge className={product.stockQuantity < product.minimumQuantity ? 'bg-yellow-500' : 'bg-green-500'}>
                                            {product.stockQuantity}
                                        </Badge>
                                    </td>
                                    <td className="p-2">
                                        <div className="flex items-center gap-2">
                                            <Input
                                                type="number"
                                                min="0"
                                                max={product.stockQuantity}
                                                value={selectedProducts.get(product.id) || ''}
                                                onChange={(e) => handleQuantityChange(product.id, parseInt(e.target.value) || 0)}
                                                className="w-24"
                                            />
                                            <Label>vnt.</Label>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </div>

                <div className="flex justify-between mt-4">
                    <div className="text-sm">
                        Pasirinkta prekių: {selectedProducts.size}
                    </div>
                    <div className="flex gap-2">
                        <Button variant="outline" onClick={onClose}>
                            Atšaukti
                        </Button>
                        <Button 
                            className="bg-blue-500 text-white"
                            disabled={selectedProducts.size === 0}
                            onClick={handleSaveReservation}
                        >
                            Rezervuoti Prekes
                        </Button>
                    </div>
                </div>
            </CardContent>

            {/* Patvirtinimo dialogas */}
            <Dialog open={showConfirmation} onOpenChange={setShowConfirmation}>
                <DialogContent>
                    <DialogTitle>Rezervacija Sėkminga</DialogTitle>
                    <p>Prekės sėkmingai rezervuotos objektui.</p>
                </DialogContent>
            </Dialog>
        </Card>
    );
};

export default ProductReservation;
