import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ConfirmDialog } from "./ConfirmDialog";

describe("ConfirmDialog", () => {
  it("renders the title and message", () => {
    render(
      <ConfirmDialog
        title="Deactivate employee?"
        message="They will lose access."
        onConfirm={vi.fn()}
        onCancel={vi.fn()}
      />,
    );

    expect(screen.getByText("Deactivate employee?")).toBeInTheDocument();
    expect(screen.getByText("They will lose access.")).toBeInTheDocument();
  });

  it("calls onConfirm when the confirm button is clicked", async () => {
    const onConfirm = vi.fn();
    const user = userEvent.setup();
    render(
      <ConfirmDialog title="Title" message="Message" confirmLabel="Deactivate" onConfirm={onConfirm} onCancel={vi.fn()} />,
    );

    await user.click(screen.getByRole("button", { name: "Deactivate" }));

    expect(onConfirm).toHaveBeenCalledOnce();
  });

  it("calls onCancel when the cancel button is clicked", async () => {
    const onCancel = vi.fn();
    const user = userEvent.setup();
    render(<ConfirmDialog title="Title" message="Message" onConfirm={vi.fn()} onCancel={onCancel} />);

    await user.click(screen.getByRole("button", { name: "Cancel" }));

    expect(onCancel).toHaveBeenCalledOnce();
  });

  it("disables both buttons while busy", () => {
    render(<ConfirmDialog title="Title" message="Message" isBusy onConfirm={vi.fn()} onCancel={vi.fn()} />);

    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Working..." })).toBeDisabled();
  });
});
