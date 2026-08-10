import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ToastProvider, useToast } from "./ToastProvider";

function ToastTrigger() {
  const { showSuccess, showError } = useToast();
  return (
    <div>
      <button onClick={() => showSuccess("Saved successfully.")}>Trigger success</button>
      <button onClick={() => showError("Something went wrong.")}>Trigger error</button>
    </div>
  );
}

describe("ToastProvider", () => {
  beforeEach(() => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("shows a success toast when triggered", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    render(
      <ToastProvider>
        <ToastTrigger />
      </ToastProvider>,
    );

    await user.click(screen.getByText("Trigger success"));

    expect(screen.getByText("Saved successfully.")).toBeInTheDocument();
  });

  it("shows an error toast when triggered", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    render(
      <ToastProvider>
        <ToastTrigger />
      </ToastProvider>,
    );

    await user.click(screen.getByText("Trigger error"));

    expect(screen.getByText("Something went wrong.")).toBeInTheDocument();
  });

  it("dismisses a toast when its close button is clicked", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    render(
      <ToastProvider>
        <ToastTrigger />
      </ToastProvider>,
    );

    await user.click(screen.getByText("Trigger success"));
    expect(screen.getByText("Saved successfully.")).toBeInTheDocument();

    await user.click(screen.getByLabelText("Dismiss"));

    expect(screen.queryByText("Saved successfully.")).not.toBeInTheDocument();
  });

  it("auto-dismisses a toast after its duration elapses", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });
    render(
      <ToastProvider>
        <ToastTrigger />
      </ToastProvider>,
    );

    await user.click(screen.getByText("Trigger success"));
    expect(screen.getByText("Saved successfully.")).toBeInTheDocument();

    vi.advanceTimersByTime(5000);

    await waitFor(() => expect(screen.queryByText("Saved successfully.")).not.toBeInTheDocument());
  });
});
