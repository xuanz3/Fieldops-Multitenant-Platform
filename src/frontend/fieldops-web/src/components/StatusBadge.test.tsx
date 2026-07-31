import {
  describe,
  expect,
  it,
} from 'vitest'
import {
  render,
  screen,
} from '@testing-library/react'
import { StatusBadge } from './StatusBadge'

describe('StatusBadge', () => {
  it('renders readable status text', () => {
    render(
      <StatusBadge
        value="AwaitingClientApproval"
      />,
    )

    expect(
      screen.getByText(
        'Awaiting Client Approval',
      ),
    ).toBeInTheDocument()
  })
})
