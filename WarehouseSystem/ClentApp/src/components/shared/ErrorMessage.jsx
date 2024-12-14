import { AlertTriangle, XCircle, Info } from 'lucide-react';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';

const ErrorMessage = ({ 
    title = 'Klaida', 
    message, 
    type = 'error',
    onClose 
}) => {
    const icons = {
        error: <XCircle className="h-5 w-5" />,
        warning: <AlertTriangle className="h-5 w-5" />,
        info: <Info className="h-5 w-5" />
    };

    const variants = {
        error: 'destructive',
        warning: 'warning',
        info: 'info'
    };

    return (
        <Alert variant={variants[type]} className="mb-4">
            {icons[type]}
            <AlertTitle>{title}</AlertTitle>
            <AlertDescription>{message}</AlertDescription>
            {onClose && (
                <button
                    onClick={onClose}
                    className="absolute top-2 right-2 text-gray-500 hover:text-gray-700"
                >
                    <XCircle className="h-4 w-4" />
                </button>
            )}
        </Alert>
    );
};

export default ErrorMessage;
