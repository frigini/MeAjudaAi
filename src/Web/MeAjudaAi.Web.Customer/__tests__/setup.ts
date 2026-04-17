import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { TextEncoder, TextDecoder } from 'util';

global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder as any;

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: Record<string, unknown> | string) => {
      if (typeof options === 'string') return options;
      
      let result = key;
      if (typeof options === 'object' && options !== null) {
        if (options.count !== undefined) {
          // Simple pluralization mock: count takes precedence over defaultValue
          result = `${key}_${options.count === 1 ? 'one' : 'other'}`;
        } else if (options.defaultValue) {
          return options.defaultValue as string;
        }

        // Simple interpolation mock
        Object.keys(options).forEach((optKey) => {
          if (optKey !== 'count' && optKey !== 'defaultValue') {
            result = result.replace(`{{${optKey}}}`, String(options[optKey]));
          }
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

if (!global.PointerEvent) {
  class PointerEvent extends MouseEvent {
    constructor(type: string, props: any = {}) {
      super(type, props);
      this.pointerId = props.pointerId || 0;
      this.pointerType = props.pointerType || '';
    }
    public pointerId: number;
    public pointerType: string;
  }
  global.PointerEvent = PointerEvent as any;
}

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

