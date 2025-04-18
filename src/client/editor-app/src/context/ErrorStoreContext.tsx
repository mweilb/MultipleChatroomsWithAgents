import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react';

type ErrorStore = {
  errorHistory: { [room: string]: any[] };
  yamlErrorCount: { [room: string]: number };
  handleErrorMessage: (msg: any) => void;
  setYamlErrorCount: (room: string, count: number) => void;
  resetErrors: (room: string) => void;
};

const ErrorStoreContext = createContext<ErrorStore | undefined>(undefined);

export const ErrorStoreProvider = ({ children }: { children: ReactNode }) => {
  const [errorHistory, setErrorHistory] = useState<{ [room: string]: any[] }>({});
  const [yamlErrorCount, setYamlErrorCountState] = useState<{ [room: string]: number }>({});

  const handleErrorMessage = useCallback((msg: any) => {
    const room = msg.SubAction || 'all';
    setErrorHistory(prev => {
      const existing = prev[room] || [];
      return { ...prev, [room]: [...existing, msg] };
    });
    console.error('Error message:', msg);
  }, []);

  const setYamlErrorCount = useCallback((room: string, count: number) => {
    setYamlErrorCountState(prev => ({ ...prev, [room]: count }));
  }, []);

  const resetErrors = useCallback((room: string) => {
    setErrorHistory(prev => ({ ...prev, [room]: [] }));
    setYamlErrorCountState(prev => ({ ...prev, [room]: 0 }));
  }, []);

  return (
    <ErrorStoreContext.Provider value={{ errorHistory, yamlErrorCount, handleErrorMessage, setYamlErrorCount, resetErrors }}>
      {children}
    </ErrorStoreContext.Provider>
  );
};

export const useErrorStoreContext = () => {
  const context = useContext(ErrorStoreContext);
  if (!context) {
    throw new Error('useErrorStoreContext must be used within an ErrorStoreProvider');
  }
  return context;
};
