import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.jsx'
import { AuthProvider } from './context/AuthContext'
import { ThemeProvider } from './context/ThemeContext'

/*
 * main.jsx is the entry point for the React app.
 *
 * What it does:
 * 1. Imports all the necessary modules
 * 2. Wraps the App with context providers:
 *    - AuthProvider: makes authentication state available to all components
 *    - ThemeProvider: makes theme state (dark/light) available to all components
 * 3. Renders the app into the DOM element with id "root"
 *
 * Why wrap with providers?
 * - Providers use React Context API to make state globally available
 * - Any component in the app can call useContext(AuthContext) or useContext(ThemeContext)
 * - Without providers, components couldn't access this state
 *
 * The nesting order matters: AuthProvider is inside ThemeProvider (but you could reverse it too)
 * Both need to be above App so their contexts are available to all routes and components.
 */

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AuthProvider>
      <ThemeProvider>
        <App />
      </ThemeProvider>
    </AuthProvider>
  </StrictMode>,
)
