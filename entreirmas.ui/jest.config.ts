import type { Config } from 'jest';

const config: Config = {
  preset: 'jest-preset-angular',
  setupFilesAfterEnv: ['<rootDir>/setup-jest.ts'],
  testMatch: ['<rootDir>/src/**/*.spec.ts'],
  testEnvironment: 'jsdom',
  clearMocks: true,
  restoreMocks: true,
  collectCoverageFrom: [
    'src/app/**/*.ts',
    '!src/app/**/*.spec.ts',
    '!src/app/**/*.module.ts',
    '!src/app/**/*.model.ts',
  ],
  coverageReporters: ['html', 'text', 'lcov', 'json', 'cobertura'],
  coverageThreshold: { global: { branches: 70, functions: 70, lines: 70, statements: 70 } },
  moduleNameMapper: {
    '^@components/(.*)$': '<rootDir>/src/app/shared/components/$1',
    '^@models/(.*)$': '<rootDir>/src/app/shared/models/$1',
    '^@services/(.*)$': '<rootDir>/src/app/shared/services/$1',
    '^@pipes/(.*)$': '<rootDir>/src/app/shared/pipes/$1',
    '^@directives/(.*)$': '<rootDir>/src/app/shared/directives/$1',
    '^@guards/(.*)$': '<rootDir>/src/app/shared/guards/$1',
    '^@interceptors/(.*)$': '<rootDir>/src/app/shared/interceptors/$1',
    '^@constants/(.*)$': '<rootDir>/src/app/shared/constants/$1',
    '^@validators/(.*)$': '<rootDir>/src/app/shared/validators/$1',
    '^@environments/(.*)$': '<rootDir>/src/environments/$1',
    '^@testing/(.*)$': '<rootDir>/src/testing/$1',
    '^@inicio/(.*)$': '<rootDir>/src/app/pages/inicio/$1',
    '^@cardapio/(.*)$': '<rootDir>/src/app/pages/cardapio/$1',
    '^@produto/(.*)$': '<rootDir>/src/app/pages/produto/$1',
    '^@finalizar-pedido/(.*)$': '<rootDir>/src/app/pages/finalizar-pedido/$1',
    '^@pedido-confirmado/(.*)$': '<rootDir>/src/app/pages/pedido-confirmado/$1',
    '^@acompanhar-pedido/(.*)$': '<rootDir>/src/app/pages/acompanhar-pedido/$1',
    '^@nao-encontrado/(.*)$': '<rootDir>/src/app/pages/nao-encontrado/$1',
    '^@admin/(.*)$': '<rootDir>/src/app/pages/admin/$1',
  },
};

export default config;
