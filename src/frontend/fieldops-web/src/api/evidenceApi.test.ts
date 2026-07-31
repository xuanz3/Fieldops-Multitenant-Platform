import {
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import {
  uploadAttachment,
} from './fieldOpsApi'

describe('evidence API client', () => {
  it('sends multipart data without forcing a JSON content type', async () => {
    const fetchMock =
      vi.fn().mockResolvedValue(
        new Response(
          JSON.stringify({
            id: 'attachment-1',
            workOrderId:
              'work-order-1',
            fileName:
              'evidence.txt',
            contentType:
              'text/plain',
            sizeBytes: 8,
            sha256: 'A'.repeat(64),
            uploadedByUserId:
              'user-1',
            uploadedByDisplayName:
              'Technician',
            uploadedAt:
              new Date().toISOString(),
          }),
          {
            status: 201,
            headers: {
              'Content-Type':
                'application/json',
            },
          },
        ),
      )

    vi.stubGlobal(
      'fetch',
      fetchMock,
    )

    const file =
      new File(
        ['evidence'],
        'evidence.txt',
        {
          type: 'text/plain',
        },
      )

    await uploadAttachment(
      'signed-token',
      'work-order-1',
      file,
    )

    const [
      path,
      request,
    ] =
      fetchMock.mock.calls[0]

    expect(path).toBe(
      '/api/work-orders/work-order-1/attachments',
    )

    const options =
      request as RequestInit
    const headers =
      new Headers(
        options.headers,
      )

    expect(
      headers.get(
        'Authorization',
      ),
    ).toBe(
      'Bearer signed-token',
    )

    expect(
      headers.has(
        'Content-Type',
      ),
    ).toBe(false)

    expect(
      options.body,
    ).toBeInstanceOf(FormData)
  })
})
