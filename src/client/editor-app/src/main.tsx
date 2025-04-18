import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { ErrorStoreProvider } from './context/ErrorStoreContext'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorStoreProvider>
      <App />
    </ErrorStoreProvider>
  </StrictMode>,
)
