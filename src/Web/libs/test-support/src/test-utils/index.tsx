import React, { ReactElement, useMemo } from 'react';
import { render, RenderOptions, renderHook, RenderHookOptions, RenderHookResult } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';

function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: Infinity,
      },
      mutations: {
        retry: false,
      },
    },
  });
}

interface AllTheProvidersProps {
  children: React.ReactNode;
}

const AllTheProviders = ({ children }: AllTheProvidersProps) => {
  const queryClient = useMemo(() => createTestQueryClient(), []);
  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
};

const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => render(ui, { wrapper: AllTheProviders, ...options });

const AllTheProvidersWrapper = ({ children }: { children: React.ReactNode }) => (
  <AllTheProviders>{children}</AllTheProviders>
);

function customRenderHook<TProps, TValue>(
  callback: (props: TProps) => TValue,
  options?: Omit<RenderHookOptions<TProps>, 'wrapper'>
): RenderHookResult<TValue, TProps> {
  return renderHook(callback, { wrapper: AllTheProvidersWrapper, ...options });
}

export * from '@testing-library/react';
export { customRender as render };
export { customRenderHook as renderHook };
export { createTestQueryClient };
export { AllTheProvidersWrapper };
