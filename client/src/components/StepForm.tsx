import { useState, type FormEvent, type ReactNode } from "react";
import { Check } from "lucide-react";

export interface WizardStep {
  title: string;
  content: ReactNode;
  /** Return an error message to block advancing past this step, or null if valid. */
  validate?: () => string | null;
}

interface StepFormProps {
  steps: WizardStep[];
  onSubmit: () => void | Promise<void>;
  submitLabel: string;
  submittingLabel: string;
  isSubmitting: boolean;
  submitDisabled?: boolean;
  error?: string | null;
}

/**
 * Multi-step form shell used for every "create/submit a request" form with
 * enough fields to benefit from being split up.
 *
 * The primary action button stays type="button" at every step (never
 * type="submit") so its DOM node never changes shape between steps —
 * flipping a button's type while its own click is still being handled is a
 * known trap: Chromium can end up honoring the new type and submit the form
 * anyway. A separate, permanently type="submit" hidden button preserves
 * Enter-key submission without exposing that risk.
 */
export function StepForm({ steps, onSubmit, submitLabel, submittingLabel, isSubmitting, submitDisabled, error }: StepFormProps) {
  const [step, setStep] = useState(0);
  const [stepError, setStepError] = useState<string | null>(null);
  const isLastStep = step === steps.length - 1;

  function goNext() {
    const validationError = steps[step].validate?.() ?? null;
    if (validationError) {
      setStepError(validationError);
      return;
    }
    setStepError(null);
    setStep((s) => Math.min(s + 1, steps.length - 1));
  }

  function goBack() {
    setStepError(null);
    setStep((s) => Math.max(s - 1, 0));
  }

  function handlePrimaryAction() {
    if (!isLastStep) {
      goNext();
    } else {
      void onSubmit();
    }
  }

  function handleFormSubmit(event: FormEvent) {
    event.preventDefault();
    handlePrimaryAction();
  }

  return (
    <form onSubmit={handleFormSubmit} className="flex flex-col gap-3">
      {steps.length > 1 && (
        <div className="mb-2 flex items-center">
          {steps.map((s, i) => (
            <div key={s.title} className="flex flex-1 items-center last:flex-initial">
              <div className="flex flex-col items-center gap-1.5">
                <div
                  className={`flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold transition-colors ${
                    i < step
                      ? "bg-yellow-600 text-white"
                      : i === step
                        ? "bg-yellow-600 text-white ring-4 ring-yellow-100"
                        : "bg-stone-100 text-stone-400"
                  }`}
                >
                  {i < step ? <Check className="h-3.5 w-3.5" /> : i + 1}
                </div>
                <span
                  className={`whitespace-nowrap text-[10px] font-medium ${i <= step ? "text-slate-700" : "text-stone-400"}`}
                >
                  {s.title}
                </span>
              </div>
              {i < steps.length - 1 && (
                <div className={`mx-2 h-0.5 flex-1 rounded-full ${i < step ? "bg-yellow-600" : "bg-stone-200"}`} />
              )}
            </div>
          ))}
        </div>
      )}

      {/* Always type="submit" (never toggled) so Enter-key presses in any
          field trigger form submission without exposing the button-type-
          flip bug the visible primary button avoids above. */}
      <button type="submit" className="hidden" aria-hidden="true" tabIndex={-1} />

      {steps[step].content}

      {stepError && <p className="text-sm text-red-600">{stepError}</p>}
      {isLastStep && error && <p className="text-sm text-red-600">{error}</p>}

      <div className="mt-1 flex items-center gap-2">
        {step > 0 && (
          <button
            type="button"
            onClick={goBack}
            className="rounded-md border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
          >
            Back
          </button>
        )}
        <button
          type="button"
          onClick={handlePrimaryAction}
          disabled={isLastStep && (isSubmitting || submitDisabled)}
          className="w-fit rounded-md bg-yellow-600 px-4 py-2 text-sm font-medium text-white hover:bg-yellow-500 disabled:opacity-50"
        >
          {isLastStep ? (isSubmitting ? submittingLabel : submitLabel) : "Next"}
        </button>
      </div>
    </form>
  );
}
