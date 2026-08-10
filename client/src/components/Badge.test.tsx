import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Badge } from "./Badge";

describe("Badge", () => {
  it("renders the label text", () => {
    render(<Badge label="Approved" tone="emerald" />);
    expect(screen.getByText("Approved")).toBeInTheDocument();
  });

  it("applies tone-specific styling", () => {
    render(<Badge label="Rejected" tone="red" />);
    expect(screen.getByText("Rejected")).toHaveClass("bg-red-100");
  });
});
