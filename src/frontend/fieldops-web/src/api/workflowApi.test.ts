import {
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  approveWorkOrder,
  assignWorkOrder,
} from './fieldOpsApi'

describe('workflow API client', () => {
  it('sends assignment version and Technician identity', async () => {
    const fetchMock =
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            id: 'work-order-1',
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

    await assignWorkOrder(
      'token',
      'work-order-1',
      'technician-1',
      3,
    )

    const [
      url,
      request,
    ] = fetchMock.mock.calls[0]

    expect(url).toBe(
      '/api/workflow/work-orders/work-order-1/assign',
    )

    expect(
      JSON.parse(
        String(
          (request as RequestInit).body,
        ),
      ),
    ).toEqual({
      technicianUserId:
        'technician-1',
      version: 3,
    })
  })

  it('sends Client approval with the current version', async () => {
    const fetchMock =
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            id: 'work-order-1',
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

    await approveWorkOrder(
      'token',
      'work-order-1',
      5,
    )

    expect(
      JSON.parse(
        String(
          (
            fetchMock.mock
              .calls[0][1] as RequestInit
          ).body,
        ),
      ),
    ).toEqual({
      version: 5,
    })
  })
})
