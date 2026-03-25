import { http, HttpResponse } from 'msw';

export const handlers = [
  // Provider profile/auth endpoints
  http.get('/api/providers/me', () =>
    HttpResponse.json({
      data: {
        id: 'provider-1',
        name: 'Prestador Teste',
        email: 'prestador@exemplo.com',
        isOnline: true,
        phones: ['(32) 99999-0000'],
        rating: 4.5,
        verificationStatus: 3,
      },
    })
  ),
  http.put('/api/providers/me', () => HttpResponse.json({ data: {} })),
];
