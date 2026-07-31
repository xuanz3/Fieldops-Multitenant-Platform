const configuredBaseUrl =
  import.meta.env.VITE_API_BASE_URL?.trim() ?? ''

const apiBaseUrl =
  configuredBaseUrl.replace(/\/$/, '')

export interface ApiRequestOptions
  extends Omit<RequestInit, 'body'> {
  token?: string
  body?: unknown
}

interface ProblemDetails {
  title?: string
  detail?: string
  error?: string
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  readonly status: number
  readonly fieldErrors:
    Record<string, string[]>

  constructor(
    status: number,
    message: string,
    fieldErrors:
      Record<string, string[]> = {},
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.fieldErrors = fieldErrors
  }
}

function buildHeaders(
  options: ApiRequestOptions,
): Headers {
  const headers =
    new Headers(options.headers)

  if (
    options.body !== undefined &&
    !(options.body instanceof FormData) &&
    !(options.body instanceof Blob) &&
    typeof options.body !== 'string'
  ) {
    headers.set(
      'Content-Type',
      'application/json',
    )
  }

  if (options.token) {
    headers.set(
      'Authorization',
      `Bearer ${options.token}`,
    )
  }

  headers.set(
    'Accept',
    'application/json',
  )

  return headers
}

function serialiseBody(
  body: unknown,
): BodyInit | undefined {
  if (body === undefined) {
    return undefined
  }

  if (
    body instanceof FormData ||
    body instanceof Blob ||
    typeof body === 'string' ||
    body instanceof URLSearchParams ||
    body instanceof ArrayBuffer
  ) {
    return body
  }

  return JSON.stringify(body)
}

async function readPayload(
  response: Response,
): Promise<unknown> {
  const text =
    await response.text()

  if (!text) {
    return null
  }

  const contentType =
    response.headers.get(
      'content-type',
    ) ?? ''

  if (
    !contentType.includes(
      'application/json',
    )
  ) {
    return text
  }

  return JSON.parse(text) as unknown
}

function errorMessage(
  status: number,
  payload: unknown,
): {
  message: string
  fieldErrors:
    Record<string, string[]>
} {
  if (
    payload !== null &&
    typeof payload === 'object'
  ) {
    const problem =
      payload as ProblemDetails

    return {
      message:
        problem.error ??
        problem.detail ??
        problem.title ??
        defaultMessage(status),
      fieldErrors:
        problem.errors ?? {},
    }
  }

  if (
    typeof payload === 'string' &&
    payload.trim()
  ) {
    return {
      message: payload,
      fieldErrors: {},
    }
  }

  return {
    message:
      defaultMessage(status),
    fieldErrors: {},
  }
}

function defaultMessage(
  status: number,
): string {
  switch (status) {
    case 400:
      return 'The request is invalid.'
    case 401:
      return 'Your session is missing or expired.'
    case 403:
      return 'Your role does not have access to this action.'
    case 404:
      return 'The requested record was not found.'
    case 409:
      return 'The record changed or conflicts with an existing record.'
    case 413:
      return 'The selected file is too large.'
    default:
      return 'The request could not be completed.'
  }
}

async function fetchApi(
  path: string,
  options: ApiRequestOptions = {},
): Promise<Response> {
  const response =
    await fetch(
      `${apiBaseUrl}${path}`,
      {
        ...options,
        headers:
          buildHeaders(options),
        body:
          serialiseBody(
            options.body,
          ),
      },
    )

  if (!response.ok) {
    const payload =
      await readPayload(response)
    const error =
      errorMessage(
        response.status,
        payload,
      )

    throw new ApiError(
      response.status,
      error.message,
      error.fieldErrors,
    )
  }

  return response
}

export async function apiRequest<T>(
  path: string,
  options: ApiRequestOptions = {},
): Promise<T> {
  const response =
    await fetchApi(
      path,
      options,
    )

  return await readPayload(
    response,
  ) as T
}

export async function apiDownload(
  path: string,
  token: string,
): Promise<Blob> {
  const response =
    await fetchApi(
      path,
      { token },
    )

  return await response.blob()
}
