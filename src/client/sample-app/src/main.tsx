import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { WebSocketProvider } from './contexts/webSocketContext.tsx'
import { AppStateProvider } from './context-app/AppStateContext.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
     <WebSocketProvider url="ws://127.0.0.1:5000/ws?token=expected_token">
         <AppStateProvider>
            < App/>
        </AppStateProvider>
     </WebSocketProvider>
  </StrictMode>,
)
