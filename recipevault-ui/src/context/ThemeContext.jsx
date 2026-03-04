import { createContext, useState, useCallback, useEffect } from 'react';

/*
 * ThemeContext manages dark/light mode globally.
 *
 * What it does:
 * - Stores theme state as 'light' or 'dark'
 * - Provides toggle() function to switch between modes
 * - Updates the DOM by adding/removing the 'dark' class on document.documentElement
 * - Tailwind CSS uses this 'dark' class to apply dark mode styles
 * - Persists the user's choice to localStorage so it survives page reloads
 *
 * How Tailwind dark mode works:
 * - In tailwind.config.js, we set darkMode: 'class'
 * - This tells Tailwind to look for a 'dark' class on the html element
 * - Any class prefixed with 'dark:' only applies when that class exists
 * - Example: bg-white dark:bg-slate-900 = white in light mode, dark gray in dark mode
 *
 * Usage in a component:
 *   const { theme, toggle } = useContext(ThemeContext);
 *   <button onClick={toggle}>Toggle to {theme === 'light' ? 'dark' : 'light'}</button>
 */

export const ThemeContext = createContext();

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState('light');

  // On first load, check localStorage for saved preference
  useEffect(() => {
    const savedTheme = localStorage.getItem('theme') || 'light';
    setTheme(savedTheme);

    // Apply the theme to the DOM
    if (savedTheme === 'dark') {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, []);

  // Toggle theme and update both state and DOM
  const toggle = useCallback(() => {
    setTheme((prevTheme) => {
      const newTheme = prevTheme === 'light' ? 'dark' : 'light';

      // Update the DOM so Tailwind dark: styles apply
      if (newTheme === 'dark') {
        document.documentElement.classList.add('dark');
      } else {
        document.documentElement.classList.remove('dark');
      }

      // Save preference to localStorage
      localStorage.setItem('theme', newTheme);

      return newTheme;
    });
  }, []);

  const value = {
    theme,
    toggle,
    isDark: theme === 'dark',
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
}
