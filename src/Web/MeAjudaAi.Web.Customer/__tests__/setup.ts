import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { TextEncoder, TextDecoder } from 'util';

global.TextEncoder = TextEncoder;
global.TextDecoder = TextDecoder as any;

// Mock i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string, options?: any) => {
      if (typeof options === 'string') return options;
      if (typeof options === 'object' && options?.defaultValue) return options.defaultValue;
      
      let result = key;
      if (typeof options === 'object' && options?.count !== undefined) {
        // Simple pluralization mock
        result = `${key}_${options.count === 1 ? 'one' : 'other'}`;
      }

      // Simple interpolation mock
      if (typeof options === 'object') {
        Object.keys(options).forEach((optKey) => {
          result = result.replace(`{{${optKey}}}`, options[optKey]);
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

