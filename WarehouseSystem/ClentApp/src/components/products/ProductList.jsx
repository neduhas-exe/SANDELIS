import { useState, useEffect } from 'react';
import { Table, Input, Button, Alert } from '@/components/ui/table';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

const ProductList = () => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');

  useEffect(() => {
    fetchProducts();
  }, []);

  // Produktų užkrovimas iš API
  const fetchProducts = async () => {
    try {
      const response = await fetch('http://localhost:5199/api/products');
      if (!response.ok) {
        throw new Error('Nepavyko gauti produktų sąrašo');
      }
      const data = await response.json();
      setProducts(data);
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
        <CardTitle>Produktų Sąrašas</CardTitle>
        <div className="flex justify-between items-center">
          <Input
            className="max-w-sm"
            placeholder="Ieškoti pagal pavadinimą..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <Button className="bg-blue-500 text-white">Naujas Produktas</Button>
        </div>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <Table>
            <thead>
              <tr className="bg-gray-100">
                <th className="p-2">ID</th>
                <th className="p-2">Pavadinimas</th>
                <th className="p-2">Barkodas</th>
                <th className="p-2">Kategorija</th>
                <th className="p-2">Kaina</th>
                <th className="p-2">Kiekis</th>
                <th className="p-2">Veiksmai</th>
              </tr>
            </thead>
            <tbody>
              {filteredProducts.map(product => (
                <tr key={product.id} className="border-b">
                  <td className="p-2">{product.id}</td>
                  <td className="p-2">{product.name}</td>
                  <td className="p-2">{product.barcode}</td>
                  <td className="p-2">{product.category}</td>
                  <td className="p-2">{product.price.toFixed(2)} €</td>
                  <td className="p-2">{product.stockQuantity}</td>
                  <td className="p-2">
                    <Button variant="outline" size="sm" className="mr-2">
                      Redaguoti
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

export default ProductList;
