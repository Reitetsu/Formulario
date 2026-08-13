const isLocalDev = typeof window !== 'undefined' && 
  (window.location.port === '4200' || window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1');

export const API_BASE_URL = isLocalDev ? 'http://localhost:5105/api' : '/api';
