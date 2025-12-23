import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.tsx'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import ApiKeysPage from './pages/ApiKeysPage.tsx'
import TelemetryPage from './pages/TelemetryPage.tsx'
import { CssBaseline } from '@mui/material'
import NavBar from './components/NavBar.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <CssBaseline />
      <NavBar />
      <Routes>
        <Route path="/" element={<App />} />
        <Route path="api-keys" element={<ApiKeysPage />} />
        <Route path='telemetry' element={<TelemetryPage />} />
      </Routes>
    </BrowserRouter>
  </StrictMode>,
)
