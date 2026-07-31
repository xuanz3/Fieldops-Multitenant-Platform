import {
  render,
  screen,
} from '@testing-library/react'
import {
  describe,
  expect,
  it,
} from 'vitest'
import { Feedback } from './Feedback'

describe('Feedback', () => {
  it('renders a successful status state', () => {
    render(
      <Feedback
        tone="success"
        title="Evidence saved"
        message="The attachment was uploaded."
      />,
    )

    const status =
      screen.getByRole('status')

    expect(status).toHaveClass(
      'feedback-success',
    )

    expect(
      screen.getByText(
        'Evidence saved',
      ),
    ).toBeInTheDocument()
  })
})
