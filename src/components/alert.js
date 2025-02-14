import React from 'react';

export const Alert = ({ children, variant = "default" }) => {
  const styles = {
    default: "bg-gray-100 border-gray-300 text-gray-900",
    destructive: "bg-red-100 border-red-400 text-red-700",
  };

  return (
    <div className={`p-4 border rounded ${styles[variant]}`}>
      {children}
    </div>
  );
};

export const AlertDescription = ({ children }) => {
  return <p className="text-sm">{children}</p>;
};
