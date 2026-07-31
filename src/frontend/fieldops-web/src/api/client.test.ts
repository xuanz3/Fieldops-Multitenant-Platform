import {
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  ApiError,
  apiRequest,
} from './client'

describe('apiRequest', () => {
  it('adds the bearer token and parses JSON', async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(
        JSON.stringify({
          value: 42,
        }),
        {
          status: 200,
          headers: {
            'Content-Type':
              'application/json',
          },
        },
      ),
    )

    vi.stubGlobal('fetch', fetchMock)

    const result = await apiRequest<{
      value: number
    }>('/api/test', {
      token: 'signed-token',
    })

    expect(result.value).toBe(42)

    const request =
      fetchMock.mock.calls[0][1] as RequestInit

    const headers = new Headers(
      request.headers,
    )

    expect(
      headers.get('Authorization'),
    ).toBe('Bearer signed-token')
  })

  it('surfaces validation errors', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            title:
              'One or more validation errors occurred.',
            errors: {
              name: ['Name is required.'],
            },
          }),
          {
            status: 400,
            headers: {
              'Content-Type':
                'application/json',
            },
          },
        ),
      ),
    )

    await expect(
      apiRequest('/api/customers', {
        method: 'POST',
        body: {},
      }),
    ).rejects.toMatchObject({
      status: 400,
      fieldErrors: {
        name: ['Name is required.'],
      },
    } satisfies Partial<ApiError>)
  })

  it('uses a clear default conflict message', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        new Response('', {
          status: 409,
        }),
      ),
    )

    await expect(
      apiRequest('/api/work-orders/1'),
    ).rejects.toThrow(
      'The record changed or conflicts with an existing record.',
    )
  })
})
