import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';
import { Button } from '@/components/ui/button';

const Pagination = ({
    currentPage = 1,
    totalPages = 1,
    onPageChange,
    itemsPerPage = 10,
    totalItems = 0
}) => {
    // Puslapių navigacijos mygtukai
    const renderPageButtons = () => {
        const buttons = [];
        const maxButtons = 5; // Maksimalus rodomų mygtukų skaičius
        
        let startPage = Math.max(1, currentPage - Math.floor(maxButtons / 2));
        let endPage = Math.min(totalPages, startPage + maxButtons - 1);

        if (endPage - startPage + 1 < maxButtons) {
            startPage = Math.max(1, endPage - maxButtons + 1);
        }

        // Pirmas puslapis
        if (startPage > 1) {
            buttons.push(
                <Button
                    key="first"
                    variant="outline"
                    size="sm"
                    onClick={() => onPageChange(1)}
                >
                    <ChevronsLeft className="h-4 w-4" />
                </Button>
            );
        }

        // Ankstesnis puslapis
        if (currentPage > 1) {
            buttons.push(
                <Button
                    key="prev"
                    variant="outline"
                    size="sm"
                    onClick={() => onPageChange(currentPage - 1)}
                >
                    <ChevronLeft className="h-4 w-4" />
                </Button>
            );
        }

        // Puslapių numeriai
        for (let i = startPage; i <= endPage; i++) {
            buttons.push(
                <Button
                    key={i}
                    variant={i === currentPage ? "default" : "outline"}
                    size="sm"
                    onClick={() => onPageChange(i)}
                >
                    {i}
                </Button>
            );
        }

        // Sekantis puslapis
        if (currentPage < totalPages) {
            buttons.push(
                <Button
                    key="next"
                    variant="outline"
                    size="sm"
                    onClick={() => onPageChange(currentPage + 1)}
                >
                    <ChevronRight className="h-4 w-4" />
                </Button>
            );
        }

        // Paskutinis puslapis
        if (endPage < totalPages) {
            buttons.push(
                <Button
                    key="last"
                    variant="outline"
                    size="sm"
                    onClick={() => onPageChange(totalPages)}
                >
                    <ChevronsRight className="h-4 w-4" />
                </Button>
            );
        }

        return buttons;
    };

    return (
        <div className="flex flex-col sm:flex-row justify-between items-center gap-4 mt-4">
            <div className="text-sm text-gray-500">
                Rodoma {Math.min((currentPage - 1) * itemsPerPage + 1, totalItems)} - {Math.min(currentPage * itemsPerPage, totalItems)} iš {totalItems} įrašų
            </div>
            <div className="flex gap-2">
                {renderPageButtons()}
            </div>
        </div>
    );
};

export default Pagination;
