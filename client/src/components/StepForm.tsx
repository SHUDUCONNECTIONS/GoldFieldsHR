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
  /** "dark" opts a single caller into the Collabrio-styled palette (KPI page only) without affecting any other caller, which all default to "light". */
  theme?: "light" | "dark";
}

const themeClasses = {
  light: {
    stepDone: "bg-yellow-600 text-white",
    stepCurrent: "bg-yellow-600 text-white ring-4 ring-yellow-100",
    stepFuture: "bg-stone-100 text-stone-400",
    stepLabelDone: "text-slate-700",
    stepLabelFuture: "text-stone-400",
    connectorDone: "bg-yellow-600",
    connectorFuture: "bg-stone-200",
    error: "text-red-600",
    backButton: "border border-slate-300 text-slate-700 hover:bg-slate-50",
    primaryButton: "bg-yellow-600 text-white hover:bg-yellow-500",
  },
  dark: {
    stepDone: "bg-[#6fbe44] text-[#131415]",
    stepCurrent: "bg-[#6fbe44] text-[#131415] ring-4 ring-[#6fbe44]/25",
    stepFuture: "bg-white/10 text-white/40",
    stepLabelDone: "text-white/90",
    stepLabelFuture: "text-white/40",
    connectorDone: "bg-[#6fbe44]",
    connectorFuture: "bg-white/10",
    error: "text-[#e69c9c]",
    backButton: "border border-white/15 text-white/80 hover:bg-white/5",
    primaryButton: "bg-[#6fbe44] text-[#131415] hover:bg-[#93d75f]",
  },
} as const;

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
export function StepForm({
  steps,
  onSubmit,
  submitLabel,
  submittingLabel,
  isSubmitting,
  submitDisabled,
  error,
  theme = "light",
}: StepFormProps) {
  const t = themeClasses[theme];
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
                    i < step ? t.stepDone : i === step ? t.stepCurrent : t.stepFuture
                  }`}
                >
                  {i < step ? <Check className="h-3.5 w-3.5" /> : i + 1}
                </div>
                <span className={`whitespace-nowrap text-[10px] font-medium ${i <= step ? t.stepLabelDone : t.stepLabelFuture}`}>
                  {s.title}
                </span>
              </div>
              {i < steps.length - 1 && (
                <div className={`mx-2 h-0.5 flex-1 rounded-full ${i < step ? t.connectorDone : t.connectorFuture}`} />
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

      {stepError && <p className={`text-sm ${t.error}`}>{stepError}</p>}
      {isLastStep && error && <p className={`text-sm ${t.error}`}>{error}</p>}

      <div className="mt-1 flex items-center gap-2">
        {step > 0 && (
          <button type="button" onClick={goBack} className={`rounded-md px-3 py-2 text-sm font-medium ${t.backButton}`}>
            Back
          </button>
        )}
        <button
          type="button"
          onClick={handlePrimaryAction}
          disabled={isLastStep && (isSubmitting || submitDisabled)}
          className={`w-fit rounded-md px-4 py-2 text-sm font-medium disabled:opacity-50 ${t.primaryButton}`}
        >
          {isLastStep ? (isSubmitting ? submittingLabel : submitLabel) : "Next"}
        </button>
      </div>
    </form>
  );
}
