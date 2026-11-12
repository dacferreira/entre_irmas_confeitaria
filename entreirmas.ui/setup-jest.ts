import { setupZoneTestEnv } from 'jest-preset-angular/setup-env/zone';

setupZoneTestEnv();

// jsdom não implementa estas APIs usadas pelo layout.
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string): MediaQueryList =>
    ({
      matches: false,
      media: query,
      onchange: null,
      addListener: jest.fn(),
      removeListener: jest.fn(),
      addEventListener: jest.fn(),
      removeEventListener: jest.fn(),
      dispatchEvent: jest.fn(),
    }) as unknown as MediaQueryList,
});

window.scrollTo = jest.fn() as unknown as typeof window.scrollTo;
