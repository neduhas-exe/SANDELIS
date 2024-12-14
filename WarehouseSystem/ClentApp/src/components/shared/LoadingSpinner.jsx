import { Loader2 } from 'lucide-react';

const LoadingSpinner = ({ size = 'default', text = 'Kraunama...' }) => {
    const sizeClasses = {
        small: 'w-4 h-4',
        default: 'w-8 h-8',
        large: 'w-12 h-12'
    };

    return (
        <div className="flex flex-col items-center justify-center p-4 space-y-2">
            <Loader2 className={`animate-spin ${sizeClasses[size]}`} />
            {text && <p className="text-sm text-gray-500">{text}</p>}
        </div>
    );
};

export default LoadingSpinner;
