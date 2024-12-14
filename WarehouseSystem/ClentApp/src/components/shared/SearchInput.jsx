import { useState, useEffect } from 'react';
import { Search, X } from 'lucide-react';
import { Input } from '@/components/ui/input';

const SearchInput = ({
    onSearch,
    placeholder = 'Ieškoti...',
    delay = 500,
    minLength = 2
}) => {
    const [searchTerm, setSearchTerm] = useState('');
    const [isFocused, setIsFocused] = useState(false);

    // Debounce efektas paieškos užklausoms
    useEffect(() => {
        if (searchTerm.length >= minLength || searchTerm.length === 0) {
            const timer = setTimeout(() => {
                onSearch(searchTerm);
            }, delay);

            return () => clearTimeout(timer);
        }
    }, [searchTerm, delay, minLength, onSearch]);

    // Paieškos lauko išvalymas
    const handleClear = () => {
        setSearchTerm('');
        onSearch('');
    };

    return (
        <div className="relative">
            <div className={`
                relative flex items-center transition-all
                ${isFocused ? 'ring-2 ring-blue-500 rounded-lg' : ''}
            `}>
                <Search className="absolute left-3 h-4 w-4 text-gray-400" />
                <Input
                    type="text"
                    className="pl-9 pr-8"
                    placeholder={placeholder}
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    onFocus={() => setIsFocused(true)}
                    onBlur={() => setIsFocused(false)}
                />
                {searchTerm && (
                    <button
                        onClick={handleClear}
                        className="absolute right-3 text-gray-400 hover:text-gray-600"
                    >
                        <X className="h-4 w-4" />
                    </button>
                )}
            </div>
            {searchTerm.length > 0 && searchTerm.length < minLength && (
                <p className="text-xs text-gray-500 mt-1">
                    Įveskite bent {minLength} simbolius paieškai pradėti
                </p>
            )}
        </div>
    );
};

export default SearchInput;
