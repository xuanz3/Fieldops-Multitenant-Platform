import type {
  InputHTMLAttributes,
  ReactNode,
  SelectHTMLAttributes,
  TextareaHTMLAttributes,
} from 'react'

interface BaseProps {
  label: string
  htmlFor: string
  hint?: string
  error?: string
  children: ReactNode
}

function FieldFrame({
  label,
  htmlFor,
  hint,
  error,
  children,
}: BaseProps) {
  return (
    <div className="form-field">
      <label htmlFor={htmlFor}>
        {label}
      </label>
      {children}
      {hint ? (
        <span className="field-hint">
          {hint}
        </span>
      ) : null}
      {error ? (
        <span
          className="field-error"
          role="alert"
        >
          {error}
        </span>
      ) : null}
    </div>
  )
}

interface InputFieldProps
  extends InputHTMLAttributes<HTMLInputElement> {
  label: string
  hint?: string
  error?: string
}

export function InputField({
  label,
  hint,
  error,
  id,
  ...props
}: InputFieldProps) {
  const fieldId = id ?? props.name

  if (!fieldId) {
    throw new Error(
      'InputField requires an id or name.',
    )
  }

  return (
    <FieldFrame
      label={label}
      htmlFor={fieldId}
      hint={hint}
      error={error}
    >
      <input
        id={fieldId}
        aria-invalid={Boolean(error)}
        {...props}
      />
    </FieldFrame>
  )
}

interface SelectFieldProps
  extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string
  hint?: string
  error?: string
  children: ReactNode
}

export function SelectField({
  label,
  hint,
  error,
  id,
  children,
  ...props
}: SelectFieldProps) {
  const fieldId = id ?? props.name

  if (!fieldId) {
    throw new Error(
      'SelectField requires an id or name.',
    )
  }

  return (
    <FieldFrame
      label={label}
      htmlFor={fieldId}
      hint={hint}
      error={error}
    >
      <select
        id={fieldId}
        aria-invalid={Boolean(error)}
        {...props}
      >
        {children}
      </select>
    </FieldFrame>
  )
}

interface TextAreaFieldProps
  extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  label: string
  hint?: string
  error?: string
}

export function TextAreaField({
  label,
  hint,
  error,
  id,
  ...props
}: TextAreaFieldProps) {
  const fieldId = id ?? props.name

  if (!fieldId) {
    throw new Error(
      'TextAreaField requires an id or name.',
    )
  }

  return (
    <FieldFrame
      label={label}
      htmlFor={fieldId}
      hint={hint}
      error={error}
    >
      <textarea
        id={fieldId}
        aria-invalid={Boolean(error)}
        {...props}
      />
    </FieldFrame>
  )
}
