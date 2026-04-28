import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { TextEncoder, TextDecoder } from 'util';

global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder as any;

import ptTranslations from '../public/locales/pt/common.json';

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: Record<string, unknown> | string) => {
      // Treat string options as defaultValue and try to resolve the key normally
      if (typeof options === 'string') {
        options = { defaultValue: options };
      }
      
      const keys = key.split('.');
      let translation: any = ptTranslations;
      for (const k of keys) {
        translation = translation?.[k];
        if (!translation) break;
      }

      let result = typeof translation === 'string' ? translation : key;

      if (typeof options === 'object' && options !== null) {
        if (options.count !== undefined) {
          const suffix = options.count === 1 ? 'one' : 'other';
          const baseKey = keys[keys.length - 1];
          const parentKeys = keys.slice(0, -1);
          let parentTranslation: any = ptTranslations;
          for (const pk of parentKeys) {
            parentTranslation = parentTranslation?.[pk];
            if (!parentTranslation) break;
          }
          
          const pluralKey = `${baseKey}_${suffix}`;
          if (parentTranslation?.[pluralKey]) {
            result = parentTranslation[pluralKey];
          }
        }

        // Use defaultValue only if key resolution found nothing
        if (options.defaultValue !== undefined && result === key) {
          return options.defaultValue as string;
        }

        Object.keys(options).forEach((optKey) => {
          result = result.replace(`{{${optKey}}}`, String(options[optKey]));
        });
      }
      return result;
    },
    i18n: {
      changeLanguage: () => Promise.resolve(),
      language: 'pt',
    },
  }),
  initReactI18next: {
    type: '3rdParty',
    init: () => {},
  },
  I18nextProvider: ({ children }: any) => children,
  Trans: ({ children }: any) => children,
}));

afterEach(() => {
  cleanup();
});

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }),
});

global.IntersectionObserver = class IntersectionObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  takeRecords() {
    return [];
  }
  unobserve() {}
} as any;

global.ResizeObserver = class ResizeObserver {
  constructor() {}
  disconnect() {}
  observe() {}
  unobserve() {}
} as any;
